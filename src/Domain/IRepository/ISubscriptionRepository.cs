using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface ISubscriptionRepository
    {
        Task<Subscription?> GetAsync(Guid id);
        Task<IEnumerable<Subscription>> GetSubscriptionsCreatedByUser(Guid userId, int count, int offset);
        Task<IEnumerable<UserSubscription>> GetSubscriptionsByUserAndSubscription(Guid userId, int count, int offset);
        Task<Subscription?> AddAsync(CreateSubscriptionBody body, UserModel creator);
        Task<UserSubscription?> CreateSubscriptionToUser(Guid id, UserModel user);
        Task<UserSubscription?> GetUserSubscriptionAsync(Guid id, Guid userId);
        Task<bool> DeleteAsync(Guid id, Guid userId);
    }
}