using System;

namespace EMR.Backend.Models
{
    public class LabResult
    {
        public int LabResultID { get; set; } // PK
        public int PatientID { get; set; } // FK to Patient
        public int StaffID { get; set; } // FK to Staff
        public string TestName { get; set; }
        public DateTime TestDate { get; set; }
        public string Result { get; set; }
        public string Status { get; set; } // e.g., Normal, Abnormal, Critical
    }
}