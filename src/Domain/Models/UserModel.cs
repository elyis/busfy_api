using System.ComponentModel.DataAnnotations;
using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
        public DateTime? LockDuration { get; set; }
        public string? LockReason { get; set; }

        public DateTime? RestoreCodeValidBefore { get; set; }
        public bool WasPasswordResetRequest { get; set; }
        public string? Image { get; set; }
        public string? BackgroundImage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<UserSession> Sessions { get; set; } = new List<UserSession>();
        public List<UserCreation> Creations { get; set; } = new List<UserCreation>();
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Subscription> Subscriptions { get; set; } = new();
        public List<UserCreationLike> Likes { get; set; } = new List<UserCreationLike>();
        public List<UserCreationComment> Comments { get; set; } = new List<UserCreationComment>();
        public List<PostLike> PostLikes { get; set; } = new List<PostLike>();
        public List<PostComment> PostComments { get; set; } = new List<PostComment>();

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
                AccountStatus = Enum.Parse<UserAccountStatus>(AccountStatus),
                Nickname = Nickname,
                UrlIcon = string.IsNullOrEmpty(Image) ? null : $"{Constants.webPathToProfileIcons}{Image}",
                UserTag = UserTag
            };
        }
    }
}