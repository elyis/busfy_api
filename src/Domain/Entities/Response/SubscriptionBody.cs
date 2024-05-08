using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Response
{
    public class SubscriptionBody
    {
        public Guid Id { get; set; }
        public float Price { get; set; }
        public ContentSubscriptionType Type { get; set; }
        public int CountDays { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}