using System.Net;
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
            IRecoveryService recoveryService
        )
        {
            _authService = authService;
            _recoveryService = recoveryService;
        }


        [SwaggerOperation("Регистрация")]
        [SwaggerResponse(200, "Успешно создан", Type = typeof(AuthorizationResultBody))]
        [SwaggerResponse(400, "Неккоректный код подтверждения или истек период действия")]


        [HttpPost("confirm-signup")]
        public async Task<IActionResult> SignUpAsync(
            ConfirmSignupBody body
        )
        {
            string role = Enum.GetName(UserRole.User)!;

            string ipAddress = "Unknown";
            var forwardedHeaders = HttpContext.Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(forwardedHeaders))
            {
                var forwardedIp = forwardedHeaders.FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedIp))
                    ipAddress = IPAddress.Parse(forwardedIp).ToString();
            }

            var sessionBody = new CreateUserSessionBody
            {
                Host = ipAddress,
                UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
            };

            var result = await _authService.SignUp(body.Email, body.ConfirmationCode, sessionBody, role);
            return result;
        }

        [HttpPost("signup")]
        [SwaggerOperation("Отправить запрос на подтверждение создания аккаунта")]
        [SwaggerResponse(200)]
        [SwaggerResponse(400)]
        [SwaggerResponse(409, Description = "Почта уже зарегистрированна")]

        public async Task<IActionResult> Confirmation(SignUpBody signUpBody)
        {

            var rnd = new Random();
            var confirmationCode = rnd.Next(100_000, 1_000_000).ToString();

            var response = await _authService.CreateConfirmationAccount(signUpBody, confirmationCode);
            if (response is OkResult result)
            {
                await _recoveryService.SendConfirmationCodeAsync(signUpBody.Email, confirmationCode);
                return Ok();
            }
            return BadRequest();
        }



        [SwaggerOperation("Авторизация")]
        [SwaggerResponse(200, "Успешно", Type = typeof(AuthorizationResultBody))]
        [SwaggerResponse(400, "Пароли не совпадают")]
        [SwaggerResponse(404, "Email не зарегистрирован")]

        [HttpPost("signin")]
        public async Task<IActionResult> SignInAsync(SignInBody signInBody)
        {
            string ipAddress = "Unknown";
            var forwardedHeaders = HttpContext.Request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrEmpty(forwardedHeaders))
            {
                var forwardedIp = forwardedHeaders.FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedIp))
                    ipAddress = IPAddress.Parse(forwardedIp).ToString();
            }

            var sessionBody = new CreateUserSessionBody
            {
                Host = ipAddress,
                UserAgent = HttpContext.Request.Headers.UserAgent.ToString(),
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