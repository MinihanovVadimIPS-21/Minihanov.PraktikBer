using Minihanov.Praktik.Core.Models;
using System;
using System.Windows;

namespace Minihanov.Praktik1
{
    public partial class AddEditTaskWindow : Window
    {
        public TaskItem TaskItem { get; private set; }

        public AddEditTaskWindow(TaskItem task = null)
        {
            InitializeComponent();

            if (task != null)
            {
                TaskItem = (TaskItem)task.Clone();
                LoadTaskData();
                Title = "Редактирование задачи";
            }
            else
            {
                TaskItem = new TaskItem();
                Title = "Добавление задачи";
            }
        }

        private void LoadTaskData()
        {
            txtTitle.Text = TaskItem.Title;
            txtDescription.Text = TaskItem.Description;

            cmbPriority.SelectedIndex = (int)TaskItem.Priority;
            dpDueDate.SelectedDate = TaskItem.DueDate;
            cmbStatus.SelectedIndex = (int)TaskItem.Status;
            chkImportant.IsChecked = TaskItem.IsImportant;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Пожалуйста, введите название задачи", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTitle.Focus();
                return;
            }

            if (dpDueDate.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, выберите срок выполнения", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                dpDueDate.Focus();
                return;
            }

            TaskItem.Title = txtTitle.Text.Trim();
            TaskItem.Description = txtDescription.Text.Trim();
            TaskItem.Priority = (TaskPriority)cmbPriority.SelectedIndex;
            TaskItem.DueDate = dpDueDate.SelectedDate.Value;
            TaskItem.Status = (TaskStatus)cmbStatus.SelectedIndex;
            TaskItem.IsImportant = chkImportant.IsChecked ?? false;

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}