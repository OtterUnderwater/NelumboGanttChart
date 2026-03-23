using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DiagramApp.Models
{
    /// <summary>
    /// Содержит информацию о задаче
    /// </summary>
    public class TaskInfo
    {
        public string TaskName { get; set; }
        public string Status { get; set; }
        public DateTime PlanStartDate { get; set; }
        public DateTime PlanExecDate { get; set; }
        public DateTime FactStartDate { get; set; }
        public DateTime? FactExecDate { get; set; }
        public Brush TaskColor { get; set; } // Цвет задачи
    }
}
