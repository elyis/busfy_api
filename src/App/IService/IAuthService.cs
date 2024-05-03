using busfy_api.src.Domain.Entities.Request;
using Microsoft.AspNetCore.Mvc;

namespace busfy_api.src.App.IService
{
    public interface IAuthService
    {
        Task<IActionResult> SignUp(string email, string confirmationCode, CreateUserSessionBody sessionBody, string rolename);
        Task<IActionResult> SignIn(SignInBody body, CreateUserSessionBody sessionBody);
        Task<IActionResult> RestoreToken(string refreshToken);
        Task<IActionResult> CreateConfirmationAccount(SignUpBody body, string confirmationCode);
    }
}