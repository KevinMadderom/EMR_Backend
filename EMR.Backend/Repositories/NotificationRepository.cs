using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class NotificationRepository
    {
        public int AddNotification(Notification n)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO Notification (PatientID, StaffID, Message)
                             VALUES (@p, @s, @m);
                             SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@p", n.PatientID);
                cmd.Parameters.AddWithValue("@s", n.StaffID);
                cmd.Parameters.AddWithValue("@m", n.Message ?? "");
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddNotification: " + ex.Message); return -1; }
            }
        }

        public List<Notification> GetByPatient(int patientId)
        {
            var list = new List<Notification>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT * FROM Notification WHERE PatientID=@id ORDER BY NotificationID DESC", conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new Notification
                            {
                                NotificationID = r.GetInt32("NotificationID"),
                                PatientID      = r.GetInt32("PatientID"),
                                StaffID        = r.GetInt32("StaffID"),
                                Message        = r.GetString("Message"),
                            });
                }
                catch (Exception ex) { Console.WriteLine("Notification.GetByPatient: " + ex.Message); }
            }
            return list;
        }
    }
}
