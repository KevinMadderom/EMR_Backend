# EMRKS - Electronic Medical Record Keeping System

CSCI 455 course project. Team: Sadie LaPlant, Ryker Ellingworth, Kevin Madderom, Madalynn Sauter.

This is the Delivery 3 implementation: a Windows Forms desktop client backed by a MySQL database, implementing every functional requirement (FR-01 through FR-19) from Delivery 2.

## Architecture

- `EMR.Backend/` - single .NET 9 WinForms project (`net9.0-windows`).
  - `Models/` - POCO entities mirroring the database tables.
  - `Repositories/` - one class per table, hand-written ADO.NET (`MySql.Data`).
  - `Services/` - `AuthService` (BCrypt password hashing + login),
    `Session` (current-user holder for role-based access), `AuditLogger`
    (writes every sensitive action to the `AuditLog` table).
  - `UI/` - `LoginForm`, `RegisterPatientForm`, and four role-specific
    dashboards (`PatientDashboardForm`, `DoctorDashboardForm`,
    `StaffDashboardForm`, `AdminDashboardForm`).
- `SQL/` - schema and migration scripts.
- `appsettings.json` - connection string (do not commit real credentials).

## Setup (first time)

1. **Install MySQL** (8.x). Make sure the server is running on `localhost:3306`.
2. **Create the database.** In MySQL Workbench, open and execute one of:
   - `SQL/00_full_schema.sql` for a fresh install (recommended for new clones).
   - The existing `database_schema.sql` plus `SQL/01_migration_add_auth.sql`
     if you already have a Delivery-2 database deployed.
3. **Set the connection string.** Edit `EMR.Backend/appsettings.json` and
   replace the password with your local MySQL root password:
   ```json
   {
     "ConnectionStrings": {
       "EMRKS": "server=localhost;database=EMRKS;uid=root;pwd=YourPassword;"
     }
   }
   ```
4. **Build and run.** Open `EMR_Backend.sln` in Visual Studio 2022+ (with the
   .NET 9 SDK installed) and press F5. Or from the command line:
   ```
   dotnet run --project EMR.Backend
   ```

## First login

A default admin user is seeded by the schema script:

| Username | Password   |
|----------|------------|
| `admin`  | `admin123` |

Log in as **Staff / Doctor / Admin**, then change the password and add real
users from the Admin Console. (Patients can self-register from the login
screen via the **Register (Patient)** button.)

## Functional-requirement coverage

| FR    | Where it lives                                                |
|-------|---------------------------------------------------------------|
| FR-01 | `RegisterPatientForm` (kiosk-style registration)              |
| FR-02 | `LoginForm` -> Patient tab -> `AuthService.LoginPatient`      |
| FR-03 | `PatientDashboardForm` -> "Medical History" tab               |
| FR-04 | `PatientDashboardForm` -> "Profile" tab                       |
| FR-05 | `PatientDashboardForm` -> "Appointments" tab                  |
| FR-06 | `PatientDashboardForm` -> "Prescriptions" tab                 |
| FR-07 | `PatientDashboardForm` -> "Chronic Conditions" tab            |
| FR-08 | `PatientDashboardForm` -> "Billing" tab                       |
| FR-09 | `LoginForm` -> Staff tab -> `AuthService.LoginStaff`          |
| FR-10 | `DoctorDashboardForm` -> "Records" tab                        |
| FR-11 | `DoctorDashboardForm` -> "My Appointments" button             |
| FR-12 | `DoctorDashboardForm` -> "Prescriptions" tab                  |
| FR-13 | `DoctorDashboardForm` -> "Chronic Tracking" tab               |
| FR-14 | `DoctorDashboardForm` -> "Lab Results" tab (submit form)      |
| FR-15 | `StaffDashboardForm` -> "Schedule" tab                        |
| FR-16 | `StaffDashboardForm` -> "Patient Billing" tab                 |
| FR-17 | `AdminDashboardForm` -> "Add Staff User" + "Users" delete     |
| FR-18 | `AdminDashboardForm` -> "Users" tab -> Update permission      |
| FR-19 | `AdminDashboardForm` -> "DB Maintenance" tab + Audit Log tab  |

## Non-functional requirements

- **NFR-04 (encryption of credentials)** - all passwords stored as BCrypt
  hashes (`workFactor=11`). Plaintext never leaves the login form.
- **NFR-05 (HIPAA / audit)** - every login, profile edit, record creation,
  prescription, lab submission, and admin action writes a row to `AuditLog`
  via `AuditLogger`.
- **NFR-06 (RBAC)** - `Session` holds the current user and role; the login
  router picks the correct dashboard based on `Staff.AuthorizationLevel`.
- **NFR-09 (UI)** - WinForms native controls; helpers in `UiHelpers.cs`
  keep fonts/sizes consistent across forms.

## Re-generating the seeded admin password hash

If you want a different default admin password, generate a new BCrypt hash
and replace the value in `00_full_schema.sql` / `01_migration_add_auth.sql`.

The application uses the `BCrypt.Net-Next` package; you can reproduce the
hash by calling `AuthService.HashPassword("yourPassword")` from any short
.NET console snippet.
