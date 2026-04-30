using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class RecordShareRepository
    {
        // Use-case scenario 5: patient shares records with another provider.
        public int AddShare(RecordShare s)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO RecordShare (RecordID, PatientID)
                             VALUES (@r, @p);
                             SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@r", s.RecordID);
                cmd.Parameters.AddWithValue("@p", s.PatientID);
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddShare: " + ex.Message); return -1; }
            }
        }

        public List<RecordShare> GetByPatient(int patientId)
        {
            var list = new List<RecordShare>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM RecordShare WHERE PatientID=@id", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new RecordShare
                            {
                                ShareID   = r.GetInt32("ShareID"),
                                RecordID  = r.GetInt32("RecordID"),
                                PatientID = r.GetInt32("PatientID"),
                            });
                }
                catch (Exception ex) { Console.WriteLine("RecordShare.GetByPatient: " + ex.Message); }
            }
            return list;
        }
    }
}
