using System.ComponentModel.DataAnnotations;
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
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public SubscriptionController(
            ISubscriptionRepository subscriptionRepository,
            IUserRepository userRepository,
            IJwtService jwtService
        )
        {
            _subscriptionRepository = subscriptionRepository;
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        [HttpGet("subscriptions/user"), Authorize]
        [SwaggerOperation("Получить список подписок на пользователей")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<ProfileWithFollowersBody>))]
        public async Task<IActionResult> GetSubscriptionUser(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Range(1, int.MaxValue)] int count = 1,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var subscriptions = await _subscriptionRepository.GetSubscriptionsByUserAndSubscription(tokenInfo.UserId, count, offset);

            var result = new List<ProfileWithFollowersBody>();
            foreach (var subscription in subscriptions)
            {
                var author = subscription.User;
                var subscriberCount = await _subscriptionRepository.GetCountSubscribersByCreator(author.Id);
                result.Add(new ProfileWithFollowersBody
                {
                    Profile = author.ToProfileBody(),
                    SubscriberCount = subscriberCount
                });
            }

            return Ok(new PaginationResponse<ProfileWithFollowersBody>
            {
                Count = count,
                Offset = offset,
                Total = subscriptions.Count(),
                Items = result
            });
        }


        [HttpPost("subscription"), Authorize]
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

            var subscription = await _subscriptionRepository.CreateSubscription(subscriptionBody, user);
            return subscription == null ? BadRequest() : Ok(subscription.ToSubscriptionBody());
        }

        [HttpGet("subscription"), Authorize]
        [SwaggerOperation("Проверить наличие подписки")]
        [SwaggerResponse(200)]
        [SwaggerResponse(204)]

        public async Task<IActionResult> GetSubscription(
            [FromQuery, Required] Guid subscriptionId,
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token)
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);
            var userSubscription = await _subscriptionRepository.GetUserSubscriptionAsync(subscriptionId, tokenPayload.UserId);
            return userSubscription == null ? NoContent() : Ok();
        }

        [HttpGet("subscriptions-created"), Authorize]
        [SwaggerOperation("Получить все созданные подписки")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<SubscriptionBody>))]

        public async Task<IActionResult> GetSubsciptions(
            [FromQuery, Required] Guid userId,
            [FromQuery, Range(0, int.MaxValue)] int count = 10,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0
        )
        {
            var subscriptions = await _subscriptionRepository.GetSubscriptionsCreatedByUser(userId, count, offset);
            var result = subscriptions.Select(e => e.ToSubscriptionBody());
            return Ok(result);
        }

        [HttpDelete("subscription"), Authorize]
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


        [HttpGet("subscriptions"), Authorize]
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

        [HttpDelete("unsubscribe"), Authorize]
        [SwaggerOperation("Отписаться")]
        [SwaggerResponse(204)]

        public async Task<IActionResult> Unsubscribe(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token,
            [FromQuery, Required] Guid subscriptionId
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var user = await _userRepository.GetAsync(tokenInfo.UserId);
            if (user == null)
                return BadRequest();

            await _subscriptionRepository.RemoveUserCreation(subscriptionId, user.Id);
            return NoContent();
        }

        [HttpPost("subscribe"), Authorize]
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

            var userSubscription = await _subscriptionRepository.CreateUserSubscription(subscriptionId, user);
            return userSubscription == null ? BadRequest() : Ok(userSubscription.ToUserSubscriptionBody());
        }
    }
}