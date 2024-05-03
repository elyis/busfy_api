using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Infrastructure.Repository
{
    public class SelectedUserCategoryRepository : ISelectedUserCategoryRepository
    {
        private readonly AppDbContext _context;

        public SelectedUserCategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SelectedUserCategory?> AddAsync(UserModel user, ContentCategory category)
        {
            var selectedUserCategory = await GetByIdAsync(user.Id, category.Name);
            if (selectedUserCategory != null)
                return null;

            selectedUserCategory = new SelectedUserCategory
            {
                User = user,
                Category = category,
            };

            await _context.SelectedUserCategories.AddAsync(selectedUserCategory);
            await _context.SaveChangesAsync();
            return selectedUserCategory;
        }

        public async Task<SelectedUserCategory?> GetByIdAsync(Guid userId, string categoryName)
        {
            return await _context.SelectedUserCategories
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CategoryName == categoryName);
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

            return true;
        }
    }
}