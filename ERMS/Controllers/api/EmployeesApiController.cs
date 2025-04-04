using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ERMS.Data;
using ERMS.Models;

namespace ERMS.Controllers.api
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmployeesApiController : Controller
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
            _context.Employees.Add(employee); // Add new employee
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetEmployee", new { id = employee.Id }, employee); // Return the created employee
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> PutEmployee(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest();
            }

            var existing = await _context.Employees.FindAsync(id); // Fetch existing employee
            if (existing == null) return NotFound();

            // Update the existing employee's fields
            existing.FullName = employee.FullName;
            existing.Email = employee.Email;
            existing.Role = employee.Role;
            existing.Manager = employee.Manager;

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
