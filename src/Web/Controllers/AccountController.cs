using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
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
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public AccountController(
            IUserRepository userRepository,
            IJwtService jwtService
        )
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        [HttpPatch("account/userTag"), Authorize]
        [SwaggerOperation("Изменить тег пользователя")]
        [SwaggerResponse(200)]
        [SwaggerResponse(409)]

        public async Task<IActionResult> ChangeUserTag(
            [FromQuery, RegularExpression("^[a-zA-Z_-]+$")] string userTag,
            [FromHeader(Name = "Authorization")] string token
        )
        {
            var tokenInfo = _jwtService.GetTokenPayload(token);
            var result = await _userRepository.UpdateUserTag(tokenInfo.UserId, userTag);
            return result == null ? Conflict() : Ok();
        }

        [HttpGet("account/userTag")]
        [SwaggerOperation("Получить пользователя по тегу")]
        [SwaggerResponse(200, Type = typeof(ProfileBody))]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetUserByUserTag(
            [FromQuery(Name = "me"), Required] string tag
        )
        {
            var user = await _userRepository.GetUserByUserTag(tag);
            return user == null ? NotFound() : Ok(user.ToProfileBody());
        }

        [HttpGet("accounts/nickname")]
        [SwaggerOperation("Поиск пользователей по патерну ника")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<GeneralAccountInformationBody>))]

        public async Task<IActionResult> GetUserByPatternNickname(
            [FromQuery, Required] string pattern,
            [FromQuery, Required] int count,
            [FromQuery, Required] int offset
        )
        {
            var users = await _userRepository.GetUsersByPatternNickname(pattern, count, offset);
            var profiles = users.Select(e => e.ToGeneralAccountInformationBody());
            return Ok(profiles);
        }

        [HttpGet("accounts/userTag")]
        [SwaggerOperation("Поиск пользователей по патерну тега")]
        [SwaggerResponse(200, Type = typeof(IEnumerable<GeneralAccountInformationBody>))]

        public async Task<IActionResult> GetUserByPatternUserTag(
            [FromQuery, Required] string pattern,
            [FromQuery, Required] int count,
            [FromQuery, Required] int offset
        )
        {
            var users = await _userRepository.GetUsersByPatternUserTag(pattern, count, offset);
            var profiles = users.Select(e => e.ToGeneralAccountInformationBody());
            return Ok(profiles);
        }

        [HttpGet("accounts"), Authorize(Roles = nameof(UserRole.Admin))]
        [SwaggerOperation("Получить список пользователей")]
        [SwaggerResponse(200, Type = typeof(PaginationResponse<GeneralAccountInformationBody>))]

        public async Task<IActionResult> GetUsers(
            [FromHeader(Name = nameof(HttpRequestHeaders.Authorization))] string token,
            [FromQuery, Range(0, int.MaxValue)] int count = 10,
            [FromQuery, Range(0, int.MaxValue)] int offset = 0
        )
        {
            var tokenPayload = _jwtService.GetTokenPayload(token);

            var users = await _userRepository.GetAll(count, offset);
            var result = users
                .Where(e => e.Id != tokenPayload.UserId)
                .Select(e => e.ToGeneralAccountInformationBody());

            var totalUsers = await _userRepository.GetCountUsers();

            return Ok(new PaginationResponse<GeneralAccountInformationBody>
            {
                Count = count,
                Items = result,
                Offset = offset,
                Total = totalUsers
            });
        }

        [HttpPost("account/lockout"), Authorize(Roles = nameof(UserRole.Admin))]
        [SwaggerOperation("Заблокировать пользователя")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]

        public async Task<IActionResult> LockOutUser(
            [FromQuery, Required] Guid userId
        )
        {
            var result = await _userRepository.LockOutUser(userId);
            return result == null ? NotFound() : Ok();
        }

        [HttpPost("account/unblock"), Authorize(Roles = nameof(UserRole.Admin))]
        [SwaggerOperation("Разблокировать пользователя")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]

        public async Task<IActionResult> UnblockUser(
            [FromQuery, Required] Guid userId
        )
        {
            var result = await _userRepository.UnlockUser(userId);
            return result == null ? NotFound() : Ok();
        }
    }
}