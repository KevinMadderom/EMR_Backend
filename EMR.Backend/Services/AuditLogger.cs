using System;
using EMR.Backend.Models;
using EMR.Backend.Repositories;

namespace EMR.Backend.Services
{
    /// <summary>
    /// Writes every sensitive action to the AuditLog table. Supports the
    /// HIPAA-compliance and security narrative in NFR-04 / NFR-05.
    /// </summary>
    public class AuditLogger
    {
        private readonly AuditLogRepository _repo = new AuditLogRepository();

        public void Log(int staffId, int patientId, string action)
        {
            _repo.Add(new AuditLog
            {
                StaffID         = staffId,
                PatientID       = patientId,
                ActionPerformed = action ?? "",
                Time_Stamp      = DateTime.Now,
            });
        }

        public void LogStaff(int staffId, string action)   => Log(staffId, 0, action);
        public void LogPatient(int patientId, string action) => Log(0, patientId, action);

        // Convenience wrapper -- logs whichever side of the session is active.
        public void LogCurrent(string action)
        {
            if (Session.IsStaff)        Log(Session.CurrentStaff.StaffID, 0, action);
            else if (Session.IsPatient) Log(0, Session.CurrentPatient.PatientID, action);
            else                        Log(0, 0, "[anonymous] " + action);
        }
    }
}
