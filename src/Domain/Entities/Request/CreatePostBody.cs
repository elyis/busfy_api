using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Request
{
    public class CreatePostBody
    {
        public string? Description { get; set; }
        public string? Text { get; set; }
        public ContentSubscriptionType SubscriptionType { get; set; }
        public bool IsCommentingAllowed { get; set; }
    }
}

