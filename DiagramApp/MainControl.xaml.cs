using DiagramApp.Helpers;
using DiagramApp.Models;
using DiagramApp.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DiagramApp
{
    public partial class MainControl : UserControl, INotifyPropertyChanged
    {
        #region [Переменные класса]
        private GanttChart _ganttChartService;
        private ObservableCollection<string> _listPersons;
        private ObservableCollection<string> _selectedPersons;
        private DateTime _currentStartDate;
        private DateTime _currentEndDate;

        // Событие для перехода на экран задачи
        public event Action<TaskInfo> TaskClicked;

        public ObservableCollection<string> ListPersons
        {
            get => _listPersons;
            set
            {
                _listPersons = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> SelectedPersons
        {
            get => _selectedPersons;
            set
            {
                _selectedPersons = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public MainControl(Hashtable parameters)
        {
            InitializeComponent();

            // Устанавливаем DataContext
            DataContext = this;

            PersonRepository personRepository = new PersonRepository((string)parameters["ConnectionString"]);
            TaskRepository taskRepository = new TaskRepository((string)parameters["ConnectionString"], (int)parameters["RegID"]);
            _ganttChartService = new GanttChart(PersonNamesPanel, GanttCanvas, DateHeaderCanvas, taskRepository);

            // Инициализация коллекций
            ListPersons = new ObservableCollection<string>(personRepository.GetPersons());
            SelectedPersons = new ObservableCollection<string>();

            // Устанавливаем даты по умолчанию
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today.AddDays(14);

            _currentStartDate = DateTime.Today;
            _currentEndDate = DateTime.Today.AddDays(14);
        }

        private void PersonsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null)
            {
                SelectedPersons.Clear();
                foreach (string item in listBox.SelectedItems)
                {
                    SelectedPersons.Add(item);
                }
            }
        }

        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (PersonsListBox != null)
            {
                PersonsListBox.UnselectAll();
            }
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

            _currentStartDate = startDate;
            _currentEndDate = endDate;

            // Обновляем диаграмму только здесь, при нажатии кнопки
            BuildChartWithFilter();
        }

        /// <summary>
        /// Построение диаграммы с учетом выбранных сотрудников
        /// Если никто не выбран - показываем всех
        /// </summary>
        private void BuildChartWithFilter()
        {
            // Если нет выбранных сотрудников, передаем null (будут показаны все)
            List<string> selectedPersonsList = SelectedPersons?.Any() == true
                ? SelectedPersons.ToList()
                : null;

            // Вызываем метод Build с фильтром по сотрудникам
            _ganttChartService.Build(_currentStartDate, _currentEndDate, selectedPersonsList);
        }
        private void PersonHeader_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var personGroup = border?.Tag as PersonTasksGroup;

            if (personGroup != null)
            {
                // Переключаем состояние свернутости
                personGroup.IsExpanded = !personGroup.IsExpanded;

                // Обновляем диаграмму при сворачивании/разворачивании
                if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
                {
                    BuildChartWithFilter();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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