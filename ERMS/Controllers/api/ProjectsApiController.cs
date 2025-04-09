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
    public class ProjectsApiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProjectsApiController> _logger;

        public ProjectsApiController(ApplicationDbContext context, ILogger<ProjectsApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            _logger.LogInformation("[API-Projects] Fetching all projects with assigned employees");
            // Fetch all projects with their assigned employees
            return await _context.Projects
            .Include(p => p.AssignedEmployees)
            .ToListAsync();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects // Fetch project by ID
            .Include(p => p.AssignedEmployees)
            .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                _logger.LogWarning($"[API-Projects] Project with ID {id} not found");
                return NotFound();
            }

            _logger.LogInformation($"[API-Projects] Project with ID {id} found: {project.Name}");
            return project;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {

            if (project.AssignedEmployees != null && project.AssignedEmployees.Any())
            {
                _logger.LogInformation($"[API-Projects] Assigning employees to project {project.Name}");
                var attachedEmployees = await _context.Employees // Fetch employees by IDs
                    .Where(e => project.AssignedEmployees.Select(a => a.Id).Contains(e.Id))
                    .ToListAsync();

                project.AssignedEmployees = attachedEmployees; // Assign employees to the project
            }

            _context.Projects.Add(project); // Add new project
            await _context.SaveChangesAsync();

            _logger.LogInformation($"[API-Projects] Project {project.Name} created successfully with ID {project.Id}");
            return CreatedAtAction("GetProject", new { id = project.Id }, project); // Return the created project
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> PutProject(int id, Project project)
        {
            if (id != project.Id)
            {
                _logger.LogWarning($"[API-Projects] Project ID mismatch: {id} != {project.Id}");
                return BadRequest();
            }

            var existingProject = await _context.Projects // Fetch existing project by ID
                .Include(p => p.AssignedEmployees)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProject == null)
            {
                _logger.LogWarning($"[API-Projects] Project with ID {id} not found for update");
                return NotFound();
            }

            // Update project fields
            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.StartDate = project.StartDate;
            existingProject.EndDate = project.EndDate;
            existingProject.Status = project.Status;

            existingProject.AssignedEmployees.Clear(); // Clear existing employees

            if (project.AssignedEmployees != null && project.AssignedEmployees.Any())
            {
                _logger.LogInformation($"[API-Projects] Reassigning employees to project {project.Name}");
                var attachedEmployees = await _context.Employees // Fetch employees by IDs
                    .Where(e => project.AssignedEmployees.Select(a => a.Id).Contains(e.Id))
                    .ToListAsync();

                existingProject.AssignedEmployees = attachedEmployees; // Assign new employees to the project
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"[API-Projects] Project {project.Name} updated successfully");
            return NoContent();
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteProject(int id)
        {
            Console.WriteLine($"Attempting to delete project with ID {id}");

            var project = await _context.Projects // Fetch project by ID
            .Include(p => p.AssignedEmployees)
            .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                _logger.LogWarning($"[API-Projects] Project with ID {id} not found for deletion");
                Console.WriteLine("Project not found");
                return NotFound();
            }

            foreach (var emp in project.AssignedEmployees.ToList())
            {
                _logger.LogInformation($"[API-Projects] Removing project assignment from employee {emp.FullName}");
                emp.ProjectId = null; 
            }

            _context.Projects.Remove(project); // Remove project
            await _context.SaveChangesAsync();
            Console.WriteLine("Project deleted successfully");

            _logger.LogInformation($"[API-Projects] Project {project.Name} deleted successfully");
            return NoContent();
        }
    }
}
