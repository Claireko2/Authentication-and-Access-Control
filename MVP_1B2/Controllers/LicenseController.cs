using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVP_1B2.Services;

namespace MVP_1B2.Controllers
{
    [AllowAnonymous]
    public class LicenseController : Controller
    {
        private readonly ILicenseService _licenseService;

        public LicenseController(
            ILicenseService licenseService)
        {
            _licenseService = licenseService;
        }

        [HttpGet]
        public IActionResult Status()
        {
            var result =
                _licenseService.Validate();

            return View(result);
        }
    }
}
