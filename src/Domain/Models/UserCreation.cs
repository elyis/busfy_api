using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Models
{
    public class UserCreation
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? Filename { get; set; }
        public string? Type { get; set; }
        public string? Text { get; set; }
        public bool IsFormed { get; set; }

        public string ContentSubscriptionType { get; set; }
        public ContentCategory ContentCategory { get; set; }
        public string ContentCategoryName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid UserId { get; set; }
        public UserModel User { get; set; }

        public List<UserCreationLike> Likes { get; set; } = new List<UserCreationLike>();
        public List<UserCreationComment> Comments { get; set; } = new List<UserCreationComment>();


        public UserCreationBody ToUserCreationBody()
        {
            string? url = Filename;
            var contentSubscriptionType = (ContentSubscriptionType)Enum.Parse(typeof(ContentSubscriptionType), ContentSubscriptionType);
            if (url != null)
            {
                switch (contentSubscriptionType)
                {
                    case Enums.ContentSubscriptionType.Public:
                        url = $"{Constants.webPathToPublicContentFile}{Filename}";
                        break;
                    case Enums.ContentSubscriptionType.Private:
                    case Enums.ContentSubscriptionType.Single:
                        url = $"{Constants.webPathToPrivateContentFile}{Filename}";
                        break;
                }

            }

            return new UserCreationBody
            {
                Id = Id,
                Description = Description,
                UrlFile = url,
                Type = Type == null ? null : (UserCreationType)Enum.Parse(typeof(UserCreationType), Type),
                ContentSubscriptionType = contentSubscriptionType,
                Date = CreatedAt.ToString("s"),
                CategoryName = ContentCategoryName,
                ProfileCreator = User.ToProfileBody(),
                Text = Text,
            };
        }
    }
}