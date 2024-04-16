using IPinfo.Models;

namespace busfy_api.src.App.IService
{
    public interface ILocationService
    {
        Task<IPResponse?> GetIPResponse(string ip);
    }
}