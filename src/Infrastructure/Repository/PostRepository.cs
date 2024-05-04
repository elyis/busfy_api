using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Infrastructure.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Post> AddAsync(CreatePostBody body, UserModel creator, ContentCategory category)
        {
            var isTextPost = body.Text != null;

            var post = new Post
            {
                Category = category,
                Description = body.Description,
                Text = body.Text,
                Type = isTextPost ? UserCreationType.Text.ToString() : null,
                IsFormed = isTextPost,
                Creator = creator,
                IsCommentingAllowed = body.IsCommentingAllowed
            };

            post = (await _context.Posts.AddAsync(post)).Entity;
            await _context.SaveChangesAsync();

            return post;
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

        public async Task<IEnumerable<Post>> GetAll(int count, int offset, bool isDescending)
        {
            var query = _context.Posts
                .Include(e => e.Creator)
                .Where(e => e.IsFormed);

            if (isDescending)
                query = query.OrderByDescending(e => e.CreatedAt);
            else
                query = query.OrderBy(e => e.CreatedAt);

            return await query
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetAllByCategories(int count, int offset, IEnumerable<string> categoryNames, bool isDescending)
        {
            var query = _context.Posts
                .Include(e => e.Creator)
                .Where(e => e.IsFormed && categoryNames.Contains(e.CategoryName));

            if (isDescending)
                query = query.OrderByDescending(e => e.CreatedAt);
            else
                query = query.OrderBy(e => e.CreatedAt);

            return await query
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetAllByCategory(string categoryName, int count, int offset, bool isDescending)
        {
            var query = _context.Posts
                .Include(e => e.Creator)
                .Where(e => e.CategoryName == categoryName && e.IsFormed);

            if (isDescending)
                query = query.OrderByDescending(e => e.CreatedAt);
            else
                query = query.OrderBy(e => e.CreatedAt);

            return await query
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetAllByCreators(IEnumerable<Guid> creatorIds, int count, int offset, bool isDescending)
        {
            var query = _context.Posts
                .Include(e => e.Creator)
                .Where(e => creatorIds.Contains(e.CreatorId) && e.IsFormed);

            if (isDescending)
                query = query.OrderByDescending(e => e.CreatedAt);
            else
                query = query.OrderBy(e => e.CreatedAt);

            return await query
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<PostLike>> GetAllLikes(Guid userId, IEnumerable<Guid> postIds)
        {
            return await _context.PostLikes
                .Where(e => e.EvaluatorId == userId && postIds.Contains(e.PostId))
                .ToListAsync();
        }

        public async Task<Post?> GetAsync(Guid id)
        {
            return await _context.Posts
                .Include(e => e.Creator)
                .FirstOrDefaultAsync(e => e.Id == id);
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
            return await _context.Posts.CountAsync();
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
            return await _context.PostLikes
                .Where(e => e.PostId == id)
                .CountAsync();
        }

        public async Task<Post?> UpdateImageAsync(Guid id, string filename, UserCreationType type)
        {
            var post = await GetAsync(id);
            if (post == null || string.IsNullOrWhiteSpace(filename))
                return null;

            post.Filename = filename;
            post.IsFormed = true;
            post.Type = type.ToString();

            await _context.SaveChangesAsync();
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
            return await _context.PostComments
                .Where(e => e.PostId == postId)
                .CountAsync();
        }
    }
}