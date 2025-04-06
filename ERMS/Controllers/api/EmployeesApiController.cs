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

        public EmployeesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            return await _context.Employees.ToListAsync(); // Fetch all employees
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id); // Fetch employee by ID
            if (employee == null)
            {
                return NotFound();
            }
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
                return Conflict("User already exists.");
            }

            // Create Identity user
            identityUser = new IdentityUser { UserName = employee.Email, Email = employee.Email };
            var password = "Default123!"; // ⚠️ Use a safe default or require password field
            var result = await userManager.CreateAsync(identityUser, password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Ensure role exists
            if (!await roleManager.RoleExistsAsync(employee.Role))
                await roleManager.CreateAsync(new IdentityRole(employee.Role));

            // Add to role
            await userManager.AddToRoleAsync(identityUser, employee.Role);

            // Save employee info
            employee.IdentityUserId = identityUser.Id;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

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
                return BadRequest();
            }

            var existing = await _context.Employees.FindAsync(id); // Fetch existing employee
            if (existing == null) return NotFound();

            // Update the existing employee's fields
            existing.FullName = employee.FullName;
            existing.Email = employee.Email;
            existing.Manager = employee.Manager;

            if (existing.Role != employee.Role)
            {
                var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = HttpContext.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

                var identityUser = await userManager.FindByIdAsync(existing.IdentityUserId);
                if (identityUser != null)
                {
                    // Remove old roles
                    var currentRoles = await userManager.GetRolesAsync(identityUser);
                    await userManager.RemoveFromRolesAsync(identityUser, currentRoles);

                    // Ensure new role exists
                    if (!await roleManager.RoleExistsAsync(employee.Role))
                        await roleManager.CreateAsync(new IdentityRole(employee.Role));

                    // Add new role
                    await userManager.AddToRoleAsync(identityUser, employee.Role);
                }
                else
                {
                    Console.WriteLine("⚠️ IdentityUser not found for Employee.");
                }

                existing.Role = employee.Role; // Finally update role in Employees table
            }

            Console.WriteLine($"Saving Employee ID: {existing.Id}, Name: {existing.FullName}, Role: {existing.Role}, Manager: {existing.Manager}");

            await _context.SaveChangesAsync();
            return NoContent();

        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id); // Fetch employee by ID
            if (employee == null)
            {
                return NotFound();
            }
            _context.Employees.Remove(employee); // Remove employee
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
