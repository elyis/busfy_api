using busfy_api.src.Domain.Enums;

namespace busfy_api.src.Domain.Entities.Request
{
    public class UpdateContentBody
    {
        public Guid Id { get; set; }
        public UserCreationType Type { get; set; }
        public string Filename { get; set; }
    }
}