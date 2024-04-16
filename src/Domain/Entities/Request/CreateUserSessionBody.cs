namespace busfy_api.src.Domain.Entities.Request
{
    public class CreateUserSessionBody
    {
        public string UserAgent { get; set; }
        public string Host { get; set; }
    }
}