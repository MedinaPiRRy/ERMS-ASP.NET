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
    public class ProjectsController : Controller
    {
        private readonly IProjectApiService _api;
        private readonly IEmployeeApiService _employeeApi;

        public ProjectsController(IProjectApiService api, IEmployeeApiService employeeApi)
        {
            _api = api;
            _employeeApi = employeeApi;
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _api.GetAllAsync(); // Fetch all projects from the API
            return View(projects);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _api.GetByIdAsync(id.Value); // Fetch project by ID from the API
            if (project == null) return NotFound();

            return View(project);
        }

        public async Task<IActionResult> Create()
        {
            var employees = (await _employeeApi.GetAllAsync()); // Fetch all employees from the API

            ViewBag.AllEmployees = new MultiSelectList(employees, "Id", "FullName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project project, int[] AssignedEmployeeIds)
        {
            ModelState.Remove("AssignedEmployees"); // Remove the AssignedEmployees from ModelState to avoid validation errors

            project.AssignedEmployees = (await _employeeApi.GetAllAsync()) // Fetch all employees from the API
                .Where(e => AssignedEmployeeIds.Contains(e.Id))
                .ToList();

            if (!ModelState.IsValid) {
                var allEmployees = await _employeeApi.GetAllAsync(); // Fetch all employees from the API
                ViewBag.AllEmployees = new MultiSelectList(allEmployees, "Id", "FullName", AssignedEmployeeIds); // Makes sure employees are displayed

                return View(project);
            };

            var success = await _api.CreateAsync(project); // Create project via API
            if (!success) ModelState.AddModelError("", "Error creating project.");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _api.GetByIdAsync(id.Value); // Fetch project by ID from the API
            if (project == null) return NotFound();

            var employees = await _employeeApi.GetAllAsync(); // Fetch all employees from the API
            var selected = project.AssignedEmployees?.Select(e => e.Id); // Get selected employee IDs
            ViewBag.AllEmployees = new MultiSelectList(employees, "Id", "FullName", selected); // Makes sure employees are displayed

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project, int[] AssignedEmployeeIds)
        {
            ModelState.Remove("AssignedEmployees"); // Remove the AssignedEmployees from ModelState to avoid validation errors

            if (id != project.Id) return NotFound();

            project.AssignedEmployees = (await _employeeApi.GetAllAsync()) // Fetch all employees from the API
                .Where(e => AssignedEmployeeIds.Contains(e.Id))
                .ToList();

            if (!ModelState.IsValid) {
                var employees = await _employeeApi.GetAllAsync(); // Fetch all employees from the API
                ViewBag.AllEmployees = new MultiSelectList(employees, "Id", "FullName", AssignedEmployeeIds); // Makes sure employees are displayed
                return View(project);
            };

            var success = await _api.UpdateAsync(project); // Update project via API
            if (!success) ModelState.AddModelError("", "Error updating project.");

            var allEmployees = await _employeeApi.GetAllAsync(); // Fetch all employees from the API
            var selected = project.AssignedEmployees?.Select(e => e.Id); // Get selected employee IDs

            ViewBag.AllEmployees = new MultiSelectList(allEmployees, "Id", "FullName", selected); // Makes sure employees are displayed

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _api.GetByIdAsync(id.Value); // Fetch project by ID from the API
            if (project == null) return NotFound();

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var success = await _api.DeleteAsync(id);   // Delete project via API

            if (!success)
            {
                Console.WriteLine($"DELETE failed: {id}");
                ModelState.AddModelError("", "Error deleting project. Please try again.");
                var project = await _api.GetByIdAsync(id); // Fetch project by ID from the API
                return View("Delete", project); // Redisplay Delete view with message
            }

            return RedirectToAction(nameof(Index));
        }

        //private readonly ApplicationDbContext _context;

        //public ProjectsController(ApplicationDbContext context)
        //{
        //    _context = context;
        //}

        //// GET: Projects
        //public async Task<IActionResult> Index()
        //{
        //    var projects = await _context.Projects
        //    .Include(p => p.AssignedEmployees)
        //    .ToListAsync();

        //    return View(projects);
        //}

        //// GET: Projects/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{

        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var project = await _context.Projects
        //    .Include(p => p.AssignedEmployees)
        //    .FirstOrDefaultAsync(m => m.Id == id);

        //    if (project == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(project);
        //}

        //// GET: Projects/Create
        //public IActionResult Create()
        //{
        //    // To access all employees and add them to drop down menu
        //    ViewBag.AllEmployees = new MultiSelectList(_context.Employees, "Id", "FullName");
        //    return View();
        //}

        //// POST: Projects/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,Status")] Project project, int[] AssignedEmployeeIds)
        //{
        //    // Form was giving problems as it was not sending the AssignedEmployees list with the project, making the post requet fail.
        //    // To fix this, we need to remove the AssignedEmployees from the ModelState so it doesn't throw an error.
        //    // This is a workaround to allow the form to be submitted without the AssignedEmployees list.
        //    // This is not the best practice, but it works for this case.
        //    //
        //    // References:
        //    // https://stackoverflow.com/questions/6843171/is-it-correct-way-to-use-modelstate-remove-to-deal-with-modelstate
        //    //
        //    ModelState.Remove("AssignedEmployees");

        //    Console.WriteLine("AssignedEmployeeIds: " + string.Join(", ", AssignedEmployeeIds));
        //    if (ModelState.IsValid)
        //    {
        //        project.AssignedEmployees = _context.Employees.Where(e => AssignedEmployeeIds.Contains(e.Id))
        //            .ToList();

        //        _context.Add(project);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    // To access all employees and add them to drop down menu
        //    ViewBag.AllEmployees = new MultiSelectList(_context.Employees, "Id", "FullName");
        //    return View(project);
        //}

        //// GET: Projects/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var project = await _context.Projects
        //    .Include(p => p.AssignedEmployees)
        //    .FirstOrDefaultAsync(p => p.Id == id);

        //    if (project == null)
        //    {
        //        return NotFound();
        //    }

        //    var selectedIds = project.AssignedEmployees.Select(e => e.Id).ToArray();
        //    ViewBag.AllEmployees = new MultiSelectList(_context.Employees, "Id", "FullName", selectedIds);

        //    return View(project);
        //}

        //// POST: Projects/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,StartDate,EndDate,Status")] Project project, int[] AssignedEmployeeIds)
        //{
        //    ModelState.Remove("AssignedEmployees");

        //    if (id != project.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var existingProject = await _context.Projects
        //            .Include(p => p.AssignedEmployees)
        //            .FirstOrDefaultAsync(p => p.Id == id);

        //            if (existingProject == null) return NotFound();

        //            // Update fields
        //            existingProject.Name = project.Name;
        //            existingProject.Description = project.Description;
        //            existingProject.StartDate = project.StartDate;
        //            existingProject.EndDate = project.EndDate;
        //            existingProject.Status = project.Status;

        //            // Update assignedEmployees
        //            existingProject.AssignedEmployees.Clear();
        //            existingProject.AssignedEmployees = _context.Employees
        //                .Where(e => AssignedEmployeeIds.Contains(e.Id)).ToList();

        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ProjectExists(project.Id))
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
        //    ViewBag.AllEmployees = new MultiSelectList(_context.Employees, "Id", "FullName", AssignedEmployeeIds);
        //    return View(project);
        //}

        //// GET: Projects/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var project = await _context.Projects
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (project == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(project);
        //}

        //// POST: Projects/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var project = await _context.Projects.FindAsync(id);
        //    if (project != null)
        //    {
        //        _context.Projects.Remove(project);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool ProjectExists(int id)
        //{
        //    return _context.Projects.Any(e => e.Id == id);
        //}
    }
}
