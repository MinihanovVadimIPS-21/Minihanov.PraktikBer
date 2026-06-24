using System;
using System.Collections.Generic;
using Minihanov.Praktik.Core.Models;

namespace Minihanov.Praktik.Core.Services
{
    public interface ITaskService
    {
        void AddTask(TaskItem task);
        void UpdateTask(TaskItem task);
        void DeleteTask(string taskId);
        TaskItem GetTaskById(string taskId);
        IEnumerable<TaskItem> GetAllTasks();
        IEnumerable<TaskItem> GetTasksByStatus(Minihanov.Praktik.Core.Models.TaskStatus status);
        IEnumerable<TaskItem> SearchTasks(string searchTerm);
        IEnumerable<TaskItem> GetTasksSortedByPriority();
        IEnumerable<TaskItem> GetTasksSortedByDueDate();
        void SaveToFile(string filePath);
        void LoadFromFile(string filePath);
        (int Total, int New, int InProgress, int Completed, int Overdue, int Important) GetStatistics();
        void ToggleImportant(string taskId);
    }
}