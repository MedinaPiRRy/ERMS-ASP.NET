using Xunit;
using Microsoft.EntityFrameworkCore;
using ERMS.Controllers.api;
using ERMS.Models;
using ERMS.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ERMS.Controllers;
using ERMS.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.AspNetCore.Identity;

namespace ERMS.Tests
{
    public class EmployeesApiControllerTests
    {
        private readonly Mock<IEmployeeApiService> _mockApiService;
        private readonly EmployeesController _controller;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmployeesController> _employeeLogger;
        private readonly ILogger<EmployeesApiController> _employeeApiLogger;

        public EmployeesApiControllerTests()
        {
            var mockEmployeeLogger = new Mock<ILogger<EmployeesController>>();
            var mockEmployeeApiLogger = new Mock<ILogger<EmployeesApiController>>();

            _employeeLogger = mockEmployeeLogger.Object;
            _employeeApiLogger = mockEmployeeApiLogger.Object;

            _context = GetContext("TestDB2");

            _mockApiService = new Mock<IEmployeeApiService>();
            _controller = new EmployeesController(_mockApiService.Object, _context, _employeeLogger);
        }

        private ApplicationDbContext GetContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        // This method sets up the user roles for the controller context.
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
        public async Task GetEmployees_ReturnsAll()
        {
            var context = GetContext(nameof(GetEmployees_ReturnsAll));
            context.Employees.Add(new Employee {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Role = "Admin",
                Manager = "Manager A"
            });
            context.Employees.Add(new Employee {
                Id = 2,
                FullName = "Jane Doe",
                Email = "jane@example.com",
                Role = "Manager",
                Manager = "Manager B"
            });
            await context.SaveChangesAsync();

            var controller = new EmployeesApiController(context, _employeeApiLogger);
            var result = await controller.GetEmployees();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Employee>>>(result);
            Assert.Equal(2, ((List<Employee>)actionResult.Value).Count);
        }

        [Fact]
        public async Task GetEmployee_ValidId_ReturnsEmployee()
        {
            var context = GetContext(nameof(GetEmployee_ValidId_ReturnsEmployee));
            var emp = new Employee {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Role = "Admin",
                Manager = "Manager A"
            };
            context.Employees.Add(emp);
            await context.SaveChangesAsync();

            var controller = new EmployeesApiController(context, _employeeApiLogger);
            var result = await controller.GetEmployee(1);

            Assert.Equal(emp.FullName, result.Value.FullName);
        }

        [Fact]
        public async Task GetEmployee_InvalidId_ReturnsNotFound()
        {
            var context = GetContext(nameof(GetEmployee_InvalidId_ReturnsNotFound));
            var controller = new EmployeesApiController(context, _employeeApiLogger);

            var result = await controller.GetEmployee(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostEmployee_AddsEmployee()
        {
            // Use ApplicationDbContext to create a new in-memory database for testing
            var context = GetContext(nameof(PostEmployee_AddsEmployee));
            var controller = new EmployeesApiController(context, _employeeApiLogger);

            // Create mock services 
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(), null, null, null, null, null, null, null, null
            );
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null
            );

            // Set up the mock UserManager and RoleManager
            userManagerMock.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser)null!);
            userManagerMock.Setup(m => m.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            roleManagerMock.Setup(m => m.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true); // Assuming the role exists

            // Set up the service provider to return the mocked UserManager and RoleManager
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(UserManager<IdentityUser>))).Returns(userManagerMock.Object);
            serviceProvider.Setup(x => x.GetService(typeof(RoleManager<IdentityRole>))).Returns(roleManagerMock.Object);

            // Set the controller context with the mocked services
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = serviceProvider.Object }
            };

            // Arrange
            var emp = new Employee
            {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Role = "Admin",
                Manager = "Manager A"
            };

            // Act
            var result = await controller.PostEmployee(emp);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(emp, created.Value);
        }

        [Fact]
        public async Task PutEmployee_ValidId_UpdatesEmployee()
        {
            var context = GetContext(nameof(PutEmployee_ValidId_UpdatesEmployee));
            var emp = new Employee {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Role = "Admin",
                Manager = "Manager A"
            };
            context.Employees.Add(emp);
            await context.SaveChangesAsync();

            var controller = new EmployeesApiController(context, _employeeApiLogger);
            emp.FullName = "New Name";
            var result = await controller.PutEmployee(1, emp);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task PutEmployee_MismatchedId_ReturnsBadRequest()
        {
            var context = GetContext(nameof(PutEmployee_MismatchedId_ReturnsBadRequest));
            var controller = new EmployeesApiController(context, _employeeApiLogger);

            var result = await controller.PutEmployee(1, new Employee { Id = 2 });

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutEmployee_NotFound_ReturnsNotFound()
        {
            var context = GetContext(nameof(PutEmployee_NotFound_ReturnsNotFound));
            var controller = new EmployeesApiController(context, _employeeApiLogger);

            var result = await controller.PutEmployee(1, new Employee { Id = 1 });

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteEmployee_ValidId_Deletes()
        {
            var context = GetContext(nameof(DeleteEmployee_ValidId_Deletes));
            context.Employees.Add(new Employee {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Role = "Admin",
                Manager = "Manager A"
            });
            await context.SaveChangesAsync();

            var controller = new EmployeesApiController(context, _employeeApiLogger);
            var result = await controller.DeleteEmployee(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEmployee_InvalidId_ReturnsNotFound()
        {
            var context = GetContext(nameof(DeleteEmployee_InvalidId_ReturnsNotFound));
            var controller = new EmployeesApiController(context, _employeeApiLogger);

            var result = await controller.DeleteEmployee(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
