
using MySql.Data.MySqlClient;

namespace EMR.Backend.Data
{
    public static class DatabaseHelper
    {
        private static string _connStr = "Server=localhost;Database=EMRKS;Uid=root;Pwd=root123;";

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connStr);
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
                catch
                {
                    return false;
                }
            }
        }
    }
}