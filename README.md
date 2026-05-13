# EMRKS - Electronic Medical Record Keeping System

This is our CSCI 455 Electronic Medical Record project. The system is meant to help store and manage patient medical information using a database and a simple desktop application.

## Team Members

- Sadie LaPlant
- Ryker Ellingworth
- Kevin Madderom
- Madalynn Sauter

## Project Description

The EMR system is designed for a healthcare clinic to manage patient records more easily. Instead of using paper records, the system stores information in a MySQL database and allows different users to access the parts of the system they need.

The main users of the system are:

- Patients
- Doctors
- Staff
- Admins

## Main Features

Some of the main features include:

- Patient registration
- Patient login
- Staff and doctor login
- Viewing medical records
- Viewing lab results
- Managing appointments
- Managing prescriptions
- Admin user management
- Audit logs for important actions

## Technologies Used

- C#
- Windows Forms
- MySQL
- .NET
- Visual Studio

## Project Structure

- `EMR.Backend/` - main C# Windows Forms project
- `SQL/` - database setup and migration scripts
- `database_schema.sql` - SQL file for creating the database tables
- `EMR_Backend.sln` - Visual Studio solution file

## How to Run the Project

1. Install MySQL and open MySQL Workbench.
2. Create the EMR database using 'SQL/00_full_schema.sql'.
3. Update the database connection string in DatabaseHelper.cs and replace the password with your local MySQL root password:

Example connection string within DatabaseHelper.cs file:

private static string _connStr = "Server=localhost;Database=EMRKS;Uid=root;Pwd=YOUR_PASSWORD;";

4. Open `EMR_Backend.sln` in Visual Studio (.NET 9 SDK must be installed)
5. Build and run the project.



## First Login

A default admin user is seeded by the schema script:

| Username | Password   |
|----------|------------|
| `admin`  | `admin123` |


Log in as **Admin**, then add users from the admin console.
