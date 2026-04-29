
CREATE DATABASE EMRKS;
USE EMRKS;

CREATE TABLE Department (
DepartmentID		int				not NULL auto_increment,
DepartmentName		varchar(100)	not NULL,
primary key (DepartmentID)
);

CREATE TABLE Staff (
StaffID				int				not NULL auto_increment,
FirstName			varchar(100)	not NULL,
LastName			varchar(100)	not NULL,
Role				varchar(100)	not NULL,
PIN					varchar(100)	not NULL,
AccessCardNumber	varchar(100)	not NULL,
AuthorizationLevel	varchar(100)	not NULL,
DepartmentID		int				not NULL,
primary key (StaffID),
foreign key (DepartmentID) references Department(DepartmentID)
);

CREATE TABLE Patient (
PatientID			int				not NULL auto_increment,
FirstName			varchar(100)	not NULL,
LastName			varchar(100)	not NULL,
DateOfBirth			date			not NULL,
Gender				varchar(100)	not NULL,
Address				varchar(100)	not NULL,
Phone				varchar(100)	not NULL,
Email				varchar(100)	not NULL,
EmergencyContact	varchar(100)	not NULL,
primary key (PatientID)
);

CREATE TABLE Insurance (
InsuranceID			int				not NULL auto_increment,
PatientID			int				not NULL,
ProviderName		varchar(100)	not NULL,
PolicyNumber		varchar(100)	not NULL,
CoverageDetails		varchar(100)	not NULL,
primary key (InsuranceID),
foreign key (PatientID) references Patient(PatientID)
);

CREATE TABLE Billing (
BillingID			int				not NULL auto_increment,
TotalAmount			double			not NULL,
PaymentStatus		varchar(100)	not NULL,
PatientID			int				not NULL,
InsuranceID			int				not NULL,
primary key (BillingID),
foreign key (PatientID) references Patient(PatientID),
foreign key (InsuranceID) references Insurance(InsuranceID)
);

CREATE TABLE Appointment (
AppointmentID		int				not NULL auto_increment,
PatientID			int				not NULL,
StaffID				int				not NULL,
AppointmentDate		date			not NULL,
AppointmentTime		time			not NULL,
Status				varchar(100)	not NULL,
AppointmentType		varchar(100)	not NULL,
primary key (AppointmentID),
foreign key (PatientID) references Patient(PatientID),
foreign key (StaffID) references Staff(StaffID)
);

ALTER TABLE Billing
ADD AppointmentID INT,
ADD foreign key (AppointmentID) references Appointment(AppointmentID);

SHOW TABLES;

CREATE TABLE Medication (
MedicationID		int				not NULL auto_increment,
MedicationName		varchar(100)	not NULL,
KnownAllergies		varchar(100)	not NULL,
primary key (MedicationID)
);

CREATE TABLE Prescription (
PrescriptionID		int				not NULL auto_increment,
PatientID			int				not NULL,
StaffID				int				not NULL,
MedicationID		int				not NULL,
DateIssued			date			not NULL,
Dosage				varchar(100)	not NULL,
RefillsRemaining	int				not NULL,
SentToPharmacy		bool			not NULL,
primary key (PrescriptionID),
foreign key (PatientID) references Patient(PatientID),
foreign key (StaffID) references Staff(StaffID),
foreign key (MedicationID) references Medication(MedicationID)
);

CREATE TABLE MedicalRecord (
RecordID			int				not NULL auto_increment,
PatientID			int				not NULL,
StaffID				int 			not NULL,
DateCreated			date			not NULL,
DateModified		date			not NULL,
Diagnosis			varchar(100)	not NULL,
Notes				varchar(100)	not NULL,
RecordType			varchar(100)	not NULL,
primary key (RecordID),
foreign key (PatientID) references Patient(PatientID),
foreign key (StaffID) references Staff(StaffID)
);

CREATE TABLE LabResult (
LabResultID			int 			not NULL auto_increment,
PatientID			int				not NULL,
StaffID				int				not NULL,
TestName			varchar(100)	not NULL,
TestDate			date			not NULL,
Result				varchar(100)	not NULL,
Status				varchar(100)	not NULL,
primary key (LabResultID),
foreign key (PatientID) references Patient(PatientID),
foreign key (StaffID) references Staff(StaffID)
);

CREATE TABLE Immunization (
ImmunizationID		int				not NULL auto_increment,
PatientID			int				not NULL,
VaccineName			varchar(100)	not NULL,
DateAdministered	date			not NULL,
AdminStaffID		int				not NULL,
primary key (ImmunizationID),
foreign key (PatientID) references Patient(PatientID),
foreign key (AdminStaffID) references Staff(StaffID)
);

CREATE TABLE Allergy (
AllergyID			int				not NULL auto_increment,
PatientID			int				not NULL,
AllergenName		varchar(100)	not NULL,
Severity			varchar(100)	not NULL,
primary key (AllergyID),
foreign key (PatientID) references Patient(PatientID)
);

CREATE TABLE MedicationAllergy (
MedAllergyID		int				not NULL auto_increment,
MedicationID		int				not NULL,
AllergyID			int				not NULL,
primary key (MedAllergyID),
foreign key (MedicationID) references Medication(MedicationID),
foreign key (AllergyID) references Allergy(AllergyID)
);

CREATE TABLE Payment (
PaymentID			int				not NULL auto_increment,
BillingID			int				not NULL,
PatientID			int				not NULL,
AmountPaid			double			not NULL,
PaymentDate			date			not NULL,
PaymentMethod		varchar(100)	not NULL,
primary key (PaymentID),
foreign key (BillingID) references Billing(BillingID),
foreign key (PatientID) references Patient(PatientID)
);

CREATE TABLE AuditLog (
LogID				int 			not NULL auto_increment,
StaffID				int				not NULL,
PatientID			int				not NULL,
ActionPerformed		varchar(100)	not NULL,
Time_Stamp			datetime		not NULL,
primary key (LogID),
foreign key (StaffID) references Staff(StaffID),
foreign key (PatientID) references Patient(PatientID)
);

CREATE TABLE Notification (
NotificationID		int				not NULL auto_increment,
PatientID			int				not NULL,
StaffID				int				not NULL,
Message				varchar(100)	not NULL,
primary key (NotificationID),
foreign key (PatientID) references Patient(PatientID),
foreign key (StaffID) references Staff(StaffID)
);

CREATE TABLE RecordShare (
ShareID				int				not NULL auto_increment,
RecordID			int				not NULL,
PatientID			int				not NULL,
primary key (ShareID),
foreign key (RecordID) references MedicalRecord(RecordID),
foreign key (PatientID) references Patient(PatientID)
);

CREATE TABLE CorrectionRequest (
RequestID			int				not NULL auto_increment,
RecordID			int				not NULL,
PatientID			int				not NULL,
primary key (RequestID),
foreign key (RecordID) references MedicalRecord(RecordID),
foreign key (PatientID) references Patient(PatientID)
);

CREATE TABLE PreventativeSchedule (
ScheduleID			int				not NULL auto_increment,
PatientID			int				not NULL,
StaffID				int				not NULL,
primary key (ScheduleID),
foreign key (PatientID) references Patient(PatientID),
foreign key (StaffID) references Staff(StaffID)
);
