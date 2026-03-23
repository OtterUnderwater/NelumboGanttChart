using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using DiagramApp.Models;
using DiagramApp.Helpers;
using System.Threading.Tasks;

namespace DiagramApp.Services
{
    public class GanttChart
    {
        #region [Переменные класа]
        // Элементы управления из MainWindow
        private ItemsControl _personNamesPanel;
        private Canvas _ganttCanvas;
        private Canvas _dateHeaderCanvas;
        // Параметры отображения
        private const double TaskHeight = 30;           // Высота задачи
        private const double TaskMargin = 2;             // Отступ между задачами
        private const double PersonHeaderHeight = 35;    // Высота заголовка сотрудника
        private const double DayWidth = 40;              //Ширна даты
        private Brush grayColor = new SolidColorBrush(Color.FromArgb(80, 200, 200, 200));
        // Данные
        private List<PersonTasksGroup> _currentData;
        #endregion

        public GanttChart(ItemsControl personNamesPanel, Canvas ganttCanvas, Canvas dateHeaderCanvas)
        {
            _personNamesPanel = personNamesPanel;
            _ganttCanvas = ganttCanvas;
            _dateHeaderCanvas = dateHeaderCanvas;
        }

        public void Build(DateTime startDate, DateTime endDate)
        {
            // Для теста
            //_currentData = CreateTest.GetTestPersonTasks(startDate, endDate);
            _currentData = TaskRepository.GetTasksInDateRange(startDate, endDate);

            if (!_currentData.Any())
            {
                MessageBox.Show("Нет задач в выбранном диапазоне дат", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Обновляем цвета для всех задач
            TaskColorHelper.UpdateTasksColorsForGroups(_currentData);

            // Отображаем имена сотрудников и задачи
            _personNamesPanel.ItemsSource = _currentData;

            // Рассчитываем общую высоту канваса
            double totalHeight = 0;
            foreach (var person in _currentData)
            {
                totalHeight += PersonHeaderHeight;
                totalHeight += person.Tasks.Count * (TaskHeight + TaskMargin);
            }

            // Рассчитываем ширину канваса
            int daysCount = (endDate - startDate).Days + 1;
            double canvasWidth = daysCount * DayWidth + 50;

            _ganttCanvas.Width = canvasWidth;
            _ganttCanvas.Height = totalHeight;

            // Очищаем канвас
            _ganttCanvas.Children.Clear();

            // Рисуем заголовки дат
            DrawDateHeaders(startDate, endDate);

            // Рисуем сетку
            DrawGrid(startDate, endDate, totalHeight);

            // Рисуем задачи
            DrawTasks(startDate, endDate);
        }

        private void DrawDateHeaders(DateTime startDate, DateTime endDate)
        {
            int daysCount = (endDate - startDate).Days + 1;

            // Очищаем заголовки
            _dateHeaderCanvas.Children.Clear();

            // Устанавливаем ширину канваса заголовков
            _dateHeaderCanvas.Width = daysCount * DayWidth;

            for (int i = 0; i < daysCount; i++)
            {
                var currentDate = startDate.AddDays(i);
                double x = i * DayWidth;

                // Фон для выходных дней
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

                // Контейнер для текста
                var dateContainer = new Grid
                {
                    Width = DayWidth
                };

                // Число месяца
                var dateText = new TextBlock
                {
                    Text = currentDate.ToString("dd.MM"),
                    FontSize = 11,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                // День недели
                var dayOfWeekText = new TextBlock
                {
                    Text = currentDate.ToString("ddd", new System.Globalization.CultureInfo("ru-RU")).ToUpper(),
                    FontSize = 9,
                    Foreground = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday ?
                                Brushes.Red : Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 25, 0, 0)
                };

                // Добавляем тексты в грид
                dateContainer.Children.Add(dateText);
                dateContainer.Children.Add(dayOfWeekText);

                // Размещаем грид на канвасе
                Canvas.SetLeft(dateContainer, x);
                Canvas.SetTop(dateContainer, 0);
                _dateHeaderCanvas.Children.Add(dateContainer);

                // Вертикальная линия
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

            // Добавляем левую границу
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
                for (int i = 0; i < person.Tasks.Count; i++)
                {
                    currentY += TaskHeight + TaskMargin;

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

        private void DrawTasks(DateTime startDate, DateTime endDate)
        {
            double currentY = 0;
            int daysCount = (endDate - startDate).Days + 1;

            foreach (var person in _currentData)
            {
                var personHeaderRect = new Rectangle
                {
                    Width = daysCount * DayWidth,
                    Height = TaskHeight,
                    Fill = grayColor,
                    Stroke = Brushes.Transparent
                };
                Canvas.SetLeft(personHeaderRect, 0);
                Canvas.SetTop(personHeaderRect, currentY);
                _ganttCanvas.Children.Add(personHeaderRect);

                // Пропускаем заголовок сотрудника
                currentY += PersonHeaderHeight;

                // Рисуем задачи (без фона)
                for (int i = 0; i < person.Tasks.Count; i++)
                {
                    var task = person.Tasks[i];
                    double taskY = currentY;
                    DrawTask(task, startDate, endDate, taskY);
                    currentY += TaskHeight + TaskMargin;
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
                    Height = TaskHeight,
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
                // Создаем линию фактического выполнения
                var actualLine = new Line
                {
                    X1 = 0,
                    Y1 = 0,
                    X2 = actualWidth,
                    Y2 = 0,
                    Stroke = Brushes.Black,
                    StrokeThickness = 3,
                };

                // Вычисляем вертикальную позицию для центрирования по задаче
                double verticalCenter = y + (TaskHeight / 2);

                // Размещаем линию в Canvas
                Canvas.SetLeft(actualLine, actualLeft);
                Canvas.SetTop(actualLine, verticalCenter);

                _ganttCanvas.Children.Add(actualLine);
            }
        }

        private double GetXPosition(DateTime date, DateTime startDate) => (date - startDate).Days * DayWidth;
 
        private double GetWidth(DateTime start, DateTime? end, DateTime chartStart, DateTime chartEnd)
        {
            // Если end == null, используем текущую дату
            DateTime actualEndDate = end ?? DateTime.Today;

            // Обрезаем по границам диаграммы
            DateTime actualStart = start < chartStart ? chartStart : start;
            DateTime actualEnd = actualEndDate > chartEnd ? chartEnd : actualEndDate;

            if (actualStart > actualEnd)
                return 0;

            int daysCount = (actualEnd - actualStart).Days + 1;
            return daysCount * DayWidth;
        }
    }
}
