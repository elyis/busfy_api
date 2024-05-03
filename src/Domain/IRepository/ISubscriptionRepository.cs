using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface ISubscriptionToAdditionalContentRepository
    {
        Task<SubscriptionToAdditionalContent?> GetAsync(Guid id);
        Task<Subscription?> GetSubscriptionAsync(Guid subId, Guid authorid);
        Task<Subscription?> AddSubscriptionAsync(UserModel sub, UserModel author);
        Task<IEnumerable<Subscription>> GetSubscriptionsWithAuthorAsync(Guid subId);
        Task<IEnumerable<SubscriptionToAdditionalContent>> GetSubscriptionsCreatedByUser(Guid userId, int count, int offset);
        Task<IEnumerable<UserSubscription>> GetSubscriptionsByUserAndSubscription(Guid userId, int count, int offset);
        Task<SubscriptionToAdditionalContent?> AddAsync(CreateSubscriptionBody body, UserModel creator);
        Task<UserSubscription?> CreateSubscriptionToUser(Guid id, UserModel user);
        Task<UserSubscription?> GetUserSubscriptionAsync(Guid id, Guid userId);
        Task<bool> RemoveSubscriptionAsync(Guid subId, Guid authorId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}