namespace busfy_api.src.Domain.Models
{
    public class FavoritePost
    {
        public Guid PostId { get; set; }
        public Post Post { get; set; }
        public Guid UserId { get; set; }
        public UserModel User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}