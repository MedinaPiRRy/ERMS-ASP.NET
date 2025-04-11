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
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;

namespace ERMS.Tests
{
    public class ProjectsControllerTests
    {
        private readonly Mock<IProjectApiService> _projectMock;
        private readonly Mock<IEmployeeApiService> _employeeMock;
        private readonly ProjectsController _controller;
        private readonly ILogger<ProjectsController> _logger;

        public ProjectsControllerTests()
        {
            var mockLogger = new Mock<ILogger<ProjectsController>>();
            _logger = mockLogger.Object;

            _projectMock = new Mock<IProjectApiService>();
            _employeeMock = new Mock<IEmployeeApiService>();
            _controller = new ProjectsController(_projectMock.Object, _employeeMock.Object, _logger);
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
        public async Task Index_ReturnsView_WithProjects()
        {
            _projectMock.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<Project> { new Project() });

            var result = await _controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<IEnumerable<Project>>(view.Model);
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
            _projectMock.Setup(p => p.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Project)null!);

            var result = await _controller.Details(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Details_ValidId_ReturnsView()
        {
            _projectMock.Setup(p => p.GetByIdAsync(1)).ReturnsAsync(new Project { Id = 1 });

            var result = await _controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<Project>(view.Model);
        }

        [Fact]
        public async Task Create_Get_ReturnsView_WithEmployees()
        {
            _employeeMock.Setup(e => e.GetAllAsync()).ReturnsAsync(new List<Employee>());

            var result = await _controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            _employeeMock.Setup(e => e.GetAllAsync()).ReturnsAsync(new List<Employee>());

            _controller.ModelState.AddModelError("Name", "Required");
            SetUserWithRoles(_controller, new[] { "Manager" });

            var project = new Project { Id = 1, Name = null };
            var result = await _controller.Create(project, new int[] { });

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(project, viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_ValidModel_Redirects()
        {
            _employeeMock.Setup(e => e.GetAllAsync()).ReturnsAsync(new List<Employee>());
            _projectMock.Setup(p => p.CreateAsync(It.IsAny<Project>())).ReturnsAsync(true);

            SetUserWithRoles(_controller, new[] { "Manager" }); // Set user role to Manager

            var result = await _controller.Create(new Project(), new int[0]);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_InvalidId_ReturnsNotFound()
        {
            var result = await _controller.Edit(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ValidId_ReturnsView()
        {
            _projectMock.Setup(p => p.GetByIdAsync(1)).ReturnsAsync(new Project { Id = 1 });

            var result = await _controller.Delete(1);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_Success_Redirects()
        {
            _projectMock.Setup(p => p.DeleteAsync(1)).ReturnsAsync(true);

            SetUserWithRoles(_controller, new[] { "Manager" }); // Set user role to Manager

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_Failure_ReturnsViewWithError()
        {
            _projectMock.Setup(p => p.DeleteAsync(1)).ReturnsAsync(false);
            _projectMock.Setup(p => p.GetByIdAsync(1)).ReturnsAsync(new Project { Id = 1 });

            SetUserWithRoles(_controller, new[] { "Manager" }); // Set user role to Manager

            var result = await _controller.DeleteConfirmed(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<Project>(view.Model);
        }
    }
}
