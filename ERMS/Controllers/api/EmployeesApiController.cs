using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ERMS.Data;
using ERMS.Models;
using Microsoft.AspNetCore.Identity;

namespace ERMS.Controllers.api
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesApiController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeesApiController> _logger;

        public EmployeesApiController(ApplicationDbContext context, ILogger<EmployeesApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            _logger.LogInformation("[API-Employees] Fetching all employees");
            return await _context.Employees.ToListAsync(); // Fetch all employees
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id); // Fetch employee by ID
            if (employee == null)
            {
                _logger.LogWarning($"[API-Employees] Employee with ID {id} not found");
                return NotFound();
            }
            _logger.LogInformation($"[API-Employees] Employee with ID {id} found: {employee.FullName}");
            return employee;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
        {
            var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

            // Check if user already exists
            var identityUser = await userManager.FindByEmailAsync(employee.Email);
            if (identityUser != null)
            {
                _logger.LogWarning($"[API-Employees] User with email {employee.Email} already exists.");
                return Conflict("User already exists.");
            }

            // Create Identity user
            identityUser = new IdentityUser { UserName = employee.Email, Email = employee.Email };
            var password = "Default123!"; // Default Password
            var result = await userManager.CreateAsync(identityUser, password);

            if (!result.Succeeded)
            {
                _logger.LogWarning($"[API-Employees] User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                return BadRequest(result.Errors);
            }

            // Ensure role exists
            if (!await roleManager.RoleExistsAsync(employee.Role))
            {
                _logger.LogWarning($"[API-Employees] Role '{employee.Role}' does not exist. Creating it.");
                await roleManager.CreateAsync(new IdentityRole(employee.Role));
            }

            // Add to role
            await userManager.AddToRoleAsync(identityUser, employee.Role);

            // Save employee info
            employee.IdentityUserId = identityUser.Id;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"[API-Employees] Employee created: {employee.FullName}, Role: {employee.Role}, Manager: {employee.Manager}");
            return CreatedAtAction("GetEmployee", new { id = employee.Id }, employee);
        }

        [HttpPut("{id}")]
        //[Authorize(Roles = "Manager")]
        [AllowAnonymous]
        public async Task<IActionResult> PutEmployee(int id, [FromBody] Employee employee)
        {

            Console.WriteLine($"Attempting to update Employee ID: {employee.Id}, New Name: {employee.FullName}, Role: {employee.Role}, Manager: {employee.Manager}");

            if (id != employee.Id)
            {
                _logger.LogWarning($"[API-Employees] ID mismatch: {id} != {employee.Id}");
                return BadRequest();
            }

            var existing = await _context.Employees.FindAsync(id); // Fetch existing employee
            if (existing == null)
            {
                _logger.LogWarning($"[API-Employees] Employee with ID {id} not found for update");
                return NotFound();
            }

            // Update the existing employee's fields
            existing.FullName = employee.FullName;
            existing.Email = employee.Email;
            existing.Manager = employee.Manager;

            if (existing.Role != employee.Role)
            {
                _logger.LogInformation($"[API-Employees] Role change detected for Employee ID: {existing.Id}, Old Role: {existing.Role}, New Role: {employee.Role}");
                var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

                var identityUser = await userManager.FindByIdAsync(existing.IdentityUserId);
                if (identityUser != null)
                {
                    _logger.LogInformation($"[API-Employees] Updating role for IdentityUser ID: {identityUser.Id}, Email: {identityUser.Email}");

                    // Remove old roles
                    var currentRoles = await userManager.GetRolesAsync(identityUser);
                    await userManager.RemoveFromRolesAsync(identityUser, currentRoles);

                    // Ensure new role exists
                    if (!await roleManager.RoleExistsAsync(employee.Role))
                    {
                        _logger.LogWarning($"[API-Employees] Role '{employee.Role}' does not exist. Creating it.");
                        await roleManager.CreateAsync(new IdentityRole(employee.Role));
                    }

                    // Add new role
                    await userManager.AddToRoleAsync(identityUser, employee.Role);
                }
                else
                {
                    _logger.LogWarning($"[API-Employees] IdentityUser with ID {existing.IdentityUserId} not found for Employee ID: {existing.Id}");
                    Console.WriteLine("IdentityUser not found for Employee.");
                }

                existing.Role = employee.Role; // Finally update role in Employees table
            }

            Console.WriteLine($"Saving Employee ID: {existing.Id}, Name: {existing.FullName}, Role: {existing.Role}, Manager: {existing.Manager}");

            await _context.SaveChangesAsync();

            _logger.LogInformation($"[API-Employees] Employee updated: {existing.FullName}, Role: {existing.Role}, Manager: {existing.Manager}");
            return NoContent();

        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id); // Fetch employee by ID
            if (employee == null)
            {
                _logger.LogWarning($"[API-Employees] Employee with ID {id} not found for deletion");
                return NotFound();
            }
            _context.Employees.Remove(employee); // Remove employee
            await _context.SaveChangesAsync();

            _logger.LogInformation($"[API-Employees] Employee deleted: {employee.FullName}");
            return NoContent();
        }
    }
}
