using System;

namespace EMR.Backend.Models
{
    public class Staff
    {
        // primary key [cite: 212, 237]
        public int StaffID { get; set; } // PK

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public string PIN { get; set; }
        public string AccessCardNumber { get; set; }
        public string AuthorizationLevel { get; set; }

        public int DepartmentID { get; set; } // FK to Department
    }
}