using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace busfy_api.src.Shared.Filters
{
    public class UploadedFileContentTypesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var supportedTextFileExtensions = new[] { "txt", "doc", "docx", "pdf", "odt", "rtf" };
            var supportedImageFileExtensions = new[] { "jpg", "jpeg", "png", "gif", "svg" };
            var supportedAudioFileExtensions = new[] { "mp3", "wav", "aif" };
            var supportedVideoFileExtensions = new[] { "mp4", "avi", "mov" };
            var supported3dModelFormats = new[] { "stl", "obj", "fbx" };

            operation.Description = $"Supported file types: {string.Join(", ", supportedTextFileExtensions)} (text), " +
                              $"{string.Join(", ", supportedImageFileExtensions)} (image), " +
                              $"{string.Join(", ", supportedAudioFileExtensions)} (audio), " +
                              $"{string.Join(", ", supportedVideoFileExtensions)} (video), " +
                              $"{string.Join(", ", supported3dModelFormats)} (3D model), " +
                              $"... (other files)";
        }
    }
}