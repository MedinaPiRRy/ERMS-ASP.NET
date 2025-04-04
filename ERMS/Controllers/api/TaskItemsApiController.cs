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
    public class TaskItemsApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskItemsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            return await _context.TaskItems // Fetch all tasks with their associated project and assigned employee
            .Include(t => t.Project)
            .Include(t => t.AssignedEmployee)
            .ToListAsync();
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var taskItem = await _context.TaskItems // Fetch task by ID with its associated project and assigned employee
            .Include(t => t.Project)
            .Include(t => t.AssignedEmployee)
            .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null)
            {
                return NotFound();
            }

            return taskItem;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<TaskItem>> PostTask(TaskItem taskItem)
        {
            _context.TaskItems.Add(taskItem); // Add new task
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetTask", new { id = taskItem.Id }, taskItem);
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> PutTask(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(taskItem).State = EntityState.Modified; // Update task

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id); // Fetch task by ID
            if (taskItem == null)
            {
                return NotFound();
            }
            _context.TaskItems.Remove(taskItem); // Remove task
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
