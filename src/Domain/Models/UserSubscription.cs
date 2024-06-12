using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Domain.Models
{
    [PrimaryKey(nameof(SubscriptionId), nameof(UserId))]
    public class UserSubscription
    {
        public Subscription Subscription { get; set; }
        public Guid SubscriptionId { get; set; }

        public UserModel User { get; set; }
        public Guid UserId { get; set; }
        public DateTime EndDate { get; set; }

        public UserSubscriptionBody ToUserSubscriptionBody()
        {
            return new UserSubscriptionBody
            {
                SubscriptionId = SubscriptionId,
                Type = Enum.Parse<ContentSubscriptionType>(Subscription.Type),
                EndDate = EndDate
            };
        }
    }
}