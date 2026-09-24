namespace MVP_1B2.Services
{
    public interface ITwoFactorService
    {
        Task SendCodeAsync(string phoneNumber);

        Task<bool> VerifyCodeAsync(
            string phoneNumber,
            string code);
    }
}
