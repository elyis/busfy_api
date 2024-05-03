using busfy_api.src.Domain.Entities.Response;

namespace busfy_api.src.Domain.Models
{
    public class PostComment
    {
        public Guid Id { get; set; }
        public Post Post { get; set; }
        public Guid PostId { get; set; }

        public UserModel User { get; set; }
        public Guid UserId { get; set; }

        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CommentBody ToCommentBody()
        {
            return new CommentBody
            {
                Comment = Comment,
                ProfileBody = User.ToProfileBody()
            };
        }
    }
}