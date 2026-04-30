using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class PreventativeScheduleRepository
    {
        // FR (Patient Reporting / Preventive schedule).
        public int Add(PreventativeSchedule s)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO PreventativeSchedule (PatientID, StaffID)
                             VALUES (@p, @s);
                             SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@p", s.PatientID);
                cmd.Parameters.AddWithValue("@s", s.StaffID);
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("PreventativeSchedule.Add: " + ex.Message); return -1; }
            }
        }

        public List<PreventativeSchedule> GetByPatient(int patientId)
        {
            var list = new List<PreventativeSchedule>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM PreventativeSchedule WHERE PatientID=@id", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new PreventativeSchedule
                            {
                                ScheduleID = r.GetInt32("ScheduleID"),
                                PatientID  = r.GetInt32("PatientID"),
                                StaffID    = r.GetInt32("StaffID"),
                            });
                }
                catch (Exception ex) { Console.WriteLine("PreventativeSchedule.GetByPatient: " + ex.Message); }
            }
            return list;
        }
    }
}
