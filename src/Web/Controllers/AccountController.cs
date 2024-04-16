using System.ComponentModel.DataAnnotations;
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
            var tokenInfo = _jwtService.GetTokenInfo(token);
            var result = await _userRepository.UpdateUserTag(tokenInfo.UserId, userTag);
            return result == null ? Conflict() : Ok();
        }

        [HttpGet("account/userTag")]
        [SwaggerOperation("Получить пользователя по тегу")]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]

        public async Task<IActionResult> GetUserByUserTag(
            [FromQuery(Name = "me"), Required] string tag
        )
        {
            var user = await _userRepository.GetUserByUserTag(tag);
            return user == null ? NotFound() : Ok(user);
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

    }
}