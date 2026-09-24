
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MVP_1B2.Models;

namespace MVP_1B2.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginWith2faModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public bool RememberMe { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter the verification code.")]
            public string Code { get; set; } = string.Empty;

            public bool RememberMachine { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Read RememberMe from the query string.
            RememberMe = GetRememberMeFromQuery();

            var user =
                await _signInManager
                    .GetTwoFactorAuthenticationUserAsync();
            Console.WriteLine(
            $"===== 2FA USER: {(user == null ? "NULL" : user.Email)} =====");

            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                ModelState.AddModelError(
                    "",
                    "No phone number is associated with this account.");

                return Page();
            }

            try
            {
                await _userManager.GenerateTwoFactorTokenAsync(
                    user,
                    "TwilioSms");
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== SMS SEND ERROR =====");
                Console.WriteLine(ex.ToString());

                ModelState.AddModelError(
                    "",
                    "We could not send the verification code. Please try again.");

                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("===== LoginWith2fa POST reached =====");
            Console.WriteLine($"Entered code: '{Input.Code}'");
            Console.WriteLine($"Code length: {Input.Code?.Length}");

            RememberMe = GetRememberMeFromQuery();

            Console.WriteLine($"RememberMe: {RememberMe}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("===== MODEL STATE INVALID =====");

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine(
                            $"Field: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }

                return Page();
            }

            var user =
                await _signInManager
                    .GetTwoFactorAuthenticationUserAsync();

            Console.WriteLine(
                $"===== 2FA USER: {(user == null ? "NULL" : user.Email)} =====");

            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                ModelState.AddModelError(
                    "",
                    "No phone number is associated with this account.");

                return Page();
            }

            try
            {
                Console.WriteLine(
                    "===== Calling TwoFactorSignInAsync =====");

                var result =
                    await _signInManager.TwoFactorSignInAsync(
                        "TwilioSms",
                        Input.Code.Trim(),
                        RememberMe,
                        Input.RememberMachine);

                Console.WriteLine("===== Identity 2FA Result =====");
                Console.WriteLine($"Succeeded: {result.Succeeded}");
                Console.WriteLine($"IsNotAllowed: {result.IsNotAllowed}");
                Console.WriteLine($"IsLockedOut: {result.IsLockedOut}");
                Console.WriteLine($"RequiresTwoFactor: {result.RequiresTwoFactor}");

                if (result.Succeeded)
                {
                    Console.WriteLine("===== 2FA SUCCESS =====");

                    return LocalRedirect(
                        ReturnUrl ?? Url.Content("~/"));
                }

                if (result.IsLockedOut)
                {
                    Console.WriteLine("===== 2FA LOCKED OUT =====");

                    return RedirectToPage("./Lockout");
                }

                ModelState.AddModelError(
                    "",
                    "The verification code is invalid or has expired.");

                return Page();
            }
            catch (Exception ex)
            {
                Console.WriteLine("===== 2FA EXCEPTION =====");
                Console.WriteLine(ex.ToString());

                ModelState.AddModelError(
                    "",
                    "An error occurred while verifying the code.");

                return Page();
            }
        }

        private bool GetRememberMeFromQuery()
        {
            var value = Request.Query["RememberMe"].ToString();

            return bool.TryParse(value, out var rememberMe)
                && rememberMe;
        }
    }
}

