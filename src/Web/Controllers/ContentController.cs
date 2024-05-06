using System.ComponentModel.DataAnnotations;
using System.Net;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace busfy_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class ContentController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IUserCreationRepository _userCreationRepository;
        private readonly ISubscriptionToAdditionalContentRepository _subscriptionToAdditionalContentRepository;
        private readonly IContentCategoryRepository _contentCategoryRepository;
        private readonly IUserRepository _userRepository;

        public ContentController(
            IJwtService jwtService,
            IUserCreationRepository userCreationRepository,
            ISubscriptionToAdditionalContentRepository subscriptionToAdditionalContentRepository,
            IContentCategoryRepository contentCategoryRepository,
            IUserRepository userRepository
        )
        {
            _jwtService = jwtService;
            _userCreationRepository = userCreationRepository;
            _contentCategoryRepository = contentCategoryRepository;
            _subscriptionToAdditionalContentRepository = subscriptionToAdditionalContentRepository;
            _userRepository = userRepository;
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
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var result = await _userCreationRepository.UpdateDescriptionAsync(body.Id, body.Description, tokenInfo.UserId);
            if (result == null)
                return BadRequest("Failed to update the description");

            return Ok();
        }

        [HttpGet("content/comments")]
        [SwaggerOperation("Получить комментарии контента")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<CommentBody>))]

        public async Task<IActionResult> GetContentComments(
            [FromQuery, Required] Guid contentId,
            [FromQuery, Range(1, int.MaxValue)] int count = 10,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0
        )
        {
            var comments = await _userCreationRepository.GetUserCreationCommentsAndUserAsync(contentId, count, offset);
            var totalComments = await _userCreationRepository.GetCountComments(contentId);

            return Ok(new PaginationResponse<CommentBody>
            {
                Total = totalComments,
                Count = count,
                Offset = offset,
                Items = comments.Select(e => e.ToCommentBody())
            });
        }

        [HttpPost("content"), Authorize]
        [SwaggerOperation("Добавить контент в портфолио")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        public async Task<IActionResult> AddContent(
            CreateUserCreationBody body,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] string categoryName
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            if (user == null)
                return BadRequest();

            var category = await _contentCategoryRepository.Get(categoryName);
            if (category == null)
                return BadRequest();

            var result = await _userCreationRepository.AddAsync(body, user, category);
            if (result == null)
                return BadRequest("Failed to add the content");

            return Ok();
        }

        [HttpGet("contents"), Authorize]
        [SwaggerOperation("Получить контент пользователя")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<UserCreationBody>))]

        public async Task<IActionResult> GetContents(
            [FromQuery, Required] Guid userId,
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery] int count = 10,
            [FromQuery] int offset = 0
        )
        {
            var types = new List<ContentSubscriptionType>() { ContentSubscriptionType.Public };
            var tokenPayload = _jwtService.GetTokenPayload(token);
            if (userId == tokenPayload.UserId)
            {
                types.Add(ContentSubscriptionType.Private);
                types.Add(ContentSubscriptionType.Single);
            }
            else
            {
                var user = await _userRepository.GetAsync(userId);
                if (user == null)
                    return NotFound();

                var subscriptions = await _subscriptionToAdditionalContentRepository.GetSubscriptionsByUserAndSubscription(userId, count, offset);
                types.AddRange(subscriptions.Select(e => Enum.Parse<ContentSubscriptionType>(e.Subscription.Type)));
            }

            var result = await _userCreationRepository.GetUserCreationsAsync(userId, types, count, offset);
            var totalContents = await _userCreationRepository.GetCountUserCreations(userId, types);

            var items = result.Select(e => e.ToUserCreationBody()).ToList();
            var postIds = items.Select(e => e.Id);
            var favouritePosts = await _userCreationRepository.GetAllLikes(tokenPayload.UserId, postIds);

            foreach (var favouritePost in favouritePosts)
            {
                var temp = items.First(e => e.Id == favouritePost.CreationId);
                temp.HasEvaluated = true;
            }

            return Ok(new PaginationResponse<UserCreationBody>
            {
                Count = count,
                Offset = offset,
                Total = totalContents,
                Items = items
            });
        }

        [HttpGet("content/likes")]
        [SwaggerOperation("Получить число лайков в посте")]
        [SwaggerResponse(200, Type = typeof(int))]
        public async Task<IActionResult> GetCountLikes([FromQuery, Required] Guid contentId)
        {
            var count = await _userCreationRepository.GetCountLikes(contentId);
            return Ok(new
            {
                count
            });
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
            var tokenInfo = _jwtService.GetTokenPayload(token);

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
            var tokenInfo = _jwtService.GetTokenPayload(token);

            var userCreation = await _userCreationRepository.GetAsync(contentId);
            if (userCreation == null)
                return NotFound();

            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            if (user == null)
                return BadRequest();

            var comment = await _userCreationRepository.CreateUserCreationCommentAsync(commentBody, userCreation, user);
            return Ok();
        }
    }

}
