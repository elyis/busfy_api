using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.Enums;
using Newtonsoft.Json;

namespace busfy_api.src.Domain.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? Filename { get; set; }
        public string? Type { get; set; }
        public string? Text { get; set; }
        public bool IsFormed { get; set; }
        public bool IsCommentingAllowed { get; set; }
        public string ContentSubscriptionType { get; set; }

        public ContentCategory Category { get; set; }
        public string CategoryName { get; set; }
        public UserModel Creator { get; set; }
        public Guid CreatorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public List<PostLike> Likes { get; set; } = new List<PostLike>();
        [JsonIgnore]
        public List<PostComment> Comments { get; set; } = new List<PostComment>();

        public PostBody ToPostBody()
        {
            if (!IsFormed)
                throw new NullReferenceException();

            return new PostBody
            {
                Id = Id,
                Description = Description,
                Text = Text,
                Type = Enum.Parse<UserCreationType>(Type),
                CategoryName = CategoryName,
                Date = CreatedAt.ToString("s"),
                ProfileCreator = Creator.ToProfileBody(),
                UrlFile = Type == UserCreationType.Text.ToString() ? null : $"{Constants.webPathToPostFiles}{Filename}",
                SubscriptionType = Enum.Parse<ContentSubscriptionType>(ContentSubscriptionType),
                IsCommentingAllowed = IsCommentingAllowed
            };
        }

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
                CategoryName = CategoryName,
                ProfileCreator = Creator.ToProfileBody(),
                Text = Text,
            };
        }
    }
}