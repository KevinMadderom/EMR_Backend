using MySql.Data.MySqlClient;
using EMR.Backend.Models;
using EMR.Backend.Data;
using System;
using System.Collections.Generic;

namespace EMR.Backend.Repositories
{
	public class PatientRepository
	{
		public bool AddPatient(Patient patient)
		{
			using (var conn = DatabaseHelper.GetConnection())
			{
				string query = "INSERT INTO Patient (FirstName, LastName, DateOfBirth, Gender, Address, Phone, Email, EmergencyContact) " +
							   "VALUES (@fname, @lname, @dob, @gender, @address, @phone, @email, @emergency)";

				MySqlCommand cmd = new MySqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@fname", patient.FirstName);
				cmd.Parameters.AddWithValue("@lname", patient.LastName);
				cmd.Parameters.AddWithValue("@dob", patient.DateOfBirth);
				cmd.Parameters.AddWithValue("@gender", patient.Gender);
				cmd.Parameters.AddWithValue("@address", patient.Address);
				cmd.Parameters.AddWithValue("@phone", patient.Phone);
				cmd.Parameters.AddWithValue("@email", patient.Email);
				cmd.Parameters.AddWithValue("@emergency", patient.EmergencyContact);

				try
				{
					conn.Open();
					return cmd.ExecuteNonQuery() > 0;
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error adding patient: " + ex.Message);
					return false;
				}
			}
		}

		public Patient GetPatientById(int patientId)
		{
			using (var conn = DatabaseHelper.GetConnection())
			{
				string query = "SELECT * FROM Patient WHERE PatientID = @id";
				MySqlCommand cmd = new MySqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@id", patientId);

				try
				{
					conn.Open();
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return new Patient
							{
								PatientID = reader.GetInt32("PatientID"),
								FirstName = reader.GetString("FirstName"),
								LastName = reader.GetString("LastName"),
								DateOfBirth = reader.GetDateTime("DateOfBirth"),
								Gender = reader.GetString("Gender"),
								Address = reader.GetString("Address"),
								Phone = reader.GetString("Phone"),
								Email = reader.GetString("Email"),
								EmergencyContact = reader.GetString("EmergencyContact")
							};
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error retrieving patient: " + ex.Message);
				}
			}
			return null;
		}
	}
}