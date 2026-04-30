-- ===========================================
-- EMRKS Full Schema (Delivery 3)
-- Use this for a fresh install. For an existing
-- Delivery-2 database, run database_schema.sql
-- (already done) followed by 01_migration_add_auth.sql.
-- ===========================================

DROP DATABASE IF EXISTS EMRKS;
CREATE DATABASE EMRKS;
USE EMRKS;

CREATE TABLE Department (
    DepartmentID    INT             NOT NULL AUTO_INCREMENT,
    DepartmentName  VARCHAR(100)    NOT NULL,
    PRIMARY KEY (DepartmentID)
);

CREATE TABLE Staff (
    StaffID             INT             NOT NULL AUTO_INCREMENT,
    FirstName           VARCHAR(100)    NOT NULL,
    LastName            VARCHAR(100)    NOT NULL,
    Role                VARCHAR(100)    NOT NULL,
    PIN                 VARCHAR(100)    NOT NULL,
    AccessCardNumber    VARCHAR(100)    NOT NULL,
    AuthorizationLevel  VARCHAR(100)    NOT NULL,
    DepartmentID        INT             NOT NULL,
    Username            VARCHAR(100)    NULL UNIQUE,
    PasswordHash        VARCHAR(255)    NULL,
    PRIMARY KEY (StaffID),
    FOREIGN KEY (DepartmentID) REFERENCES Department(DepartmentID)
);

CREATE TABLE Patient (
    PatientID           INT             NOT NULL AUTO_INCREMENT,
    FirstName           VARCHAR(100)    NOT NULL,
    LastName            VARCHAR(100)    NOT NULL,
    DateOfBirth         DATE            NOT NULL,
    Gender              VARCHAR(100)    NOT NULL,
    Address             VARCHAR(100)    NOT NULL,
    Phone               VARCHAR(100)    NOT NULL,
    Email               VARCHAR(100)    NOT NULL,
    EmergencyContact    VARCHAR(100)    NOT NULL,
    Username            VARCHAR(100)    NULL UNIQUE,
    PasswordHash        VARCHAR(255)    NULL,
    PRIMARY KEY (PatientID)
);

CREATE TABLE Insurance (
    InsuranceID     INT             NOT NULL AUTO_INCREMENT,
    PatientID       INT             NOT NULL,
    ProviderName    VARCHAR(100)    NOT NULL,
    PolicyNumber    VARCHAR(100)    NOT NULL,
    CoverageDetails VARCHAR(100)    NOT NULL,
    PRIMARY KEY (InsuranceID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID)
);

CREATE TABLE Appointment (
    AppointmentID   INT             NOT NULL AUTO_INCREMENT,
    PatientID       INT             NOT NULL,
    StaffID         INT             NOT NULL,
    AppointmentDate DATE            NOT NULL,
    AppointmentTime TIME            NOT NULL,
    Status          VARCHAR(100)    NOT NULL,
    AppointmentType VARCHAR(100)    NOT NULL,
    PRIMARY KEY (AppointmentID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (StaffID)   REFERENCES Staff(StaffID)
);

CREATE TABLE Billing (
    BillingID       INT             NOT NULL AUTO_INCREMENT,
    TotalAmount     DOUBLE          NOT NULL,
    PaymentStatus   VARCHAR(100)    NOT NULL,
    PatientID       INT             NOT NULL,
    InsuranceID     INT             NULL,
    AppointmentID   INT             NULL,
    PRIMARY KEY (BillingID),
    FOREIGN KEY (PatientID)     REFERENCES Patient(PatientID),
    FOREIGN KEY (InsuranceID)   REFERENCES Insurance(InsuranceID),
    FOREIGN KEY (AppointmentID) REFERENCES Appointment(AppointmentID)
);

CREATE TABLE Medication (
    MedicationID    INT             NOT NULL AUTO_INCREMENT,
    MedicationName  VARCHAR(100)    NOT NULL,
    KnownAllergies  VARCHAR(100)    NOT NULL,
    PRIMARY KEY (MedicationID)
);

CREATE TABLE Prescription (
    PrescriptionID    INT             NOT NULL AUTO_INCREMENT,
    PatientID         INT             NOT NULL,
    StaffID           INT             NOT NULL,
    MedicationID      INT             NOT NULL,
    DateIssued        DATE            NOT NULL,
    Dosage            VARCHAR(100)    NOT NULL,
    RefillsRemaining  INT             NOT NULL,
    SentToPharmacy    BOOL            NOT NULL,
    PRIMARY KEY (PrescriptionID),
    FOREIGN KEY (PatientID)    REFERENCES Patient(PatientID),
    FOREIGN KEY (StaffID)      REFERENCES Staff(StaffID),
    FOREIGN KEY (MedicationID) REFERENCES Medication(MedicationID)
);

CREATE TABLE MedicalRecord (
    RecordID        INT             NOT NULL AUTO_INCREMENT,
    PatientID       INT             NOT NULL,
    StaffID         INT             NOT NULL,
    DateCreated     DATE            NOT NULL,
    DateModified    DATE            NOT NULL,
    Diagnosis       VARCHAR(100)    NOT NULL,
    Notes           VARCHAR(1000)   NOT NULL,
    RecordType      VARCHAR(100)    NOT NULL,
    PRIMARY KEY (RecordID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (StaffID)   REFERENCES Staff(StaffID)
);

CREATE TABLE LabResult (
    LabResultID INT             NOT NULL AUTO_INCREMENT,
    PatientID   INT             NOT NULL,
    StaffID     INT             NOT NULL,
    TestName    VARCHAR(100)    NOT NULL,
    TestDate    DATE            NOT NULL,
    Result      VARCHAR(500)    NOT NULL,
    Status      VARCHAR(100)    NOT NULL,
    PRIMARY KEY (LabResultID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (StaffID)   REFERENCES Staff(StaffID)
);

CREATE TABLE Immunization (
    ImmunizationID    INT             NOT NULL AUTO_INCREMENT,
    PatientID         INT             NOT NULL,
    VaccineName       VARCHAR(100)    NOT NULL,
    DateAdministered  DATE            NOT NULL,
    AdminStaffID      INT             NOT NULL,
    PRIMARY KEY (ImmunizationID),
    FOREIGN KEY (PatientID)    REFERENCES Patient(PatientID),
    FOREIGN KEY (AdminStaffID) REFERENCES Staff(StaffID)
);

CREATE TABLE Allergy (
    AllergyID    INT             NOT NULL AUTO_INCREMENT,
    PatientID    INT             NOT NULL,
    AllergenName VARCHAR(100)    NOT NULL,
    Severity     VARCHAR(100)    NOT NULL,
    PRIMARY KEY (AllergyID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID)
);

CREATE TABLE MedicationAllergy (
    MedAllergyID INT NOT NULL AUTO_INCREMENT,
    MedicationID INT NOT NULL,
    AllergyID    INT NOT NULL,
    PRIMARY KEY (MedAllergyID),
    FOREIGN KEY (MedicationID) REFERENCES Medication(MedicationID),
    FOREIGN KEY (AllergyID)    REFERENCES Allergy(AllergyID)
);

CREATE TABLE Payment (
    PaymentID     INT             NOT NULL AUTO_INCREMENT,
    BillingID     INT             NOT NULL,
    PatientID     INT             NOT NULL,
    AmountPaid    DOUBLE          NOT NULL,
    PaymentDate   DATE            NOT NULL,
    PaymentMethod VARCHAR(100)    NOT NULL,
    PRIMARY KEY (PaymentID),
    FOREIGN KEY (BillingID) REFERENCES Billing(BillingID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID)
);

CREATE TABLE AuditLog (
    LogID           INT             NOT NULL AUTO_INCREMENT,
    StaffID         INT             NULL,
    PatientID       INT             NULL,
    ActionPerformed VARCHAR(255)    NOT NULL,
    Time_Stamp      DATETIME        NOT NULL,
    PRIMARY KEY (LogID),
    FOREIGN KEY (StaffID)   REFERENCES Staff(StaffID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID)
);

CREATE TABLE Notification (
    NotificationID INT             NOT NULL AUTO_INCREMENT,
    PatientID      INT             NOT NULL,
    StaffID        INT             NOT NULL,
    Message        VARCHAR(500)    NOT NULL,
    PRIMARY KEY (NotificationID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (StaffID)   REFERENCES Staff(StaffID)
);

CREATE TABLE RecordShare (
    ShareID   INT NOT NULL AUTO_INCREMENT,
    RecordID  INT NOT NULL,
    PatientID INT NOT NULL,
    PRIMARY KEY (ShareID),
    FOREIGN KEY (RecordID)  REFERENCES MedicalRecord(RecordID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID)
);

CREATE TABLE CorrectionRequest (
    RequestID INT NOT NULL AUTO_INCREMENT,
    RecordID  INT NOT NULL,
    PatientID INT NOT NULL,
    PRIMARY KEY (RequestID),
    FOREIGN KEY (RecordID)  REFERENCES MedicalRecord(RecordID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID)
);

CREATE TABLE PreventativeSchedule (
    ScheduleID INT NOT NULL AUTO_INCREMENT,
    PatientID  INT NOT NULL,
    StaffID    INT NOT NULL,
    PRIMARY KEY (ScheduleID),
    FOREIGN KEY (PatientID) REFERENCES Patient(PatientID),
    FOREIGN KEY (StaffID)   REFERENCES Staff(StaffID)
);

-- Seed an admin user. Default username 'admin' / password 'admin123'.
-- CHANGE THIS PASSWORD IMMEDIATELY ON FIRST LOGIN.
INSERT INTO Department (DepartmentName) VALUES ('Administration');

INSERT INTO Staff
    (FirstName, LastName, Role, PIN, AccessCardNumber, AuthorizationLevel,
     DepartmentID, Username, PasswordHash)
VALUES
    ('System', 'Admin', 'Admin', '0000', 'ADMIN-CARD-0000', 'Admin',
     LAST_INSERT_ID(), 'admin',
     '$2b$11$NscHxT9sOkYEz94NrJUiI.NUhIM/nDSRyRZp9FT/5BYJ5g44zikme');