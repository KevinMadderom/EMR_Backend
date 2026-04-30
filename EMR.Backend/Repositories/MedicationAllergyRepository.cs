using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class MedicationAllergyRepository
    {
        public int Link(int medicationId, int allergyId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO MedicationAllergy (MedicationID, AllergyID)
                             VALUES (@m, @a);
                             SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@m", medicationId);
                cmd.Parameters.AddWithValue("@a", allergyId);
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("MedAllergy.Link: " + ex.Message); return -1; }
            }
        }

        // Cross-checking prescriptions against patient allergies (Main Feature #3).
        public bool HasConflictForPatient(int medicationId, int patientId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"SELECT COUNT(*) FROM MedicationAllergy ma
                             JOIN Allergy a ON a.AllergyID = ma.AllergyID
                             WHERE ma.MedicationID=@m AND a.PatientID=@p";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@m", medicationId);
                cmd.Parameters.AddWithValue("@p", patientId);
                try
                {
                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch (Exception ex) { Console.WriteLine("HasConflict: " + ex.Message); return false; }
            }
        }
    }
}
