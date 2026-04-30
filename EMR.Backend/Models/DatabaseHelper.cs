using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace EMR.Backend.Data
{
    public static class DatabaseHelper
    {
        private static readonly string _connString = LoadConnectionString();

        private static string LoadConnectionString()
        {
            try
            {
                var basePath = AppContext.BaseDirectory;
                var configPath = Path.Combine(basePath, "appsettings.json");
                if (File.Exists(configPath))
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(basePath)
                        .AddJsonFile("appsettings.json", optional: false)
                        .Build();
                    var conn = config.GetConnectionString("EMRKS");
                    if (!string.IsNullOrWhiteSpace(conn))
                        return conn;
                }
            }
            catch
            {
                // fall through to default
            }

            // Fallback for local dev if appsettings.json is missing
            return "server=localhost;database=EMRKS;uid=root;pwd=root123;";
        }

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
