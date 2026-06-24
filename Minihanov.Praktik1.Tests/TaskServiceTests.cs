using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minihanov.Praktik.Core.Helpers;
using Minihanov.Praktik.Core.Models;
using Minihanov.Praktik.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Minihanov.Praktik1.Tests
{
    [TestClass]
    public class TaskManagerTests : IDisposable
    {
        private readonly ITaskService _taskService;
        private readonly string _testFilePath;

        public TaskManagerTests()
        {
            _taskService = new TaskService();
            _testFilePath = Path.Combine(Path.GetTempPath(), $"test_tasks_{Guid.NewGuid()}.json");
        }

        public void Dispose()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        [TestMethod]
        public void AddTask_ShouldAddTaskSuccessfully()
        {
            var task = new TaskItem
            {
                Title = "Test Task",
                Description = "Test Description",
                Priority = TaskPriority.High,
                DueDate = DateTime.Now.AddDays(5)
            };

            _taskService.AddTask(task);
            var tasks = _taskService.GetAllTasks();

            Assert.AreEqual(1, tasks.Count());
            Assert.AreEqual("Test Task", tasks.First().Title);
            Assert.AreEqual("Test Description", tasks.First().Description);
            Assert.AreEqual(TaskPriority.High, tasks.First().Priority);
            Assert.IsNotNull(tasks.First().Id);
        }

        [TestMethod]
        public void UpdateTask_ShouldUpdateTaskSuccessfully()
        {
            var task = new TaskItem { Title = "Original Title" };
            _taskService.AddTask(task);
            var createdTask = _taskService.GetAllTasks().First();

            createdTask.Title = "Updated Title";
            createdTask.Description = "Updated Description";
            createdTask.Priority = TaskPriority.Critical;
            createdTask.Status = TaskStatus.InProgress;
            createdTask.IsImportant = true;

            _taskService.UpdateTask(createdTask);
            var updatedTask = _taskService.GetTaskById(createdTask.Id);

            Assert.AreEqual("Updated Title", updatedTask.Title);
            Assert.AreEqual("Updated Description", updatedTask.Description);
            Assert.AreEqual(TaskPriority.Critical, updatedTask.Priority);
            Assert.AreEqual(TaskStatus.InProgress, updatedTask.Status);
            Assert.IsTrue(updatedTask.IsImportant);
        }

        [TestMethod]
        public void DeleteTask_ShouldDeleteTaskSuccessfully()
        {
            var task = new TaskItem { Title = "Test Task" };
            _taskService.AddTask(task);
            var createdTask = _taskService.GetAllTasks().First();

            _taskService.DeleteTask(createdTask.Id);
            var tasks = _taskService.GetAllTasks();

            Assert.AreEqual(0, tasks.Count());
        }

        [TestMethod]
        public void GetTaskById_ShouldReturnTaskSuccessfully()
        {
            var task = new TaskItem { Title = "Test Task" };
            _taskService.AddTask(task);
            var createdTask = _taskService.GetAllTasks().First();

            var retrievedTask = _taskService.GetTaskById(createdTask.Id);

            Assert.IsNotNull(retrievedTask);
            Assert.AreEqual(createdTask.Id, retrievedTask.Id);
            Assert.AreEqual(createdTask.Title, retrievedTask.Title);
        }

        [TestMethod]
        public void GetTaskById_ShouldReturnNull_WhenTaskNotFound()
        {
            var result = _taskService.GetTaskById(Guid.NewGuid().ToString());
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetAllTasks_ShouldReturnAllTasks()
        {
            _taskService.AddTask(new TaskItem { Title = "Task 1" });
            _taskService.AddTask(new TaskItem { Title = "Task 2" });
            _taskService.AddTask(new TaskItem { Title = "Task 3" });

            var tasks = _taskService.GetAllTasks();

            Assert.AreEqual(3, tasks.Count());
        }

        [TestMethod]
        public void GetTasksByStatus_ShouldReturnCorrectTasks()
        {
            _taskService.AddTask(new TaskItem { Title = "Task 1", Status = TaskStatus.New });
            _taskService.AddTask(new TaskItem { Title = "Task 2", Status = TaskStatus.Completed });
            _taskService.AddTask(new TaskItem { Title = "Task 3", Status = TaskStatus.New });

            var newTasks = _taskService.GetTasksByStatus(TaskStatus.New);

            Assert.AreEqual(2, newTasks.Count());
        }

        [TestMethod]
        public void SearchTasks_ShouldReturnResultsByTitle()
        {
            _taskService.AddTask(new TaskItem { Title = "Buy groceries", Description = "Milk and bread" });
            _taskService.AddTask(new TaskItem { Title = "Write report", Description = "Monthly report" });
            _taskService.AddTask(new TaskItem { Title = "Call plumber", Description = "Fix the sink" });

            var results = _taskService.SearchTasks("report");

            Assert.AreEqual(1, results.Count());
            Assert.AreEqual("Write report", results.First().Title);
        }

        [TestMethod]
        public void GetTasksSortedByPriority_ShouldReturnSortedTasks()
        {
            _taskService.AddTask(new TaskItem { Title = "Low", Priority = TaskPriority.Low });
            _taskService.AddTask(new TaskItem { Title = "High", Priority = TaskPriority.High });
            _taskService.AddTask(new TaskItem { Title = "Medium", Priority = TaskPriority.Medium });

            var sorted = _taskService.GetTasksSortedByPriority().ToList();

            Assert.AreEqual(TaskPriority.High, sorted[0].Priority);
            Assert.AreEqual(TaskPriority.Medium, sorted[1].Priority);
            Assert.AreEqual(TaskPriority.Low, sorted[2].Priority);
        }


        [TestMethod]
        public void LoadFromFile_ShouldLoadTasksSuccessfully()
        {
            _taskService.AddTask(new TaskItem { Title = "Task 1" });
            _taskService.AddTask(new TaskItem { Title = "Task 2" });
            _taskService.SaveToFile(_testFilePath);

            var newService = new TaskService();
            newService.LoadFromFile(_testFilePath);
            var tasks = newService.GetAllTasks();

            Assert.AreEqual(2, tasks.Count());
            Assert.IsTrue(tasks.Any(t => t.Title == "Task 1"));
            Assert.IsTrue(tasks.Any(t => t.Title == "Task 2"));
        }

        [TestMethod]
        public void GetStatistics_ShouldReturnCorrectCounts()
        {
            _taskService.AddTask(new TaskItem { Title = "Task 1", Status = TaskStatus.New });
            _taskService.AddTask(new TaskItem { Title = "Task 2", Status = TaskStatus.InProgress });
            _taskService.AddTask(new TaskItem { Title = "Task 3", Status = TaskStatus.Completed });
            _taskService.AddTask(new TaskItem { Title = "Task 4", Status = TaskStatus.New, IsImportant = true });

            var stats = _taskService.GetStatistics();

            Assert.AreEqual(4, stats.Total);
            Assert.AreEqual(2, stats.New);
            Assert.AreEqual(1, stats.InProgress);
            Assert.AreEqual(1, stats.Completed);
            Assert.AreEqual(1, stats.Important);
        }

        [TestMethod]
        public void ToggleImportant_ShouldToggleFlag()
        {
            var task = new TaskItem { Title = "Test Task", IsImportant = false };
            _taskService.AddTask(task);
            var createdTask = _taskService.GetAllTasks().First();

            _taskService.ToggleImportant(createdTask.Id);
            var updatedTask = _taskService.GetTaskById(createdTask.Id);

            Assert.IsTrue(updatedTask.IsImportant);

            _taskService.ToggleImportant(createdTask.Id);
            updatedTask = _taskService.GetTaskById(createdTask.Id);

            Assert.IsFalse(updatedTask.IsImportant);
        }

        [TestMethod]
        public void IsOverdue_ShouldReturnTrue_WhenTaskIsOverdue()
        {
            var task = new TaskItem
            {
                Title = "Overdue Task",
                DueDate = DateTime.Now.AddDays(-1),
                Status = TaskStatus.New
            };

            Assert.IsTrue(task.IsOverdue);
        }

        [TestMethod]
        public void TaskItem_Constructor_ShouldInitializeDefaultValues()
        {
            var task = new TaskItem();

            Assert.IsNotNull(task.Id);
            Assert.IsTrue(task.Id.Length > 0);
            Assert.AreEqual(TaskStatus.New, task.Status);
            Assert.AreEqual(TaskPriority.Medium, task.Priority);
            Assert.IsFalse(task.IsImportant);
        }

        [TestMethod]
        public void TaskItem_Clone_ShouldCreateDeepCopy()
        {
            var original = new TaskItem
            {
                Title = "Test Title",
                Description = "Test Description",
                Priority = TaskPriority.High,
                Status = TaskStatus.InProgress,
                IsImportant = true
            };

            var clone = (TaskItem)original.Clone();

            Assert.AreEqual(original.Title, clone.Title);
            Assert.AreEqual(original.Description, clone.Description);
            Assert.AreEqual(original.Priority, clone.Priority);
            Assert.AreEqual(original.Status, clone.Status);
            Assert.AreEqual(original.IsImportant, clone.IsImportant);

            clone.Title = "Modified";
            Assert.AreNotEqual(original.Title, clone.Title);
        }

        [TestMethod]
        public void JsonHelper_LoadFromFile_ShouldLoadDataSuccessfully()
        {
            var originalData = new List<TaskItem>
            {
                new TaskItem { Title = "Task 1" },
                new TaskItem { Title = "Task 2" }
            };

            JsonHelper.SaveToFile(originalData, _testFilePath);

            var loadedData = JsonHelper.LoadFromFile<List<TaskItem>>(_testFilePath);

            Assert.IsNotNull(loadedData);
            Assert.AreEqual(2, loadedData.Count);
            Assert.AreEqual("Task 1", loadedData[0].Title);
            Assert.AreEqual("Task 2", loadedData[1].Title);
        }
    }
}