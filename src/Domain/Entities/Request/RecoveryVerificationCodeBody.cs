using System.ComponentModel.DataAnnotations;

namespace busfy_api.src.Domain.Entities.Request
{
    public class RecoveryVerificationCodeBody
    {
        [EmailAddress]
        public string Email { get; set; }
        public string RecoveryCode { get; set; }
    }
}