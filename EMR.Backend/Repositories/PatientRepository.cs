using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
    public class PatientRepository
    {
        // FR-01: Register a new patient (kiosk / tablet / web).
        public int AddPatient(Patient patient)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = @"INSERT INTO Patient
                    (FirstName, LastName, DateOfBirth, Gender, Address, Phone, Email, EmergencyContact, Username, PasswordHash)
                    VALUES (@fname, @lname, @dob, @gender, @addr, @phone, @email, @emergency, @username, @pwhash);
                    SELECT LAST_INSERT_ID();";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@fname", patient.FirstName);
                cmd.Parameters.AddWithValue("@lname", patient.LastName);
                cmd.Parameters.AddWithValue("@dob", patient.DateOfBirth);
                cmd.Parameters.AddWithValue("@gender", patient.Gender);
                cmd.Parameters.AddWithValue("@addr", patient.Address);
                cmd.Parameters.AddWithValue("@phone", patient.Phone);
                cmd.Parameters.AddWithValue("@email", patient.Email);
                cmd.Parameters.AddWithValue("@emergency", patient.EmergencyContact);
                cmd.Parameters.AddWithValue("@username", (object)patient.Username ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@pwhash", (object)patient.PasswordHash ?? DBNull.Value);

                try
                {
                    conn.Open();
                    var newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return newId;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error adding patient: " + ex.Message);
                    return -1;
                }
            }
        }

        // FR-04: Patient updates personal info.
        public bool UpdatePatient(Patient p)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = @"UPDATE Patient SET
                        FirstName=@fname, LastName=@lname, DateOfBirth=@dob,
                        Gender=@gender, Address=@addr, Phone=@phone,
                        Email=@email, EmergencyContact=@emergency
                    WHERE PatientID=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@fname", p.FirstName);
                cmd.Parameters.AddWithValue("@lname", p.LastName);
                cmd.Parameters.AddWithValue("@dob", p.DateOfBirth);
                cmd.Parameters.AddWithValue("@gender", p.Gender);
                cmd.Parameters.AddWithValue("@addr", p.Address);
                cmd.Parameters.AddWithValue("@phone", p.Phone);
                cmd.Parameters.AddWithValue("@email", p.Email);
                cmd.Parameters.AddWithValue("@emergency", p.EmergencyContact);
                cmd.Parameters.AddWithValue("@id", p.PatientID);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("UpdatePatient: " + ex.Message); return false; }
            }
        }

        public Patient GetPatientById(int patientId)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Patient WHERE PatientID = @id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", patientId);
                try
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                        if (reader.Read()) return Map(reader);
                }
                catch (Exception ex) { Console.WriteLine("GetPatientById: " + ex.Message); }
            }
            return null;
        }

        // FR-02: Patient login.
        public Patient GetPatientByUsername(string username)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Patient WHERE Username = @u LIMIT 1";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", username);
                try
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                        if (reader.Read()) return Map(reader);
                }
                catch (Exception ex) { Console.WriteLine("GetPatientByUsername: " + ex.Message); }
            }
            return null;
        }

        public List<Patient> GetAllPatients()
        {
            var list = new List<Patient>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "SELECT * FROM Patient ORDER BY LastName, FirstName";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(Map(r));
                }
                catch (Exception ex) { Console.WriteLine("GetAllPatients: " + ex.Message); }
            }
            return list;
        }

        public List<Patient> SearchPatients(string text)
        {
            var list = new List<Patient>();
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = @"SELECT * FROM Patient
                    WHERE LOWER(FirstName) LIKE @q
                       OR LOWER(LastName)  LIKE @q
                       OR LOWER(Email)     LIKE @q
                    ORDER BY LastName, FirstName";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@q", "%" + (text ?? "").ToLower() + "%");
                try
                {
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) list.Add(Map(r));
                }
                catch (Exception ex) { Console.WriteLine("SearchPatients: " + ex.Message); }
            }
            return list;
        }

        public bool SetPassword(int patientId, string passwordHash)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                string query = "UPDATE Patient SET PasswordHash=@h WHERE PatientID=@id";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@h", passwordHash);
                cmd.Parameters.AddWithValue("@id", patientId);
                try { conn.Open(); return cmd.ExecuteNonQuery() > 0; }
                catch (Exception ex) { Console.WriteLine("SetPassword: " + ex.Message); return false; }
            }
        }

        private static Patient Map(MySqlDataReader r)
        {
            return new Patient
            {
                PatientID        = r.GetInt32("PatientID"),
                FirstName        = r.GetString("FirstName"),
                LastName         = r.GetString("LastName"),
                DateOfBirth      = r.GetDateTime("DateOfBirth"),
                Gender           = r.GetString("Gender"),
                Address          = r.GetString("Address"),
                Phone            = r.GetString("Phone"),
                Email            = r.GetString("Email"),
                EmergencyContact = r.GetString("EmergencyContact"),
                Username         = r.IsDBNull(r.GetOrdinal("Username"))     ? null : r.GetString("Username"),
                PasswordHash     = r.IsDBNull(r.GetOrdinal("PasswordHash")) ? null : r.GetString("PasswordHash"),
            };
        }
    }
}
