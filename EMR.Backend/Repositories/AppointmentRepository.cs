using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class AppointmentRepository
    {
        // FR-15: Staff schedules an appointment for a patient.
        public int AddAppointment(Appointment a)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = @"INSERT INTO Appointment
                    (PatientID, StaffID, AppointmentDate, AppointmentTime, Status, AppointmentType)
                    VALUES (@p, @s, @d, @t, @st, @ty);
                    SELECT LAST_INSERT_ID();";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                cmd.Parameters.AddWithValue("@p", a.PatientID);
                cmd.Parameters.AddWithValue("@s", a.StaffID);
                cmd.Parameters.AddWithValue("@d", a.AppointmentDate);
                cmd.Parameters.AddWithValue("@t", a.AppointmentTime);
                cmd.Parameters.AddWithValue("@st", a.Status ?? "Scheduled");
                cmd.Parameters.AddWithValue("@ty", a.AppointmentType ?? "Consultation");
                try { conn.Open(); return Convert.ToInt32(cmd.ExecuteScalar()); }
                catch (Exception ex) { Console.WriteLine("AddAppointment: " + ex.Message); return -1; }
            }
        }

        public bool UpdateStatus(int appointmentId, string status)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE Appointment SET Status=@s WHERE AppointmentID=@id", conn);
                cmd.Parameters.AddWithValue("@s", status);
                cmd.Parameters.AddWithValue("@id", appointmentId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("UpdateStatus: " + ex.Message); return false; }
            }
        }

        public bool Reschedule(int appointmentId, DateTime newDate, TimeSpan newTime)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE Appointment SET AppointmentDate=@d, AppointmentTime=@t WHERE AppointmentID=@id", conn);
                cmd.Parameters.AddWithValue("@d", newDate);
                cmd.Parameters.AddWithValue("@t", newTime);
                cmd.Parameters.AddWithValue("@id", appointmentId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("Reschedule: " + ex.Message); return false; }
            }
        }

        // FR-05 / FR-11: View appointments for a patient (or doctor).
        public List<Appointment> GetByPatient(int patientId)
            => GetWhere("PatientID = @id", patientId);

        public List<Appointment> GetByStaff(int staffId)
            => GetWhere("StaffID = @id", staffId);

        public List<Appointment> GetAll()
            => GetWhere(null, 0);

        private List<Appointment> GetWhere(string whereClause, int idParam)
        {
            var list = new List<Appointment>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                string q = "SELECT * FROM Appointment";
                if (!string.IsNullOrEmpty(whereClause)) q += " WHERE " + whereClause;
                q += " ORDER BY AppointmentDate DESC, AppointmentTime DESC";
                MySqlCommand cmd = new MySqlCommand(q, conn);
                if (!string.IsNullOrEmpty(whereClause))
                    cmd.Parameters.AddWithValue("@id", idParam);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(Map(r));
                }
                catch (Exception ex) { Console.WriteLine("Appointment.Get: " + ex.Message); }
            }
            return list;
        }

        private static Appointment Map(MySqlDataReader r)
        {
            return new Appointment
            {
                AppointmentID   = r.GetInt32("AppointmentID"),
                PatientID       = r.GetInt32("PatientID"),
                StaffID         = r.GetInt32("StaffID"),
                AppointmentDate = r.GetDateTime("AppointmentDate"),
                AppointmentTime = r.GetTimeSpan("AppointmentTime"),
                Status          = r.GetString("Status"),
                AppointmentType = r.GetString("AppointmentType"),
            };
        }
    }
}
