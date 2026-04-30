using System;

namespace EMR.Backend.Models
{
    public class MedicalRecord
    {
        public int RecordID { get; set; } // PK
        public int PatientID { get; set; } // FK to Patient
        public int StaffID { get; set; } // FK to Staff
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public string Diagnosis { get; set; }
        public string Notes { get; set; }
        public string RecordType { get; set; } // e.g., Consultation, Lab Result, Imaging, Prescription
    }
}