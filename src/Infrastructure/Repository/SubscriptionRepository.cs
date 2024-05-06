using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace busfy_api.src.Infrastructure.Repository
{
    public class SubscriptionToAdditionalContentRepository : ISubscriptionToAdditionalContentRepository
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

        public SubscriptionToAdditionalContentRepository(
            AppDbContext context,
            IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        public async Task<SubscriptionToAdditionalContent?> AddAsync(CreateSubscriptionBody body, UserModel creator)
        {
            var subscription = new SubscriptionToAdditionalContent
            {
                CountDays = body.CountDays,
                Creator = creator,
                Price = body.Price,
                Type = body.Type.ToString(),
            };

            subscription = (await _context.SubscriptionsToAdditionalContent.AddAsync(subscription)).Entity;
            await _context.SaveChangesAsync();
            await _distributedCache.SetStringAsync($"{_prefixAdditional}{subscription.Id}", SerializeObject(subscription), _options);

            return subscription;
        }

        public async Task<UserSubscription?> CreateSubscriptionToUser(Guid id, UserModel user)
        {
            var subscription = await GetSubscriptionToAdditionalContentAsync(id);
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
            await _distributedCache.SetStringAsync($"{_prefixUserSubscription}{id}:{user.Id}", SerializeObject(userSubscription), _options);

            return userSubscription;
        }

        public async Task<UserSubscription?> GetUserSubscriptionAsync(Guid id, Guid userId)
        {
            var cachedString = await _distributedCache.GetStringAsync($"{_prefixUserSubscription}{id}");
            UserSubscription? subscription = null;
            if (cachedString != null)
            {
                subscription = DeserializeObject<UserSubscription>(cachedString);
                if (subscription != null)
                {
                    _context.Attach(subscription);
                    return subscription;
                }
            }

            subscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(e =>
                    e.SubscriptionId == id && e.UserId == userId);
            if (subscription != null)
                await _distributedCache.SetStringAsync($"{_prefixUserSubscription}{id}:{userId}", SerializeObject(subscription), _options);

            return subscription;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId)
        {
            var subscription = await GetSubscriptionToAdditionalContentAsync(id);
            if (subscription == null)
                return true;

            if (subscription.CreatorId != userId)
                return false;

            _context.SubscriptionsToAdditionalContent.Remove(subscription);
            await _context.SaveChangesAsync();
            await _distributedCache.RemoveAsync($"{_prefixAdditional}{subscription.Id}");

            return true;
        }

        public async Task<SubscriptionToAdditionalContent?> GetSubscriptionToAdditionalContentAsync(Guid id)
        {
            var cachedString = await _distributedCache.GetStringAsync($"{_prefixAdditional}{id}");
            SubscriptionToAdditionalContent? subscription = null;
            if (cachedString != null)
            {
                subscription = DeserializeObject<SubscriptionToAdditionalContent>(cachedString);
                if (subscription != null)
                {
                    _context.Attach(subscription);
                    return subscription;
                }
            }

            subscription = await _context.SubscriptionsToAdditionalContent
                .FirstOrDefaultAsync(e => e.Id == id);
            if (subscription != null)
                await _distributedCache.SetStringAsync($"{_prefixAdditional}{subscription.Id}", SerializeObject(subscription), _options);

            return subscription;
        }

        public async Task<IEnumerable<SubscriptionToAdditionalContent>> GetSubscriptionsCreatedByUser(Guid userId, int count, int offset)
        {
            return await _context.SubscriptionsToAdditionalContent
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

        public async Task<Subscription?> GetSubscriptionAsync(Guid subId, Guid authorid)
        {
            var cachedString = await _distributedCache.GetStringAsync($"{_prefix}{subId}:{authorid}");
            Subscription? subscription = null;
            if (cachedString != null)
            {
                subscription = DeserializeObject<Subscription>(cachedString);
                if (subscription != null)
                {
                    _context.Attach(subscription);
                    return subscription;
                }
            }

            subscription = await _context.Subscriptions
                .FirstOrDefaultAsync(e => e.SubId == subId && e.AuthorId == authorid);
            if (subscription != null)
                await _distributedCache.SetStringAsync($"{_prefix}{subId}:{authorid}", SerializeObject(subscription), _options);

            return subscription;
        }

        public async Task<Subscription?> AddSubscriptionAsync(UserModel sub, UserModel author)
        {
            var subscription = await GetSubscriptionAsync(sub.Id, author.Id);
            if (subscription == null)
                return null;

            subscription = new Subscription
            {
                Author = author,
                Subscriber = sub
            };

            subscription = (await _context.Subscriptions.AddAsync(subscription))?.Entity;
            await _context.SaveChangesAsync();
            await _distributedCache.SetStringAsync($"{_prefix}{sub.Id}:{author.Id}", SerializeObject(subscription), _options);
            return subscription;
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsWithAuthorAsync(Guid subId, int count, int offset)
        {
            return await _context.Subscriptions
                .Where(e => e.SubId == subId)
                .Skip(offset)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetCountSubscriptionsByAuthor(Guid id)
        {
            return await _context.Subscriptions
                .Where(e => e.AuthorId == id)
                .CountAsync();
        }
        public async Task<int> GetSubscriptionsCount(Guid subId)
        {
            return await _context.Subscriptions
                .Where(e => e.SubId == subId)
                .CountAsync();
        }

        public async Task<bool> RemoveSubscriptionAsync(Guid subId, Guid authorId)
        {
            var subscription = await GetSubscriptionAsync(subId, authorId);
            if (subscription == null)
                return true;

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
            await _distributedCache.RemoveAsync($"{_prefix}{subId}:{authorId}");

            return true;
        }

        private static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private static T? DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}