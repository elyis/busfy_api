using System.ComponentModel.DataAnnotations;

namespace busfy_api.src.Domain.Entities.Request
{
    public class CreateCommentBody
    {
        [Required]
        public string? Comment { get; set; }
    }
}