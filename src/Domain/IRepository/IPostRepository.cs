using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface IPostRepository
    {
        Task<Post> AddAsync(CreatePostBody body, UserModel creator, ContentCategory category, Subscription? subscription);
        Task<Post?> UpdateFileAsync(Guid id, string filename, UserCreationType type);
        Task<Post?> UpdateSubscriptionType(Guid postId, Subscription subscription);
        Task<int> GetCountPosts();
        Task<int> GetCountLikes(Guid id);
        Task<int> GetCountLikesByAuthor(Guid userId);
        Task<int> GetCountFavouritePosts(Guid userId);
        Task<bool> RemoveLike(Guid postId, Guid userId);
        Task<IEnumerable<PostComment>> GetCommentsWithUser(Guid postId, int count, int offset, bool isDescending);
        Task<int> GetCountPostByCreators(IEnumerable<Guid> creatorIds);
        Task<IEnumerable<Post>> GetFavouritePosts(Guid userId, int count, int offset, bool isDescending);
        Task<PostComment?> GetPostComment(Guid userId, Guid postId);
        Task<PostComment?> AddPostComment(Post post, UserModel user, CreateCommentBody body);
        Task<PostLike?> GetPostLike(Guid userId, Guid postId);
        Task<PostLike?> AddPostLike(Post post, UserModel user);
        Task<IEnumerable<Post>> GetAllByCategories(IEnumerable<ContentSubscriptionType> subscriptionTypes, int count, int offset, IEnumerable<string> categoryNames, bool isDescending);
        Task<IEnumerable<Post>> GetAll(IEnumerable<ContentSubscriptionType> subscriptionTypes, int count, int offset, bool isDescending);
        Task<IEnumerable<Post>> GetAllByCreator(IEnumerable<ContentSubscriptionType> subscriptionTypes, Guid creatorId, int count, int offset, bool isDescending);
        Task<int> GetCountPostsByCreator(Guid creatorId);
        Task<IEnumerable<Post>> GetAllByCategory(IEnumerable<ContentSubscriptionType> subscriptionTypes, string categoryName, int count, int offset, bool isDescending);
        Task<int> GetCountPostsByCategories(IEnumerable<string> categories);
        Task<IEnumerable<Post>> GetAllByCreators(IEnumerable<ContentSubscriptionType> subscriptionTypes, IEnumerable<Guid> creatorIds, int count, int offset, bool isDescending);
        Task<Post?> GetAsync(Guid id);
        Task<IEnumerable<PostLike>> GetAllLikes(Guid userId, IEnumerable<Guid> postIds);
        Task<int> GetCountComments(Guid postId);
    }
}