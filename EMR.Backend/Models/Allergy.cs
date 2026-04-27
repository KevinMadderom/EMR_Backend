using System;

namespace EMR.Backend.Models
{
    public class Allergy
    {
        public int AllergyID { get; set; } // PK
        public int PatientID { get; set; } // FK to Patient
        public string AllergenName { get; set; }
        public string Severity { get; set; } // e.g., Mild, Moderate, Severe
    }
}