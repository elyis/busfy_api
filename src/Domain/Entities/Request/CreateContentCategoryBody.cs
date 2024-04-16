using System.ComponentModel.DataAnnotations;

namespace busfy_api.src.Domain.Entities.Request
{
    public class CreateContentCategoryBody
    {
        [Required]
        public string Name { get; set; }
    }
}