using busfy_api.src.Domain.Entities.Response;

namespace busfy_api.src.Domain.Models
{
    public class UserSession
    {
        public Guid Id { get; set; }
        public string Host { get; set; }
        public string UserAgent { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenValidBefore { get; set; }
        public string Location { get; set; }

        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public UserModel User { get; set; } = null!;
        public Guid UserId { get; set; }


        public UserSessionBody ToUserSessionBody()
        {
            return new UserSessionBody
            {
                Id = Id,
                Host = Host,
                UserAgent = UserAgent,
                Location = Location,
                IsVerified = IsVerified,
            };
        }
    }
}