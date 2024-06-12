using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface ISubscriptionRepository
    {
        Task<Subscription?> GetSubscriptionAsync(Guid id);
        Task<int> GetCountSubscribersByCreator(Guid id);
        Task<bool> RemoveUserCreation(Guid subscriptionId, Guid userId);
        Task<int> GetCountSubscriptions(Guid userId);
        Task<IEnumerable<Subscription>> GetSubscriptionsCreatedByUser(Guid userId, int count, int offset);
        Task<IEnumerable<UserSubscription>> GetSubscriptionsByUserAndSubscription(Guid userId, int count, int offset);
        Task<Subscription?> CreateSubscription(CreateSubscriptionBody body, UserModel creator);
        Task<UserSubscription?> CreateUserSubscription(Guid id, UserModel user);
        Task<UserSubscription?> GetUserSubscriptionAsync(Guid subscriptionId, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
        Task<IEnumerable<UserSubscription>> GetSubscriptionsToCreator(Guid creatorId, Guid subId);
    }
}