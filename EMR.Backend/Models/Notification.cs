
namespace EMR.Backend.Models
{
    public class Notification
    {
        public int NotificationID { get; set; } // PK
        public int PatientID { get; set; } // FK to Patient
        public int StaffID { get; set; } // FK to Staff (who created the notification)
        public string Message { get; set; }
    }
}