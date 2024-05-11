using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace busfy_api.src.Infrastructure.Repository
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _distributedCache;
        private readonly string _prefix = "subscription:";
        private readonly string _prefixAdditional = "subscription:additional:";
        private readonly string _prefixUserSubscription = "userSubscription:";
        private readonly DistributedCacheEntryOptions _options = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(3),
            AbsoluteExpiration = DateTime.UtcNow.AddMinutes(6)
        };

        public SubscriptionRepository(
            AppDbContext context,
            IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        public async Task<Subscription?> CreateSubscription(CreateSubscriptionBody body, UserModel creator)
        {
            var subscription = new Subscription
            {
                CountDays = body.CountDays,
                Creator = creator,
                Price = body.Price,
                Type = body.Type.ToString(),
            };

            var userSubscription = new UserSubscription
            {
                Subscription = subscription,
                User = creator,
                EndDate = DateTime.UtcNow.AddYears(10),
            };

            subscription = (await _context.Subscriptions.AddAsync(subscription)).Entity;
            await _context.UserSubscriptions.AddAsync(userSubscription);
            await _context.SaveChangesAsync();
            // await _distributedCache.SetStringAsync($"{_prefixAdditional}{subscription.Id}", SerializeObject(subscription), _options);

            return subscription;
        }

        public async Task<UserSubscription?> CreateUserSubscription(Guid id, UserModel user)
        {
            var subscription = await GetSubscriptionAsync(id);
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
            // await _distributedCache.SetStringAsync($"{_prefixUserSubscription}{id}:{user.Id}", SerializeObject(userSubscription), _options);

            return userSubscription;
        }

        public async Task<UserSubscription?> GetUserSubscriptionAsync(Guid subscriptionId, Guid userId)
        {
            // var cachedString = await _distributedCache.GetStringAsync($"{_prefixUserSubscription}{subscriptionId}");
            UserSubscription? subscription = null;
            // if (cachedString != null)
            // {
            //     subscription = DeserializeObject<UserSubscription>(cachedString);
            //     if (subscription != null)
            //     {
            //         _context.Attach(subscription);
            //         return subscription;
            //     }
            // }

            subscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(e =>
                    e.SubscriptionId == subscriptionId && e.UserId == userId);
            // if (subscription != null)
            //     await _distributedCache.SetStringAsync($"{_prefixUserSubscription}{subscriptionId}:{userId}", SerializeObject(subscription), _options);

            return subscription;
        }

        public async Task<int> GetCountSubscriptions(Guid userId)
        {
            return await _context.UserSubscriptions
                .Where(e => e.UserId == userId)
                .CountAsync();
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var subscription = await GetSubscriptionAsync(id);
            if (subscription == null)
                return true;

            if (subscription.CreatorId != userId)
                return false;

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
            // await _distributedCache.RemoveAsync($"{_prefixAdditional}{subscription.Id}");

            return true;
        }

        public async Task<Subscription?> GetSubscriptionAsync(Guid id)
        {
            // var cachedString = await _distributedCache.GetStringAsync($"{_prefixAdditional}{id}");
            Subscription? subscription = null;
            // if (cachedString != null)
            // {
            //     subscription = DeserializeObject<Subscription>(cachedString);
            //     if (subscription != null)
            //     {
            //         _context.Attach(subscription);
            //         return subscription;
            //     }
            // }

            subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(e => e.Id == id);
            // if (subscription != null)
            //     await _distributedCache.SetStringAsync($"{_prefixAdditional}{subscription.Id}", SerializeObject(subscription), _options);

            return subscription;
        }

        public async Task<bool> RemoveUserCreation(Guid subscriptionId, Guid userId)
        {
            var userSubscription = await GetUserSubscriptionAsync(subscriptionId, userId);
            if (userSubscription == null)
                return true;

            _context.UserSubscriptions.Remove(userSubscription);
            await _context.SaveChangesAsync();
            return true;
        }

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
                    .ThenInclude(e => e.UniqueContent)
                .Where(e => e.UserId == userId)
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }


        private static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private static T? DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<int> GetCountSubscribersByCreator(Guid id)
        {
            var subscriptions = await _context.Subscriptions
                .Where(e => e.CreatorId == id)
                .ToListAsync();

            if (!subscriptions.Any())
                return 0;

            var ids = subscriptions.Select(e => e.Id);
            return await _context.UserSubscriptions
                .Where(e => ids.Contains(e.SubscriptionId))
                .CountAsync();
        }
    }
}