using busfy_api.src.App.IService;
using busfy_api.src.Domain.Entities.Config;
using IPinfo;
using IPinfo.Models;

namespace busfy_api.src.App.Service
{
    public class LocationService : ILocationService
    {
        private readonly string _token;

        public LocationService(LocationServiceSettings settings)
        {
            _token = settings.Token;
        }

        public async Task<IPResponse?> GetIPResponse(string ip)
        {
            try
            {
                var client = new IPinfoClient.Builder().AccessToken(_token).Build();
                return await client.IPApi.GetDetailsAsync(ip);

            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }
    }
}