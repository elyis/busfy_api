using System.ComponentModel.DataAnnotations;
using System.Net;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Shared.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimeDetective;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace busfy_api.src.Web.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IUserCreationRepository _userCreationRepository;
        private readonly ISubscriptionToAdditionalContentRepository _subscriptionRepository;
        private readonly IPostRepository _postRepository;
        private readonly IJwtService _jwtService;
        private readonly IFileUploaderService _fileUploaderService;
        private readonly ContentInspector _contentInspector;


        private readonly IEnumerable<string> _supportedTextFileExtensions = new string[] { "txt", "doc", "docx", "pdf", "odt", "rtf" };
        private readonly IEnumerable<string> _supportedImageFileExtensions = new string[] { "jpg", "jpeg", "png", "gif", "svg" };
        private readonly IEnumerable<string> _supportedAudioFileExtensions = new string[] { "mp3", "wav", "aif" };
        private readonly IEnumerable<string> _supportedVideoFileExtensions = new string[] { "mp4", "avi", "mov" };
        private readonly IEnumerable<string> _supported3dModelFormats = new string[] { "stl", "obj", "fbx" };



        public UploadController(
            IUserRepository userRepository,
            IContentCategoryRepository contentCategoryRepository,
            IPostRepository postRepository,
            IUserCreationRepository userCreationRepository,
            ISubscriptionToAdditionalContentRepository subscriptionRepository,
            IJwtService jwtService,
            IFileUploaderService fileUploaderService,
            ContentInspector contentInspector
        )
        {
            _userRepository = userRepository;
            _userCreationRepository = userCreationRepository;
            _subscriptionRepository = subscriptionRepository;
            _postRepository = postRepository;
            _jwtService = jwtService;
            _contentCategoryRepository = contentCategoryRepository;
            _fileUploaderService = fileUploaderService;
            _contentInspector = contentInspector;
        }

        [HttpPost("profile"), Authorize]
        [SwaggerOperation("Загрузить иконку профиля")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(string))]

        public async Task<IActionResult> UploadProfileIcon(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader] IFormFile file
        )
        {
            var resultUpload = await UploadIconAsync(Constants.localPathToProfileIcons, file);

            if (resultUpload is OkObjectResult result)
            {
                var filename = (string)result.Value;
                var tokenInfo = _jwtService.GetTokenPayload(token);
                await _userRepository.UpdateProfileIconAsync(tokenInfo.UserId, filename);
            }
            return resultUpload;
        }

        [HttpPost("profile-background"), Authorize]
        [SwaggerOperation("Загрузить задний фон")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(string))]

        public async Task<IActionResult> UploadProfileBackground(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromHeader] IFormFile file
        )
        {
            var resultUpload = await UploadIconAsync(Constants.localPathToBackground, file);

            if (resultUpload is OkObjectResult result)
            {
                var filename = (string)result.Value;
                var tokenInfo = _jwtService.GetTokenPayload(token);
                await _userRepository.UpdateBackgroundImage(tokenInfo.UserId, filename);
            }
            return resultUpload;
        }

        [HttpGet("profile-background/{filename}")]
        [SwaggerOperation("Получить задний фон")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(File))]
        [SwaggerResponse(404, Description = "Неверное имя файла")]

        public async Task<IActionResult> GetBackgroundImage(string filename)
            => await GetIconAsync(Constants.localPathToBackground, filename);

        [HttpPost("upload/content-category"), Authorize(Roles = nameof(UserRole.Admin))]
        [SwaggerOperation("Загрузить изображение категории контента")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> UploadContentCategoryImage(
            [FromHeader] IFormFile file,
            [FromQuery] string categoryName
        )
        {
            var contentCategory = await _contentCategoryRepository.Get(categoryName);
            if (contentCategory == null)
                return BadRequest();

            var resultUpload = await UploadIconAsync(Constants.localPathToContentCategoryFiles, file);

            if (resultUpload is OkObjectResult result)
            {
                var filename = (string)result.Value;
                await _contentCategoryRepository.UpdateImageAsync(categoryName, filename);
            }
            return resultUpload;

        }

        [HttpGet("upload/content/private/{filename}"), Authorize]
        [SwaggerResponse(200)]
        [SwaggerResponse(400, Description = "Получаемый контент публичный")]
        [SwaggerResponse(403, Description = "Нет подписки")]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetPrivateFileContent(
            [Required] string filename,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid contentId
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var userCreation = await _userCreationRepository.GetAsync(contentId);
            if (userCreation == null)
                return NotFound();

            var contentSubscriptionType = Enum.Parse<ContentSubscriptionType>(userCreation.ContentSubscriptionType);
            if (contentSubscriptionType == ContentSubscriptionType.Public)
                return BadRequest();

            var userSubscriptions = await _subscriptionRepository.GetSubscriptionsByUserAndSubscription(tokenInfo.UserId, int.MaxValue, 0);
            var subscriptionsToUser = userSubscriptions
                        .Where(e =>
                            e.Subscription.Type == userCreation.ContentSubscriptionType &&
                            e.Subscription.CreatorId == userCreation.UserId);

            if (!subscriptionsToUser.Any())
                return Forbid();

            return await GetFileAsync(Constants.localPathToPublicContentFiles, filename);
        }

        [HttpGet("profile/{filename}")]
        [SwaggerOperation("Получить иконку профиля")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(File))]
        [SwaggerResponse(404, Description = "Неверное имя файла")]

        public async Task<IActionResult> GetProfileIcon(string filename)
            => await GetIconAsync(Constants.localPathToProfileIcons, filename);


        [HttpGet("content-category/{filename}")]
        [SwaggerOperation("Получить изображение категории контента")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(File))]
        [SwaggerResponse(404, Description = "Неверное имя файла")]

        public async Task<IActionResult> GetCategoryImage(string filename)
            => await GetIconAsync(Constants.localPathToContentCategoryFiles, filename);

        private async Task<IActionResult> GetIconAsync(string path, string filename)
        {
            var bytes = await _fileUploaderService.GetStreamFileAsync(path, filename);
            if (bytes == null)
                return NotFound();

            var fileExtension = Path.GetExtension(filename);
            return File(bytes, $"image/{fileExtension}", filename);
        }

        private async Task<IActionResult> UploadIconAsync(string path, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            string? fileExtension = null;
            var stream = file.OpenReadStream();
            var result = _contentInspector.Inspect(stream);
            stream.Seek(0, SeekOrigin.Begin);

            var inspectResult = result.MaxBy(e => e.Points);
            fileExtension = inspectResult?.Definition.File.Extensions.First();
            if (fileExtension == null || !_supportedImageFileExtensions.Contains(fileExtension))
                return new UnsupportedMediaTypeResult();

            string? filename = await _fileUploaderService.UploadFileAsync(path, stream, $"{fileExtension}");
            return filename == null ? BadRequest("Failed to upload the file") : Ok(filename);
        }

        [HttpGet("upload/content/pub/{filename}")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetPublicFileContent([Required] string filename)
        {
            return await GetFileAsync(Constants.localPathToPublicContentFiles, filename);
        }

        [HttpPost("upload/content"), Authorize]
        [SwaggerOperationFilter(typeof(UploadedFileContentTypesOperationFilter))]
        [SwaggerResponse(200, Type = typeof(Guid))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> UploadContent(
            [FromForm, Required] IFormFile file,
            [FromHeader, Required] Guid postId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var tokenInfo = _jwtService.GetTokenPayload(token);

            var userCreation = await _userCreationRepository.GetAsync(postId);
            if (userCreation?.UserId != tokenInfo.UserId)
                return Forbid();

            string? fileExtension = null;
            var fileStream = file.OpenReadStream();
            var result = _contentInspector.Inspect(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            try
            {
                if (result.Length == 0)
                    fileExtension = file.FileName.Split(".").Last();
                else
                {
                    var inspectResult = result.MaxBy(e => e.Points);
                    fileExtension = inspectResult.Definition.File.Extensions.First();
                }
            }
            catch (Exception e)
            {
                fileExtension = "txt";
            }

            if (fileExtension == null)
                return BadRequest();

            var contentSubscriptionType = Enum.Parse<ContentSubscriptionType>(userCreation.ContentSubscriptionType);
            string pathToContent = "";
            switch (contentSubscriptionType)
            {
                case ContentSubscriptionType.Public:
                    pathToContent = Constants.localPathToPublicContentFiles;
                    break;

                case ContentSubscriptionType.Private:
                    pathToContent = Constants.localPathToPrivateContentFiles;
                    break;

                case ContentSubscriptionType.Single:
                    pathToContent = Constants.localPathToPrivateContentFiles;
                    break;
            }

            var filename = await _fileUploaderService.UploadFileAsync(pathToContent, fileStream, fileExtension);
            if (filename == null)
                return BadRequest("Failed to upload the file");

            UserCreationType type = GetUserCreationType(fileExtension);
            UpdateContentBody userCreationBody = new()
            {
                Id = userCreation.Id,
                Filename = filename,
                Type = type
            };

            userCreation = await _userCreationRepository.UpdateAsync(userCreationBody, tokenInfo.UserId);

            return Ok(userCreation.Id);
        }

        [HttpGet("upload/post/{filename}")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetPost([Required] string filename)
        {
            return await GetFileAsync(Constants.localPathToPostFiles, filename);
        }

        [HttpPost("upload/post"), Authorize]
        [SwaggerOperationFilter(typeof(UploadedFileContentTypesOperationFilter))]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> UploadPost(
            [FromForm, Required] IFormFile file,
            [FromHeader, Required] Guid postId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var post = await _postRepository.GetAsync(postId);
            if (post?.CreatorId != tokenInfo.UserId)
                return Forbid();

            if (post.IsFormed && post.Type == UserCreationType.Text.ToString())
                return BadRequest();

            string? fileExtension = null;
            var fileStream = file.OpenReadStream();
            var result = _contentInspector.Inspect(fileStream);
            fileStream.Seek(0, SeekOrigin.Begin);

            try
            {
                if (result.Length == 0)
                    fileExtension = file.FileName.Split(".").Last();
                else
                {
                    var inspectResult = result.MaxBy(e => e.Points);
                    fileExtension = inspectResult.Definition.File.Extensions.First();
                }
            }
            catch (Exception e)
            {
                fileExtension = "txt";
            }

            if (fileExtension == null)
                return BadRequest();

            var filename = await _fileUploaderService.UploadFileAsync(Constants.localPathToPostFiles, fileStream, fileExtension);
            if (filename == null)
                return BadRequest("Failed to upload the file");

            UserCreationType type = GetUserCreationType(fileExtension);
            post = await _postRepository.UpdateImageAsync(postId, filename, type);
            return Ok();
        }

        private async Task<IActionResult> GetFileAsync(string path, string filename)
        {
            var bytes = await _fileUploaderService.GetStreamFileAsync(path, filename);
            if (bytes == null)
                return NotFound();

            return File(bytes, "application/octet-stream", filename);
        }

        private UserCreationType GetUserCreationType(string fileExtension)
        {
            return fileExtension switch
            {
                _ when _supportedTextFileExtensions.Contains(fileExtension) => UserCreationType.Text,
                _ when _supportedImageFileExtensions.Contains(fileExtension) => UserCreationType.Image,
                _ when _supportedAudioFileExtensions.Contains(fileExtension) => UserCreationType.Audio,
                _ when _supportedVideoFileExtensions.Contains(fileExtension) => UserCreationType.Video,
                _ when _supported3dModelFormats.Contains(fileExtension) => UserCreationType.Model3D,
                _ => UserCreationType.Other
            };
        }
    }
}