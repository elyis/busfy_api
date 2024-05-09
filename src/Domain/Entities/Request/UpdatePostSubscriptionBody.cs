namespace busfy_api.src.Domain.Entities.Request
{
    public class UpdatePostSubscriptionBody
    {
        public Guid PostId { get; set; }
        public Guid SubscriptionId { get; set; }
    }
}