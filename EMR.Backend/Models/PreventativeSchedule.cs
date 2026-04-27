
namespace EMR.Backend.Models
{
    public class PreventativeSchedule
    {
        public int ScheduleID { get; set; } // PK
        public int PatientID { get; set; } // FK to Patient
        public int StaffID { get; set; } // FK to Staff (who created the schedule)
    }
}