using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface IContentCategoryRepository
    {
        Task<ContentCategory?> AddAsync(CreateContentCategoryBody body);
        Task<IEnumerable<ContentCategory>> GetAllAsync(int count, int offset);
        Task<ContentCategory?> Get(string name);
    }
}