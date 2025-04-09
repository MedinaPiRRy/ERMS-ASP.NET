using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ERMS.Data;
using ERMS.Models;
using ERMS.Services;
using Microsoft.AspNetCore.Identity;

namespace ERMS.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly IEmployeeApiService _api;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(IEmployeeApiService api, ApplicationDbContext context, ILogger<EmployeesController> logger)
        {
            _api = api;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _api.GetAllAsync(); // Fetch all employees from the API

            _logger.LogInformation("User accessed index page for Employees");
            return View(employees);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                _logger.LogError("User attempted to access Details page for Employees with null ID");
                return NotFound();
            }

            var employee = await _api.GetByIdAsync(id.Value); // Fetch employee by ID from the API
            if (employee == null) 
            {
                _logger.LogError("User attempted to access Details page for Employees with invalid ID: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("User accessed Details page for Employees");
            return View(employee);
        }

        public async Task<IActionResult> Create()
        {
            await LoadEmployeeDropdowns(); // Load dropdowns for managers and roles

            _logger.LogInformation("User accessed Create page for Employees");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!ModelState.IsValid) { 
                await LoadEmployeeDropdowns(employee.Manager, employee.Role);
                _logger.LogInformation("User accessed Create page for Employees with invalid model state");
                return View(employee);
            }

            if (!User.IsInRole("Manager") || User.IsInRole("Admin"))
            {
                _logger.LogError("User attempted to create an employee without proper authorization");
                return Forbid(); // Prevent unauthorized access
            }

            if (string.IsNullOrEmpty(employee.Manager))
            {
                employee.Manager = "Unassigned"; // Set manager to Unassigned if manager was not sellected
            }

            var success = await _api.CreateAsync(employee); // Create employee via API
            if (!success) 
            {
                ModelState.AddModelError("", "Error creating employee.");
                _logger.LogError("Error creating employee: {Employee}", employee);
                return View();
            };

            _logger.LogInformation("User created a new employee: {Employee}", employee);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) 
            {
                _logger.LogError("User attempted to edit an employee with null ID");
                return NotFound();
            };

            var employee = await _api.GetByIdAsync(id.Value); // Fetch employee by ID from the API
            if (employee == null) 
            {
                _logger.LogError("User attempted to edit an employee that does not exist: {Id}", id);
                return NotFound();
            };

            await LoadEmployeeDropdowns(employee.Manager, employee.Role);
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id) 
            {
                _logger.LogError("User attempted to edit an employee with mismatched ID: {Id}", id);
                return NotFound();
            }
            // Check if the user is in the Manager role
            if (!User.IsInRole("Manager") || User.IsInRole("Admin"))
            {
                _logger.LogError("User attempted to edit an employee without proper authorization");
                return Forbid(); // Prevent unauthorized access
            }

            if (!ModelState.IsValid) {
                _logger.LogInformation("User accessed Edit page for Employees with invalid model state");
                await LoadEmployeeDropdowns(employee.Manager, employee.Role);
                return View(employee);
            }

            if (string.IsNullOrEmpty(employee.Manager))
            {
                employee.Manager = "Unassigned"; // Set manager to Unassigned if manager was not sellected
            }

            var success = await _api.UpdateAsync(employee); // Update employee via API
            if (!success) 
            {
                _logger.LogError("Error updating employee: {Employee}", employee);
                ModelState.AddModelError("", "Error updating employee.");
            }

            _logger.LogInformation($"User created employee successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) 
            {
                _logger.LogError("User attempted to delete an employee with null ID");
                return NotFound();
            }

            var employee = await _api.GetByIdAsync(id.Value); // Fetch employee by ID from the API
            if (employee == null) 
            {
                _logger.LogError("User attempted to delete an employee that does not exist: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("User accessed Delete page for Employees");
            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.IsInRole("Manager") || User.IsInRole("Admin"))
            {
                _logger.LogError("User attempted to delete an employee without proper authorization");
                return Forbid(); // Prevent unauthorized access
            }

            await _api.DeleteAsync(id); // Delete employee via API
            return RedirectToAction(nameof(Index));
        }

        // This method is used to load the dropdowns for managers and roles.
        // Managers will come from all Employees with the role of "Manager".
        // Roles are hardcoded for simplicity, however, they follow the SeededRoles in Program.cs.
        private async Task LoadEmployeeDropdowns(string selectedManager = null, string selectedRole = null)
        {
            var managers = await _context.Employees
                .Where(e => e.Role == "Manager")
                .Select(e => e.FullName)
                .ToListAsync();

            managers.Insert(0, "Unassigned");

            ViewBag.Managers = new SelectList(managers, selectedManager);
            ViewBag.Roles = new SelectList(new[] { "Employee", "Manager", "Admin" }, selectedRole);
        }


        //private readonly ApplicationDbContext _context;
        //private readonly EmployeeApiService _api;

        //public EmployeesController(ApplicationDbContext context, EmployeeApiService api)
        //{
        //    _context = context;
        //    _api = api;
        //}

        //// GET: Employees
        //public async Task<IActionResult> Index()
        //{
        //    var data = await _api.GetAllAsync();
        //    return View(data);
        //}

        //// GET: Employees/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var employee = await _context.Employees
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (employee == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(employee);
        //}

        //// GET: Employees/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: Employees/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,FullName,Email,Role,Manager")] Employee employee)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(employee);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(employee);
        //}

        //// GET: Employees/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var employee = await _context.Employees.FindAsync(id);
        //    if (employee == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(employee);
        //}

        //// POST: Employees/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,FullName,Email,Role,Manager")] Employee employee)
        //{
        //    if (id != employee.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(employee);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!EmployeeExists(employee.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(employee);
        //}

        //// GET: Employees/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var employee = await _context.Employees
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (employee == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(employee);
        //}

        //// POST: Employees/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var employee = await _context.Employees.FindAsync(id);
        //    if (employee != null)
        //    {
        //        _context.Employees.Remove(employee);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool EmployeeExists(int id)
        //{
        //    return _context.Employees.Any(e => e.Id == id);
        //}
    }
}
