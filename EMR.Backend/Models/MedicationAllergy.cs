using System;

namespace EMR.Backend.Models
{
    public class MedicationAllergy
    {
        public int MedicationAllergyID { get; set; } // PK
        public int MedicationID { get; set; } // FK to Medication
        public int AllergyID { get; set; } // FK to Allergy
    }
}