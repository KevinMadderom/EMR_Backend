using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class PaymentRepository
    {
        public int AddPayment(Payment p)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO Payment
                    (BillingID, PatientID, AmountPaid, PaymentDate, PaymentMethod)
                    VALUES (@b, @p, @a, @d, @m);
                    SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@b", p.BillingID);
                cmd.Parameters.AddWithValue("@p", p.PatientID);
                cmd.Parameters.AddWithValue("@a", p.AmountPaid);
                cmd.Parameters.AddWithValue("@d", p.PaymentDate == default ? DateTime.Today : p.PaymentDate);
                cmd.Parameters.AddWithValue("@m", p.PaymentMethod ?? "Card");
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddPayment: " + ex.Message); return -1; }
            }
        }

        public List<Payment> GetByPatient(int patientId)
        {
            var list = new List<Payment>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM Payment WHERE PatientID=@id ORDER BY PaymentDate DESC", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new Payment
                            {
                                PaymentID     = r.GetInt32("PaymentID"),
                                BillingID     = r.GetInt32("BillingID"),
                                PatientID     = r.GetInt32("PatientID"),
                                AmountPaid    = r.GetDouble("AmountPaid"),
                                PaymentDate   = r.GetDateTime("PaymentDate"),
                                PaymentMethod = r.GetString("PaymentMethod"),
                            });
                }
                catch (Exception ex) { Console.WriteLine("Payment.GetByPatient: " + ex.Message); }
            }
            return list;
        }
    }
}
