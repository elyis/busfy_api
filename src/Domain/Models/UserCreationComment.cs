using busfy_api.src.Domain.Entities.Response;

namespace busfy_api.src.Domain.Models
{
    public class UserCreationComment
    {
        public Guid Id { get; set; }
        public UserCreation Creation { get; set; }
        public Guid CreationId { get; set; }

        public UserModel User { get; set; }
        public Guid UserId { get; set; }

        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public CommentBody ToCommentBody()
        {
            return new CommentBody
            {
                Comment = Comment,
                ProfileBody = User.ToProfileBody(),
            };
        }
    }
}