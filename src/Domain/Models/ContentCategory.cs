using busfy_api.src.Domain.Entities.Response;
using Microsoft.EntityFrameworkCore;

namespace busfy_api.src.Domain.Models
{
    [PrimaryKey(nameof(Name))]
    public class ContentCategory
    {
        public string Name { get; set; }

        public List<Post> Posts { get; set; } = new List<Post>();


        public ContentCategoryBody ToContentCategoryBody()
        {
            return new ContentCategoryBody
            {
                Name = Name
            };
        }
    }
}