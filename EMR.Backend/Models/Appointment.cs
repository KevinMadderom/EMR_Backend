using System;

namespace EMR.Backend.Models
{
    public class Appointment
    {
       
        public int AppointmentID { get; set; } // PK

        public int PatientID { get; set; } // FK to Patient
        public int StaffID { get; set; } // FK to Staff

        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }

        public string Status { get; set; } // e.g., Scheduled, Completed, Canceled
        public string AppointmentType { get; set; } // e.g., Consultation, Follow-up, Procedure
    }
}