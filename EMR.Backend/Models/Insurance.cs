using System;

namespace EMR.Backend.Models
{
    public class Insurance
    {
        public int InsuranceID { get; set; } // PK
        public int PatientID { get; set; } // FK to Patient
        public string ProviderName { get; set; }
        public string PolicyNumber { get; set; }
        public string CoverageDetails { get; set; }
    }
}