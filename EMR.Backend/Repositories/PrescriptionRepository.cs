using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class PrescriptionRepository
    {
        public int AddPrescription(Prescription p)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO Prescription
                    (PatientID, StaffID, MedicationID, DateIssued, Dosage, RefillsRemaining, SentToPharmacy)
                    VALUES (@p, @s, @m, @d, @dose, @refills, @sent);
                    SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@p", p.PatientID);
                cmd.Parameters.AddWithValue("@s", p.StaffID);
                cmd.Parameters.AddWithValue("@m", p.MedicationID);
                cmd.Parameters.AddWithValue("@d", p.DateIssued == default ? DateTime.Today : p.DateIssued);
                cmd.Parameters.AddWithValue("@dose", p.Dosage ?? "");
                cmd.Parameters.AddWithValue("@refills", p.RefillsRemaining);
                cmd.Parameters.AddWithValue("@sent", p.SentToPharmacy);
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddPrescription: " + ex.Message); return -1; }
            }
        }

        // FR-06 / FR-12: View prescriptions for a patient.
        public List<Prescription> GetByPatient(int patientId)
        {
            var list = new List<Prescription>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM Prescription WHERE PatientID=@id ORDER BY DateIssued DESC", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(Map(r));
                }
                catch (Exception ex) { Console.WriteLine("Prescription.GetByPatient: " + ex.Message); }
            }
            return list;
        }

        public bool MarkSentToPharmacy(int prescriptionId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE Prescription SET SentToPharmacy=TRUE WHERE PrescriptionID=@id", conn);
                cmd.Parameters.AddWithValue("@id", prescriptionId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("MarkSent: " + ex.Message); return false; }
            }
        }

        public bool DecrementRefill(int prescriptionId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE Prescription SET RefillsRemaining = GREATEST(0, RefillsRemaining-1) WHERE PrescriptionID=@id", conn);
                cmd.Parameters.AddWithValue("@id", prescriptionId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("DecrementRefill: " + ex.Message); return false; }
            }
        }

        private static Prescription Map(MySqlDataReader r) => new Prescription
        {
            PrescriptionID   = r.GetInt32("PrescriptionID"),
            PatientID        = r.GetInt32("PatientID"),
            StaffID          = r.GetInt32("StaffID"),
            MedicationID     = r.GetInt32("MedicationID"),
            DateIssued       = r.GetDateTime("DateIssued"),
            Dosage           = r.GetString("Dosage"),
            RefillsRemaining = r.GetInt32("RefillsRemaining"),
            SentToPharmacy   = r.GetBoolean("SentToPharmacy"),
        };
    }
}
