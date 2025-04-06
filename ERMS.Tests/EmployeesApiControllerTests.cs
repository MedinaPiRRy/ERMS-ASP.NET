using Xunit;
using Microsoft.EntityFrameworkCore;
using ERMS.Controllers.api;
using ERMS.Models;
using ERMS.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERMS.Tests
{
    public class EmployeesApiControllerTests
    {
        private ApplicationDbContext GetContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
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

            var controller = new EmployeesApiController(context);
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

            var controller = new EmployeesApiController(context);
            var result = await controller.GetEmployee(1);

            Assert.Equal(emp.FullName, result.Value.FullName);
        }

        [Fact]
        public async Task GetEmployee_InvalidId_ReturnsNotFound()
        {
            var context = GetContext(nameof(GetEmployee_InvalidId_ReturnsNotFound));
            var controller = new EmployeesApiController(context);

            var result = await controller.GetEmployee(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostEmployee_AddsEmployee()
        {
            var context = GetContext(nameof(PostEmployee_AddsEmployee));
            var controller = new EmployeesApiController(context);

            var emp = new Employee {
                Id = 1,
                FullName = "John Doe",
                Email = "john@example.com",
                Role = "Admin",
                Manager = "Manager A"
            };
            var result = await controller.PostEmployee(emp);

            Assert.IsType<CreatedAtActionResult>(result.Result);
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

            var controller = new EmployeesApiController(context);
            emp.FullName = "New Name";
            var result = await controller.PutEmployee(1, emp);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task PutEmployee_MismatchedId_ReturnsBadRequest()
        {
            var context = GetContext(nameof(PutEmployee_MismatchedId_ReturnsBadRequest));
            var controller = new EmployeesApiController(context);

            var result = await controller.PutEmployee(1, new Employee { Id = 2 });

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutEmployee_NotFound_ReturnsNotFound()
        {
            var context = GetContext(nameof(PutEmployee_NotFound_ReturnsNotFound));
            var controller = new EmployeesApiController(context);

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

            var controller = new EmployeesApiController(context);
            var result = await controller.DeleteEmployee(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEmployee_InvalidId_ReturnsNotFound()
        {
            var context = GetContext(nameof(DeleteEmployee_InvalidId_ReturnsNotFound));
            var controller = new EmployeesApiController(context);

            var result = await controller.DeleteEmployee(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
