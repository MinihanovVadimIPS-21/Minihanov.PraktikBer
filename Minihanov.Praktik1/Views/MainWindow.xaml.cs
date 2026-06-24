using Microsoft.Win32;
using Minihanov.Praktik.Core.Models;
using Minihanov.Praktik.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Minihanov.Praktik1
{
    public partial class MainWindow : Window
    {
        private readonly ITaskService _taskService;
        private TaskItem _selectedTask;
        private bool _isInitialized;

        public MainWindow()
        {
            _taskService = new TaskService();
            InitializeComponent();
            _isInitialized = true;
            RefreshTaskList(); 
        }

        private void RefreshTaskList()
        {
            var tasks = _taskService.GetAllTasks().ToList();
            if (cmbStatusFilter.SelectedItem is ComboBoxItem selectedStatus)
            {
                var statusText = selectedStatus.Content.ToString();
                if (statusText != "Все")
                {
                    var status = (Minihanov.Praktik.Core.Models.TaskStatus)Enum.Parse(
                        typeof(Minihanov.Praktik.Core.Models.TaskStatus), statusText);

                    var statusIds = new HashSet<object>(
                        _taskService.GetTasksByStatus(status).Select(t => (object)t.Id));
                    tasks = tasks.Where(t => statusIds.Contains(t.Id)).ToList();
                }
            }
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchIds = new HashSet<object>(
                    _taskService.SearchTasks(txtSearch.Text).Select(t => (object)t.Id));
                tasks = tasks.Where(t => searchIds.Contains(t.Id)).ToList();
            }
            if (cmbSort.SelectedItem is ComboBoxItem selectedSort)
            {
                var sortText = selectedSort.Content.ToString();
                if (sortText == "По приоритету")
                {
                    var order = _taskService.GetTasksSortedByPriority()
                                             .Select(t => t.Id).ToList();
                    tasks = tasks.OrderBy(t => order.IndexOf(t.Id)).ToList();
                }
                else if (sortText == "По сроку")
                {
                    var order = _taskService.GetTasksSortedByDueDate()
                                             .Select(t => t.Id).ToList();
                    tasks = tasks.OrderBy(t => order.IndexOf(t.Id)).ToList();
                }
            }

            lstTasks.ItemsSource = tasks;
            UpdateStatistics();
        }
        private void UpdateStatistics()
        {
            var stats = _taskService.GetStatistics();
            txtStats.Text = $"Статистика: Всего: {stats.Total} | Новая: {stats.New} | " +
                           $"В процессе: {stats.InProgress} | Завершена: {stats.Completed} | " +
                           $"Просрочено: {stats.Overdue} | Важных: {stats.Important}";
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditTaskWindow();
            if (addWindow.ShowDialog() == true)
            {
                _taskService.AddTask(addWindow.TaskItem);
                RefreshTaskList();
            }
        }
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask == null)
            {
                MessageBox.Show("Пожалуйста, выберите задачу для редактирования", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var editWindow = new AddEditTaskWindow(_selectedTask);
            if (editWindow.ShowDialog() == true)
            {
                _taskService.UpdateTask(editWindow.TaskItem);
                RefreshTaskList();
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask == null)
            {
                MessageBox.Show("Пожалуйста, выберите задачу для удаления", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Вы уверены, что хотите удалить задачу '{_selectedTask.Title}'?",
                              "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _taskService.DeleteTask(_selectedTask.Id);
                _selectedTask = null;
                RefreshTaskList();
            }
        }
        private void btnImportant_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask == null)
            {
                MessageBox.Show("Пожалуйста, выберите задачу", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _taskService.ToggleImportant(_selectedTask.Id);
            RefreshTaskList();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json",
                FileName = "tasks.json"
            };
            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    _taskService.SaveToFile(saveDialog.FileName);
                    MessageBox.Show("Задачи успешно сохранены", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    _taskService.LoadFromFile(openDialog.FileName);
                    RefreshTaskList();
                    MessageBox.Show("Задачи успешно загружены", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void lstTasks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTask = lstTasks.SelectedItem as TaskItem;
        }

        private void cmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;
            RefreshTaskList();
        }

        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;
            RefreshTaskList();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isInitialized) return;
            RefreshTaskList();
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = string.Empty;
        }
    }
}