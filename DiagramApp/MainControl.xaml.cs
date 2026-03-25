using DiagramApp.Helpers;
using DiagramApp.Models;
using DiagramApp.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DiagramApp
{
    public partial class MainControl : UserControl
    {
        private GanttChart _ganttChartService;

        // Событие для перехода на экран задачи
        public event Action<TaskInfo> TaskClicked;

        public MainControl(Hashtable parameters)
        {
            InitializeComponent();

            TaskRepository taskRepository = new TaskRepository((string)parameters["ConnectionString"], (int)parameters["RegID"]);
            _ganttChartService = new GanttChart(PersonNamesPanel, GanttCanvas, DateHeaderCanvas, taskRepository);
            
            // Устанавливаем даты по умолчанию
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(14);
        }

        /// <summary>
        /// Обработчик клика по названию задачи в левой панели
        /// </summary>
        private void TaskName_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var task = border?.Tag as TaskInfo;

            if (task != null)
            {
                TaskNavigationHelper.OpenTask(task.RowType, task.ItemID, task.Goods_TaskID, task.ClientOrderID);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик клика по задаче из диаграммы
        /// </summary>
        private void OnTaskClicked(object sender, TaskInfo task)
        {
            TaskNavigationHelper.OpenTask(task.RowType, task.ItemID, task.Goods_TaskID, task.ClientOrderID);
        }

        /// <summary>
        /// Обработка события нажатия на кнопку
        /// </summary>
        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (!StartDatePicker.SelectedDate.HasValue || !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите начальную и конечную дату", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var startDate = StartDatePicker.SelectedDate.Value;
            var endDate = EndDatePicker.SelectedDate.Value;

            if (startDate > endDate)
            {
                MessageBox.Show("Начальная дата не может быть позже конечной", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _ganttChartService.Build(startDate, endDate);
        }

        private void PersonHeader_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var personGroup = border?.Tag as PersonTasksGroup;

            if (personGroup != null)
            {
                // Переключаем состояние свернутости
                personGroup.IsExpanded = !personGroup.IsExpanded;

                // Обновляем диаграмму
                if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
                {
                    RebuildChart();
                }
            }
        }

        private void RebuildChart()
        {
            var startDate = StartDatePicker.SelectedDate.Value;
            var endDate = EndDatePicker.SelectedDate.Value;

            // Перестраиваем диаграмму с учетом свернутых групп
            _ganttChartService.Build(startDate, endDate);
        }

    #region [Прокрутка диаграммы] 

    // Горизонталь
    private void GanttScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            DateHeaderScrollViewer.ScrollToHorizontalOffset(GanttScrollViewer.HorizontalOffset);
            LeftScrollViewer.ScrollToVerticalOffset(GanttScrollViewer.VerticalOffset);
        }

        // Заголовки двигают диаграмму
        private void DateHeaderScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            GanttScrollViewer.ScrollToHorizontalOffset(DateHeaderScrollViewer.HorizontalOffset);
        }

        // Вертикаль
        private void LeftScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            GanttScrollViewer.ScrollToVerticalOffset(LeftScrollViewer.VerticalOffset);
        }

        #endregion
    }
}