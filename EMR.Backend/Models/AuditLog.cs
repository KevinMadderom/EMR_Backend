using System;

namespace EMR.Backend.Models
{
    public class AuditLog
    {
        public int LogID { get; set; } // PK
        public int StaffID { get; set; } // FK to Staff
        public int PatientID { get; set; } // FK to Patient
        public string ActionPerformed { get; set; } // e.g., View, Edit, Delete
        public DateTime Time_Stamp { get; set; }
    }
}