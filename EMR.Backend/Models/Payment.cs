using System;

namespace EMR.Backend.Models
{
    public class Payment
    {
        public int PaymentID { get; set; } // PK
        public int BillingID { get; set; } // FK to Billing
        public int PatientID { get; set; } // FK to Patient
        public double AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } // e.g., Credit Card, Cash, Insurance
    }
}