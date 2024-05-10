using System.ComponentModel.DataAnnotations;
using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace busfy_api.src.Domain.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class UserModel
    {
        public Guid Id { get; set; }

        [StringLength(256, MinimumLength = 3)]
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? RestoreCode { get; set; }
        public string RoleName { get; set; }
        public string Nickname { get; set; }
        public string? UserTag { get; set; }
        public string? Bio { get; set; }

        public string AccountStatus { get; set; }

        public DateTime? RestoreCodeValidBefore { get; set; }
        public bool WasPasswordResetRequest { get; set; }
        public string? Image { get; set; }
        public string? BackgroundImage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public List<UserSession> Sessions { get; set; } = new List<UserSession>();

        [JsonIgnore]
        public List<Post> Posts { get; set; } = new List<Post>();

        [JsonIgnore]
        public List<PostLike> PostLikes { get; set; } = new List<PostLike>();

        [JsonIgnore]
        public List<PostComment> PostComments { get; set; } = new List<PostComment>();
        public List<FavoritePost> Favorites { get; set; } = new();

        public ProfileBody ToProfileBody()
        {
            return new ProfileBody
            {
                Id = Id,
                Email = Email,
                Role = Enum.Parse<UserRole>(RoleName),
                UrlIcon = string.IsNullOrEmpty(Image) ? null : $"{Constants.webPathToProfileIcons}{Image}",
                UrlBackgroundImage = string.IsNullOrEmpty(BackgroundImage) ? null : $"{Constants.webPathToProfileBackground}{BackgroundImage}",
                Nickname = Nickname,
                UserTag = UserTag,
                Bio = Bio,
                AccountStatus = Enum.Parse<UserAccountStatus>(AccountStatus)
            };
        }

        public GeneralAccountInformationBody ToGeneralAccountInformationBody()
        {
            return new GeneralAccountInformationBody
            {
                Id = Id,
                AccountStatus = Enum.Parse<UserAccountStatus>(AccountStatus),
                Nickname = Nickname,
                UrlIcon = string.IsNullOrEmpty(Image) ? null : $"{Constants.webPathToProfileIcons}{Image}",
                UserTag = UserTag
            };
        }
    }
}