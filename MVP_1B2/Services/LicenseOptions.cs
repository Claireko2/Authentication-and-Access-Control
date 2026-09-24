namespace MVP_1B2.Services
{
    public sealed class LicenseOptions
    {
        public string LicenseFilePath { get; set; }
            = "App_Data/license.json";

        public string PublicKeyPath { get; set; }
            = "Keys/license-public.pem";
    }
}
