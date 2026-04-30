using System;

namespace EMR.Backend.Models
{
    public class Staff
    {
        public int StaffID { get; set; } // PK

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }                 // e.g., Doctor, Nurse, Receptionist, Admin
        public string PIN { get; set; }
        public string AccessCardNumber { get; set; }
        public string AuthorizationLevel { get; set; }   // e.g., Doctor, Staff, Admin

        public int DepartmentID { get; set; }            // FK to Department

        // Auth (added in Delivery 3)
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}
