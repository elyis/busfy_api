using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Infrastructure.Repository
{
    public class ContentCategoryRepository : IContentCategoryRepository
    {
        private readonly AppDbContext _context;

        public ContentCategoryRepository(AppDbContext context)
        {
            _context = context;
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

            return category;
        }

        public async Task<bool> DeleteCategoryAsync(string name)
        {
            var category = await Get(name);
            if (category == null)
                return true;

            _context.ContentCategories.Remove(category);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ContentCategory?> Get(string name)
        {
            var nameInLower = name.ToLower();
            return await _context.ContentCategories.FirstOrDefaultAsync(e => e.Name.ToLower() == nameInLower);
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

            return category;
        }
    }
}