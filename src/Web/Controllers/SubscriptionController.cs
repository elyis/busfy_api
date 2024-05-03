using System.ComponentModel.DataAnnotations;
using System.Net;
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
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionToAdditionalContentRepository _subscriptionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public SubscriptionController(
            ISubscriptionToAdditionalContentRepository subscriptionRepository,
            IUserRepository userRepository,
            IJwtService jwtService
        )
        {
            _subscriptionRepository = subscriptionRepository;
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        [HttpPost("subscription/user"), Authorize]
        [SwaggerOperation("Подписаться на аккаунт")]
        [SwaggerResponse(200, Type = typeof(SubscriptionBody))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreateSubscription(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid authorId
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var author = await _userRepository.GetAsync(authorId);
            if (author == null)
                return BadRequest();

            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            if (user == null)
                return BadRequest();

            var result = await _subscriptionRepository.AddSubscriptionAsync(user, author);
            return result == null ? Conflict() : Ok();
        }

        [HttpDelete("subscription/user"), Authorize]
        [SwaggerOperation("Отписаться от аккаунта")]
        [SwaggerResponse(204)]

        public async Task<IActionResult> RemoveSubscription(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid authorId
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            await _subscriptionRepository.RemoveSubscriptionAsync(tokenInfo.UserId, authorId);
            return NoContent();
        }

        [HttpGet("subscriptions/user"), Authorize]
        [SwaggerOperation("Получить список подписок на пользователей")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<ProfileBody>))]
        public async Task<IActionResult> GetSubscriptionUser(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var subscriptions = await _subscriptionRepository.GetSubscriptionsWithAuthorAsync(tokenInfo.UserId);
            var result = subscriptions.Select(e => e.Author.ToProfileBody());
            return Ok(result);
        }


        [HttpPost("subscription/additional-content"), Authorize]
        [SwaggerOperation("Создать подписку на свой аккаунт")]
        [SwaggerResponse(200, Type = typeof(SubscriptionBody))]
        [SwaggerResponse(400)]

        public async Task<IActionResult> CreateSubscription(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            CreateSubscriptionBody subscriptionBody
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);

            var subscription = await _subscriptionRepository.AddAsync(subscriptionBody, user);
            return subscription == null ? BadRequest() : Ok(subscription.ToSubscriptionBody());
        }

        [HttpGet("subscriptions-created/additional-content"), Authorize]
        [SwaggerOperation("Получить все созданные подписки")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<SubscriptionBody>))]

        public async Task<IActionResult> GetSubsciptions(
            [FromQuery, Required] Guid userId,
            [FromQuery] int count = 1,
            [FromQuery] int offset = 0
        )
        {
            var subscriptions = await _subscriptionRepository.GetSubscriptionsCreatedByUser(userId, count, offset);
            var result = subscriptions.Select(e => e.ToSubscriptionBody());
            return Ok(result);
        }

        [HttpDelete("subscription/additional-content"), Authorize]
        [SwaggerOperation("Удалить созданную подписку")]
        [SwaggerResponse(204)]
        [SwaggerResponse(403)]

        public async Task<IActionResult> RemoveAsync(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid subId
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var result = await _subscriptionRepository.DeleteAsync(subId, tokenInfo.UserId);

            return result ? NoContent() : Forbid();
        }


        [HttpGet("subscriptions/additional-content"), Authorize]
        [SwaggerOperation("Получить подписки пользователя на других")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<UserSubscriptionBody>))]

        public async Task<IActionResult> GetMySubscriptionToUser(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery] int count = 1,
            [FromQuery] int offset = 0
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);

            var subscriptions = await _subscriptionRepository.GetSubscriptionsByUserAndSubscription(tokenInfo.UserId, count, offset);
            var result = subscriptions.Select(e => e.ToUserSubscriptionBody());
            return Ok(result);
        }

        [HttpPost("subscribe/additional-content"), Authorize]
        [SwaggerOperation("Оформить подписку")]
        [SwaggerResponse(200, Type = typeof(UserSubscriptionBody))]

        public async Task<IActionResult> SubscribeToUser(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid subscriptionId
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            if (user == null)
                return BadRequest();

            var userSubscription = await _subscriptionRepository.CreateSubscriptionToUser(subscriptionId, user);
            return userSubscription == null ? BadRequest() : Ok(userSubscription.ToUserSubscriptionBody());
        }
    }
}