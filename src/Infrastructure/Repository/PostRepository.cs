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
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _distributedCache;
        private readonly string _prefix = "post:";
        private readonly string _prefixForMany = "posts:";
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

        public PostRepository(
            AppDbContext context,
            IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        public async Task<Post> AddAsync(CreatePostBody body, UserModel creator, ContentCategory category, Subscription? subscription)
        {
            var isTextPost = body.Text != null;

            var subscriptionType = ContentSubscriptionType.Public.ToString();
            if (subscription != null)
                subscriptionType = subscription.Type;

            var post = new Post
            {
                Category = category,
                Description = body.Description,
                Text = body.Text,
                Type = isTextPost ? UserCreationType.Text.ToString() : null,
                IsFormed = isTextPost,
                Creator = creator,
                IsCommentingAllowed = body.IsCommentingAllowed,
                ContentSubscriptionType = subscriptionType,
                Subscription = subscription
            };

            post = (await _context.Posts.AddAsync(post)).Entity;
            await _context.SaveChangesAsync();
            // await _distributedCache.SetStringAsync($"{_prefix}{post.Id}", SerializeObject(post), _optionsForSingleEntity);

            return post;
        }

        public async Task<Post?> GetByFilename(string filename)
        {
            return await _context.Posts
                .FirstOrDefaultAsync(e => e.Filename == filename);
        }

        public async Task<int> GetCountLikesByAuthor(Guid userId)
        {
            // var cachedKey = $"{_prefix}likes:user:{userId}";
            // var cachedData = await _distributedCache.GetStringAsync(cachedKey);
            // if (!string.IsNullOrEmpty(cachedData))
            //     return JsonConvert.DeserializeObject<int>(cachedData);

            var countLikes = await _context.Posts
                .Where(e => e.CreatorId == userId && e.IsFormed)
                .SelectMany(post => post.Likes)
                .CountAsync();

            // await _distributedCache.SetStringAsync(cachedKey, countLikes.ToString());
            return countLikes;
        }

        public async Task<PostComment?> AddPostComment(Post post, UserModel user, CreateCommentBody body)
        {
            var postComment = await GetPostComment(user.Id, post.Id);
            if (postComment != null || !post.IsCommentingAllowed)
                return null;

            postComment = new PostComment
            {
                Post = post,
                User = user,
                Comment = body.Comment,
            };

            postComment = (await _context.PostComments.AddAsync(postComment)).Entity;
            await _context.SaveChangesAsync();

            return postComment;
        }

        public async Task<PostLike?> AddPostLike(Post post, UserModel user)
        {
            var postLike = await GetPostLike(user.Id, post.Id);
            if (postLike != null)
                return null;

            postLike = new PostLike
            {
                Post = post,
                Evaluator = user
            };
            postLike = (await _context.PostLikes.AddAsync(postLike)).Entity;
            await _context.SaveChangesAsync();
            return postLike;
        }

        public async Task<IEnumerable<Post>> GetAll(IEnumerable<ContentSubscriptionType> subscriptionTypes, int count, int offset, bool isDescending)
        {
            // var cachedKey = $"{_prefixForMany}:{count}:{offset}:{isDescending}";
            // var cachedData = await _distributedCache.GetStringAsync(cachedKey);

            // if (!string.IsNullOrEmpty(cachedData))
            //     return JsonConvert.DeserializeObject<IEnumerable<Post>>(cachedData);

            var types = subscriptionTypes.Distinct().Select(e => e.ToString());
            var query = _context.Posts
               .Include(e => e.Creator)
               .Where(e => e.IsFormed && types.Contains(e.ContentSubscriptionType));

            if (isDescending)
                query = query.OrderByDescending(e => e.CreatedAt);
            else
                query = query.OrderBy(e => e.CreatedAt);

            var posts = await query
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            // var serializedData = JsonConvert.SerializeObject(posts);
            // await _distributedCache.SetStringAsync(cachedKey, serializedData, _optionsForEntities);
            return posts;
        }

        public async Task<IEnumerable<Post>> GetAllByCategories(IEnumerable<ContentSubscriptionType> subscriptionTypes, int count, int offset, IEnumerable<string> categoryNames, bool isDescending)
        {
            // var cachedKey = $"{_prefixForMany}{string.Join("_", categoryNames)}:{count}:{offset}:{isDescending}";
            // var cachedData = await _distributedCache.GetStringAsync(cachedKey);

            // if (!string.IsNullOrEmpty(cachedData))
            //     return JsonConvert.DeserializeObject<IEnumerable<Post>>(cachedData);
            var types = subscriptionTypes.Distinct().Select(e => e.ToString());

            var query = _context.Posts
                .Include(e => e.Creator)
                .Where(e => e.IsFormed && categoryNames.Contains(e.CategoryName) && types.Contains(e.ContentSubscriptionType));

            if (isDescending)
                query = query.OrderByDescending(e => e.CreatedAt);
            else
                query = query.OrderBy(e => e.CreatedAt);

            var posts = await query
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            // var serializedData = JsonConvert.SerializeObject(posts);
            // await _distributedCache.SetStringAsync(cachedKey, serializedData, _optionsForEntities);
            return posts;
        }

        public async Task<IEnumerable<Post>> GetAllByCategory(IEnumerable<ContentSubscriptionType> subscriptionTypes, string categoryName, int count, int offset, bool isDescending)
        {
            // var cachedKey = $"{_prefixForMany}{categoryName}:{count}:{offset}:{isDescending}";
            // var cachedData = await _distributedCache.GetStringAsync(cachedKey);

            // if (!string.IsNullOrEmpty(cachedData))
            //     return JsonConvert.DeserializeObject<IEnumerable<Post>>(cachedData);

            var types = subscriptionTypes.Distinct().Select(e => e.ToString());

            var query = _context.Posts
                .Include(e => e.Creator)
                .Where(e => e.CategoryName == categoryName && e.IsFormed && types.Contains(e.ContentSubscriptionType));

            if (isDescending)
                query = query.OrderByDescending(e => e.CreatedAt);
            else
                query = query.OrderBy(e => e.CreatedAt);

            var posts = await query
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            // var serializedData = JsonConvert.SerializeObject(posts);
            // await _distributedCache.SetStringAsync(cachedKey, serializedData, _optionsForEntities);
            return posts;
        }

        public async Task<IEnumerable<Post>> GetAllByCreators(IEnumerable<ContentSubscriptionType> subscriptionTypes, IEnumerable<Guid> creatorIds, int count, int offset, bool isDescending)
        {
            // var cachedKey = $"{_prefixForMany}creators:{string.Join("_", creatorIds)}:{count}:{offset}:{isDescending}";
            // var cachedData = await _distributedCache.GetStringAsync(cachedKey);

            // if (!string.IsNullOrEmpty(cachedData))
            //     return JsonConvert.DeserializeObject<IEnumerable<Post>>(cachedData);
            var types = subscriptionTypes.Distinct().Select(e => e.ToString());

            var query = _context.Posts
                .Include(e => e.Creator)
                .Where(e => creatorIds.Contains(e.CreatorId) && e.IsFormed && types.Contains(e.ContentSubscriptionType));

            if (isDescending)
                query = query.OrderByDescending(e => e.CreatedAt);
            else
                query = query.OrderBy(e => e.CreatedAt);

            var posts = await query
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            // var serializedData = JsonConvert.SerializeObject(posts);
            // await _distributedCache.SetStringAsync(cachedKey, serializedData, _optionsForEntities);
            return posts;
        }

        public async Task<IEnumerable<PostLike>> GetAllLikes(Guid userId, IEnumerable<Guid> postIds)
        {
            return await _context.PostLikes
                .Where(e => e.EvaluatorId == userId && postIds.Contains(e.PostId))
                .ToListAsync();
        }

        public async Task<Post?> GetAsync(Guid id)
        {
            // var cachedString = await _distributedCache.GetStringAsync($"{_prefix}{id}");
            Post? post = null;

            // if (!string.IsNullOrEmpty(cachedString))
            // {
            //     post = DeserializeObject<Post>(cachedString);
            //     if (post != null)
            //     {
            //         _context.Attach(post);
            //         return post;
            //     }
            // }

            post = await _context.Posts
                .Include(e => e.Creator)
                .FirstOrDefaultAsync(e => e.Id == id);
            // if (post != null)
            // {
            //     var resultString = SerializeObject(post);
            //     await _distributedCache.SetStringAsync($"{_prefix}{post.Id}", resultString, _optionsForSingleEntity);
            // }

            return post;
        }

        public async Task<int> GetCountFavouritePosts(Guid userId)
        {
            return await _context.PostLikes
                .Where(e => e.EvaluatorId == userId)
                .CountAsync();
        }

        public async Task<int> GetCountPostByCreators(IEnumerable<Guid> creatorIds)
        {
            return await _context.Posts
                .Where(e => creatorIds.Contains(e.CreatorId) && e.IsFormed)
                .CountAsync();
        }

        public async Task<int> GetCountPosts()
        {
            return await _context.Posts.CountAsync(e => e.IsFormed);
        }

        public async Task<int> GetCountPostsByCategories(IEnumerable<string> categories)
        {
            return await _context.Posts
                .Where(e => e.IsFormed && categories.Contains(e.CategoryName))
                .CountAsync();
        }

        public async Task<IEnumerable<Post>> GetFavouritePosts(Guid userId, int count, int offset, bool isDescending)
        {
            var query = _context.PostLikes.Where(e => e.EvaluatorId == userId);
            if (isDescending)
                query.OrderByDescending(e => e.CreatedAt);
            else
                query.OrderBy(e => e.CreatedAt);

            var result = await query
                .Include(e => e.Post)
                    .ThenInclude(e => e.Creator)
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            return result.Select(e => e.Post);
        }

        public async Task<PostComment?> GetPostComment(Guid userId, Guid postId)
        {
            return await _context.PostComments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.PostId == postId);
        }

        public async Task<PostLike?> GetPostLike(Guid userId, Guid postId)
        {
            return await _context.PostLikes
                .FirstOrDefaultAsync(e =>
                    e.EvaluatorId == userId && e.PostId == postId);
        }

        public async Task<int> GetCountLikes(Guid id)
        {
            // var cachedKey = $"{_prefix}likes:post:{id}";
            // var cachedData = await _distributedCache.GetStringAsync(cachedKey);
            // if (!string.IsNullOrEmpty(cachedData))
            //     return JsonConvert.DeserializeObject<int>(cachedData);

            var countLikes = await _context.PostLikes
                .Where(e => e.PostId == id)
                .CountAsync();

            // await _distributedCache.SetStringAsync(cachedKey, countLikes.ToString());
            return countLikes;
        }

        public async Task<Post?> UpdateFileAsync(Guid id, string filename, UserCreationType type)
        {
            var post = await GetAsync(id);
            if (post == null || string.IsNullOrWhiteSpace(filename))
                return null;

            post.Filename = filename;
            post.IsFormed = true;
            post.Type = type.ToString();

            await _context.SaveChangesAsync();
            // await _distributedCache.SetStringAsync($"{_prefix}{post.Id}", SerializeObject(post), _optionsForSingleEntity);
            return post;
        }

        public async Task<IEnumerable<PostComment>> GetCommentsWithUser(Guid postId, int count, int offset, bool isDescending)
        {
            var query = _context.PostComments
                .Include(e => e.User)
                .Where(e => e.PostId == postId);

            if (isDescending)
                query = query.OrderByDescending(e => e.CreatedAt);
            else
                query = query.OrderBy(e => e.CreatedAt);

            var result = await query
                .Skip(offset)
                .Take(count)
                .ToListAsync();

            return result;
        }

        public async Task<int> GetCountComments(Guid postId)
        {
            // var cachedKey = $"{_prefix}comments:post:{postId}";
            // var cachedData = await _distributedCache.GetStringAsync(cachedKey);
            // if (!string.IsNullOrEmpty(cachedData))
            //     return JsonConvert.DeserializeObject<int>(cachedData);

            var countLikes = await _context.PostComments
                .Where(e => e.PostId == postId)
                .CountAsync();

            // await _distributedCache.SetStringAsync(cachedKey, countLikes.ToString());
            return countLikes;
        }

        private static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private static T? DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<bool> RemoveLike(Guid postId, Guid userId)
        {
            var like = await GetPostLike(userId, postId);
            if (like == null)
                return true;

            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Post>> GetAllByCreator(IEnumerable<ContentSubscriptionType> subscriptionTypes, Guid creatorId, int count, int offset, bool isDescending)
        {
            var types = subscriptionTypes.Distinct().Select(e => e.ToString());

            var query = _context.Posts
               .Include(e => e.Creator)
               .Where(e => e.IsFormed && e.CreatorId == creatorId && types.Contains(e.ContentSubscriptionType));

            if (isDescending)
                query = query.OrderByDescending(e => e.CreatedAt);
            else
                query = query.OrderBy(e => e.CreatedAt);

            return await query
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetCountPostsByCreator(Guid creatorId)
        {
            return await _context.Posts
                .Where(e => e.IsFormed && e.CreatorId == creatorId)
                .CountAsync();
        }

        public async Task<Post?> UpdateSubscriptionType(Guid postId, Subscription subscription)
        {
            var post = await GetAsync(postId);
            if (post == null || subscription == null)
                return null;

            post.Subscription = subscription;
            post.ContentSubscriptionType = ContentSubscriptionType.Single.ToString();
            await _context.SaveChangesAsync();

            return post;
        }
    }
}