namespace busfy_api.src.Domain.Entities.Response
{
    public class PostBody
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? Text { get; set; }
        public string Type { get; set; }
        public string? UrlImage { get; set; }
        public string CategoryName { get; set; }
        public bool IsCommentingAllowed { get; set; }
        public string Date { get; set; }
        public ProfileBody ProfileCreator { get; set; }
        public bool HasEvaluated { get; set; }
    }
}