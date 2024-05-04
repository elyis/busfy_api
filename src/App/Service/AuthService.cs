using busfy_api.src.App.IService;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Entities.Shared;
using busfy_api.src.Domain.IRepository;
using Microsoft.AspNetCore.Mvc;
using webApiTemplate.src.App.IService;
using webApiTemplate.src.App.Provider;
using webApiTemplate.src.Domain.Entities.Shared;

namespace busfy_api.src.App.Service
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IJwtService _jwtService;
        private readonly ILocationService _locationService;

        public AuthService
        (
            IUserRepository userRepository,
            IAccountRepository accountRepository,
            IJwtService jwtService,
            ILocationService locationService
        )
        {
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _jwtService = jwtService;
            _locationService = locationService;
        }

        public async Task<IActionResult> RestoreToken(string refreshToken)
        {
            var session = await _userRepository.GetUserSessionByTokenAndUserAsync(refreshToken);
            if (session == null)
                return new NotFoundResult();

            await _userRepository.VerifySession(session.Id);

            var user = session.User;
            var tokenPair = await UpdateToken(user.RoleName, user.Id, session.Id);
            return new OkObjectResult(tokenPair);
        }

        public async Task<IActionResult> CreateConfirmationAccount(SignUpBody body, string confirmationCode)
        {
            var user = await _userRepository.GetAsync(body.Email);
            if (user != null)
                return new ConflictResult();

            var result = await _accountRepository.CreateOrUpdateCode(body, confirmationCode);
            return result == null ? new BadRequestResult() : new OkResult();
        }

        public async Task<IActionResult> SignIn(SignInBody body, CreateUserSessionBody sessionBody)
        {
            var user = await _userRepository.GetAsync(body.Email);
            if (user == null)
                return new NotFoundResult();

            var inputPasswordHash = Hmac512Provider.Compute(body.Password);
            if (user.PasswordHash != inputPasswordHash)
                return new BadRequestResult();

            var iPResponse = await _locationService.GetIPResponse(sessionBody.Host);
            string location = "Unknown";
            if (iPResponse != null)
                location = $"{iPResponse.City}/{iPResponse.Country}";

            var session = await _userRepository.GetUserSession(user.Id, sessionBody.Host, sessionBody.UserAgent);
            session ??= await _userRepository.CreateUserSessionAsync(sessionBody, user, location);

            await _userRepository.VerifySession(session.Id);

            var tokenPair = await UpdateToken(user.RoleName, user.Id, session.Id);
            var result = new AuthorizationResultBody
            {
                Profile = user.ToProfileBody(),
                TokenPair = tokenPair
            };
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> SignUp(string email, string confirmationCode, CreateUserSessionBody sessionBody, string rolename)
        {
            var account = await _accountRepository.Get(email);
            if (account == null)
                return new BadRequestResult();

            if (account.ValidityPeriodCode < DateTime.UtcNow || account.ConfirmationCode != confirmationCode)
                return new BadRequestResult();

            var body = account.ToSignUpBody();
            var user = await _userRepository.AddAsync(body, rolename);
            if (user == null)
                return new ConflictResult();

            var iPResponse = await _locationService.GetIPResponse(sessionBody.Host);
            string location = "Unknown";
            if (iPResponse != null)
                location = $"{iPResponse.City}/{iPResponse.Country}";

            var session = await _userRepository.GetUserSession(user.Id, sessionBody.Host, sessionBody.UserAgent);
            session ??= await _userRepository.CreateUserSessionAsync(sessionBody, user, location);

            await _userRepository.VerifySession(session.Id);
            await _accountRepository.Remove(email);

            var tokenPair = await UpdateToken(rolename, user.Id, session.Id);
            var result = new AuthorizationResultBody
            {
                Profile = user.ToProfileBody(),
                TokenPair = tokenPair
            };
            return new OkObjectResult(result);
        }

        private async Task<TokenPair> UpdateToken(string rolename, Guid userId, Guid sessionId)
        {
            var tokenInfo = new TokenInfo
            {
                Role = rolename,
                UserId = userId,
                SessionId = sessionId
            };

            var tokenPair = _jwtService.GenerateDefaultTokenPair(tokenInfo);
            tokenPair.RefreshToken = await _userRepository.UpdateTokenAsync(tokenPair.RefreshToken, tokenInfo.SessionId);
            return tokenPair;
        }
    }
}