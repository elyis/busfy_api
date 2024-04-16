using System.ComponentModel.DataAnnotations;

namespace busfy_api.src.Domain.Entities.Request
{
    public class SignUpBody
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Nickname { get; set; }

        [Required]
        public string Password { get; set; }
    }
}