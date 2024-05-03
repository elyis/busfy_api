using busfy_api.src.Domain.Entities.Request;

namespace busfy_api.src.Domain.Models
{
    public class UnconfirmedAccount
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }
        public DateTime ValidityPeriodCode { get; set; }
        public string ConfirmationCode { get; set; }

        public SignUpBody ToSignUpBody()
        {
            return new SignUpBody
            {
                Email = Email,
                Nickname = Nickname,
                Password = Password
            };
        }
    }
}