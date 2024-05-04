using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Infrastructure.Repository
{
    public class UserCreationRepository : IUserCreationRepository
    {
        private readonly AppDbContext _context;

        public UserCreationRepository(AppDbContext context)
        {
            _context = context;
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

        public async Task<int> GetCountComments(Guid userCreationId)
        {
            return await _context.UserCreationComments
                .Where(e => e.CreationId == userCreationId)
                .CountAsync();
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
            => await _context.UserCreations
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);


        public async Task<IEnumerable<UserCreation>> GetUserCreationsAsync(Guid userId, IEnumerable<ContentSubscriptionType> types, int count, int offset)
        {
            var temp = types.Distinct().Select(e => e.ToString());
            if (!temp.Any())
                return new List<UserCreation>();

            return await _context.UserCreations
                .Include(e => e.User)
                .Where(e => e.UserId == userId && temp.Contains(e.ContentSubscriptionType))
                .OrderByDescending(e => e.CreatedAt)
                .Skip(offset)
                .Take(count)
                .ToListAsync();
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
            return userCreation;
        }

        public async Task<UserCreation?> UpdateDescriptionAsync(Guid id, string? description, Guid userId)
        {
            var userCreation = await GetAsync(id);
            if (userCreation == null || userCreation.UserId != userId)
                return null;

            userCreation.Description = description;
            await _context.SaveChangesAsync();

            return userCreation;
        }
    }
}