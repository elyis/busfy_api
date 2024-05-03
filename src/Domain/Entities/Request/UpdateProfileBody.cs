using System.ComponentModel.DataAnnotations;

namespace busfy_api.src.Domain.Entities.Request
{
    public class UpdateProfileBody
    {
        [EmailAddress, Required]
        public string Email { get; set; }

        [Required]
        public string Nickname { get; set; }
        public string? UserTag { get; set; }
        public string? Bio { get; set; }
    }
}