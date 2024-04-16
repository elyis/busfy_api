using busfy_api.src.Domain.Entities.Request;

namespace busfy_api.src.App.IService
{
    public interface IRecoveryService
    {
        Task<bool> SendRecoveryCodeAsync(RecoveryBody recoveryBody);
        Task<bool> VerifyRecoveryCodeAsync(RecoveryVerificationCodeBody verificationCodeBody);
        Task<bool> ResetPasswordAsync(ResetPasswordBody body);
    }
}