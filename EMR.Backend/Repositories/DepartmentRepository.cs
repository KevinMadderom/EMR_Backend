using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class DepartmentRepository
    {
        public int AddDepartment(string name)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = "INSERT INTO Department (DepartmentName) VALUES (@n); SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@n", name);
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddDepartment: " + ex.Message); return -1; }
            }
        }

        public List<Department> GetAll()
        {
            var list = new List<Department>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Department ORDER BY DepartmentName", conn);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new Department
                            {
                                DepartmentID   = r.GetInt32("DepartmentID"),
                                DepartmentName = r.GetString("DepartmentName")
                            });
                }
                catch (Exception ex) { Console.WriteLine("Department.GetAll: " + ex.Message); }
            }
            return list;
        }
    }
}
