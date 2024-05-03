using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface IContentCategoryRepository
    {
        Task<ContentCategory?> AddAsync(CreateContentCategoryBody body);
        Task<IEnumerable<ContentCategory>> GetAllAsync(int count, int offset);
        Task<IEnumerable<ContentCategory>> GetAll(IEnumerable<string> names);
        Task<ContentCategory?> Get(string name);
        Task<ContentCategory?> UpdateImageAsync(string name, string newImage);
        Task<bool> DeleteCategoryAsync(string name);
        Task<int> GetCountAsync();
    }
}

