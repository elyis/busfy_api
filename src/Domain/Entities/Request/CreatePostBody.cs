namespace busfy_api.src.Domain.Entities.Request
{
    public class CreatePostBody
    {
        public string? Description { get; set; }
        public string? Text { get; set; }
        public bool IsCommentingAllowed { get; set; }
    }
}

