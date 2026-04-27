using System;

namespace EMR.Backend.Models
{
    public class Prescription
    {
        public int PrescriptionID { get; set; } // PK
        public int PatientID { get; set; } // FK to Patient
        public int StaffID { get; set; } // FK to Staff
        public int MedicationID { get; set; } // FK to Medication
        public DateTime DateIssued { get; set; }
        public string Dosage { get; set; }
        public int RefillsRemaining { get; set; }
        public bool SentToPharmacy { get; set; }
    }
}