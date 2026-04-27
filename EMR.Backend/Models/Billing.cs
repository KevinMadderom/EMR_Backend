
namespace EMR.Backend.Models
{
    public class Billing
    {
        public int BillingID { get; set; } // PK
        public double TotalAmount { get; set; }
        public string PaymentStatus { get; set; } // e.g., Paid, Unpaid, Pending
        public int PatientID { get; set; } // FK to Patient
        public int InsuranceID { get; set; } // FK to Insurance
        public int AppointmentID { get; set; } // FK to Appointment
    }
}