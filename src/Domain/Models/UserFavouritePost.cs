using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Domain.Models
{
    [PrimaryKey(nameof(UserId), nameof(PostId))]
    public class UserFavouritePost
    {
        public Guid PostId { get; set; }
        public Post Post { get; set; }

        public UserModel User { get; set; }
        public Guid UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}