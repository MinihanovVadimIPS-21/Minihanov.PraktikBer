using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Minihanov.Praktik.Core.Models
{
    public class TaskItem : INotifyPropertyChanged, ICloneable
    {
        private string _id;
        private string _title;
        private string _description;
        private TaskPriority _priority;
        private DateTime _dueDate;
        private Minihanov.Praktik.Core.Models.TaskStatus _status;
        private bool _isImportant;

        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public TaskPriority Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
            }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                OnPropertyChanged();
            }
        }

        public Minihanov.Praktik.Core.Models.TaskStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public bool IsImportant
        {
            get => _isImportant;
            set
            {
                _isImportant = value;
                OnPropertyChanged();
            }
        }

        public bool IsOverdue => Status != Minihanov.Praktik.Core.Models.TaskStatus.Completed && DueDate.Date < DateTime.Today;

        public TaskItem()
        {
            Id = Guid.NewGuid().ToString();
            Status = Minihanov.Praktik.Core.Models.TaskStatus.New;
            Priority = TaskPriority.Medium;
            DueDate = DateTime.Now.AddDays(7);
            IsImportant = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public object Clone()
        {
            return new TaskItem
            {
                Id = this.Id,
                Title = this.Title,
                Description = this.Description,
                Priority = this.Priority,
                DueDate = this.DueDate,
                Status = this.Status,
                IsImportant = this.IsImportant
            };
        }

        public override string ToString()
        {
            return $"{Title} - {Status} - {Priority} - {DueDate:dd.MM.yyyy}";
        }
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum TaskStatus
    {
        New,
        InProgress,
        Completed
    }
}