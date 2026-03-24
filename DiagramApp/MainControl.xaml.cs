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

        public MainControl(Hashtable parameters)
        {
            InitializeComponent();

            TaskRepository taskRepository = new TaskRepository((string)parameters["ConnectionString"], (int)parameters["RegID"]);
            _ganttChartService = new GanttChart(PersonNamesPanel, GanttCanvas, DateHeaderCanvas, taskRepository);

            // Устанавливаем даты по умолчанию
            StartDatePicker.SelectedDate = new DateTime(2025, 11, 3);
            EndDatePicker.SelectedDate = new DateTime(2025, 11, 16);
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
