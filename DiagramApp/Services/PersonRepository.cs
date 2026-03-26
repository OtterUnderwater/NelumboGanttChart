using DiagramApp.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace DiagramApp.Services
{
    public class PersonRepository
    {
        private string _connection;
        public PersonRepository(string connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Получение всех сотрудников
        /// </summary>
        /// <returns></returns>
        public List<string> GetPersons()
        {
            List<string> persons = new List<string>();

            string query = @"
                SELECT PersonName
                FROM dbo.Person
                ORDER BY PersonName";

            try
            {
                using (SqlConnection conn = new SqlConnection(_connection))
                {
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                persons.Add(reader.GetString(reader.GetOrdinal("PersonName")));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return persons;
        }
    }
}
