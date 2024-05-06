using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace busfy_api.src.Infrastructure.Repository
{
    public class SelectedUserCategoryRepository : ISelectedUserCategoryRepository
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _distributedCache;

        private readonly string _prefix = "selectedCategory:";
        private readonly DistributedCacheEntryOptions _options = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(3),
            AbsoluteExpiration = DateTime.UtcNow.AddMinutes(6)
        };

        public SelectedUserCategoryRepository(
            AppDbContext context,
            IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        public async Task<SelectedUserCategory?> AddAsync(UserModel user, ContentCategory category)
        {
            var selectedCategory = await GetByIdAsync(user.Id, category.Name);
            if (selectedCategory != null)
                return null;

            selectedCategory = new SelectedUserCategory
            {
                User = user,
                Category = category,
            };

            await _context.SelectedUserCategories.AddAsync(selectedCategory);
            await _context.SaveChangesAsync();
            await _distributedCache.SetStringAsync($"{_prefix}{user.Id}:{category.Name}", SerializeObject(selectedCategory), _options);

            return selectedCategory;
        }

        public async Task<SelectedUserCategory?> GetByIdAsync(Guid userId, string categoryName)
        {
            var cachedString = await _distributedCache.GetStringAsync($"{_prefix}{userId}:{categoryName}");
            if (!string.IsNullOrEmpty(cachedString))
            {
                var cachedCategory = DeserializeObject<SelectedUserCategory>(cachedString);
                if (cachedCategory != null)
                {
                    _context.Attach(cachedCategory);
                    return cachedCategory;
                }
            }

            var selectedCategory = await _context.SelectedUserCategories
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CategoryName == categoryName);
            if (selectedCategory != null)
            {
                await _distributedCache.SetStringAsync($"{_prefix}{selectedCategory.UserId}:{selectedCategory.CategoryName}", SerializeObject(selectedCategory), _options);
                _context.Attach(selectedCategory);
            }

            return selectedCategory;
        }

        public async Task<IEnumerable<SelectedUserCategory>> GetAllByUserIdAsync(Guid userId)
        {
            return await _context.SelectedUserCategories
                .Where(suc => suc.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> RemoveAsync(Guid userId, string categoryName)
        {
            var selectedUserCategory = await GetByIdAsync(userId, categoryName);
            if (selectedUserCategory == null)
                return true;

            _context.SelectedUserCategories.Remove(selectedUserCategory);
            await _context.SaveChangesAsync();
            await _distributedCache.RemoveAsync($"{_prefix}{userId}:{selectedUserCategory.CategoryName}");

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