namespace busfy_api.src.Domain.Entities.Response
{
    public class ProfileWithFollowersBody
    {
        public ProfileBody Profile { get; set; }
        public int SubscriberCount { get; set; }
    }
}