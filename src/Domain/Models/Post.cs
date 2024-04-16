using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Domain.Models
{
    [PrimaryKey(nameof(CreatorId), nameof(CreationId), nameof(CategoryName))]
    public class Post
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public UserModel Creator { get; set; }
        public Guid CreatorId { get; set; }
        public UserCreation Creation { get; set; }
        public Guid CreationId { get; set; }

        public ContentCategory Category { get; set; }
        public string CategoryName { get; set; }

        public List<PostLike> Likes { get; set; } = new List<PostLike>();
        public List<PostComment> Comments { get; set; } = new List<PostComment>();
        public List<UserFavouritePost> FavouritedBy { get; set; } = new List<UserFavouritePost>();
    }
}