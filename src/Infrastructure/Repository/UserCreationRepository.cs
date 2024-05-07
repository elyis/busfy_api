using System.Transactions;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace busfy_api.src.Infrastructure.Repository
{
    public class UserCreationRepository : IUserCreationRepository
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _distributedCache;
        private readonly string _prefix = "creation:";
        private readonly string _prefixForMany = "creations:";
        private readonly DistributedCacheEntryOptions _optionsForSingleEntity = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(3),
            AbsoluteExpiration = DateTime.UtcNow.AddMinutes(6)
        };

        private readonly DistributedCacheEntryOptions _optionsForEntities = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(1),
            AbsoluteExpiration = DateTime.UtcNow.AddMinutes(2)
        };

        public UserCreationRepository(
            AppDbContext context,
            IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        public async Task<int> GetCountLikesByAuthor(Guid userId)
        {
            var cachedKey = $"{_prefix}likes:user:{userId}";
            var cachedData = await _distributedCache.GetStringAsync(cachedKey);
            if (string.IsNullOrEmpty(cachedData))
                return JsonConvert.DeserializeObject<int>(cachedData);

            var countLikes = await _context.UserCreations
                .Where(e => e.UserId == userId && e.IsFormed)
                .SelectMany(post => post.Likes)
                .CountAsync();

            await _distributedCache.SetStringAsync(cachedKey, countLikes.ToString());
            return countLikes;
        }
        public async Task<UserCreation> AddAsync(CreateUserCreationBody userCreationBody, UserModel user, ContentCategory contentCategory)
        {
            var isTextContent = userCreationBody.Text != null;
            var userCreation = new UserCreation
            {
                Description = userCreationBody.Description,
                ContentSubscriptionType = userCreationBody.ContentSubscriptionType.ToString(),
                User = user,
                Text = userCreationBody.Text,
                Type = isTextContent ? UserCreationType.Text.ToString() : null,
                IsFormed = isTextContent,
                ContentCategory = contentCategory,
            };

            userCreation = (await _context.UserCreations.AddAsync(userCreation)).Entity;
            var resultString = SerializeObject(userCreation);
            await _distributedCache.SetStringAsync($"{_prefix}{userCreation.Id}", resultString, _optionsForSingleEntity);

            await _context.SaveChangesAsync();

            return userCreation;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var userCreation = await GetAsync(id);
            if (userCreation == null)
                return true;

            if (userCreation.UserId != userId)
                return false;

            _context.UserCreations.Remove(userCreation);
            await _context.SaveChangesAsync();
            await _distributedCache.RemoveAsync($"{_prefix}{id}");

            return true;
        }

        public async Task<UserCreationLike?> CreateUserCreationLikeAsync(UserModel user, UserCreation userCreation)
        {
            var userCreationLike = await GetUserCreationLikeAsync(userCreation.Id, user.Id);
            if (userCreationLike != null)
                return null;

            var like = new UserCreationLike
            {
                Evaluator = user,
                Creation = userCreation
            };

            like = (await _context.UserCreationLikes.AddAsync(like)).Entity;
            await _context.SaveChangesAsync();

            return like;
        }

        public async Task<UserCreationLike?> GetUserCreationLikeAsync(Guid userCreationId, Guid userId)
        {
            return await _context.UserCreationLikes
                .FirstOrDefaultAsync(e => e.EvaluatorId == userId && e.CreationId == userCreationId);
        }

        public async Task<UserCreationComment?> GetUserCreationCommentAsync(Guid id)
        {
            return await _context.UserCreationComments
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<UserCreationLike>> GetAllLikes(Guid userId, IEnumerable<Guid> creationIds)
        {
            return await _context.UserCreationLikes
                .Where(e => e.EvaluatorId == userId && creationIds.Contains(e.CreationId))
                .ToListAsync();
        }

        public async Task<int> GetCountComments(Guid userCreationId)
        {
            var cachedKey = $"{_prefix}comments:creation:{userCreationId}";
            var cachedData = await _distributedCache.GetStringAsync(cachedKey);
            if (string.IsNullOrEmpty(cachedData))
                return JsonConvert.DeserializeObject<int>(cachedData);

            var countLikes = await _context.UserCreationComments
                .Where(e => e.CreationId == userCreationId)
                .CountAsync();

            await _distributedCache.SetStringAsync(cachedKey, countLikes.ToString());
            return countLikes;
        }

        public async Task<IEnumerable<UserCreationComment>> GetUserCreationCommentsAndUserAsync(Guid id, int count, int offset)
        {
            return await _context.UserCreationComments
                .Include(e => e.User)
                .OrderByDescending(e => e.CreatedAt)
                .Where(e => e.CreationId == id)
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task<UserCreationComment> CreateUserCreationCommentAsync(CreateCommentBody commentBody, UserCreation userCreation, UserModel user)
        {
            var comment = new UserCreationComment
            {
                Comment = commentBody.Comment,
                Creation = userCreation,
                User = user
            };

            await _context.UserCreationComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task<UserCreation?> GetAsync(Guid id)
        {
            var cachedString = await _distributedCache.GetStringAsync($"{_prefix}{id}");
            UserCreation? userCreation = null;

            if (!string.IsNullOrEmpty(cachedString))
            {
                userCreation = DeserializeObject<UserCreation>(cachedString);
                if (userCreation != null)
                {
                    _context.Attach(userCreation);
                    return userCreation;
                }
            }

            userCreation = await _context.UserCreations
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);
            if (userCreation != null)
            {
                var resultString = SerializeObject(userCreation);
                await _distributedCache.SetStringAsync($"{_prefix}{userCreation.Id}", resultString, _optionsForSingleEntity);
            }

            return userCreation;
        }


        public async Task<IEnumerable<UserCreation>> GetUserCreationsAsync(
            Guid userId,
            IEnumerable<ContentSubscriptionType> types,
            int count,
            int offset)
        {
            var temp = types.Distinct().Select(e => e.ToString());
            if (!temp.Any())
                return new List<UserCreation>();

            var cachedKey = $"{_prefixForMany}{userId}:{string.Join("_", temp)}:{count}:{offset}";
            var cachedData = await _distributedCache.GetStringAsync(cachedKey);

            if (!string.IsNullOrEmpty(cachedData))
                return JsonConvert.DeserializeObject<IEnumerable<UserCreation>>(cachedData);

            var creations = await _context.UserCreations
                .Include(e => e.User)
                .Where(e => e.UserId == userId && temp.Contains(e.ContentSubscriptionType))
                .OrderByDescending(e => e.CreatedAt)
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            var serializedData = JsonConvert.SerializeObject(creations);
            await _distributedCache.SetStringAsync(cachedKey, serializedData, _optionsForEntities);
            return creations;
        }

        public async Task<int> GetCountLikes(Guid id)
        {
            var cachedKey = $"{_prefix}likes:creation:{id}";
            var cachedData = await _distributedCache.GetStringAsync(cachedKey);
            if (string.IsNullOrEmpty(cachedData))
                return JsonConvert.DeserializeObject<int>(cachedData);

            var countLikes = await _context.UserCreationLikes
                .Where(e => e.CreationId == id)
                .CountAsync();

            await _distributedCache.SetStringAsync(cachedKey, countLikes.ToString());
            return countLikes;
        }

        public async Task<int> GetCountUserCreations(Guid userId, IEnumerable<ContentSubscriptionType> types)
        {
            var temp = types.Distinct().Select(e => e.ToString());
            if (!temp.Any())
                return 0;

            return await _context.UserCreations
                .Where(e => e.UserId == userId && temp.Contains(e.ContentSubscriptionType))
                .CountAsync();
        }

        public async Task<UserCreation?> UpdateAsync(UpdateContentBody body, Guid userId)
        {
            var userCreation = await GetAsync(body.Id);
            if (userCreation == null || userCreation.UserId != userId)
                return null;

            userCreation.Filename = body.Filename;
            userCreation.Type = body.Type.ToString();
            userCreation.IsFormed = true;

            await _context.SaveChangesAsync();
            var resultString = SerializeObject(userCreation);
            await _distributedCache.SetStringAsync($"{_prefix}{userCreation.Id}", resultString, _optionsForSingleEntity);

            return userCreation;
        }

        public async Task<UserCreation?> UpdateDescriptionAsync(Guid id, string? description, Guid userId)
        {
            var userCreation = await GetAsync(id);
            if (userCreation == null || userCreation.UserId != userId)
                return null;

            userCreation.Description = description;
            await _context.SaveChangesAsync();
            var resultString = SerializeObject(userCreation);
            await _distributedCache.SetStringAsync($"{_prefix}{userCreation.Id}", resultString, _optionsForSingleEntity);

            return userCreation;
        }

        private static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private static T? DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<bool> RemoveLike(Guid userId, Guid creationId)
        {
            var like = await GetUserCreationLikeAsync(creationId, userId);
            if (like == null)
                return true;

            _context.UserCreationLikes.Remove(like);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}