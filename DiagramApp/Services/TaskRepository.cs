using DiagramApp.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;

namespace DiagramApp.Services
{
    public class TaskRepository
    {
        private string _connection;
        private int _regId;
        public TaskRepository(string connection, int regId)
        {
            _connection = connection;
            _regId = regId;
        }

        /// <summary>
        /// Получение всех задач
        /// </summary>
        /// <returns></returns>
        public List<PersonTasksGroup> GetTasks()
        {
            List<PersonTasksGroup> groupedTasks = new List<PersonTasksGroup>();
            Dictionary<string, List<TaskInfo>> tasksByPerson = new Dictionary<string, List<TaskInfo>>();

            using (SqlConnection connection = new SqlConnection(_connection))
            {
                using (SqlCommand command = new SqlCommand("dbo.obc_Task", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Добавляем параметры
                    command.Parameters.AddWithValue("@RegID", _regId);
                    command.Parameters.AddWithValue("@ActionID", 1);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        // Получаем индексы нужных колонок
                        int personName = reader.GetOrdinal("PersonID_PersonName"); // Фио сотрудника
                        int taskName = reader.GetOrdinal("TaskName"); // Название задачи
                        int status = reader.GetOrdinal("ExecNote"); // Статус задачи
                        int planStartDate = reader.GetOrdinal("PlanStartDate"); // Плановая дата начала выполнения задачи
                        int planExecDate = reader.GetOrdinal("ToDate"); // Плановая дата выполнения задачи
                        int factStartDate = reader.GetOrdinal("FactStartDate"); // Дата начала выполнения задачи
                        int factExecDate = reader.GetOrdinal("ExecDate"); // Дата выполнения задачи

                        while (reader.Read())
                        {
                            // Проверяем обязательные поля (не должны быть null)
                            if (reader.IsDBNull(personName) || reader.IsDBNull(taskName) || reader.IsDBNull(status) ||
                                reader.IsDBNull(planStartDate) || reader.IsDBNull(planExecDate) || reader.IsDBNull(factStartDate))
                            {
                                continue; // Пропускаем записи с null в обязательных полях
                            }

                            string currentPersonName = reader.GetString(personName);

                            TaskInfo task = new TaskInfo
                            {
                                TaskName = reader.GetString(taskName),
                                Status = reader.GetString(status),
                                PlanStartDate = reader.GetDateTime(planStartDate),
                                PlanExecDate = reader.GetDateTime(planExecDate),
                                FactStartDate = reader.GetDateTime(factStartDate),
                                FactExecDate = reader.IsDBNull(factExecDate) ? (DateTime?)null : reader.GetDateTime(factExecDate),
                            };

                            // Группируем по сотруднику
                            if (!tasksByPerson.ContainsKey(currentPersonName))
                            {
                                tasksByPerson[currentPersonName] = new List<TaskInfo>();
                            }
                            tasksByPerson[currentPersonName].Add(task);
                        }
                    }
                }
            }

            // Преобразуем словарь в список PersonTasksGroup
            groupedTasks = tasksByPerson
                .Select(kvp => new PersonTasksGroup
                {
                    PersonName = kvp.Key,
                    Tasks = kvp.Value
                })
                .OrderBy(g => g.PersonName)
                .ToList();

            return groupedTasks;
        }

        /// <summary>
        /// Получение задач по диапазону дат
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<PersonTasksGroup> GetTasksInDateRange(DateTime startDate, DateTime endDate)
        {
            var allTasks = GetTasks();
            var result = new List<PersonTasksGroup>();

            foreach (var person in allTasks)
            {
                var filteredTasks = person.Tasks
                    .Where(t => t.PlanStartDate >= startDate && t.PlanStartDate <= endDate)
                    .ToList();

                if (filteredTasks.Any())
                {
                    result.Add(new PersonTasksGroup
                    {
                        PersonName = person.PersonName,
                        Tasks = filteredTasks
                    });
                }
            }

            return result;
        }
    }
}
