using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class MedicationRepository
    {
        public int AddMedication(Medication m)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = "INSERT INTO Medication (MedicationName, KnownAllergies) VALUES (@n, @a); SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@n", m.MedicationName);
                cmd.Parameters.AddWithValue("@a", m.KnownAllergies ?? "");
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddMedication: " + ex.Message); return -1; }
            }
        }

        public List<Medication> GetAll()
        {
            var list = new List<Medication>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Medication ORDER BY MedicationName", conn);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new Medication
                            {
                                MedicationID   = r.GetInt32("MedicationID"),
                                MedicationName = r.GetString("MedicationName"),
                                KnownAllergies = r.GetString("KnownAllergies"),
                            });
                }
                catch (Exception ex) { Console.WriteLine("Medication.GetAll: " + ex.Message); }
            }
            return list;
        }

        public Medication GetById(int id)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Medication WHERE MedicationID=@id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        if (r.Read())
                            return new Medication
                            {
                                MedicationID   = r.GetInt32("MedicationID"),
                                MedicationName = r.GetString("MedicationName"),
                                KnownAllergies = r.GetString("KnownAllergies"),
                            };
                }
                catch (Exception ex) { Console.WriteLine("Medication.GetById: " + ex.Message); }
            }
            return null;
        }
    }
}
