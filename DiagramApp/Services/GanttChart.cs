using DiagramApp.Helpers;
using DiagramApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DiagramApp.Services
{
    public class GanttChart
    {
        #region [Переменные класса]
        private ItemsControl _personNamesPanel;
        private Canvas _ganttCanvas;
        private Canvas _dateHeaderCanvas;
        private const double DayWidth = 40;
        private Brush grayColor = new SolidColorBrush(Color.FromArgb(80, 200, 200, 200));

        private List<PersonTasksGroup> _currentData;
        private TaskRepository _taskRepository;
        private DateTime _currentStartDate;
        private DateTime _currentEndDate;
        #endregion

        public GanttChart(ItemsControl personNamesPanel, Canvas ganttCanvas, Canvas dateHeaderCanvas, TaskRepository taskRepository)
        {
            _personNamesPanel = personNamesPanel;
            _ganttCanvas = ganttCanvas;
            _dateHeaderCanvas = dateHeaderCanvas;
            _taskRepository = taskRepository;
        }

        public void Build(DateTime startDate, DateTime endDate)
        {
            bool datesChanged = _currentStartDate != startDate || _currentEndDate != endDate;

            if (datesChanged || _currentData == null)
            {
                var newData = _taskRepository.GetTasksInDateRange(startDate, endDate);

                if (!newData.Any())
                {
                    MessageBox.Show("Нет задач в выбранном диапазоне дат", "Информация",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                TaskColorHelper.UpdateTasksColorsForGroups(newData);

                _currentData = newData;
                _currentStartDate = startDate;
                _currentEndDate = endDate;

                _personNamesPanel.ItemsSource = _currentData;
            }

            // Принудительно обновляем layout для получения реальных высот
            _personNamesPanel.UpdateLayout();

            // Получаем актуальные высоты заголовков сотрудников и задач
            UpdateHeights();

            RedrawChart(startDate, endDate);
        }

        private void UpdateHeights()
        {
            if (_currentData == null) return;

            try
            {
                // Ищем все заголовки сотрудников
                var personHeaders = FindVisualChildren<Border>(_personNamesPanel)
                    .Where(b => b.Background != null &&
                                b.Background.ToString().Contains("#50C8C8C8"));

                int personIndex = 0;
                foreach (var header in personHeaders)
                {
                    if (personIndex < _currentData.Count && header.ActualHeight > 0)
                    {
                        _currentData[personIndex].HeaderHeight = header.ActualHeight;
                        personIndex++;
                    }
                }

                // Ищем все задачи и обновляем их высоты
                var taskBorders = FindVisualChildren<Border>(_personNamesPanel);

                foreach (var border in taskBorders)
                {
                    var taskInfo = border.DataContext as TaskInfo;
                    if (taskInfo != null && border.ActualHeight > 0)
                    {
                        taskInfo.TaskHeight = border.ActualHeight;
                    }
                }
            }
            catch
            {
                // Если не удалось получить высоты, оставляем значения по умолчанию
            }
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void RedrawChart(DateTime startDate, DateTime endDate)
        {
            if (_currentData == null) return;

            // Рассчитываем общую высоту на основе реальных высот из моделей
            double totalHeight = 0;
            foreach (var person in _currentData)
            {
                totalHeight += person.HeaderHeight;
                if (person.IsExpanded)
                {
                    for (int i = 0; i < person.Tasks.Count; i++)
                    {
                        var task = person.Tasks[i];
                        totalHeight += task.TaskHeight;
                    }
                }
            }

            int daysCount = (endDate - startDate).Days + 1;
            double canvasWidth = daysCount * DayWidth + 50;

            _ganttCanvas.Width = canvasWidth;
            _ganttCanvas.Height = totalHeight;

            _ganttCanvas.Children.Clear();

            DrawDateHeaders(startDate, endDate);
            DrawGrid(startDate, endDate, totalHeight);
            DrawTasks(startDate, endDate);
        }

        private void DrawGrid(DateTime startDate, DateTime endDate, double totalHeight)
        {
            int daysCount = (endDate - startDate).Days + 1;

            // Вертикальные линии
            for (int i = 0; i <= daysCount; i++)
            {
                double x = i * DayWidth;
                var line = new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = totalHeight,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                _ganttCanvas.Children.Add(line);
            }

            // Горизонтальные линии
            double currentY = 0;
            foreach (var person in _currentData)
            {
                currentY += person.HeaderHeight;

                if (person.IsExpanded)
                {
                    for (int i = 0; i < person.Tasks.Count; i++)
                    {
                        var task = person.Tasks[i];
                        currentY += task.TaskHeight;

                        // Добавляем линию после задачи
                        var lineAfterTask = new Line
                        {
                            X1 = 0,
                            Y1 = currentY,
                            X2 = daysCount * DayWidth,
                            Y2 = currentY,
                            Stroke = Brushes.LightGray,
                            StrokeThickness = 0.5
                        };
                        _ganttCanvas.Children.Add(lineAfterTask);
                    }
                }
            }
        }

        private void DrawTasks(DateTime startDate, DateTime endDate)
        {
            double currentY = 0;
            int daysCount = (endDate - startDate).Days + 1;

            foreach (var person in _currentData)
            {
                // Рисуем фон заголовка сотрудника
                var personHeaderRect = new Rectangle
                {
                    Width = daysCount * DayWidth,
                    Height = person.HeaderHeight,
                    Fill = grayColor,
                    Stroke = Brushes.Transparent
                };
                Canvas.SetLeft(personHeaderRect, 0);
                Canvas.SetTop(personHeaderRect, currentY);
                _ganttCanvas.Children.Add(personHeaderRect);

                currentY += person.HeaderHeight;

                // Рисуем задачи только если группа развернута
                if (person.IsExpanded)
                {
                    for (int i = 0; i < person.Tasks.Count; i++)
                    {
                        var task = person.Tasks[i];
                        double taskY = currentY;
                        DrawTask(task, startDate, endDate, taskY);
                        currentY += task.TaskHeight;
                    }
                }
            }
        }

        private void DrawTask(TaskInfo task, DateTime startDate, DateTime endDate, double y)
        {
            // Плановый период
            double plannedLeft = GetXPosition(task.PlanStartDate, startDate);
            double plannedWidth = GetWidth(task.PlanStartDate, task.PlanExecDate, startDate, endDate);

            if (plannedWidth > 0 && plannedLeft >= 0)
            {
                var plannedRect = new Rectangle
                {
                    Width = plannedWidth,
                    Height = task.TaskHeight,
                    Fill = task.TaskColor,
                    RadiusX = 3,
                    RadiusY = 3,
                };
                Canvas.SetLeft(plannedRect, plannedLeft);
                Canvas.SetTop(plannedRect, y);
                _ganttCanvas.Children.Add(plannedRect);
            }

            // Фактический период
            double actualLeft = GetXPosition(task.FactStartDate, startDate);
            double actualWidth = GetWidth(task.FactStartDate, task.FactExecDate, startDate, endDate);

            if (actualWidth > 0 && actualLeft >= 0)
            {
                var actualLine = new Line
                {
                    X1 = 0,
                    Y1 = 0,
                    X2 = actualWidth,
                    Y2 = 0,
                    Stroke = Brushes.Black,
                    StrokeThickness = 3,
                };

                double verticalCenter = y + (task.TaskHeight / 2);
                Canvas.SetLeft(actualLine, actualLeft);
                Canvas.SetTop(actualLine, verticalCenter);
                _ganttCanvas.Children.Add(actualLine);
            }
        }
      
        private void DrawDateHeaders(DateTime startDate, DateTime endDate)
        {
            int daysCount = (endDate - startDate).Days + 1;
            _dateHeaderCanvas.Children.Clear();
            _dateHeaderCanvas.Width = daysCount * DayWidth;

            for (int i = 0; i < daysCount; i++)
            {
                var currentDate = startDate.AddDays(i);
                double x = i * DayWidth;

                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    var weekendBg = new Rectangle
                    {
                        Width = DayWidth,
                        Height = 50,
                        Fill = grayColor
                    };
                    Canvas.SetLeft(weekendBg, x);
                    Canvas.SetTop(weekendBg, 0);
                    _dateHeaderCanvas.Children.Add(weekendBg);
                }

                var dateContainer = new Grid { Width = DayWidth };

                var dateText = new TextBlock
                {
                    Text = currentDate.ToString("dd.MM"),
                    FontSize = 11,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                var dayOfWeekText = new TextBlock
                {
                    Text = currentDate.ToString("ddd", new System.Globalization.CultureInfo("ru-RU")).ToUpper(),
                    FontSize = 9,
                    Foreground = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday ?
                                Brushes.Red : Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 25, 0, 0)
                };

                dateContainer.Children.Add(dateText);
                dateContainer.Children.Add(dayOfWeekText);

                Canvas.SetLeft(dateContainer, x);
                Canvas.SetTop(dateContainer, 0);
                _dateHeaderCanvas.Children.Add(dateContainer);

                var line = new Line
                {
                    X1 = x + DayWidth,
                    Y1 = 0,
                    X2 = x + DayWidth,
                    Y2 = 50,
                    Stroke = Brushes.LightGray,
                    StrokeThickness = 0.5
                };
                _dateHeaderCanvas.Children.Add(line);
            }

            var leftBorder = new Line
            {
                X1 = 0,
                Y1 = 0,
                X2 = 0,
                Y2 = 50,
                Stroke = Brushes.LightGray,
                StrokeThickness = 1
            };
            _dateHeaderCanvas.Children.Add(leftBorder);
        }

        private double GetXPosition(DateTime date, DateTime startDate) => (date - startDate).Days * DayWidth;

        private double GetWidth(DateTime start, DateTime? end, DateTime chartStart, DateTime chartEnd)
        {
            DateTime actualEndDate = end ?? DateTime.Today;
            DateTime actualStart = start < chartStart ? chartStart : start;
            DateTime actualEnd = actualEndDate > chartEnd ? chartEnd : actualEndDate;

            if (actualStart > actualEnd)
                return 0;

            int daysCount = (actualEnd - actualStart).Days + 1;
            return daysCount * DayWidth;
        }

        // Метод для переключения состояния группы
        public void TogglePersonExpanded(PersonTasksGroup personGroup)
        {
            if (personGroup != null && _currentData != null)
            {
                personGroup.IsExpanded = !personGroup.IsExpanded;
                RedrawChart(_currentStartDate, _currentEndDate);
            }
        }
    }
}