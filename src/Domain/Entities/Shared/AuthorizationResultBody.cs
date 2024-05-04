using busfy_api.src.Domain.Entities.Response;

namespace busfy_api.src.Domain.Entities.Shared
{
    public class AuthorizationResultBody
    {
        public TokenPair TokenPair { get; set; }
        public ProfileBody Profile { get; set; }
    }
}