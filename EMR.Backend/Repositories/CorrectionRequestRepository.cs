using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class CorrectionRequestRepository
    {
        // Use-case scenario 8: patient requests medical-record corrections.
        public int AddRequest(CorrectionRequest c)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO CorrectionRequest (RecordID, PatientID)
                             VALUES (@r, @p);
                             SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@r", c.RecordID);
                cmd.Parameters.AddWithValue("@p", c.PatientID);
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddCorrectionRequest: " + ex.Message); return -1; }
            }
        }

        public List<CorrectionRequest> GetByPatient(int patientId)
        {
            var list = new List<CorrectionRequest>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM CorrectionRequest WHERE PatientID=@id", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new CorrectionRequest
                            {
                                RequestID = r.GetInt32("RequestID"),
                                RecordID  = r.GetInt32("RecordID"),
                                PatientID = r.GetInt32("PatientID"),
                            });
                }
                catch (Exception ex) { Console.WriteLine("CorrectionRequest.GetByPatient: " + ex.Message); }
            }
            return list;
        }
    }
}
