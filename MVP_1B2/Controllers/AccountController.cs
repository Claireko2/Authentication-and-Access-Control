using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP_1B2.Data;
using MVP_1B2.Models;

namespace MVP_1B2.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ClientContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ClientContext context,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult LoginWithMicrosoft(string? returnUrl = "/")
        {
            Console.WriteLine("===== LoginWithMicrosoft CALLED =====");

            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(
                    returnUrl ?? Url.Content("~/"));
            }

            var redirectUrl = Url.Action(
                nameof(MicrosoftCallback),
                "Account",
                new { returnUrl });

            var properties =
                _signInManager.ConfigureExternalAuthenticationProperties(
                    "AzureAD",
                    redirectUrl!);

            return Challenge(properties, "AzureAD");
        }

        [AllowAnonymous]
        public async Task<IActionResult> MicrosoftCallback(
            string? returnUrl = "/")
        {
            Console.WriteLine("===== MicrosoftCallback CALLED =====");

            var info =
                await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                Console.WriteLine("===== External login info is NULL =====");

                return RedirectToPage(
                    "/Account/Login",
                    new
                    {
                        area = "Identity",
                        ReturnUrl = returnUrl
                    });
            }

            var email =
                info.Principal.FindFirstValue(ClaimTypes.Email)
                ?? info.Principal.FindFirstValue("preferred_username");

            Console.WriteLine($"Microsoft email = {email}");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToPage(
                    "/Account/Login",
                    new
                    {
                        area = "Identity",
                        ReturnUrl = returnUrl
                    });
            }

            email = email.Trim();

            // --------------------------------------------------
            // Find existing Client
            // --------------------------------------------------

            var client =
                await _context.Clients
                    .FirstOrDefaultAsync(c => c.Email == email);

            if (client == null)
            {
                Console.WriteLine("===== Client NOT FOUND =====");

                return RedirectToPage(
                    "/Account/Login",
                    new
                    {
                        area = "Identity",
                        ReturnUrl = returnUrl
                    });
            }

            Console.WriteLine($"Client found: {client.ID}");

            // --------------------------------------------------
            // Find EXISTING ApplicationUser
            // --------------------------------------------------

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                Console.WriteLine(
                    "===== ApplicationUser NOT FOUND =====");

                return RedirectToPage(
                    "/Account/Login",
                    new
                    {
                        area = "Identity",
                        ReturnUrl = returnUrl
                    });
            }

            Console.WriteLine(
                $"ApplicationUser found: {user.Id}");

            // --------------------------------------------------
            // Verify Client mapping
            // --------------------------------------------------

            if (user.ClientID != client.ID)
            {
                user.ClientID = client.ID;
                user.PhoneNumber = client.PhoneNumber;

                var updateResult =
                    await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        Console.WriteLine(
                            $"Update error: {error.Description}");
                    }

                    return RedirectToPage(
                        "/Account/Login",
                        new
                        {
                            area = "Identity",
                            ReturnUrl = returnUrl
                        });
                }
            }

            // --------------------------------------------------
            // Ensure Client role
            // --------------------------------------------------

            if (!await _userManager.IsInRoleAsync(user, "Client"))
            {
                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        "Client");

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        Console.WriteLine(
                            $"Role error: {error.Description}");
                    }

                    return RedirectToPage(
                        "/Account/Login",
                        new
                        {
                            area = "Identity",
                            ReturnUrl = returnUrl
                        });
                }
            }

            // --------------------------------------------------
            // Check whether Microsoft login already exists
            // --------------------------------------------------

            var linkedUser =
                await _userManager.FindByLoginAsync(
                    info.LoginProvider,
                    info.ProviderKey);

            if (linkedUser != null &&
                linkedUser.Id != user.Id)
            {
                Console.WriteLine(
                    "===== Microsoft login linked to DIFFERENT user =====");

                return RedirectToPage(
                    "/Account/Login",
                    new
                    {
                        area = "Identity",
                        ReturnUrl = returnUrl
                    });
            }

            // --------------------------------------------------
            // First Microsoft login: link to existing user
            // --------------------------------------------------

            if (linkedUser == null)
            {
                Console.WriteLine(
                    "===== Adding Microsoft login =====");

                var addLoginResult =
                    await _userManager.AddLoginAsync(
                        user,
                        info);

                if (!addLoginResult.Succeeded)
                {
                    foreach (var error in addLoginResult.Errors)
                    {
                        Console.WriteLine(
                            $"AddLogin error: {error.Code} - {error.Description}");
                    }

                    return RedirectToPage(
                        "/Account/Login",
                        new
                        {
                            area = "Identity",
                            ReturnUrl = returnUrl
                        });
                }
            }

            // --------------------------------------------------
            // Sign in using the linked Microsoft login
            // --------------------------------------------------

            Console.WriteLine(
                "===== Calling ExternalLoginSignInAsync =====");

            var signInResult =
                await _signInManager.ExternalLoginSignInAsync(
                    info.LoginProvider,
                    info.ProviderKey,
                    isPersistent: true,
                    bypassTwoFactor: false);

            Console.WriteLine(
                $"Succeeded={signInResult.Succeeded}, " +
                $"RequiresTwoFactor={signInResult.RequiresTwoFactor}, " +
                $"LockedOut={signInResult.IsLockedOut}");

            if (signInResult.Succeeded)
            {
                return LocalRedirect(
                    returnUrl ?? Url.Content("~/"));
            }

            if (signInResult.RequiresTwoFactor)
            {
                return RedirectToPage(
                    "/Account/LoginWith2fa",
                    new
                    {
                        area = "Identity",
                        ReturnUrl = returnUrl,
                        RememberMe = true
                    });
            }

            if (signInResult.IsLockedOut)
            {
                return RedirectToPage("/Account/Lockout");
            }

            return RedirectToPage(
                "/Account/Login",
                new
                {
                    area = "Identity",
                    ReturnUrl = returnUrl
                });
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home");
        }
    }
}