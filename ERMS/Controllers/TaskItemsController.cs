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

namespace ERMS.Controllers
{
    public class TaskItemsController : Controller
    {
        private readonly ITaskItemApiService _taskApi;
        private readonly IEmployeeApiService _employeeApi;
        private readonly IProjectApiService _projectApi;

        public TaskItemsController(ITaskItemApiService taskApi, IEmployeeApiService employeeApi, IProjectApiService projectApi)
        {
            _taskApi = taskApi;
            _employeeApi = employeeApi;
            _projectApi = projectApi;
        }

        public async Task<IActionResult> Index()
        {
            var tasks = await _taskApi.GetAllAsync(); // Fetch all tasks from the API
            return View(tasks);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var task = await _taskApi.GetByIdAsync(id.Value); // Fetch task by ID from the API
            if (task == null) return NotFound();

            return View(task);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.EmployeeId = new SelectList(await _employeeApi.GetAllAsync(), "Id", "FullName"); // Makes sure all employees displayed
            ViewBag.ProjectId = new SelectList(await _projectApi.GetAllAsync(), "Id", "Name"); // Makes sure all projects displayed
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem task)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Employees = new SelectList(await _employeeApi.GetAllAsync(), "Id", "FullName"); // Makes sure all employees displayed
                ViewBag.Projects = new SelectList(await _projectApi.GetAllAsync(), "Id", "Name"); // Makes sure all projects displayed
                return View(task);
            }

            if (!User.IsInRole("Manager") || User.IsInRole("Admin"))
            {
                return Forbid(); // Prevent unauthorized access
            }

            await _taskApi.CreateAsync(task); // Create task via API
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var task = await _taskApi.GetByIdAsync(id.Value); // Fetch task by ID from the API
            if (task == null) return NotFound();

            ViewBag.EmployeeId = new SelectList(await _employeeApi.GetAllAsync(), "Id", "FullName", task.EmployeeId); // Makes sure all employees displayed
            ViewBag.ProjectId = new SelectList(await _projectApi.GetAllAsync(), "Id", "Name", task.ProjectId); // Makes sure all projects displayed

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem task)
        {
            if (id != task.Id) return NotFound();

            if (!User.IsInRole("Manager") || User.IsInRole("Admin"))
            {
                return Forbid(); // Prevent unauthorized access
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Employees = new SelectList(await _employeeApi.GetAllAsync(), "Id", "FullName", task.EmployeeId); // Makes sure all employees displayed
                ViewBag.Projects = new SelectList(await _projectApi.GetAllAsync(), "Id", "Name", task.ProjectId); // Makes sure all projects displayed
                return View(task); 
            }

            await _taskApi.UpdateAsync(task); // Update task via API
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var task = await _taskApi.GetByIdAsync(id.Value); // Fetch task by ID from the API
            if (task == null) return NotFound();

            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!User.IsInRole("Manager") || User.IsInRole("Admin"))
            {
                return Forbid(); // Prevent unauthorized access
            }

            await _taskApi.DeleteAsync(id); // Delete task via API
            return RedirectToAction(nameof(Index));
        }

        //private readonly ApplicationDbContext _context;

        //public TaskItemsController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        //// GET: TaskItems
        //public async Task<IActionResult> Index()
        //{
        //    var applicationDbContext = _context.TaskItems.Include(t => t.AssignedEmployee).Include(t => t.Project);
        //    return View(await applicationDbContext.ToListAsync());
        //}

        //// GET: TaskItems/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var taskItem = await _context.TaskItems
        //        .Include(t => t.AssignedEmployee)
        //        .Include(t => t.Project)
        //        .FirstOrDefaultAsync(m => m.Id == id);

        //    if (taskItem == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(taskItem);
        //}

        //// GET: TaskItems/Create
        //public IActionResult Create()
        //{
        //    ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName");
        //    ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
        //    return View();
        //}

        //// POST: TaskItems/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Title,Description,Status,Priority,ProjectId,EmployeeId")] TaskItem taskItem)
        //{
        //    // Remove the properties that are not needed for model binding and are causing issues.
        //    ModelState.Remove("Project");
        //    ModelState.Remove("AssignedEmployee");

        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(taskItem);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", taskItem.EmployeeId);
        //    ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);
        //    return View(taskItem);
        //}

        //// GET: TaskItems/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var taskItem = await _context.TaskItems.FindAsync(id);
        //    if (taskItem == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", taskItem.EmployeeId);
        //    ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);
        //    return View(taskItem);
        //}

        //// POST: TaskItems/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,Priority,ProjectId,EmployeeId")] TaskItem taskItem)
        //{
        //    if (id != taskItem.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(taskItem);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TaskItemExists(taskItem.Id))
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
        //    ViewData["EmployeeId"] = new SelectList(_context.Employees, "Id", "FullName", taskItem.EmployeeId);
        //    ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);
        //    return View(taskItem);
        //}

        //// GET: TaskItems/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var taskItem = await _context.TaskItems
        //        .Include(t => t.AssignedEmployee)
        //        .Include(t => t.Project)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (taskItem == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(taskItem);
        //}

        //// POST: TaskItems/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var taskItem = await _context.TaskItems.FindAsync(id);
        //    if (taskItem != null)
        //    {
        //        _context.TaskItems.Remove(taskItem);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool TaskItemExists(int id)
        //{
        //    return _context.TaskItems.Any(e => e.Id == id);
        //}
    }
}
