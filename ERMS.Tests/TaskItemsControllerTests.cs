﻿using Xunit;
using Moq;
using ERMS.Controllers;
using ERMS.Models;
using ERMS.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERMS.Tests
{
    public class TaskItemsControllerTests
    {
        private readonly Mock<ITaskItemApiService> _taskApi;
        private readonly Mock<IEmployeeApiService> _employeeApi;
        private readonly Mock<IProjectApiService> _projectApi;
        private readonly TaskItemsController _controller;

        public TaskItemsControllerTests()
        {
            _taskApi = new Mock<ITaskItemApiService>();
            _employeeApi = new Mock<IEmployeeApiService>();
            _projectApi = new Mock<IProjectApiService>();
            _controller = new TaskItemsController(_taskApi.Object, _employeeApi.Object, _projectApi.Object);
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
            _taskApi.Setup(t => t.GetByIdAsync(999)).ReturnsAsync((TaskItem)null!);

            var result = await _controller.Edit(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_InvalidModel_ReturnsView()
        {
            var task = new TaskItem { Id = 1 };
            _controller.ModelState.AddModelError("Title", "Required");

            _employeeApi.Setup(e => e.GetAllAsync()).ReturnsAsync(new List<Employee>());
            _projectApi.Setup(p => p.GetAllAsync()).ReturnsAsync(new List<Project>());

            var result = await _controller.Edit(1, task);

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_Redirects()
        {
            var task = new TaskItem { Id = 1 };
            _taskApi.Setup(t => t.UpdateAsync(task)).ReturnsAsync(true);

            var result = await _controller.Edit(1, task);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_RedirectsToIndex()
        {
            _taskApi.Setup(t => t.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }
    }
}
