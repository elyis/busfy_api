using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.Enums;
using Newtonsoft.Json;

namespace busfy_api.src.Domain.Models
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public float Price { get; set; }
        public string Type { get; set; }
        public int CountDays { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public UserModel Creator { get; set; }
        public Guid CreatorId { get; set; }

        [JsonIgnore]
        public List<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();

        public SubscriptionBody ToSubscriptionBody()
        {
            return new SubscriptionBody
            {
                Id = Id,
                Price = Price,
                Type = Enum.Parse<ContentSubscriptionType>(Type),
                CountDays = CountDays,
                CreatedAt = CreatedAt,
            };
        }
    }
}