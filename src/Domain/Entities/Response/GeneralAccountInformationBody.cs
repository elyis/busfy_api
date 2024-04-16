using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Response
{
    public class GeneralAccountInformationBody
    {
        public string? UrlIcon { get; set; }
        public string Nickname { get; set; }
        public string? UserTag { get; set; }
        public UserAccountStatus AccountStatus { get; set; }
    }
}