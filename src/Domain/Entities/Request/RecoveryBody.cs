using System.ComponentModel.DataAnnotations;

namespace busfy_api.src.Domain.Entities.Request
{
    public class RecoveryBody
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}