using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Domain.Models
{
    [PrimaryKey(nameof(UserId), nameof(CategoryName))]
    public class SelectedUserCategory
    {
        public Guid UserId { get; set; }
        public UserModel User { get; set; }

        public string CategoryName { get; set; }
        public ContentCategory Category { get; set; }
    }
}