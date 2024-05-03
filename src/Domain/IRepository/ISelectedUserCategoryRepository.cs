using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface ISelectedUserCategoryRepository
    {
        Task<SelectedUserCategory?> AddAsync(UserModel user, ContentCategory category);
        Task<SelectedUserCategory?> GetByIdAsync(Guid userId, string categoryName);
        Task<IEnumerable<SelectedUserCategory>> GetAllByUserIdAsync(Guid userId);
        Task<bool> RemoveAsync(Guid userId, string categoryName);
    }
}