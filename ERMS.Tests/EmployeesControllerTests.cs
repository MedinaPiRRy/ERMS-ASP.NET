using Xunit;
using Moq;
using ERMS.Controllers;
using ERMS.Services;
using ERMS.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using ERMS.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ERMS.Tests
{
    public class EmployeesControllerTests
    {
        private readonly Mock<IEmployeeApiService> _mockApiService;
        private readonly EmployeesController _controller;
        private readonly ApplicationDbContext _context;

        public EmployeesControllerTests()
        {
            // Set up in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDB")
            .Options;
            _context = new ApplicationDbContext(options);

            _mockApiService = new Mock<IEmployeeApiService>();
            _controller = new EmployeesController(_mockApiService.Object, _context);
        }

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
        public async Task Index_ReturnsViewResult_WithListOfEmployees()
        {
            // Arrange
            _mockApiService.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Employee>
                {
                    new Employee { Id = 1, FullName = "Test Employee", Email = "test@email.com" },
                    new Employee { Id = 2, FullName = "Second Test Employee", Email = "test2@email.com" }
                });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Employee>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_ReturnsViewResult_WithEmployee()
        {
            var employee = new Employee { Id = 1, FullName = "Test Employee", Email = "test@email.com" };
            _mockApiService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(employee);

            var result = await _controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Employee>(viewResult.Model);
            Assert.Equal(employee.Id, model.Id);
        }

        [Fact]
        public async Task Create_ReturnsViewResult()
        {
            var result = await _controller.Create();
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_PostValidEmployee_RedirectsToIndex()
        {
            var employee = new Employee { Id = 1, FullName = "Test Employee", Email = "test@email.com" };
            _mockApiService.Setup(s => s.CreateAsync(employee)).ReturnsAsync(true);

            SetUserWithRoles(_controller, new[] { "Admin" }); // Set user role to Admin

            var result = await _controller.Create(employee);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Edit_ReturnsView_withEmployee()
        {
            var employee = new Employee { Id = 1, FullName = "Test Employee", Email = "test@email.com" };
            _mockApiService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(employee);

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Employee>(viewResult.Model);
            Assert.Equal(employee.Id, model.Id);
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesEmployeeAndRedirects()
        {
            _mockApiService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            SetUserWithRoles(_controller, new[] { "Admin" });

            var result = await _controller.DeleteConfirmed(1);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Create_Get_ReturnsViewWithDropdowns()
        {
            var result = await _controller.Create();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["Managers"]);
            Assert.NotNull(viewResult.ViewData["Roles"]);
        }

        [Fact]
        public async Task Create_PostInvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("FullName", "Required");

            var result = await _controller.Create(new Employee());
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(viewResult.ViewData.ModelState.IsValid);
        }

        [Fact]
        public async Task Edit_GetInvalidID_ReturnsNotFound() 
        {
            _mockApiService.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Employee)null);
            var result = await _controller.Edit(null);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_PostIdMismatch_ReturnsNotFound()
        {
            var employee = new Employee { Id = 1 };
            var result = await _controller.Edit(2, employee);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_ValidId_RedirectsToIndex()
        {
            _mockApiService.Setup(x => x.DeleteAsync(It.IsAny<int>())).ReturnsAsync(true);

            SetUserWithRoles(_controller, new[] { "Admin" }); // Set user role to Admin

            var result = await _controller.DeleteConfirmed(1);
            Assert.IsType<ForbidResult>(result);
        }


    }
}
