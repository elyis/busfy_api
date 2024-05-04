using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Response
{
    public class ProfileBody
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public string? UrlIcon { get; set; }
        public string? UrlBackgroundImage { get; set; }
        public string Nickname { get; set; }
        public string? Bio { get; set; }
        public string? UserTag { get; set; }
        public UserAccountStatus AccountStatus { get; set; }
    }
}