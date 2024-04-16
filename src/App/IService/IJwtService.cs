using busfy_api.src.Domain.Entities.Shared;
using webApiTemplate.src.Domain.Entities.Shared;

namespace webApiTemplate.src.App.IService
{
    public interface IJwtService
    {
        TokenPair GenerateDefaultTokenPair(TokenInfo tokenInfo);
        TokenInfo GetTokenInfo(string token);
    }
}