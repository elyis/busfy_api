using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Models;

namespace busfy_api.src.Domain.IRepository
{
    public interface IUserRepository
    {
        Task<UserModel?> AddAsync(SignUpBody body, string role);
        Task<IEnumerable<UserModel>> GetAll(int count, int offset);
        Task<int> GetCountUsers();
        Task<bool> RemoveSession(Guid sessionId);
        Task<UserModel?> GetAsync(Guid id);
        Task<UserModel?> UpdateProfileAsync(UpdateProfileBody body, Guid id);
        Task<UserSession?> GetUserSession(Guid userId, string host, string userAgent);
        Task<UserModel?> GetAsync(string email);
        Task<IEnumerable<UserSession>> GetUserSessions(Guid userId);
        Task<bool> IsVerifiedSession(Guid sessionId);
        Task<UserSession?> VerifySession(Guid sessionId);
        Task<UserModel?> LockOutUser(Guid userId);
        Task<string?> UpdateTokenAsync(string refreshToken, Guid sessionId, TimeSpan? duration = null);
        Task<UserSession?> GetUserSessionByTokenAndUserAsync(string refreshTokenHash);
        Task<UserSession?> GetUserSession(Guid id);
        Task<UserModel?> UpdateProfileIconAsync(Guid userId, string filename);
        Task<UserSession?> CreateUserSessionAsync(CreateUserSessionBody body, UserModel user, string location);
        Task<UserModel?> GetUserByUserTag(string userTag);
        Task<UserModel?> UpdateUserTag(Guid userId, string userTag);
        Task<IEnumerable<UserModel>> GetUsersByPatternNickname(string pattern, int count, int offset);
        Task<IEnumerable<UserModel>> GetUsersByPatternUserTag(string pattern, int count, int offset);
        Task<bool> VerifyRecoveryCode(string email, string recoveryCode);
        Task<string?> SetRecoveryCode(string email, string recoveryCode, TimeSpan? interval = null);
        Task<UserModel?> ResetPasswordAndRemoveSessions(ResetPasswordBody resetPasswordBody);
        Task<UserModel?> UpdateBackgroundImage(Guid id, string filename);
        Task<UserModel?> UnlockUser(Guid userId);
    }
}