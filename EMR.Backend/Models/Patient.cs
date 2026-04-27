namespace EMR.Backend.Models
{
    public class Patient
    {
         // primary key [cite: 212, 237]
        public int PatientID { get; set; } // PK

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string EmergencyContact { get; set; }

        public DateTime DateOfBirth { get; set; } 
    }
}