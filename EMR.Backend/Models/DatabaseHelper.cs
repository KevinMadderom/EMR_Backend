using MySql.Data.MySqlClient;
using System;

namespace EMR.Backend.Data
{
    public class DatabaseHelper
    {
        private static string _connString = "server=localhost;database=EMRKS;uid=root;pwd=root123;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connString);
        }

        public static bool TestConnection()
        {
            using (var conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database connection failed: {ex.Message}");
                    return false;
                }
            }
        }
    }
}
