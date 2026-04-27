
using EMR.Backend.Data;
using EMR.Backend.Models;
using EMR.Backend.Repositories;

namespace EMR.Backend
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing database connection...");
            if (DatabaseHelper.TestConnection())
            {
                Console.WriteLine("Database connection successful!");

                PatientRepository patientRepo = new PatientRepository();
                Console.WriteLine("\nSearching for patient with ID 1...");
                
                Patient p = patientRepo.GetPatientById(1);

                if (p != null)
                {
                    Console.WriteLine($"Patient found: {p.FirstName} {p.LastName}, DOB: {p.DateOfBirth.ToShortDateString()}");
                }
                else
                {
                    Console.WriteLine("Patient not found.");
                }
            }
        }
    }
}
