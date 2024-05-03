using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface IPostRepository
    {
        Task<Post> AddAsync(CreatePostBody body, UserModel creator, ContentCategory category);
        Task<Post?> UpdateImageAsync(Guid id, string filename, UserCreationType type);
        Task<int> GetCountPosts();
        Task<int> GetCountFavouritePosts(Guid userId);
        Task<IEnumerable<PostComment>> GetCommentsWithUser(Guid postId, int count, int offset, bool isDescending);
        Task<int> GetCountPostByCreators(IEnumerable<Guid> creatorIds);
        Task<IEnumerable<Post>> GetFavouritePosts(Guid userId, int count, int offset, bool isDescending);
        Task<PostComment?> GetPostComment(Guid userId, Guid postId);
        Task<PostComment?> AddPostComment(Post post, UserModel user);
        Task<PostLike?> GetPostLike(Guid userId, Guid postId);
        Task<PostLike?> AddPostLike(Post post, UserModel user);
        Task<IEnumerable<Post>> GetAllByCategories(int count, int offset, IEnumerable<string> categoryNames, bool isDescending);
        Task<IEnumerable<Post>> GetAll(int count, int offset, bool isDescending);
        Task<IEnumerable<Post>> GetAllByCategory(string categoryName, int count, int offset, bool isDescending);
        Task<int> GetCountPostsByCategories(IEnumerable<string> categories);
        Task<IEnumerable<Post>> GetAllByCreators(IEnumerable<Guid> creatorIds, int count, int offset, bool isDescending);
        Task<Post?> GetAsync(Guid id);
        Task<IEnumerable<PostLike>> GetAllLikes(Guid userId, IEnumerable<Guid> postIds);
        Task<int> GetCountComments(Guid postId);
    }
}