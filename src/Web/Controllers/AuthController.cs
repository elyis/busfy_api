using busfy_api.src.App.IService;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Entities.Shared;
using busfy_api.src.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace busfy_api.src.Web.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRecoveryService _recoveryService;


        public AuthController(
            IAuthService authService,
            IRecoveryService recoveryService)
        {
            _authService = authService;
            _recoveryService = recoveryService;
        }


        [SwaggerOperation("Регистрация")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(TokenPair))]
        [SwaggerResponse(400, "Токен не валиден или активирован")]
        [SwaggerResponse(409, "Почта уже существует")]


        [HttpPost("signup")]
        public async Task<IActionResult> SignUpAsync(SignUpBody signUpBody)
        {
            string role = Enum.GetName(UserRole.User)!;
            var sessionBody = new CreateUserSessionBody
            {
                Host = Request.Host.Host,
                UserAgent = Request.Headers.UserAgent.ToString(),
            };

            var result = await _authService.SignUp(signUpBody, sessionBody, role);
            return result;
        }



        [SwaggerOperation("Авторизация")]
        [SwaggerResponse(200, "Успешно", Type = typeof(TokenPair))]
        [SwaggerResponse(400, "Пароли не совпадают")]
        [SwaggerResponse(404, "Email не зарегистрирован")]

        [HttpPost("signin")]
        public async Task<IActionResult> SignInAsync(SignInBody signInBody)
        {
            var sessionBody = new CreateUserSessionBody
            {
                Host = Request.Host.Host,
                UserAgent = Request.Headers.UserAgent.ToString(),
            };

            var result = await _authService.SignIn(signInBody, sessionBody);
            return result;
        }

        [SwaggerOperation("Восстановление токена")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(TokenPair))]
        [SwaggerResponse(404, "Токен не используется")]

        [HttpPost("token")]
        public async Task<IActionResult> RestoreTokenAsync(TokenBody body)
        {
            var result = await _authService.RestoreToken(body.Value);
            return result;
        }

        [SwaggerOperation("Отправка кода восстановления")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        [HttpPost("code/recovery")]
        public async Task<IActionResult> SendRecoveryCode(RecoveryBody recoveryBody)
        {
            var result = await _recoveryService.SendRecoveryCodeAsync(recoveryBody);
            return result ? Ok() : BadRequest();
        }

        [SwaggerOperation("Верификация кода восстановления")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        [HttpPost("code/verify")]
        public async Task<IActionResult> VerifyRecoveryCode(RecoveryVerificationCodeBody body)
        {
            var result = await _recoveryService.VerifyRecoveryCodeAsync(body);
            return result ? Ok() : BadRequest();
        }

        [SwaggerOperation("Сброс пароля")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordBody resetPasswordBody)
        {
            var result = await _recoveryService.ResetPasswordAsync(resetPasswordBody);
            return result ? Ok() : BadRequest();
        }
    }
}