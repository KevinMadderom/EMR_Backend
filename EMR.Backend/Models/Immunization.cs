using System;

namespace EMR.Backend.Models
{
    public class Immunization
    {
        public int ImmunizationID { get; set; } // PK
        public int PatientID { get; set; } // FK to Patient
        public string VaccineName { get; set; }
        public DateTime DateAdministered { get; set; }
        public int AdminStaffID { get; set; } // staff who administered the vaccine
    }
}