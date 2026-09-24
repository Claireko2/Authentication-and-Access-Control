using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVP_1B2.Models;
using MVP_1B2.ViewModels;

namespace MVP_1B2.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .Include(u => u.Employee)
                .Include(u => u.Client)
                .OrderBy(u => u.Email)
                .ToListAsync();

            var result = new List<AdminUserViewModel>();

            foreach (var user in users)
            {
                var roles =
                    await _userManager.GetRolesAsync(user);

                result.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    ClientID = user.ClientID,
                    EmployeeID = user.EmployeeID,
                    EmployeeName = user.Employee?.Name ?? "",
                    Roles = roles.ToList()
                });
            }

            return View(result);
        }
    }
}
