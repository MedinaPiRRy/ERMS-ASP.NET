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
    public class TaskItemsApiControllerTests
    {
        private ApplicationDbContext GetContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetTasks_ReturnsAllTasks()
        {
            var context = GetContext(nameof(GetTasks_ReturnsAllTasks));
            context.Employees.Add(new Employee { Id = 1, FullName = "Test", Email = "t@t.com", Role = "Dev", Manager = "Boss" });
            context.Projects.Add(new Project { Id = 1, Name = "Test Project", Description = "Test Desc", Status = "Active" });
            context.TaskItems.Add(new TaskItem
            {
                Id = 1,
                Title = "Fix Bug",
                Description = "Fix the null ref bug",
                Status = "In Progress",
                Priority = "High",
                EmployeeId = 1,
                ProjectId = 1
            });
            await context.SaveChangesAsync();

            var controller = new TaskItemsApiController(context);
            var result = await controller.GetTasks();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskItem>>>(result);
            Assert.Single((List<TaskItem>)actionResult.Value);
        }

        [Fact]
        public async Task GetTask_ValidId_ReturnsTask()
        {
            var context = GetContext(nameof(GetTask_ValidId_ReturnsTask));
            context.Employees.Add(new Employee { Id = 1, FullName = "Test", Email = "t@t.com", Role = "Dev", Manager = "Boss" });
            context.Projects.Add(new Project { Id = 1, Name = "Test Project", Description = "Test Desc", Status = "Active" });
            context.TaskItems.Add(new TaskItem
            {
                Id = 1,
                Title = "Fix Bug",
                Description = "Fix the null ref bug",
                Status = "In Progress",
                Priority = "High",
                EmployeeId = 1,
                ProjectId = 1
            });

            await context.SaveChangesAsync();

            var controller = new TaskItemsApiController(context);
            var result = await controller.GetTask(1);

            Assert.Equal("Fix Bug", result.Value.Title);
        }

        [Fact]
        public async Task GetTask_InvalidId_ReturnsNotFound()
        {
            var context = GetContext(nameof(GetTask_InvalidId_ReturnsNotFound));
            var controller = new TaskItemsApiController(context);

            var result = await controller.GetTask(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostTask_CreatesTask()
        {
            var context = GetContext(nameof(PostTask_CreatesTask));
            var controller = new TaskItemsApiController(context);

            var task = new TaskItem {
                Id = 1,
                Title = "Fix Bug",
                Description = "Fix the null ref bug",
                Status = "In Progress",
                Priority = "High",
                EmployeeId = 1,
                ProjectId = 1
            };
            var result = await controller.PostTask(task);

            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public async Task PutTask_ValidId_UpdatesTask()
        {
            var context = GetContext(nameof(PutTask_ValidId_UpdatesTask));
            var task = new TaskItem {
                Id = 1,
                Title = "Fix Bug",
                Description = "Fix the null ref bug",
                Status = "In Progress",
                Priority = "High",
                EmployeeId = 1,
                ProjectId = 1
            };
            context.TaskItems.Add(task);
            await context.SaveChangesAsync();

            task.Title = "Updated";
            var controller = new TaskItemsApiController(context);
            var result = await controller.PutTask(1, task);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task PutTask_MismatchedId_ReturnsBadRequest()
        {
            var context = GetContext(nameof(PutTask_MismatchedId_ReturnsBadRequest));
            var controller = new TaskItemsApiController(context);

            var result = await controller.PutTask(1, new TaskItem { Id = 2 });

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteTask_ValidId_Deletes()
        {
            var context = GetContext(nameof(DeleteTask_ValidId_Deletes));
            context.TaskItems.Add(new TaskItem {
                Id = 1,
                Title = "Fix Bug",
                Description = "Fix the null ref bug",
                Status = "In Progress",
                Priority = "High",
                EmployeeId = 1,
                ProjectId = 1
            });
            await context.SaveChangesAsync();

            var controller = new TaskItemsApiController(context);
            var result = await controller.DeleteTask(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTask_InvalidId_ReturnsNotFound()
        {
            var context = GetContext(nameof(DeleteTask_InvalidId_ReturnsNotFound));
            var controller = new TaskItemsApiController(context);

            var result = await controller.DeleteTask(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
