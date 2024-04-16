using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Response
{
    public class UserSubscriptionBody
    {
        public SubscriptionType Type { get; set; }
        public DateTime EndDate { get; set; }
    }
}