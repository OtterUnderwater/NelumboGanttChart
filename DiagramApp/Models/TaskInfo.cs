using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace DiagramApp.Models
{
    /// <summary>
    /// Содержит информацию о задаче
    /// </summary>
    public class TaskInfo
    {
        private double _taskHeight = 30; // Высота задачи по умолчанию
        public string TaskName { get; set; }
        public string Status { get; set; }
        public DateTime PlanStartDate { get; set; }
        public DateTime PlanExecDate { get; set; }
        public DateTime FactStartDate { get; set; }
        public DateTime? FactExecDate { get; set; }
        public Brush TaskColor { get; set; } // Цвет задачи

        public double TaskHeight
        {
            get => _taskHeight;
            set
            {
                if (_taskHeight != value)
                {
                    _taskHeight = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
