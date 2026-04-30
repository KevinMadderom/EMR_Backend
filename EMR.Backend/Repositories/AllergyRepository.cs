using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class AllergyRepository
    {
        public int AddAllergy(Allergy a)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO Allergy
                    (PatientID, AllergenName, Severity)
                    VALUES (@p, @n, @s);
                    SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@p", a.PatientID);
                cmd.Parameters.AddWithValue("@n", a.AllergenName);
                cmd.Parameters.AddWithValue("@s", a.Severity ?? "Mild");
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddAllergy: " + ex.Message); return -1; }
            }
        }

        public List<Allergy> GetByPatient(int patientId)
        {
            var list = new List<Allergy>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM Allergy WHERE PatientID=@id ORDER BY AllergenName", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new Allergy
                            {
                                AllergyID    = r.GetInt32("AllergyID"),
                                PatientID    = r.GetInt32("PatientID"),
                                AllergenName = r.GetString("AllergenName"),
                                Severity     = r.GetString("Severity"),
                            });
                }
                catch (Exception ex) { Console.WriteLine("Allergy.GetByPatient: " + ex.Message); }
            }
            return list;
        }

        public bool DeleteAllergy(int allergyId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("DELETE FROM Allergy WHERE AllergyID=@id", conn);
                cmd.Parameters.AddWithValue("@id", allergyId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("DeleteAllergy: " + ex.Message); return false; }
            }
        }
    }
}
