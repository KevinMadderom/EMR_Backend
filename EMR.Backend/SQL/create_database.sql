-- ===========================================
-- EMR Database Setup - Phase 1
-- Run this in MySQL Workbench or mysql CLI
-- ===========================================

-- Create the database
CREATE DATABASE IF NOT EXISTS emr_db;
USE emr_db;

-- Department table (needed first since Staff references it)
CREATE TABLE Department (
    DepartmentID    int             NOT NULL AUTO_INCREMENT,
    DepartmentName  varchar(100)    NOT NULL,
    PRIMARY KEY (DepartmentID)
);

-- Staff table (needed before Patient-related tables that reference Staff)
CREATE TABLE Staff (
    StaffID             int             NOT NULL AUTO_INCREMENT,
    FirstName           varchar(100)    NOT NULL,
    LastName            varchar(100)    NOT NULL,
    Role                varchar(100)    NOT NULL,
    PIN                 varchar(100)    NOT NULL,
    AccessCardNumber    varchar(100)    NOT NULL,
    AuthorizationLevel  varchar(100)    NOT NULL,
    DepartmentID        int             NOT NULL,
    PRIMARY KEY (StaffID),
    FOREIGN KEY (DepartmentID) REFERENCES Department(DepartmentID)
);

-- Patient table (the focus of our prototype)
CREATE TABLE Patient (
    PatientID           int             NOT NULL AUTO_INCREMENT,
    FirstName           varchar(100)    NOT NULL,
    LastName            varchar(100)    NOT NULL,
    DateOfBirth         date            NOT NULL,
    Gender              varchar(100)    NOT NULL,
    Address             varchar(100)    NOT NULL,
    Phone               varchar(100)    NOT NULL,
    Email               varchar(100)    NOT NULL,
    EmergencyContact    varchar(100)    NOT NULL,
    PRIMARY KEY (PatientID)
);

-- ===========================================
-- Insert some test data to verify it works
-- ===========================================

INSERT INTO Patient (FirstName, LastName, DateOfBirth, Gender, Address, Phone, Email, EmergencyContact)
VALUES 
    ('John', 'Doe', '1985-03-15', 'Male', '123 Main St, Grand Forks, ND', '701-555-0101', 'john.doe@email.com', 'Jane Doe - 701-555-0102'),
    ('Sarah', 'Johnson', '1992-07-22', 'Female', '456 Oak Ave, Grand Forks, ND', '701-555-0201', 'sarah.j@email.com', 'Mike Johnson - 701-555-0202'),
    ('Alex', 'Smith', '2000-11-30', 'Non-binary', '789 Elm St, Grand Forks, ND', '701-555-0301', 'alex.smith@email.com', 'Pat Smith - 701-555-0302');

-- Verify the data was inserted
SELECT * FROM Patient;