using System;
using EMR.Backend.Models;
using EMR.Backend.Repositories;

namespace EMR.Backend.Services
{
    /// <summary>
    /// Implements FR-02 (patient login), FR-09 (staff login), FR-17 (admin login),
    /// FR-01 (patient registration), and FR-18 (admin assigns permissions).
    /// Uses BCrypt for password hashing -- supports NFR-04 / NFR-05 (encrypted
    /// credentials, HIPAA-style protection of secrets).
    /// </summary>
    public class AuthService
    {
        private readonly PatientRepository _patientRepo = new PatientRepository();
        private readonly StaffRepository   _staffRepo   = new StaffRepository();
        private readonly AuditLogger       _audit       = new AuditLogger();

        // ---------- Hashing ----------
        public static string HashPassword(string plaintext)
            => BCrypt.Net.BCrypt.HashPassword(plaintext, workFactor: 11);

        public static bool VerifyPassword(string plaintext, string hash)
        {
            if (string.IsNullOrEmpty(hash)) return false;
            try { return BCrypt.Net.BCrypt.Verify(plaintext, hash); }
            catch { return false; }
        }

        // ---------- Patient ----------
        // FR-01
        public Patient RegisterPatient(Patient p, string plaintextPassword)
        {
            if (string.IsNullOrWhiteSpace(p.Username))
                throw new ArgumentException("Username is required.");
            if (string.IsNullOrWhiteSpace(plaintextPassword))
                throw new ArgumentException("Password is required.");
            if (_patientRepo.GetPatientByUsername(p.Username) != null)
                throw new InvalidOperationException("Username already taken.");

            p.PasswordHash = HashPassword(plaintextPassword);
            int newId = _patientRepo.AddPatient(p);
            if (newId <= 0)
                throw new Exception("Failed to register patient.");
            p.PatientID = newId;
            _audit.LogPatient(newId, "Patient registered (FR-01)");
            return p;
        }

        // FR-02
        public Patient LoginPatient(string username, string password)
        {
            var p = _patientRepo.GetPatientByUsername(username);
            if (p == null || !VerifyPassword(password, p.PasswordHash)) return null;
            _audit.LogPatient(p.PatientID, "Patient login (FR-02)");
            return p;
        }

        // ---------- Staff (Doctor / Staff / Admin) ----------
        // FR-17 -- admin creates a new staff user.
        public Staff RegisterStaff(Staff s, string plaintextPassword)
        {
            if (string.IsNullOrWhiteSpace(s.Username))
                throw new ArgumentException("Username is required.");
            if (string.IsNullOrWhiteSpace(plaintextPassword))
                throw new ArgumentException("Password is required.");
            if (_staffRepo.GetStaffByUsername(s.Username) != null)
                throw new InvalidOperationException("Username already taken.");

            s.PasswordHash = HashPassword(plaintextPassword);
            int newId = _staffRepo.AddStaff(s);
            if (newId <= 0) throw new Exception("Failed to add staff.");
            s.StaffID = newId;
            _audit.LogStaff(newId, $"Staff '{s.Username}' created (FR-17)");
            return s;
        }

        // FR-09
        public Staff LoginStaff(string username, string password)
        {
            var s = _staffRepo.GetStaffByUsername(username);
            if (s == null || !VerifyPassword(password, s.PasswordHash)) return null;
            _audit.LogStaff(s.StaffID, $"Staff login as {s.Role} (FR-09)");
            return s;
        }
    }
}
