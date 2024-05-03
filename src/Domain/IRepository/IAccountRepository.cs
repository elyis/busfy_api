using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface IAccountRepository
    {
        Task<UnconfirmedAccount?> CreateOrUpdateCode(SignUpBody body, string confirmationCode);
        Task<UnconfirmedAccount?> Get(string email);
        Task<bool> Remove(string email);
    }
}