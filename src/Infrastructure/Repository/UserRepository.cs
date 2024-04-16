using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.Enums;
using busfy_api.src.Domain.IRepository;
using busfy_api.src.Domain.Models;
using busfy_api.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using webApiTemplate.src.App.Provider;

namespace busfy_api.src.Infrastructure.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        private const int _countDaysForSessionVerification = 7;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserModel?> AddAsync(SignUpBody body, string role)
        {
            var user = await GetAsync(body.Email);
            if (user != null)
                return null;

            user = new UserModel
            {
                Email = body.Email,
                PasswordHash = Hmac512Provider.Compute(body.Password),
                RoleName = role,
                AccountStatus = UserAccountStatus.Active.ToString(),
                Nickname = body.Nickname
            };

            var result = await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return result?.Entity;
        }

        public async Task<UserSession?> GetUserSession(Guid userId, string host, string userAgent)
        {
            return await _context.UserSessions
                .FirstOrDefaultAsync(e => e.UserId == userId && e.Host == host && e.UserAgent == userAgent);
        }

        public async Task<string?> SetRecoveryCode(string email, string recoveryCode, TimeSpan? interval = null)
        {
            interval ??= TimeSpan.FromMinutes(10.0);
            var user = await GetAsync(email);
            if (user == null || string.IsNullOrEmpty(recoveryCode))
                return null;

            user.RestoreCode = recoveryCode;
            user.RestoreCodeValidBefore = DateTime.UtcNow.Add(interval.Value);
            user.WasPasswordResetRequest = true;
            await _context.SaveChangesAsync();

            return user.RestoreCode;
        }

        public async Task<bool> VerifyRecoveryCode(string email, string recoveryCode)
        {
            var user = await GetAsync(email);
            if (user == null)
                return false;

            var currentDate = DateTime.UtcNow;
            if
            (
                user.RestoreCodeValidBefore == null ||
                user.RestoreCodeValidBefore < currentDate ||
                !user.WasPasswordResetRequest ||
                user.RestoreCode != recoveryCode
            )
                return false;

            return true;
        }

        public async Task<UserModel?> ResetPasswordAndRemoveSessions(ResetPasswordBody resetPasswordBody)
        {
            var user = await GetAsync(resetPasswordBody.Email);
            if (user == null)
                return null;

            var currentDate = DateTime.UtcNow;
            if
            (
                user.RestoreCodeValidBefore == null ||
                user.RestoreCodeValidBefore < currentDate ||
                !user.WasPasswordResetRequest ||
                user.RestoreCode != resetPasswordBody.RecoveryCode
            )
                return null;


            user.PasswordHash = Hmac512Provider.Compute(resetPasswordBody.NewPassword);
            user.RestoreCodeValidBefore = null;
            user.RestoreCode = null;
            user.WasPasswordResetRequest = false;

            var sessions = await GetUserSessions(user.Id);
            _context.UserSessions.RemoveRange(sessions);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<UserSession?> CreateUserSessionAsync(CreateUserSessionBody body, UserModel user, string location)
        {
            if (user == null)
                return null;

            var session = await GetUserSession(user.Id, body.Host, body.UserAgent);
            if (session != null)
                return null;

            session = new UserSession
            {
                UserId = user.Id,
                Host = body.Host,
                IsVerified = false,
                UserAgent = body.UserAgent,
                Location = location
            };

            var result = await _context.UserSessions.AddAsync(session);
            await _context.SaveChangesAsync();
            return result?.Entity;
        }

        public async Task<IEnumerable<UserSession>> GetUserSessions(Guid userId)
        {
            return await _context.UserSessions
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserModel?> GetAsync(Guid id)
            => await _context.Users
                .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<UserModel?> GetAsync(string email)
            => await _context.Users
                .FirstOrDefaultAsync(e => e.Email == email);

        public async Task<UserSession?> GetUserSessionByTokenAndUserAsync(string refreshTokenHash)
            => await _context.UserSessions
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Token == refreshTokenHash);

        public async Task<UserSession?> GetUserSession(Guid id)
        => await _context.UserSessions
            .FirstOrDefaultAsync(e => e.Id == id);

        public async Task<UserModel?> UpdateProfileIconAsync(Guid userId, string filename)
        {
            var user = await GetAsync(userId);
            if (user == null)
                return null;

            user.Image = filename;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<string?> UpdateTokenAsync(string refreshToken, Guid sessionId, TimeSpan? duration = null)
        {
            var session = await GetUserSession(sessionId);
            if (session == null)
                return null;

            if (duration == null)
                duration = TimeSpan.FromDays(15);

            if (session.TokenValidBefore <= DateTime.UtcNow || session.TokenValidBefore == null)
            {
                session.TokenValidBefore = DateTime.UtcNow.Add((TimeSpan)duration);
                session.Token = refreshToken;
                await _context.SaveChangesAsync();
            }

            return session.Token;
        }

        public async Task<bool> IsVerifiedSession(Guid sessionId)
        {
            var session = await GetUserSession(sessionId);
            return session?.IsVerified ?? false;
        }

        public async Task<UserSession?> VerifySession(Guid sessionId)
        {
            var session = await GetUserSession(sessionId);
            if (session == null)
                return null;

            if (!session.IsVerified && session.CreatedAt.AddDays(_countDaysForSessionVerification) <= DateTime.UtcNow)
            {
                session.IsVerified = true;
                await _context.SaveChangesAsync();
                return session;
            }

            return null;
        }

        public async Task<bool> RemoveSession(Guid sessionId)
        {
            var session = await GetUserSession(sessionId);
            if (session == null)
                return true;

            _context.UserSessions.Remove(session);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserModel?> GetUserByUserTag(string userTag)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.UserTag == userTag);
        }

        public async Task<UserModel?> UpdateUserTag(Guid userId, string userTag)
        {
            var user = await GetAsync(userId);
            if (user == null)
                return null;

            var otherUser = await GetUserByUserTag(userTag);
            if (otherUser != null)
                return null;


            user.UserTag = userTag;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<UserModel>> GetUsersByPatternNickname(string pattern, int count, int offset)
        {
            var input = pattern.Replace(" ", "").Replace("\t", "");
            var users = await _context.Users
                .Where(e => EF.Functions.Like(e.Nickname, $"%{input}%"))
                .Take(count)
                .Skip(offset)
                .ToListAsync();
            return users;
        }

        public async Task<IEnumerable<UserModel>> GetUsersByPatternUserTag(string pattern, int count, int offset)
        {
            var input = pattern.Replace(" ", "").Replace("\t", "");
            var users = await _context.Users
                .Where(e => e.UserTag != null && EF.Functions.Like(e.UserTag, $"%{input}%"))
                .Take(count)
                .Skip(offset)
                .ToListAsync();
            return users;
        }
    }
}