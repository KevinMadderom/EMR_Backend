using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class ImmunizationRepository
    {
        public int AddImmunization(Immunization i)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO Immunization
                    (PatientID, VaccineName, DateAdministered, AdminStaffID)
                    VALUES (@p, @v, @d, @s);
                    SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@p", i.PatientID);
                cmd.Parameters.AddWithValue("@v", i.VaccineName);
                cmd.Parameters.AddWithValue("@d", i.DateAdministered == default ? DateTime.Today : i.DateAdministered);
                cmd.Parameters.AddWithValue("@s", i.AdminStaffID);
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddImmunization: " + ex.Message); return -1; }
            }
        }

        public List<Immunization> GetByPatient(int patientId)
        {
            var list = new List<Immunization>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM Immunization WHERE PatientID=@id ORDER BY DateAdministered DESC", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new Immunization
                            {
                                ImmunizationID   = r.GetInt32("ImmunizationID"),
                                PatientID        = r.GetInt32("PatientID"),
                                VaccineName      = r.GetString("VaccineName"),
                                DateAdministered = r.GetDateTime("DateAdministered"),
                                AdminStaffID     = r.GetInt32("AdminStaffID"),
                            });
                }
                catch (Exception ex) { Console.WriteLine("Immunization.GetByPatient: " + ex.Message); }
            }
            return list;
        }
    }
}
