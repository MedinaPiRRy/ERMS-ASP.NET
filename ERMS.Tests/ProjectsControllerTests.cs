using Xunit;
using Moq;
using ERMS.Controllers;
using ERMS.Models;
using ERMS.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERMS.Tests
{
    public class ProjectsControllerTests
    {
        private readonly Mock<IProjectApiService> _projectMock;
        private readonly Mock<IEmployeeApiService> _employeeMock;
        private readonly ProjectsController _controller;

        public ProjectsControllerTests()
        {
            _projectMock = new Mock<IProjectApiService>();
            _employeeMock = new Mock<IEmployeeApiService>();
            _controller = new ProjectsController(_projectMock.Object, _employeeMock.Object);
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
            var fakeProject = new Project { Id = 1, Name = "Invalid Project" };
            var assignedEmployeeIds = new int[] { 1, 2 };

            _controller.ModelState.AddModelError("Name", "Required");

            _employeeMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Employee>
            {
                new Employee { Id = 1, FullName = "John" },
                new Employee { Id = 2, FullName = "Jane" }
            });

            var result = await _controller.Create(fakeProject, assignedEmployeeIds);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(fakeProject, viewResult.Model);
        }

        [Fact]
        public async Task Create_Post_ValidModel_Redirects()
        {
            _employeeMock.Setup(e => e.GetAllAsync()).ReturnsAsync(new List<Employee>());
            _projectMock.Setup(p => p.CreateAsync(It.IsAny<Project>())).ReturnsAsync(true);

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

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_Failure_ReturnsViewWithError()
        {
            _projectMock.Setup(p => p.DeleteAsync(1)).ReturnsAsync(false);
            _projectMock.Setup(p => p.GetByIdAsync(1)).ReturnsAsync(new Project { Id = 1 });

            var result = await _controller.DeleteConfirmed(1);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<Project>(view.Model);
        }
    }
}
