namespace MVP_1B2.Services
{
    public sealed class LicensePayload
    {
        public string LicenseId { get; set; } = string.Empty;

        public string CustomerId { get; set; } = string.Empty;

        public string LicenseType { get; set; } = string.Empty;

        public DateTimeOffset IssuedAtUtc { get; set; }

        public DateTimeOffset ValidFromUtc { get; set; }

        public DateTimeOffset ValidUntilUtc { get; set; }
    }
}
