using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MVP_1B2.Data;
using MVP_1B2.Models;

namespace MVP_1B2.Areas.Identity.Pages.Account
{
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ClientContext _context;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ClientContext context,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? LoginProvider { get; set; }

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            public string? Email { get; set; }
        }

        // ------------------------------------------------------------
        // Initial external-login request
        // ------------------------------------------------------------

        public IActionResult OnGet()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(
            string provider,
            string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                return RedirectToPage(
                    "./Login",
                    new { ReturnUrl = returnUrl });
            }

            _logger.LogInformation(
                "Starting external login using provider {Provider}.",
                provider);

            var redirectUrl =
                Url.Page(
                    "./ExternalLogin",
                    pageHandler: "Callback",
                    values: new
                    {
                        returnUrl
                    });

            var properties =
                _signInManager
                    .ConfigureExternalAuthenticationProperties(
                        provider,
                        redirectUrl!);

            return new ChallengeResult(
                provider,
                properties);
        }

        // ------------------------------------------------------------
        // Microsoft / OpenID Connect callback
        // ------------------------------------------------------------

        public async Task<IActionResult> OnGetCallbackAsync(
            string? returnUrl = null,
            string? remoteError = null)
        {
            _logger.LogInformation(
                "===== ExternalLogin Callback reached =====");

            if (!string.IsNullOrWhiteSpace(remoteError))
            {
                _logger.LogWarning(
                    "External provider error: {RemoteError}",
                    remoteError);

                ModelState.AddModelError(
                    string.Empty,
                    $"External login error: {remoteError}");

                return RedirectToPage("./Login");
            }

            // Get Microsoft login information from Identity's
            // external authentication cookie.
            var info =
                await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                _logger.LogWarning(
                    "External login information could not be loaded.");

                return RedirectToPage("./Login");
            }

            _logger.LogInformation(
                "Login provider: {Provider}",
                info.LoginProvider);

            _logger.LogInformation(
                "Provider key: {ProviderKey}",
                info.ProviderKey);

            // --------------------------------------------------------
            // Get Microsoft email
            // --------------------------------------------------------

            var email =
                info.Principal.FindFirstValue(ClaimTypes.Email)
                ?? info.Principal.FindFirstValue(
                    "preferred_username");

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning(
                    "Microsoft account did not provide an email.");

                ModelState.AddModelError(
                    string.Empty,
                    "Microsoft did not provide an email address.");

                return RedirectToPage("./Login");
            }

            email = email.Trim();

            _logger.LogInformation(
                "Microsoft email: {Email}",
                email);

            // --------------------------------------------------------
            // 1. Administrator must have already created Client
            // --------------------------------------------------------

            var client =
                await _context.Clients
                    .FirstOrDefaultAsync(
                        c => c.Email == email);

            if (client == null)
            {
                _logger.LogWarning(
                    "No Client found for {Email}.",
                    email);

                ModelState.AddModelError(
                    string.Empty,
                    "No Client account exists for this Microsoft account. " +
                    "Please contact the administrator.");

                return RedirectToPage("./Login");
            }

            _logger.LogInformation(
                "Client found: {ClientId}",
                client.ID);

            // --------------------------------------------------------
            // 2. Find existing ApplicationUser
            // --------------------------------------------------------

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning(
                    "No ApplicationUser found for {Email}.",
                    email);

                ModelState.AddModelError(
                    string.Empty,
                    "Your Client login account has not been configured. " +
                    "Please contact the administrator.");

                return RedirectToPage("./Login");
            }

            _logger.LogInformation(
                "ApplicationUser found: {UserId}",
                user.Id);

            // --------------------------------------------------------
            // 3. Verify Client mapping
            // --------------------------------------------------------

            if (user.ClientID == null)
            {
                user.ClientID = client.ID;

                if (!string.IsNullOrWhiteSpace(
                    client.PhoneNumber))
                {
                    user.PhoneNumber =
                        client.PhoneNumber;
                }

                var updateResult =
                    await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        _logger.LogError(
                            "ApplicationUser update error: {Code} - {Description}",
                            error.Code,
                            error.Description);
                    }

                    return RedirectToPage("./Login");
                }

                _logger.LogInformation(
                    "ApplicationUser {UserId} linked to Client {ClientId}.",
                    user.Id,
                    client.ID);
            }
            else if (user.ClientID != client.ID)
            {
                _logger.LogError(
                    "Client mismatch. User ClientID={UserClientId}, " +
                    "actual Client={ClientId}.",
                    user.ClientID,
                    client.ID);

                ModelState.AddModelError(
                    string.Empty,
                    "Your Microsoft account does not match the configured Client account.");

                return RedirectToPage("./Login");
            }

            // --------------------------------------------------------
            // 4. Copy phone number for Twilio 2FA
            // --------------------------------------------------------

            if (user.PhoneNumber != client.PhoneNumber)
            {
                user.PhoneNumber = client.PhoneNumber;

                var phoneUpdateResult =
                    await _userManager.UpdateAsync(user);

                if (!phoneUpdateResult.Succeeded)
                {
                    foreach (var error in phoneUpdateResult.Errors)
                    {
                        _logger.LogError(
                            "Phone update error: {Code} - {Description}",
                            error.Code,
                            error.Description);
                    }

                    return RedirectToPage("./Login");
                }
            }

            // --------------------------------------------------------
            // 5. Make sure Client role exists
            // --------------------------------------------------------

            if (!await _userManager.IsInRoleAsync(
                user,
                "Client"))
            {
                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        "Client");

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        _logger.LogError(
                            "Role error: {Code} - {Description}",
                            error.Code,
                            error.Description);
                    }

                    return RedirectToPage("./Login");
                }
            }

            // --------------------------------------------------------
            // 6. Check whether Microsoft login is already linked
            // --------------------------------------------------------

            var linkedUser =
                await _userManager.FindByLoginAsync(
                    info.LoginProvider,
                    info.ProviderKey);

            if (linkedUser != null &&
                linkedUser.Id != user.Id)
            {
                _logger.LogError(
                    "Microsoft login is already linked to another user.");

                ModelState.AddModelError(
                    string.Empty,
                    "This Microsoft account is already linked to another account.");

                return RedirectToPage("./Login");
            }

            // --------------------------------------------------------
            // 7. Link Microsoft to existing ApplicationUser
            // --------------------------------------------------------

            if (linkedUser == null)
            {
                _logger.LogInformation(
                    "Adding Microsoft login to ApplicationUser {UserId}.",
                    user.Id);

                var addLoginResult =
                    await _userManager.AddLoginAsync(
                        user,
                        info);

                if (!addLoginResult.Succeeded)
                {
                    foreach (var error in addLoginResult.Errors)
                    {
                        _logger.LogError(
                            "AddLogin error: {Code} - {Description}",
                            error.Code,
                            error.Description);
                    }

                    return RedirectToPage("./Login");
                }
            }

            // --------------------------------------------------------
            // 8. Sign in through Identity
            // --------------------------------------------------------

            var result =
                await _signInManager.ExternalLoginSignInAsync(
                    info.LoginProvider,
                    info.ProviderKey,
                    isPersistent: false,
                    bypassTwoFactor: false);

            _logger.LogInformation(
                "External sign-in result: " +
                "Succeeded={Succeeded}, " +
                "RequiresTwoFactor={RequiresTwoFactor}, " +
                "IsLockedOut={IsLockedOut}",
                result.Succeeded,
                result.RequiresTwoFactor,
                result.IsLockedOut,
                result.IsNotAllowed);

            if (result.Succeeded)
            {
                return LocalRedirect(
                    returnUrl ?? Url.Content("~/"));
            }

            // --------------------------------------------------------
            // 9. Twilio 2FA
            // --------------------------------------------------------

            if (result.RequiresTwoFactor)
            {
                _logger.LogInformation(
                    "External login requires 2FA for {Email}.",
                    email);

                return RedirectToPage(
                    "/Account/LoginWith2fa",
                    new
                    {
                        area = "Identity",
                        ReturnUrl = returnUrl,
                        RememberMe = false
                    });
            }

            // --------------------------------------------------------
            // 10. Lockout
            // --------------------------------------------------------

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }

            _logger.LogWarning(
                "External sign-in failed for {Email}.",
                email);

            return RedirectToPage("./Login");
        }
    }
}
