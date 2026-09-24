using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public sealed class LicensePayload
{
    public string LicenseId { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public string LicenseType { get; set; } = string.Empty;

    public DateTimeOffset IssuedAtUtc { get; set; }

    public DateTimeOffset ValidFromUtc { get; set; }

    public DateTimeOffset ValidUntilUtc { get; set; }
}

internal class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        try
        {
            switch (args[0].ToLowerInvariant())
            {
                case "generate-key":
                    return GenerateKey(args);

                case "issue":
                    return IssueLicense(args);

                default:
                    PrintUsage();
                    return 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"ERROR: {ex.Message}");

            return 1;
        }
    }

    private static int GenerateKey(string[] args)
    {
        var keyDirectory =
            args.Length >= 2
                ? args[1]
                : "LicensingKeys";

        Directory.CreateDirectory(
            keyDirectory);

        var privateKeyPath =
            Path.Combine(
                keyDirectory,
                "license-private.pem");

        var publicKeyPath =
            Path.Combine(
                keyDirectory,
                "license-public.pem");

        using var rsa =
            RSA.Create(3072);

        File.WriteAllText(
            privateKeyPath,
            rsa.ExportRSAPrivateKeyPem());

        File.WriteAllText(
            publicKeyPath,
            rsa.ExportRSAPublicKeyPem());

        Console.WriteLine(
            $"Private key: {Path.GetFullPath(privateKeyPath)}");

        Console.WriteLine(
            $"Public key:  {Path.GetFullPath(publicKeyPath)}");

        Console.WriteLine();
        Console.WriteLine(
            "IMPORTANT: Never deploy the private key with the MVC application.");

        return 0;
    }

    private static int IssueLicense(string[] args)
    {
        if (args.Length != 8)
        {
            Console.WriteLine(
                "Usage:");

            Console.WriteLine(
                "issue <privateKeyPath> <outputPath> " +
                "<licenseId> <customerId> <licenseType> " +
                "<validFromUtc> <validUntilUtc>");

            return 1;
        }

        var privateKeyPath = args[1];
        var outputPath = args[2];
        var licenseId = args[3];
        var customerId = args[4];
        var licenseType = args[5];

        var validFrom =
            DateTimeOffset.Parse(args[6])
                .ToUniversalTime();

        var validUntil =
            DateTimeOffset.Parse(args[7])
                .ToUniversalTime();

        if (validUntil <= validFrom)
        {
            throw new ArgumentException(
                "ValidUntilUtc must be later than ValidFromUtc.");
        }

        if (!string.Equals(
                licenseType,
                "Trial",
                StringComparison.OrdinalIgnoreCase)
            &&
            !string.Equals(
                licenseType,
                "Annual",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "LicenseType must be Trial or Annual.");
        }

        if (!File.Exists(privateKeyPath))
        {
            throw new FileNotFoundException(
                "Private key not found.",
                privateKeyPath);
        }

        var issuedAt =
            DateTimeOffset.UtcNow;

        var payload = new LicensePayload
        {
            LicenseId = licenseId,
            CustomerId = customerId,
            LicenseType = licenseType,
            IssuedAtUtc = issuedAt,
            ValidFromUtc = validFrom,
            ValidUntilUtc = validUntil
        };

        var payloadJson =
            JsonSerializer.Serialize(
                payload,
                JsonOptions);

        var payloadBytes =
            Encoding.UTF8.GetBytes(
                payloadJson);

        byte[] signature;

        using (var rsa = RSA.Create())
        {
            rsa.ImportFromPem(
                File.ReadAllText(privateKeyPath));

            signature =
                rsa.SignData(
                    payloadBytes,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);
        }

        var signatureBase64 =
            Convert.ToBase64String(signature);

        var licenseDocument =
            "{\n" +
            "  \"license\": " +
            payloadJson +
            ",\n" +
            "  \"signature\": " +
            JsonSerializer.Serialize(
                signatureBase64) +
            "\n}";

        var directory =
            Path.GetDirectoryName(
                Path.GetFullPath(outputPath));

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(
            outputPath,
            licenseDocument);

        Console.WriteLine(
            $"License created: {Path.GetFullPath(outputPath)}");

        Console.WriteLine(
            $"Customer: {customerId}");

        Console.WriteLine(
            $"Type: {licenseType}");

        Console.WriteLine(
            $"Valid from: {validFrom}");

        Console.WriteLine(
            $"Valid until: {validUntil}");

        return 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine(
            "MVP_1B2 License Issuer");

        Console.WriteLine();

        Console.WriteLine(
            "Generate RSA keys:");

        Console.WriteLine(
            "  dotnet run -- generate-key LicensingKeys");

        Console.WriteLine();

        Console.WriteLine(
            "Issue a license:");

        Console.WriteLine(
            "  dotnet run -- issue " +
            "<privateKeyPath> <outputPath> " +
            "<licenseId> <customerId> <licenseType> " +
            "<validFromUtc> <validUntilUtc>");
    }
}
