using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Infrastructure.Repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly AppDbContext _context;

        public SubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Subscription?> AddAsync(CreateSubscriptionBody body, UserModel creator)
        {
            var subscription = new Subscription
            {
                CountDays = body.CountDays,
                Creator = creator,
                Price = body.Price,
                Type = body.Type.ToString(),
            };

            subscription = (await _context.Subscriptions.AddAsync(subscription)).Entity;
            await _context.SaveChangesAsync();

            return subscription;
        }

        public async Task<UserSubscription?> CreateSubscriptionToUser(Guid id, UserModel user)
        {
            var subscription = await GetAsync(id);
            if (subscription == null)
                return null;

            var userSubscription = await GetUserSubscriptionAsync(id, user.Id);
            if (userSubscription == null)
            {
                userSubscription = new UserSubscription
                {
                    Subscription = subscription,
                    User = user,
                    EndDate = DateTime.UtcNow.AddDays(subscription.CountDays)
                };
                await _context.UserSubscriptions.AddAsync(userSubscription);
            }
            else
                userSubscription.EndDate = DateTime.UtcNow.AddDays(subscription.CountDays);

            await _context.SaveChangesAsync();

            return userSubscription;
        }

        public async Task<UserSubscription?> GetUserSubscriptionAsync(Guid id, Guid userId)
        {
            return await _context.UserSubscriptions
                .FirstOrDefaultAsync(e =>
                    e.SubscriptionId == id && e.UserId == userId);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var subscription = await GetAsync(id);
            if (subscription == null)
                return true;

            if (subscription.CreatorId != userId)
                return false;

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Subscription?> GetAsync(Guid id)
            => await _context.Subscriptions
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<IEnumerable<Subscription>> GetSubscriptionsCreatedByUser(Guid userId, int count, int offset)
        {
            return await _context.Subscriptions
                .Where(e => e.CreatorId == userId)
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserSubscription>> GetSubscriptionsByUserAndSubscription(Guid userId, int count, int offset)
        {
            return await _context.UserSubscriptions
                .Include(e => e.Subscription)
                .Where(e => e.UserId == userId)
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }
    }
}