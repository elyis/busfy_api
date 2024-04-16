namespace busfy_api.src.Domain.Entities.Request
{
    public class UpdateContentDescriptionBody
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
    }
}