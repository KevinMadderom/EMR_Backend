using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class BillingRepository
    {
        public int AddBilling(Billing b)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO Billing
                    (TotalAmount, PaymentStatus, PatientID, InsuranceID, AppointmentID)
                    VALUES (@t, @s, @p, @i, @a);
                    SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@t", b.TotalAmount);
                cmd.Parameters.AddWithValue("@s", b.PaymentStatus ?? "Unpaid");
                cmd.Parameters.AddWithValue("@p", b.PatientID);
                cmd.Parameters.AddWithValue("@i", b.InsuranceID == 0 ? (object)DBNull.Value : b.InsuranceID);
                cmd.Parameters.AddWithValue("@a", b.AppointmentID == 0 ? (object)DBNull.Value : b.AppointmentID);
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddBilling: " + ex.Message); return -1; }
            }
        }

        // FR-08 / FR-16: View billing for a patient.
        public List<Billing> GetByPatient(int patientId)
        {
            var list = new List<Billing>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM Billing WHERE PatientID=@id ORDER BY BillingID DESC", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(Map(r));
                }
                catch (Exception ex) { Console.WriteLine("Billing.GetByPatient: " + ex.Message); }
            }
            return list;
        }

        public bool UpdateStatus(int billingId, string status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE Billing SET PaymentStatus=@s WHERE BillingID=@id", conn);
                cmd.Parameters.AddWithValue("@s", status);
                cmd.Parameters.AddWithValue("@id", billingId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("Billing.UpdateStatus: " + ex.Message); return false; }
            }
        }

        public double GetBalance(int patientId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"SELECT IFNULL(
                    (SELECT SUM(TotalAmount) FROM Billing WHERE PatientID=@id),0) -
                    IFNULL(
                    (SELECT SUM(AmountPaid)  FROM Payment WHERE PatientID=@id),0)";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    var v = cmd.ExecuteScalar();
                    return v == null || v == DBNull.Value ? 0 : Convert.ToDouble(v);
                }
                catch (Exception ex) { Console.WriteLine("GetBalance: " + ex.Message); return 0; }
            }
        }

        private static Billing Map(MySqlDataReader r) => new Billing
        {
            BillingID     = r.GetInt32("BillingID"),
            TotalAmount   = r.GetDouble("TotalAmount"),
            PaymentStatus = r.GetString("PaymentStatus"),
            PatientID     = r.GetInt32("PatientID"),
            InsuranceID   = r.IsDBNull(r.GetOrdinal("InsuranceID")) ? 0 : r.GetInt32("InsuranceID"),
            AppointmentID = r.IsDBNull(r.GetOrdinal("AppointmentID")) ? 0 : r.GetInt32("AppointmentID"),
        };
    }
}
