using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Domain.Models
{
    [PrimaryKey(nameof(EvaluatorId), nameof(CreationId))]
    public class UserCreationLike
    {
        public UserModel Evaluator { get; set; }
        public Guid EvaluatorId { get; set; }

        public UserCreation Creation { get; set; }
        public Guid CreationId { get; set; }
    }
}