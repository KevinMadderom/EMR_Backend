using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class AuditLogRepository
    {
        // Sensitive-action log used by AuditLogger.
        public int Add(AuditLog log)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO AuditLog (StaffID, PatientID, ActionPerformed, Time_Stamp)
                             VALUES (@s, @p, @a, @t);
                             SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@s", log.StaffID == 0 ? (object)DBNull.Value : log.StaffID);
                cmd.Parameters.AddWithValue("@p", log.PatientID == 0 ? (object)DBNull.Value : log.PatientID);
                cmd.Parameters.AddWithValue("@a", log.ActionPerformed ?? "");
                cmd.Parameters.AddWithValue("@t", log.Time_Stamp == default ? DateTime.Now : log.Time_Stamp);
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AuditLog.Add: " + ex.Message); return -1; }
            }
        }

        public List<AuditLog> GetRecent(int limit = 100)
        {
            var list = new List<AuditLog>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM AuditLog ORDER BY Time_Stamp DESC LIMIT @lim", conn);
                cmd.Parameters.AddWithValue("@lim", limit);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new AuditLog
                            {
                                LogID           = r.GetInt32("LogID"),
                                StaffID         = r.IsDBNull(r.GetOrdinal("StaffID"))   ? 0 : r.GetInt32("StaffID"),
                                PatientID       = r.IsDBNull(r.GetOrdinal("PatientID")) ? 0 : r.GetInt32("PatientID"),
                                ActionPerformed = r.GetString("ActionPerformed"),
                                Time_Stamp      = r.GetDateTime("Time_Stamp"),
                            });
                }
                catch (Exception ex) { Console.WriteLine("AuditLog.GetRecent: " + ex.Message); }
            }
            return list;
        }
    }
}
