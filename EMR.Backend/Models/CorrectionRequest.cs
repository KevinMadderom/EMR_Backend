
namespace EMR.Backend.Models
{
    public class CorrectionRequest
    {
        public int RequestID { get; set; } // PK
        public int RecordID { get; set; } // FK to MedicalRecord
        public int PatientID { get; set; } // FK to Patient
    }
}