namespace busfy_api.src.Domain.Entities.Response
{
    public class CommentBody
    {
        public ProfileBody ProfileBody { get; set; }
        public string? Comment { get; set; }
    }
}