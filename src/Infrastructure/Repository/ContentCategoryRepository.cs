using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace busfy_api.src.Infrastructure.Repository
{
    public class ContentCategoryRepository : IContentCategoryRepository
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _distributedCache;
        private readonly string _prefix = "contentCategory:";
        private readonly DistributedCacheEntryOptions _options = new()
        {
            SlidingExpiration = TimeSpan.FromMinutes(3),
            AbsoluteExpiration = DateTime.UtcNow.AddMinutes(6)
        };


        public ContentCategoryRepository(
            AppDbContext context,
            IDistributedCache distributedCache)
        {
            _context = context;
            _distributedCache = distributedCache;
        }

        public async Task<ContentCategory?> AddAsync(CreateContentCategoryBody body)
        {
            var category = await Get(body.Name);
            if (category != null)
                return null;

            category = new ContentCategory
            {
                Name = body.Name
            };

            await _context.ContentCategories.AddAsync(category);
            await _context.SaveChangesAsync();
            await _distributedCache.SetStringAsync($"{_prefix}{category.Name}", SerializeObject(category), _options);

            return category;
        }

        public async Task<bool> DeleteCategoryAsync(string name)
        {
            var category = await Get(name);
            if (category == null)
                return true;

            _context.ContentCategories.Remove(category);
            await _context.SaveChangesAsync();
            await _distributedCache.RemoveAsync($"{_prefix}{category.Name}");

            return true;
        }

        public async Task<ContentCategory?> Get(string name)
        {
            var cachedString = await _distributedCache.GetStringAsync($"{_prefix}{name}");
            ContentCategory? contentCategory = null;
            if (!string.IsNullOrEmpty(cachedString))
            {
                contentCategory = DeserializeObject<ContentCategory>(cachedString);
                if (contentCategory != null)
                {
                    _context.Attach(contentCategory);
                    return contentCategory;
                }
            }

            var nameInLower = name.ToLower();
            contentCategory = await _context.ContentCategories.FirstOrDefaultAsync(e => e.Name.ToLower() == nameInLower);
            if (contentCategory != null)
            {
                var resultString = SerializeObject(contentCategory);
                await _distributedCache.SetStringAsync($"{_prefix}{contentCategory.Name}", resultString, _options);
                _context.Attach(contentCategory);
            }

            return contentCategory;
        }

        public async Task<IEnumerable<ContentCategory>> GetAll(IEnumerable<string> names)
        {
            var namesInLower = names.Select(e => e.ToLower());
            return await _context.ContentCategories
                .Where(e => namesInLower.Contains(e.Name.ToLower()))
                .ToListAsync();
        }

        public async Task<IEnumerable<ContentCategory>> GetAllAsync(int count, int offset)
        {
            return await _context.ContentCategories
                .OrderBy(e => e.Name)
                .Take(count)
                .Skip(offset)
                .ToListAsync();
        }

        public async Task<int> GetCountAsync()
        {
            return await _context.ContentCategories.CountAsync();
        }

        public async Task<ContentCategory?> UpdateImageAsync(string name, string newImage)
        {
            var category = await Get(name);
            if (category == null)
                return null;

            category.Image = newImage;
            await _context.SaveChangesAsync();
            await _distributedCache.SetStringAsync($"{_prefix}{name}", SerializeObject(category), _options);

            return category;
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