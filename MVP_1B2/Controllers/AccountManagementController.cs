using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP_1B2.Models;

namespace MVP_1B2.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AccountManagementController : Controller
    {
        private readonly ClientContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountManagementController(
            ClientContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClientAccount(Guid clientId)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ID == clientId);

            if (client == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(client.Email))
            {
                TempData["Error"] =
                    "This client does not have an email address.";

                return RedirectToAction(
                    "Details",
                    "Clients",
                    new { id = clientId });
            }

            // Check whether this client already has an account
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.ClientID == clientId);

            if (existingUser != null)
            {
                TempData["Error"] =
                    "This client already has an account.";

                return RedirectToAction(
                    "Details",
                    "Clients",
                    new { id = clientId });
            }

            // Check whether the email is already used
            var existingEmailUser =
                await _userManager.FindByEmailAsync(client.Email);

            if (existingEmailUser != null)
            {
                TempData["Error"] =
                    "This email address is already associated with an account.";

                return RedirectToAction(
                    "Details",
                    "Clients",
                    new { id = clientId });
            }

            var temporaryPassword = "TempPassword123!";

            var user = new ApplicationUser
            {
                UserName = client.Email,
                Email = client.Email,
                EmailConfirmed = false,
                ClientID = client.ID,
                PhoneNumber = client.PhoneNumber
            };

            var result = await _userManager.CreateAsync(
                user,
                temporaryPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return RedirectToAction(
                    "Details",
                    "Clients",
                    new { id = clientId });
            }

            await _userManager.AddToRoleAsync(
                user,
                "Client");

            TempData["Success"] =
                $"Client account created. Temporary password: {temporaryPassword}";

            return RedirectToAction(
                "Details",
                "Clients",
                new { id = clientId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployeeAccount(Guid employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.ID == employeeId);

            if (employee == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                TempData["Error"] =
                    "This employee does not have an email address.";

                return RedirectToAction(
                    "Details",
                    "Employees",
                    new { id = employeeId });
            }

            // Check whether employee already has an account
            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.EmployeeID == employeeId);

            if (existingUser != null)
            {
                TempData["Error"] =
                    "This employee already has an account.";

                return RedirectToAction(
                    "Details",
                    "Employees",
                    new { id = employeeId });
            }

            // Check email
            var existingEmailUser =
                await _userManager.FindByEmailAsync(employee.Email);

            if (existingEmailUser != null)
            {
                TempData["Error"] =
                    "This email address is already associated with an account.";

                return RedirectToAction(
                    "Details",
                    "Employees",
                    new { id = employeeId });
            }

            var temporaryPassword = "TempPassword123!";

            var user = new ApplicationUser
            {
                UserName = employee.Email,
                Email = employee.Email,
                EmailConfirmed = false,
                EmployeeID = employee.ID,
                PhoneNumber = employee.PhoneNumber
            };

            var result = await _userManager.CreateAsync(
                user,
                temporaryPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return RedirectToAction(
                    "Details",
                    "Employees",
                    new { id = employeeId });
            }

            await _userManager.AddToRoleAsync(
                user,
                "Employee");

            TempData["Success"] =
                $"Employee account created. Temporary password: {temporaryPassword}";

            return RedirectToAction(
                "Details",
                "Employees",
                new { id = employeeId });
        }
    }
}