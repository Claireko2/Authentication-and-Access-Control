using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVP_1B2.Models;
using MVP_1B2.Services;

namespace MVP_1B2.Controllers
{
    [Authorize]
    public class TwoFactorController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITwoFactorService _twoFactorService;

        public TwoFactorController(
            UserManager<ApplicationUser> userManager,
            ITwoFactorService twoFactorService)
        {
            _userManager = userManager;
            _twoFactorService = twoFactorService;
        }

        //Get Security View
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        //Send Code
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode()
        {
            Console.WriteLine("===== SendCode() STARTED =====");

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Console.WriteLine("===== USER IS NULL =====");
                return Challenge();
            }

            Console.WriteLine($"User: {user.Email}");
            Console.WriteLine($"Phone: {user.PhoneNumber}");

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                Console.WriteLine("===== PHONE NUMBER IS EMPTY =====");

                TempData["Error"] =
                    "No phone number is associated with this account.";

                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("===== CALLING TWILIO SERVICE =====");

            await _twoFactorService.SendCodeAsync(
                user.PhoneNumber);

            Console.WriteLine("===== TWILIO SERVICE COMPLETED =====");

            TempData["Success"] =
                "A verification code has been sent to your phone.";

            return RedirectToAction(nameof(Verify));
        }
        //Return View
        [HttpGet]
        public IActionResult Verify()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(string code)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Challenge();
            }

            if (string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                return BadRequest(
                    "No phone number is associated with this account.");
            }

            var valid =
                await _twoFactorService.VerifyCodeAsync(
                    user.PhoneNumber,
                    code);

            if (!valid)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid or expired verification code.");

                return View();
            }

            user.PhoneNumberConfirmed = true;
            user.TwoFactorEnabled = true;

            await _userManager.UpdateAsync(user);

            TempData["Success"] =
                "SMS two-factor authentication has been enabled.";

            return RedirectToAction(
                "Index",
                "Home");
        }



    }



}
