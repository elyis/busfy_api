using System.ComponentModel.DataAnnotations;

namespace busfy_api.src.Domain.Entities.Request
{
    public class ConfirmSignupBody
    {
        [EmailAddress]
        public string Email { get; set; }
        [Required, StringLength(6, MinimumLength = 6)]
        public string ConfirmationCode { get; set; }
    }
}