using Microsoft.AspNetCore.Identity;
using MVP_1B2.Models;

namespace MVP_1B2.Services
{
    public class TwilioTwoFactorTokenProvider
        : IUserTwoFactorTokenProvider<ApplicationUser>
    {
        private readonly ITwoFactorService _twoFactorService;

        public TwilioTwoFactorTokenProvider(
            ITwoFactorService twoFactorService)
        {
            _twoFactorService = twoFactorService;
        }

        public async Task<bool> CanGenerateTwoFactorTokenAsync(
            UserManager<ApplicationUser> manager,
            ApplicationUser user)
        {
            return !string.IsNullOrWhiteSpace(user.PhoneNumber);
        }

        public async Task<string> GenerateAsync(
            string purpose,
            UserManager<ApplicationUser> manager,
            ApplicationUser user)
        {
            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                throw new InvalidOperationException(
                    "The user does not have a phone number.");
            }

            await _twoFactorService.SendCodeAsync(
                user.PhoneNumber);

            // Twilio Verify generates and sends
            // the actual verification code.
            return string.Empty;
        }

        public async Task<bool> ValidateAsync(
            string purpose,
            string token,
            UserManager<ApplicationUser> manager,
            ApplicationUser user)
        {
            Console.WriteLine("===== Twilio ValidateAsync =====");
            Console.WriteLine($"Purpose: {purpose}");
            Console.WriteLine($"Token entered: {token}");
            Console.WriteLine($"Phone: {user.PhoneNumber}");

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                return false;
            }

            return await _twoFactorService.VerifyCodeAsync(
                user.PhoneNumber,
                token);
        }
    }
}
