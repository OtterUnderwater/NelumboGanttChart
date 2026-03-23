using DiagramApp.Models;
using System;
using System.Collections.Generic;

namespace DiagramApp.Services
{
    public class CreateTest
    {

        public static List<PersonTasksGroup> GetTestPersonTasks(DateTime startDate, DateTime endDate)
        {
            var today = DateTime.Today;
            //добавить даты
            return new List<PersonTasksGroup>
            {
                new PersonTasksGroup { PersonName = "Марк", Tasks = new List<TaskInfo> {
                    new TaskInfo { TaskName = "Работа 1", Status = "Завершена",
                        PlanStartDate = today.AddDays(-5), PlanExecDate = today.AddDays(-3),
                        FactStartDate = today.AddDays(-5), FactExecDate = today.AddDays(-3)},
                    new TaskInfo { TaskName = "Работа 2", Status = "Завершена",
                        PlanStartDate = today.AddDays(-4), PlanExecDate = today.AddDays(-2),
                        FactStartDate = today.AddDays(-4), FactExecDate = today.AddDays(-2)},
                    new TaskInfo { TaskName = "Работа 3", Status = "Исполняется",
                        PlanStartDate = today.AddDays(-2), PlanExecDate = today.AddDays(2),
                        FactStartDate = today.AddDays(-2), FactExecDate = null}
                    }
                },
                new PersonTasksGroup { PersonName = "Иван", Tasks = new List<TaskInfo> {
                    new TaskInfo {TaskName = "Хорошая Работа 1", Status = "Завершена",
                        PlanStartDate = today.AddDays(-7), PlanExecDate = today.AddDays(-4),
                        FactStartDate = today.AddDays(-7), FactExecDate = today.AddDays(-4)},
                    new TaskInfo { TaskName = "Хорошая Работа 2", Status = "Назначена",
                        PlanStartDate = today.AddDays(-6), PlanExecDate = today.AddDays(-3),
                        FactStartDate = today.AddDays(-6), FactExecDate = today.AddDays(-3)}
                    }
                }
            };
        }

    }
}
