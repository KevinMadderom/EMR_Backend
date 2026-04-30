using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class LabResultRepository
    {
        // FR-14: Doctor submits a lab result.
        public int AddLabResult(LabResult lr)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO LabResult
                    (PatientID, StaffID, TestName, TestDate, Result, Status)
                    VALUES (@p, @s, @n, @d, @r, @st);
                    SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@p", lr.PatientID);
                cmd.Parameters.AddWithValue("@s", lr.StaffID);
                cmd.Parameters.AddWithValue("@n", lr.TestName);
                cmd.Parameters.AddWithValue("@d", lr.TestDate == default ? DateTime.Today : lr.TestDate);
                cmd.Parameters.AddWithValue("@r", lr.Result ?? "");
                cmd.Parameters.AddWithValue("@st", lr.Status ?? "Final");
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddLabResult: " + ex.Message); return -1; }
            }
        }

        // §4.1: Patient views their lab results.
        public List<LabResult> GetByPatient(int patientId)
        {
            var list = new List<LabResult>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM LabResult WHERE PatientID=@id ORDER BY TestDate DESC", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(Map(r));
                }
                catch (Exception ex) { Console.WriteLine("LabResult.GetByPatient: " + ex.Message); }
            }
            return list;
        }

        private static LabResult Map(MySqlDataReader r) => new LabResult
        {
            LabResultID = r.GetInt32("LabResultID"),
            PatientID   = r.GetInt32("PatientID"),
            StaffID     = r.GetInt32("StaffID"),
            TestName    = r.GetString("TestName"),
            TestDate    = r.GetDateTime("TestDate"),
            Result      = r.GetString("Result"),
            Status      = r.GetString("Status"),
        };
    }
}
