using System.ComponentModel.DataAnnotations;
using System.Net;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Entities.Response;
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
    [Route("api")]
    public class ContentController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IFileUploaderService _fileUploaderService;
        private readonly IUserCreationRepository _userCreationRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ContentInspector _contentInspector;

        private readonly IEnumerable<string> _supportedTextFileExtensions = new string[] { "txt", "doc", "docx", "pdf", "odt", "rtf" };
        private readonly IEnumerable<string> _supportedImageFileExtensions = new string[] { "jpg", "jpeg", "png", "gif", "svg" };
        private readonly IEnumerable<string> _supportedAudioFileExtensions = new string[] { "mp3", "wav", "aif" };
        private readonly IEnumerable<string> _supportedVideoFileExtensions = new string[] { "mp4", "avi", "mov" };
        private readonly IEnumerable<string> _supported3dModelFormats = new string[] { "stl", "obj", "fbx" };

        public ContentController(
            IJwtService jwtService,
            IFileUploaderService fileUploaderService,
            IUserCreationRepository userCreationRepository,
            ISubscriptionRepository subscriptionRepository,
            IUserRepository userRepository,
            ContentInspector contentInspector
        )
        {
            _jwtService = jwtService;
            _fileUploaderService = fileUploaderService;
            _userCreationRepository = userCreationRepository;
            _subscriptionRepository = subscriptionRepository;
            _userRepository = userRepository;
            _contentInspector = contentInspector;
        }

        [HttpPatch("content"), Authorize]
        [SwaggerOperation("Обновить описание контента")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> ChangeDescriptionContent(
            UpdateContentDescriptionBody body,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var result = await _userCreationRepository.UpdateDescriptionAsync(body.Id, body.Description, tokenInfo.UserId);
            if (result == null)
                return BadRequest("Failed to update the description");

            return Ok();
        }

        [HttpGet("content/comments")]
        [SwaggerOperation("Получить комментарии контента")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<CommentBody>))]

        public async Task<IActionResult> GetContentComments(
            [FromQuery, Required] Guid contentId,
            [FromQuery] int count = 1,
            [FromQuery] int offset = 0
        )
        {
            var comments = await _userCreationRepository.GetUserCreationCommentsAndUserAsync(contentId, count, offset);
            var result = comments.Select(e => e.ToCommentBody());
            return Ok(result);
        }

        [HttpPost("content"), Authorize]
        [SwaggerOperation("Добавить контент в портфолио")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> AddContent(
            CreateUserCreationBody body,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            if (user == null)
                return BadRequest();

            var result = await _userCreationRepository.AddAsync(body, user);
            if (result == null)
                return BadRequest("Failed to add the content");

            return Ok();
        }

        [HttpGet("contents"), Authorize]
        [SwaggerOperation("Получить контент пользователя")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<UserCreationBody>))]

        public async Task<IActionResult> GetContents(
            [FromQuery, Required] Guid userId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery] int count = 10,
            [FromQuery] int offset = 0
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var currentUser = await _userRepository.GetAsync(tokenInfo.UserId);

            var contents = (await _userCreationRepository.GetUserCreationsAsync(userId, ContentSubscriptionType.Public, count, offset)).Select(e => e.ToUserCreationBody());
            return Ok(contents);
        }

        [HttpPost("upload/content"), Authorize]
        [SwaggerOperationFilter(typeof(UploadedFileContentTypesOperationFilter))]
        [SwaggerResponse(200, Type = typeof(Guid))]
        [SwaggerResponse(400)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> UploadContent(
            [FromForm, Required] IFormFile file,
            [FromHeader, Required] Guid contentId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var tokenInfo = _jwtService.GetTokenInfo(token);

            var userCreation = await _userCreationRepository.GetAsync(contentId);
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


        [HttpGet("upload/content/pub/{filename}")]
        [SwaggerResponse(200)]

        public async Task<IActionResult> GetPublicFileContent([Required] string filename)
        {
            return await GetFileAsync(Constants.localPathToPublicContentFiles, filename);
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
            var tokenInfo = _jwtService.GetTokenInfo(token);
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

        [HttpPost("like/content"), Authorize]
        [SwaggerOperation("Оценить контент пользователя")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(404)]
        [SwaggerResponse(409)]

        public async Task<IActionResult> AddUserCreationContent(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid contentId
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);

            var userCreation = await _userCreationRepository.GetAsync(contentId);
            if (userCreation == null)
                return NotFound();

            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            if (user == null)
                return BadRequest();

            var like = await _userCreationRepository.CreateUserCreationLikeAsync(user, userCreation);
            return like == null ? Conflict() : Ok();
        }

        [HttpPost("comment/content"), Authorize]
        [SwaggerOperation("Добавить комментарий к контенту")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(404)]

        public async Task<IActionResult> AddUserCreationComment(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            CreateCommentBody commentBody,
            [FromQuery, Required] Guid contentId
        )
        {
            var tokenInfo = _jwtService.GetTokenInfo(token);

            var userCreation = await _userCreationRepository.GetAsync(contentId);
            if (userCreation == null)
                return NotFound();

            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            if (user == null)
                return BadRequest();

            var comment = await _userCreationRepository.CreateUserCreationCommentAsync(commentBody, userCreation, user);
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
