using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class InsuranceRepository
    {
        public int AddInsurance(Insurance i)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO Insurance
                    (PatientID, ProviderName, PolicyNumber, CoverageDetails)
                    VALUES (@p, @prov, @pol, @cov);
                    SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@p", i.PatientID);
                cmd.Parameters.AddWithValue("@prov", i.ProviderName);
                cmd.Parameters.AddWithValue("@pol", i.PolicyNumber);
                cmd.Parameters.AddWithValue("@cov", i.CoverageDetails ?? "");
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddInsurance: " + ex.Message); return -1; }
            }
        }

        public List<Insurance> GetByPatient(int patientId)
        {
            var list = new List<Insurance>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM Insurance WHERE PatientID=@id", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new Insurance
                            {
                                InsuranceID     = r.GetInt32("InsuranceID"),
                                PatientID       = r.GetInt32("PatientID"),
                                ProviderName    = r.GetString("ProviderName"),
                                PolicyNumber    = r.GetString("PolicyNumber"),
                                CoverageDetails = r.GetString("CoverageDetails"),
                            });
                }
                catch (Exception ex) { Console.WriteLine("Insurance.GetByPatient: " + ex.Message); }
            }
            return list;
        }
    }
}
