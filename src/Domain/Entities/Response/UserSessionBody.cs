namespace busfy_api.src.Domain.Entities.Response
{
    public class UserSessionBody
    {
        public Guid Id { get; set; }
        public string Host { get; set; }
        public string UserAgent { get; set; }
        public string Location { get; set; }

        public bool IsVerified { get; set; }
        public bool IsCurrent { get; set; }
    }
}