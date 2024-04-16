using System.ComponentModel.DataAnnotations;

namespace busfy_api.src.Domain.Entities.Request
{
    public class ResetPasswordBody
    {
        [EmailAddress]
        public string Email { get; set; }

        public string RecoveryCode { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}