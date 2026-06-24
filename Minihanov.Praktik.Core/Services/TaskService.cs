using System;
using System.Collections.Generic;
using System.Linq;
using Minihanov.Praktik.Core.Models;
using Minihanov.Praktik.Core.Helpers;

namespace Minihanov.Praktik.Core.Services
{
    public class TaskService : ITaskService
    {
        private readonly List<TaskItem> _tasks = new List<TaskItem>();
        private readonly object _lockObject = new object();

        public void AddTask(TaskItem task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Название задачи не может быть пустым");

            lock (_lockObject)
            {
                task.Id = Guid.NewGuid().ToString();
                _tasks.Add((TaskItem)task.Clone());
            }
        }

        public void UpdateTask(TaskItem task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Название задачи не может быть пустым");

            lock (_lockObject)
            {
                var existing = _tasks.FirstOrDefault(t => t.Id == task.Id);
                if (existing == null)
                    throw new KeyNotFoundException($"Задача с ID {task.Id} не найдена");

                existing.Title = task.Title;
                existing.Description = task.Description;
                existing.Priority = task.Priority;
                existing.DueDate = task.DueDate;
                existing.Status = task.Status;
                existing.IsImportant = task.IsImportant;
            }
        }

        public void DeleteTask(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("ID задачи не может быть пустым");

            lock (_lockObject)
            {
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null)
                    throw new KeyNotFoundException($"Задача с ID {taskId} не найдена");

                _tasks.Remove(task);
            }
        }

        public TaskItem GetTaskById(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("ID задачи не может быть пустым");

            lock (_lockObject)
            {
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                return (TaskItem)task?.Clone();
            }
        }

        public IEnumerable<TaskItem> GetAllTasks()
        {
            lock (_lockObject)
            {
                return _tasks.Select(t => (TaskItem)t.Clone()).ToList();
            }
        }

        public IEnumerable<TaskItem> GetTasksByStatus(Minihanov.Praktik.Core.Models.TaskStatus status)
        {
            lock (_lockObject)
            {
                return _tasks.Where(t => t.Status == status)
                           .Select(t => (TaskItem)t.Clone())
                           .ToList();
            }
        }

        public IEnumerable<TaskItem> SearchTasks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return GetAllTasks();

            lock (_lockObject)
            {
                return _tasks.Where(t =>
                    t.Title.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (t.Description != null && t.Description.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0))
                    .Select(t => (TaskItem)t.Clone())
                    .ToList();
            }
        }

        public IEnumerable<TaskItem> GetTasksSortedByPriority()
        {
            lock (_lockObject)
            {
                return _tasks.OrderByDescending(t => t.Priority)
                           .ThenBy(t => t.DueDate)
                           .Select(t => (TaskItem)t.Clone())
                           .ToList();
            }
        }

        public IEnumerable<TaskItem> GetTasksSortedByDueDate()
        {
            lock (_lockObject)
            {
                return _tasks.OrderBy(t => t.DueDate)
                           .ThenByDescending(t => t.Priority)
                           .Select(t => (TaskItem)t.Clone())
                           .ToList();
            }
        }

        public void SaveToFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым");

            try
            {
                lock (_lockObject)
                {
                    JsonHelper.SaveToFile(_tasks, filePath);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при сохранении файла: {ex.Message}", ex);
            }
        }

        public void LoadFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым");

            try
            {
                var tasks = JsonHelper.LoadFromFile<List<TaskItem>>(filePath);
                if (tasks != null)
                {
                    lock (_lockObject)
                    {
                        _tasks.Clear();
                        foreach (var task in tasks)
                        {
                            if (string.IsNullOrWhiteSpace(task.Id))
                                task.Id = Guid.NewGuid().ToString();
                            _tasks.Add(task);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Ошибка при загрузке файла: {ex.Message}", ex);
            }
        }

        public (int Total, int New, int InProgress, int Completed, int Overdue, int Important) GetStatistics()
        {
            lock (_lockObject)
            {
                var total = _tasks.Count;
                var newTasks = _tasks.Count(t => t.Status == Minihanov.Praktik.Core.Models.TaskStatus.New);
                var inProgress = _tasks.Count(t => t.Status == Minihanov.Praktik.Core.Models.TaskStatus.InProgress);
                var completed = _tasks.Count(t => t.Status == Minihanov.Praktik.Core.Models.TaskStatus.Completed);
                var overdue = _tasks.Count(t => t.IsOverdue);
                var important = _tasks.Count(t => t.IsImportant);

                return (total, newTasks, inProgress, completed, overdue, important);
            }
        }

        public void ToggleImportant(string taskId)
        {
            if (string.IsNullOrWhiteSpace(taskId))
                throw new ArgumentException("ID задачи не может быть пустым");

            lock (_lockObject)
            {
                var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                if (task == null)
                    throw new KeyNotFoundException($"Задача с ID {taskId} не найдена");

                task.IsImportant = !task.IsImportant;
            }
        }
    }
}