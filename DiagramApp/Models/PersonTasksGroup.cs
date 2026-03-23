using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiagramApp.Models
{
    /// <summary>
    /// Содержит имя пользователя и его задачи
    /// </summary>
    public class PersonTasksGroup
    {
        public string PersonName { get; set; }
        public List<TaskInfo> Tasks { get; set; }
    }
}
