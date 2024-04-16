using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Response
{
    public class UserCreationBody
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? UrlFile { get; set; }
        public UserCreationType? Type { get; set; }
        public ContentSubscriptionType ContentSubscriptionType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}