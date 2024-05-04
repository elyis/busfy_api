using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Response
{
    public class UserCreationBody
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? Text { get; set; }
        public string? UrlFile { get; set; }
        public UserCreationType? Type { get; set; }
        public ContentSubscriptionType ContentSubscriptionType { get; set; }
        public string CategoryName { get; set; }
        public string Date { get; set; }
        public ProfileBody ProfileCreator { get; set; }
        public bool HasEvaluated { get; set; }
    }
}