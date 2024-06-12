using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Response
{
    public class UserSubscriptionBody
    {
        public Guid SubscriptionId { get; set; }
        public ContentSubscriptionType Type { get; set; }
        public DateTime EndDate { get; set; }
    }
}