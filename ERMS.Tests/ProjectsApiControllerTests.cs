using Xunit;
using Microsoft.EntityFrameworkCore;
using ERMS.Controllers.api;
using ERMS.Models;
using ERMS.Data;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ERMS.Tests
{
    public class ProjectsApiControllerTests
    {
        private readonly ILogger<ProjectsApiController> _logger;

        public ProjectsApiControllerTests()
        {
            var mockLogger = new Mock<ILogger<ProjectsApiController>>();
            _logger = mockLogger.Object;
        }

        // This method creates a new in-memory database for each test case
        // and initializes the ProjectsApiController with it.
        // This allows for isolated testing of the controller's methods
        // by providing a different name to each database.
        private (ProjectsApiController controller, ApplicationDbContext context) CreateController(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var context = new ApplicationDbContext(options);
            var controller = new ProjectsApiController(context, _logger);
            return (controller, context);
        }

        [Fact]
        public async Task GetAllProjects_ReturnsOkResult_WithListOfProjects()
        {
            // Arrange
            var (controller, context) = CreateController(nameof(GetAllProjects_ReturnsOkResult_WithListOfProjects) + "_GetAllProjects");

            var project1 = new Project { Id = 1, Name = "Project 1", Description = "Description 1", Status = "Done" };
            var project2 = new Project { Id = 2, Name = "Project 2", Description = "Description 2", Status = "In Progress" };
            context.Projects.Add(project1);
            context.Projects.Add(project2);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetProjects();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Project>>>(result);
            var projects = Assert.IsAssignableFrom<List<Project>>(actionResult.Value);
            Assert.Equal(2, projects.Count);
        }

        [Fact]
        public async Task PutProject_InvalidId_ReturnsBadRequest()
        {
            // Arrange
            var (controller, context) = CreateController(nameof(GetAllProjects_ReturnsOkResult_WithListOfProjects) + "_ListOfProjects");

            var project = new Project { Id = 2, Name = "Invalid Project" };

            // Act
            var result = await controller.PutProject(1, project);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task DeleteProject_ProjectNotFound_ReturnsNotFound()
        {
            // Arrange
            var (controller, context) = CreateController(nameof(GetAllProjects_ReturnsOkResult_WithListOfProjects) + "_NotFound");

            var projectId = 999;

            // Act
            var result = await controller.DeleteProject(projectId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetProject_NotFound_Returns404()
        {
            var (controller, context) = CreateController(nameof(GetAllProjects_ReturnsOkResult_WithListOfProjects) + "_WithListOfProjects");

            var result = await controller.GetProject(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PostProject_ValidProject_ReturnsCreatedAtAction()
        {
            var (controller, context) = CreateController(nameof(GetAllProjects_ReturnsOkResult_WithListOfProjects) + "_CreatedAtAction");

            var project = new Project { Id = 10, Name = "New Project", Description = "Test Description", Status = "Done" };
            var result = await controller.PostProject(project);
            Assert.IsType<CreatedAtActionResult>(result.Result);
        }

        [Fact]
        public async Task PutProject_IdMismatch_ReturnsBadRequest()
        {
            var (controller, context) = CreateController(nameof(GetAllProjects_ReturnsOkResult_WithListOfProjects) + "_BadRequest");

            var project = new Project { Id = 2 };
            var result = await controller.PutProject(1, project);
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task GetProject_ValidId_ReturnsProject()
        {
            var (controller, context) = CreateController(nameof(GetProject_ValidId_ReturnsProject));

            var project = new Project { Id = 1, Name = "Test", Description = "Desc", Status = "Open" };
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var result = await controller.GetProject(1);

            Assert.NotNull(result.Value);
            Assert.Equal("Test", result.Value.Name);
        }

        [Fact]
        public async Task PutProject_ValidUpdate_UpdatesProject()
        {
            var (controller, context) = CreateController(nameof(PutProject_ValidUpdate_UpdatesProject));

            var project = new Project { Id = 1, Name = "Old Name", Description = "Old", Status = "Pending" };
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var updatedProject = new Project { Id = 1, Name = "Updated Name", Description = "Updated", Status = "Done" };
            var result = await controller.PutProject(1, updatedProject);

            Assert.IsType<NoContentResult>(result);
            var updated = await context.Projects.FindAsync(1);
            Assert.Equal("Updated Name", updated!.Name);
        }

        [Fact]
        public async Task DeleteProject_ValidId_DeletesProject()
        {
            var (controller, context) = CreateController(nameof(DeleteProject_ValidId_DeletesProject));

            var project = new Project { Id = 1, Name = "To Delete", Description = "Trash", Status = "Done" };
            context.Projects.Add(project);
            await context.SaveChangesAsync();

            var result = await controller.DeleteProject(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(await context.Projects.FindAsync(1));
        }


    }
}
