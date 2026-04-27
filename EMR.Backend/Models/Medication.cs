using System;

namespace EMR.Backend.Models
{
    public class Medication
    {
        public int MedicationID { get; set; } // PK
        public string MedicationName { get; set; }
        public string KnownAllergies { get; set; } // Comma-separated list of known allergies related to this medication
    }
}