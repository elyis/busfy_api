using System.Net;
using System.Net.Http.Headers;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Entities.Response;
using busfy_api.src.Domain.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using webApiTemplate.src.App.IService;

namespace busfy_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IJwtService _jwtService;

        public ProfileController(
            IUserRepository userRepository,
            IPostRepository postRepository,
            ISubscriptionRepository subscriptionRepository,
            IJwtService jwtService
        )
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _subscriptionRepository = subscriptionRepository;
            _jwtService = jwtService;
        }


        [HttpGet("profile"), Authorize]
        [SwaggerOperation("Получить профиль")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(ProfileWithFollowersAndLikesBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetFullProfileAsync(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            if (user == null)
                return NotFound();

            var countLikes = await _postRepository.GetCountLikesByAuthor(user.Id);
            var subscriberCount = await _subscriptionRepository.GetCountSubscribersByCreator(user.Id);

            return Ok(new ProfileWithFollowersAndLikesBody
            {
                Profile = user.ToProfileBody(),
                CountLikes = countLikes,
                SubscriberCount = subscriberCount
            });
        }

        [HttpGet("profile/followers/{userId}")]
        [SwaggerOperation("Получить профиль")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(int))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetCountFollowersAsync(
            Guid userId
        )
        {
            var subscriberCount = await _subscriptionRepository.GetCountSubscribersByCreator(userId);

            return Ok(new
            {
                subscriberCount
            });
        }

        [HttpGet("profile/{userId}")]
        [SwaggerOperation("Получить профиль")]
        [SwaggerResponse(200, Description = "Успешно", Type = typeof(ProfileWithFollowersAndLikesBody))]
        [SwaggerResponse(404)]
        public async Task<IActionResult> GetFullProfileAsync(
            Guid userId
        )
        {
            var user = await _userRepository.GetAsync(userId);
            if (user == null)
                return NotFound();

            var countLikes = await _postRepository.GetCountLikesByAuthor(user.Id);
            var subscriberCount = await _subscriptionRepository.GetCountSubscribersByCreator(user.Id);

            return Ok(new ProfileWithFollowersAndLikesBody
            {
                Profile = user.ToProfileBody(),
                CountLikes = countLikes,
                SubscriberCount = subscriberCount
            });
        }

        [HttpPut("profile"), Authorize]
        [SwaggerOperation("Обновить профиль")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> UpdateProfileAsync(
            UpdateProfileBody body,
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var result = await _userRepository.UpdateProfileAsync(body, tokenPayload.UserId);
            return result == null ? BadRequest() : Ok(result.ToProfileBody());
        }
    }
}