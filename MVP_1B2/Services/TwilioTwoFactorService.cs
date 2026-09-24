using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace MVP_1B2.Services
{
    public class TwilioTwoFactorService : ITwoFactorService
    {
        private readonly TwilioOptions _options;

        public TwilioTwoFactorService(
            IOptions<TwilioOptions> options)
        {
            _options = options.Value;

            TwilioClient.Init(
                _options.AccountSid,
                _options.AuthToken);
        }

        public async Task SendCodeAsync(
            string phoneNumber)
        {
            await VerificationResource.CreateAsync(
                to: phoneNumber,
                channel: "sms",
                pathServiceSid: _options.VerificationSid);
        }

        public async Task<bool> VerifyCodeAsync(
            string phoneNumber,
            string code)
        {
            var verificationCheck =
                await VerificationCheckResource.CreateAsync(
                    to: phoneNumber,
                    code: code,
                    pathServiceSid: _options.VerificationSid);

            return verificationCheck.Status == "approved";
        }
    }
}
