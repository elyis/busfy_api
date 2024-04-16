using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Domain.Models
{
    [PrimaryKey(nameof(EvaluatorId), nameof(PostId))]
    public class PostLike
    {
        public UserModel Evaluator { get; set; }
        public Guid EvaluatorId { get; set; }

        public Post Post { get; set; }
        public Guid PostId { get; set; }
    }
}