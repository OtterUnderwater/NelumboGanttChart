using DiagramApp.Models;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace DiagramApp.Helpers
{
    public class TaskColorHelper
    {
        /// <summary>
        /// Определяет цвет задачи в зависимости от статуса и дат
        /// </summary>
        /// <param name="task">Задача</param>
        /// <returns>Цвет для отображения</returns>
        public static Brush GetTaskColor(TaskInfo task)
        {
            SolidColorBrush blue = new SolidColorBrush(Color.FromArgb(180, 100, 149, 237));
            SolidColorBrush yellow = new SolidColorBrush(Color.FromArgb(180, 255, 220, 100));
            SolidColorBrush green = new SolidColorBrush(Color.FromArgb(180, 144, 238, 144));
            SolidColorBrush darkGreen = new SolidColorBrush(Color.FromArgb(76, 144, 175, 80));
            SolidColorBrush red = new SolidColorBrush(Color.FromArgb(180, 255, 100, 100));
            SolidColorBrush gray = new SolidColorBrush(Color.FromArgb(180, 200, 200, 200));
            SolidColorBrush orange = new SolidColorBrush(Color.FromArgb(180, 255, 165, 0));

            // Извлекаем основной статус (до символа \r или \n)
            string mainStatus = task.Status?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0] ?? "";

            switch (mainStatus)
            {
                case "Назначена":
                    return blue;
                case "Исполняется":
                    return (task.PlanExecDate < DateTime.Today && !task.FactExecDate.HasValue) ? red : yellow;
                case "Пауза":
                    return (task.PlanExecDate < DateTime.Today && !task.FactExecDate.HasValue) ? red : orange;
                case "Выполнена":
                    return (task.FactExecDate.HasValue && task.FactExecDate.Value <= task.PlanExecDate) ? green : red;
                case "Завершена":
                    return (task.FactExecDate.HasValue && task.FactExecDate.Value <= task.PlanExecDate) ? darkGreen : red;
                case "Отменена":
                    return gray;
                default:
                    return gray;
            }

        }

        /// <summary>
        /// Обновляет цвета для задач в сгруппированных данных
        /// </summary>
        public static void UpdateTasksColorsForGroups(List<PersonTasksGroup> groups)
        {
            foreach (var group in groups)
            {
                foreach (var task in group.Tasks)
                {
                    task.TaskColor = GetTaskColor(task);
                }
            }
        }
    }
}
