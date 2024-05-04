namespace busfy_api.src.Domain.Entities.Response
{
    public class ProfileWithFollowersAndLikesBody
    {
        public ProfileBody Profile { get; set; }
        public int SubscriberCount { get; set; }
        public int CountLikes { get; set; }
    }
}