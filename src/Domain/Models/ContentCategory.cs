using busfy_api.src.Domain.Entities.Response;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace busfy_api.src.Domain.Models
{
    [PrimaryKey(nameof(Name))]
    public class ContentCategory
    {
        public string Name { get; set; }
        public string? Image { get; set; }

        [JsonIgnore]
        public List<Post> Posts { get; set; } = new List<Post>();


        public ContentCategoryBody ToContentCategoryBody()
        {
            return new ContentCategoryBody
            {
                Name = Name,
                UrlImage = Image == null ? null : $"{Constants.webPathToContentCategories}{Image}",
            };
        }
    }
}