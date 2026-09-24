using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVP_1B2;
using MVP_1B2.Models;

namespace MVP_1B2.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ClientContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmployeesController(ClientContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //My Profile Action
        [Authorize(Roles = "Employee,Manager")]
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || user.EmployeeID == null)
            {
                return Forbid();
            }

            var employee = await _context.Employees
                .Include(e => e.Service)
                .FirstOrDefaultAsync(e => e.ID == user.EmployeeID);

            if (employee == null)
            {
                return NotFound();
            }

            return View("Details", employee);
        }

        // GET: Employees
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Index()
        {
            var clientContext = _context.Employees.Include(e => e.Service);
            return View(await clientContext.ToListAsync());
        }

        // GET: Employees/Details/5
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Service)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (employee == null)
            {
                return NotFound();
            }
            
            return View(employee);
        }

        // GET: Employees/Create
        [Authorize(Roles = "Administrator,Manager")]
        public IActionResult Create()
        {
            ViewData["ServiceID"] = new SelectList(_context.Services, "ID", "Name");
            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
           [Bind("Salary,ServiceID,ID,Name,Address,Email,PhoneNumber")]
    Employee employee)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ServiceID"] =
                    new SelectList(
                        _context.Services,
                        "ID",
                        "Name",
                        employee.ServiceID);

                return View(employee);
            }

            var existingUser =
                await _userManager.FindByEmailAsync(employee.Email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");

                ViewData["ServiceID"] =
                    new SelectList(
                        _context.Services,
                        "ID",
                        "Name",
                        employee.ServiceID);

                return View(employee);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Create Employee
                employee.ID = Guid.NewGuid();

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // 2. Create Identity user
                var user = new ApplicationUser
                {
                    UserName = employee.Email,
                    Email = employee.Email,
                    EmployeeID = employee.ID,
                    PhoneNumber = employee.PhoneNumber
                };

                var result = await _userManager.CreateAsync(
                    user,
                    "TempPassword123!");

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(
                            "",
                            error.Description);
                    }

                    await transaction.RollbackAsync();

                    ViewData["ServiceID"] =
                        new SelectList(
                            _context.Services,
                            "ID",
                            "Name",
                            employee.ServiceID);

                    return View(employee);
                }

                // 3. Assign Employee role
                var roleResult =
                    await _userManager.AddToRoleAsync(
                        user,
                        "Employee");

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            "",
                            error.Description);
                    }

                    await transaction.RollbackAsync();

                    ViewData["ServiceID"] =
                        new SelectList(
                            _context.Services,
                            "ID",
                            "Name",
                            employee.ServiceID);

                    return View(employee);
                }

                // 4. Everything succeeded
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        // GET: Employees/Edit/5
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Able to delete service
            var employee = await _context.Employees
                .Include(e => e.Service)
                .FirstOrDefaultAsync(e => e.ID == id);


            if (employee == null)
            {
                return NotFound();
            }

            ViewData["Services"] = new SelectList(_context.Services, "ID", "Name", employee.ServiceID);

            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
           Guid id,
           [Bind("Salary,ServiceID,ID,Name,Address,Email,PhoneNumber")] Employee employee,
           bool UnselectService)
        {
            if (id != employee.ID)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["Services"] =
                    new SelectList(
                        _context.Services,
                        "ID",
                        "Name",
                        employee.ServiceID);

                return View(employee);
            }

            try
            {
                if (UnselectService)
                {
                    employee.ServiceID = null;
                }

                // Update Employee
                _context.Update(employee);
                await _context.SaveChangesAsync();

                // Find the Identity account associated with this employee
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.EmployeeID == employee.ID);

                if (user != null)
                {
                    user.PhoneNumber = employee.PhoneNumber;
                    user.Email = employee.Email;
                    user.UserName = employee.Email;

                    await _userManager.UpdateAsync(user);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Employees.Any(e => e.ID == employee.ID))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        /// POST: Employees/Delete/5
        [Authorize(Roles = "Administrator,Manager")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            // Find the ASP.NET Identity account associated with this employee
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.EmployeeID == employee.ID);

            // Delete the Identity account first
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View("Delete", employee);
                }
            }

            // Now delete the Employee
            _context.Employees.Remove(employee);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PromoteToManager(Guid employeeId)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.ID == employeeId);

            if (employee == null)
            {
                return NotFound();
            }

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.EmployeeID == employeeId);

            if (user == null)
            {
                TempData["Error"] =
                    "This employee does not have an account.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = employeeId });
            }

            if (await _userManager.IsInRoleAsync(user, "Manager"))
            {
                TempData["Error"] =
                    "This employee is already a Manager.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = employeeId });
            }

            await _userManager.RemoveFromRoleAsync(user, "Employee");
            await _userManager.AddToRoleAsync(user, "Manager");

            TempData["Success"] =
                "Employee has been promoted to Manager.";

            return RedirectToAction(
                nameof(Details),
                new { id = employeeId });
        }
        // GET: Employees/Delete/5
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Service)
                .FirstOrDefaultAsync(e => e.ID == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }
        private bool EmployeeExists(Guid id)
        {
            return _context.Employees.Any(e => e.ID == id);
        }
    }
}
