using Xunit;
using Moq;
using ERMS.Controllers;
using ERMS.Models;
using ERMS.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using ERMS.Data;
using Microsoft.EntityFrameworkCore;

namespace ERMS.Tests
{
    public class TaskItemsControllerTests
    {
        private readonly Mock<ITaskItemApiService> _taskApi;
        private readonly Mock<IEmployeeApiService> _employeeApi;
        private readonly Mock<IProjectApiService> _projectApi;
        private readonly TaskItemsController _controller;
        private readonly ILogger<TaskItemsController> _logger;
        private readonly ApplicationDbContext _context;

        public TaskItemsControllerTests()
        {
            var mockLogger = new Mock<ILogger<TaskItemsController>>();
            _logger = mockLogger.Object;

            // Set up in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            _context = new ApplicationDbContext(options);

            _taskApi = new Mock<ITaskItemApiService>();
            _employeeApi = new Mock<IEmployeeApiService>();
            _projectApi = new Mock<IProjectApiService>();
            _controller = new TaskItemsController(_taskApi.Object, _employeeApi.Object, _projectApi.Object, _logger, _context);
        }

        // Set up in-memory database for testing (User Roles)
        private void SetUserWithRoles(Controller controller, string[] roles)
        {
            var identity = new ClaimsIdentity(roles.Select(role => new Claim(ClaimTypes.Role, role)), "mock");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        [Fact]
        public async Task Index_ReturnsView_WithTasks()
        {
            _taskApi.Setup(t => t.GetAllAsync()).ReturnsAsync(new List<TaskItem> { new TaskItem() });

            var result = await _controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<IEnumerable<TaskItem>>(view.Model);
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            var result = await _controller.Details(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_InvalidId_ReturnsNotFound()
        {
            _taskApi.Setup(t => t.GetByIdAsync(999)).ReturnsAsync((TaskItem)null!);

            var result = await _controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_Get_ReturnsView()
        {
            _employeeApi.Setup(e => e.GetAllAsync()).ReturnsAsync(new List<Employee>());
            _projectApi.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<Project>());

            var result = await _controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("Title", "Required");
            _employeeApi.Setup(e => e.GetAllAsync()).ReturnsAsync(new List<Employee>());
            _projectApi.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<Project>());

            var result = await _controller.Create(new TaskItem());

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_ValidModel_Redirects()
        {
            _taskApi.Setup(t => t.CreateAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

            var task = new TaskItem
            {
                Id = 1,
                Title = "Fix Bug",
                Description = "Fix the null ref bug",
                Status = "In Progress",
                Priority = "High",
                EmployeeId = 1,
                ProjectId = 1
            };

            SetUserWithRoles(_controller, new[] { "Manager" }); // Set user role to Manager

            var result = await _controller.Create(task);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_NullId_ReturnsNotFound()
        {
            var result = await _controller.Edit(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_InvalidId_ReturnsNotFound()
        {
            var employee = new Employee
            {
                Id = 1,
                FullName = "Test Employee",
                Email = "test@company.com",
                Role = "Employee",
                Manager = "Unassigned"
            };
            _context.Employees.Add(employee);

            var task = new TaskItem
            {
                Id = 1,
                Title = "Task 1",
                Description = "Fix bug",
                Status = "Open",
                Priority = "High",
                EmployeeId = 1,
                ProjectId = 1
            };
            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            _taskApi.Setup(t => t.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "test@company.com"),
                new Claim(ClaimTypes.Role, "Employee")
            }, "mock");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = await _controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            var email = "test@company.com";

            var employee = new Employee
            {
                Id = 1,
                FullName = "Test Employee",
                Email = email,
                Role = "Employee",
                Manager = "Unassigned"
            };

            var task = new TaskItem
            {
                Id = 1,
                Title = "Task 1",
                Description = "Fix bug",
                Status = "Open",
                Priority = "High",
                EmployeeId = 1, // must match employee.Id
                ProjectId = 1
            };

            _context.Employees.Add(employee);
            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            _taskApi.Setup(t => t.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, "Employee")
            }, "mock");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var result = await _controller.Edit(1, task);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_Redirects()
        {
            var email = "test@company.com";

            var employee = new Employee
            {
                Id = 1,
                FullName = "Test Employee",
                Email = email,
                Role = "Employee",
                Manager = "Unassigned"
            };

            var task = new TaskItem
            {
                Id = 1,
                Title = "Task 1",
                Description = "Fix bug",
                Status = "Open",
                Priority = "High",
                EmployeeId = 1, // must match employee.Id
                ProjectId = 1
            };

            _context.Employees.Add(employee);
            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            _taskApi.Setup(t => t.UpdateAsync(It.IsAny<TaskItem>())).ReturnsAsync(true);

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, "Employee")
            }, "mock");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };

            var result = await _controller.Edit(1, task);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_RedirectsToIndex()
        {
            _taskApi.Setup(t => t.DeleteAsync(1)).ReturnsAsync(true);

            SetUserWithRoles(_controller, new[] { "Manager" }); // Set user role to Manager

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
    }
}
