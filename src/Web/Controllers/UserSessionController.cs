using System.ComponentModel.DataAnnotations;
using System.Net;
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
    public class UserSessionController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public UserSessionController(
            IJwtService jwtService,
            IUserRepository userRepository
        )
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
        }

        [HttpGet("sessions"), Authorize]
        [SwaggerOperation("Получить список сессий пользователя")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<UserSessionBody>))]

        public async Task<IActionResult> GetUserSessions(
            [FromHeader(Name = nameof(HttpRequestHeader.Authorization))] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var sessions = (await _userRepository.GetUserSessions(tokenInfo.UserId))
                .Select(session => session.ToUserSessionBody())
                .ToList();

            sessions.First(e => e.Id == tokenInfo.SessionId).IsCurrent = true;
            return Ok(sessions);
        }

        [HttpDelete("session"), Authorize]

        public async Task<IActionResult> RemoveSession(
            [FromQuery, Required] Guid sessionId
        )
        {
            var isVerifiedSession = await _userRepository.IsVerifiedSession(sessionId);
            if (!isVerifiedSession)
                return Forbid();

            await _userRepository.RemoveSession(sessionId);
            return NoContent();
        }


    }
}