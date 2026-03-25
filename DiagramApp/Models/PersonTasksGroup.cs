using System.Collections.Generic;
using System.ComponentModel;

namespace DiagramApp.Models
{
    public class PersonTasksGroup : INotifyPropertyChanged
    {
        private bool _isExpanded = false;
        private double _headerHeight = 30; // Высота заголовка сотрудника

        public string PersonName { get; set; }
        public List<TaskInfo> Tasks { get; set; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public double HeaderHeight
        {
            get => _headerHeight;
            set
            {
                if (_headerHeight != value)
                {
                    _headerHeight = value;
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
