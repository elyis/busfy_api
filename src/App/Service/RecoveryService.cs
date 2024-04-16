using busfy_api.src.App.IService;
using busfy_api.src.Domain.Entities.Request;
using busfy_api.src.Domain.IRepository;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace busfy_api.src.App.Service
{
    public class RecoveryService : IRecoveryService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<RecoveryService> _logger;

        public RecoveryService(
            IUserRepository userRepository,
            IEmailService emailService,
            ILogger<RecoveryService> logger
        )
        {
            _emailService = emailService;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<bool> SendRecoveryCodeAsync(RecoveryBody recoveryBody)
        {
            var recoveryCode = GenerateRecoveryCode();
            var user = await _userRepository.SetRecoveryCode(recoveryBody.Email, recoveryCode);
            if (user == null)
                return false;


            try
            {
                await _emailService.SendMessage(
                            recoveryBody.Email,
                            "Restoring account access",
                            $"Recovery code: {recoveryCode}"
                        );
                return true;
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError($"invalid data: {ex.Message}");
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError($"Auth error: {ex.Message}");
            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError("Protocol error while trying to authenticate: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ex: {ex.Message}");
            }

            return false;
        }

        private static string GenerateRecoveryCode()
        {
            var rnd = new Random();
            return rnd.Next(100_000, 1_000_000).ToString();
        }

        public async Task<bool> VerifyRecoveryCodeAsync(RecoveryVerificationCodeBody verificationCodeBody)
        {
            var result = await _userRepository.VerifyRecoveryCode(verificationCodeBody.Email, verificationCodeBody.RecoveryCode);
            return result;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordBody body)
        {
            var result = await _userRepository.ResetPasswordAndRemoveSessions(body);
            return result != null;
        }
    }
}