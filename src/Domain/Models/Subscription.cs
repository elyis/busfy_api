using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Domain.Models
{
    [PrimaryKey(nameof(SubId), nameof(AuthorId))]
    public class Subscription
    {
        public Guid SubId { get; set; }
        public UserModel Subscriber { get; set; }

        public Guid AuthorId { get; set; }
        public UserModel Author { get; set; }
    }
}