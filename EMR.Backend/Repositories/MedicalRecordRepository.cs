using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class MedicalRecordRepository
    {
        // Doctor (d) adds a new medical record for patient (p) -- §4.2.
        public int AddRecord(MedicalRecord m)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO MedicalRecord
                    (PatientID, StaffID, DateCreated, DateModified, Diagnosis, Notes, RecordType)
                    VALUES (@p, @s, @cd, @md, @diag, @notes, @type);
                    SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@p", m.PatientID);
                cmd.Parameters.AddWithValue("@s", m.StaffID);
                cmd.Parameters.AddWithValue("@cd", m.DateCreated == default ? DateTime.Today : m.DateCreated);
                cmd.Parameters.AddWithValue("@md", m.DateModified == default ? DateTime.Today : m.DateModified);
                cmd.Parameters.AddWithValue("@diag", m.Diagnosis ?? "");
                cmd.Parameters.AddWithValue("@notes", m.Notes ?? "");
                cmd.Parameters.AddWithValue("@type", m.RecordType ?? "Consultation");
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddRecord: " + ex.Message); return -1; }
            }
        }

        public bool UpdateRecord(MedicalRecord m)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"UPDATE MedicalRecord SET
                    DateModified=@md, Diagnosis=@diag, Notes=@notes, RecordType=@type
                    WHERE RecordID=@id";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@md", DateTime.Today);
                cmd.Parameters.AddWithValue("@diag", m.Diagnosis ?? "");
                cmd.Parameters.AddWithValue("@notes", m.Notes ?? "");
                cmd.Parameters.AddWithValue("@type", m.RecordType ?? "Consultation");
                cmd.Parameters.AddWithValue("@id", m.RecordID);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("UpdateRecord: " + ex.Message); return false; }
            }
        }

        // FR-03 / FR-10: View patient medical history.
        public List<MedicalRecord> GetByPatient(int patientId)
        {
            var list = new List<MedicalRecord>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = "SELECT * FROM MedicalRecord WHERE PatientID=@id ORDER BY DateCreated DESC";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(Map(r));
                }
                catch (Exception ex) { Console.WriteLine("MedicalRecord.GetByPatient: " + ex.Message); }
            }
            return list;
        }

        public MedicalRecord GetById(int recordId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM MedicalRecord WHERE RecordID=@id", conn);
                cmd.Parameters.AddWithValue("@id", recordId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader()) if (r.Read()) return Map(r);
                }
                catch (Exception ex) { Console.WriteLine("MedicalRecord.GetById: " + ex.Message); }
            }
            return null;
        }

        // FR-07 / FR-13: Track chronic-condition records over time.
        public List<MedicalRecord> GetChronicByPatient(int patientId)
        {
            var list = new List<MedicalRecord>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"SELECT * FROM MedicalRecord
                             WHERE PatientID=@id AND LOWER(RecordType) LIKE '%chronic%'
                             ORDER BY DateCreated";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(Map(r));
                }
                catch (Exception ex) { Console.WriteLine("GetChronic: " + ex.Message); }
            }
            return list;
        }

        private static MedicalRecord Map(MySqlDataReader r) => new MedicalRecord
        {
            RecordID     = r.GetInt32("RecordID"),
            PatientID    = r.GetInt32("PatientID"),
            StaffID      = r.GetInt32("StaffID"),
            DateCreated  = r.GetDateTime("DateCreated"),
            DateModified = r.GetDateTime("DateModified"),
            Diagnosis    = r.GetString("Diagnosis"),
            Notes        = r.GetString("Notes"),
            RecordType   = r.GetString("RecordType"),
        };
    }
}
