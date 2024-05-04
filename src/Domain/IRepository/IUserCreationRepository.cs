using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface IUserCreationRepository
    {
        Task<UserCreation> AddAsync(CreateUserCreationBody userCreationBody, UserModel user, ContentCategory category);
        Task<int> GetCountLikes(Guid id);
        Task<UserCreation?> UpdateAsync(UpdateContentBody body, Guid userId);
        Task<UserCreation?> GetAsync(Guid id);
        Task<UserCreation?> UpdateDescriptionAsync(Guid id, string? description, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task<int> GetCountComments(Guid userCreationId);
        Task<IEnumerable<UserCreation>> GetUserCreationsAsync(Guid userId, IEnumerable<ContentSubscriptionType> types, int count, int offset);
        Task<UserCreationLike?> CreateUserCreationLikeAsync(UserModel user, UserCreation userCreation);
        Task<UserCreationLike?> GetUserCreationLikeAsync(Guid userCreationId, Guid userId);
        Task<UserCreationComment> CreateUserCreationCommentAsync(CreateCommentBody commentBody, UserCreation userCreation, UserModel user);
        Task<int> GetCountUserCreations(Guid userId, IEnumerable<ContentSubscriptionType> types);
        Task<UserCreationComment?> GetUserCreationCommentAsync(Guid id);
        Task<IEnumerable<UserCreationComment>> GetUserCreationCommentsAndUserAsync(Guid id, int count, int offset);
    }
}