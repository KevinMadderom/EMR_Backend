using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class StaffRepository
    {
        // FR-17 / FR-18: Admin adds new staff & assigns permissions.
        public int AddStaff(Staff s)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = @"INSERT INTO Staff
                    (FirstName, LastName, Role, PIN, AccessCardNumber, AuthorizationLevel,
                     DepartmentID, Username, PasswordHash)
                    VALUES (@fn, @ln, @role, @pin, @card, @auth, @dept, @user, @hash);
                    SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@fn", s.FirstName);
                cmd.Parameters.AddWithValue("@ln", s.LastName);
                cmd.Parameters.AddWithValue("@role", s.Role);
                cmd.Parameters.AddWithValue("@pin", s.PIN);
                cmd.Parameters.AddWithValue("@card", s.AccessCardNumber);
                cmd.Parameters.AddWithValue("@auth", s.AuthorizationLevel);
                cmd.Parameters.AddWithValue("@dept", s.DepartmentID);
                cmd.Parameters.AddWithValue("@user", (object)s.Username ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@hash", (object)s.PasswordHash ?? DBNull.Value);
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddStaff: " + ex.Message); return -1; }
            }
        }

        public bool UpdateStaff(Staff s)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = @"UPDATE Staff SET
                    FirstName=@fn, LastName=@ln, Role=@role, PIN=@pin,
                    AccessCardNumber=@card, AuthorizationLevel=@auth, DepartmentID=@dept
                    WHERE StaffID=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@fn", s.FirstName);
                cmd.Parameters.AddWithValue("@ln", s.LastName);
                cmd.Parameters.AddWithValue("@role", s.Role);
                cmd.Parameters.AddWithValue("@pin", s.PIN);
                cmd.Parameters.AddWithValue("@card", s.AccessCardNumber);
                cmd.Parameters.AddWithValue("@auth", s.AuthorizationLevel);
                cmd.Parameters.AddWithValue("@dept", s.DepartmentID);
                cmd.Parameters.AddWithValue("@id", s.StaffID);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("UpdateStaff: " + ex.Message); return false; }
            }
        }

        // FR-18: change permissions only.
        public bool UpdateAuthorizationLevel(int staffId, string level)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "UPDATE Staff SET AuthorizationLevel=@a WHERE StaffID=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@a", level);
                cmd.Parameters.AddWithValue("@id", staffId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("UpdateAuth: " + ex.Message); return false; }
            }
        }

        // FR-17: Admin removes a staff member.
        public bool DeleteStaff(int staffId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "DELETE FROM Staff WHERE StaffID=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", staffId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("DeleteStaff: " + ex.Message); return false; }
            }
        }

        public Staff GetStaffById(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Staff WHERE StaffID=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader()) if (r.Read()) return Map(r);
                }
                catch (Exception ex) { Console.WriteLine("GetStaffById: " + ex.Message); }
            }
            return null;
        }

        public Staff GetStaffByUsername(string username)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Staff WHERE Username=@u LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", username);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader()) if (r.Read()) return Map(r);
                }
                catch (Exception ex) { Console.WriteLine("GetStaffByUsername: " + ex.Message); }
            }
            return null;
        }

        public List<Staff> GetAllStaff()
        {
            var list = new List<Staff>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Staff ORDER BY LastName, FirstName";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(Map(r));
                }
                catch (Exception ex) { Console.WriteLine("GetAllStaff: " + ex.Message); }
            }
            return list;
        }

        public List<Staff> GetStaffByRole(string role)
        {
            var list = new List<Staff>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Staff WHERE Role=@role ORDER BY LastName";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@role", role);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(Map(r));
                }
                catch (Exception ex) { Console.WriteLine("GetStaffByRole: " + ex.Message); }
            }
            return list;
        }

        public bool SetPassword(int staffId, string passwordHash)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "UPDATE Staff SET PasswordHash=@h WHERE StaffID=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@h", passwordHash);
                cmd.Parameters.AddWithValue("@id", staffId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("SetPassword: " + ex.Message); return false; }
            }
        }

        private static Staff Map(MySqlDataReader r)
        {
            return new Staff
            {
                StaffID            = r.GetInt32("StaffID"),
                FirstName          = r.GetString("FirstName"),
                LastName           = r.GetString("LastName"),
                Role               = r.GetString("Role"),
                PIN                = r.GetString("PIN"),
                AccessCardNumber   = r.GetString("AccessCardNumber"),
                AuthorizationLevel = r.GetString("AuthorizationLevel"),
                DepartmentID       = r.GetInt32("DepartmentID"),
                Username           = r.IsDBNull(r.GetOrdinal("Username"))     ? null : r.GetString("Username"),
                PasswordHash       = r.IsDBNull(r.GetOrdinal("PasswordHash")) ? null : r.GetString("PasswordHash"),
            };
        }
    }
}
