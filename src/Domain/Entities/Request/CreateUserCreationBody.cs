using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Request
{
    public class CreateUserCreationBody
    {
        public string? Description { get; set; }
        public ContentSubscriptionType ContentSubscriptionType { get; set; }
    }
}