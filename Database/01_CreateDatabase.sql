IF DB_ID(N'OrphansManagementDB') IS NULL CREATE DATABASE OrphansManagementDB;
GO
USE OrphansManagementDB;
GO

CREATE TABLE Roles(RoleID int IDENTITY PRIMARY KEY, RoleName nvarchar(50) NOT NULL UNIQUE);
CREATE TABLE Users(UserID int IDENTITY PRIMARY KEY, Username nvarchar(50) NOT NULL UNIQUE, PasswordHash varchar(64) NOT NULL, DisplayName nvarchar(120) NOT NULL, RoleID int NOT NULL REFERENCES Roles(RoleID), IsActive bit NOT NULL DEFAULT 1, CreatedDate datetime2 NOT NULL DEFAULT SYSDATETIME());
CREATE TABLE Guardians(GuardianID int IDENTITY PRIMARY KEY, FullName nvarchar(150) NOT NULL, Relationship nvarchar(50), NationalID nvarchar(50), PhoneNumber nvarchar(30), Address nvarchar(250), FamilySize int, HouseholdIncome decimal(18,2), HousingCondition nvarchar(100), VulnerabilityClass nvarchar(50), WalletDetails nvarchar(200));
CREATE TABLE Orphans(OrphanID int IDENTITY PRIMARY KEY, OrphanCode nvarchar(30) NOT NULL UNIQUE, FullName nvarchar(150) NOT NULL, Gender nvarchar(10), DateOfBirth date, PlaceOfBirth nvarchar(100), NationalID nvarchar(50) NULL, Governorate nvarchar(100), District nvarchar(100), Address nvarchar(250), EducationLevel nvarchar(100), SchoolName nvarchar(150), HealthCondition nvarchar(250), DisabilityStatus nvarchar(100), GuardianID int NULL REFERENCES Guardians(GuardianID), RegistrationDate date NOT NULL DEFAULT CAST(GETDATE() AS date), OrphanStatus nvarchar(30) NOT NULL DEFAULT 'Active', PhotoPath nvarchar(500), CreatedBy int NULL REFERENCES Users(UserID), CreatedDate datetime2 NOT NULL DEFAULT SYSDATETIME(), ModifiedDate datetime2 NULL);
CREATE UNIQUE INDEX UX_Orphans_NationalID ON Orphans(NationalID) WHERE NationalID IS NOT NULL AND NationalID<>'';
CREATE TABLE Sponsors(SponsorID int IDENTITY PRIMARY KEY, SponsorCode nvarchar(30) NOT NULL UNIQUE, SponsorName nvarchar(150) NOT NULL, SponsorType nvarchar(50), PhoneNumber nvarchar(30), EmailAddress nvarchar(150), Address nvarchar(250), Country nvarchar(100), IsActive bit NOT NULL DEFAULT 1, CreatedDate datetime2 NOT NULL DEFAULT SYSDATETIME());
CREATE TABLE Sponsorships(SponsorshipID int IDENTITY PRIMARY KEY, OrphanID int NOT NULL REFERENCES Orphans(OrphanID), SponsorID int NOT NULL REFERENCES Sponsors(SponsorID), SponsorshipAmount decimal(18,2) NOT NULL, PaymentFrequency nvarchar(30), StartDate date NOT NULL, EndDate date NULL, SponsorshipStatus nvarchar(30) NOT NULL DEFAULT 'Active', Notes nvarchar(max), CreatedDate datetime2 NOT NULL DEFAULT SYSDATETIME(), CONSTRAINT CK_SponsorshipAmount CHECK(SponsorshipAmount>0), CONSTRAINT CK_SponsorshipDates CHECK(EndDate IS NULL OR EndDate>=StartDate));
CREATE TABLE SponsorshipPayments(PaymentID int IDENTITY PRIMARY KEY, SponsorshipID int NOT NULL REFERENCES Sponsorships(SponsorshipID), Amount decimal(18,2) NOT NULL, PaymentDate date NOT NULL, ReceiptNumber nvarchar(50), PaymentStatus nvarchar(30) NOT NULL DEFAULT 'Received');
CREATE TABLE AssistanceTypes(AssistanceTypeID int IDENTITY PRIMARY KEY, TypeName nvarchar(100) NOT NULL UNIQUE);
CREATE TABLE AssistanceDistributions(DistributionID int IDENTITY PRIMARY KEY, OrphanID int NULL REFERENCES Orphans(OrphanID), AssistanceTypeID int NOT NULL REFERENCES AssistanceTypes(AssistanceTypeID), Amount decimal(18,2), Quantity decimal(18,2), DistributionDate date NOT NULL, Donor nvarchar(150), Project nvarchar(150), ReceiptNumber nvarchar(50), Location nvarchar(150), RecipientName nvarchar(150));
CREATE TABLE EducationRecords(EducationRecordID int IDENTITY PRIMARY KEY, OrphanID int NOT NULL REFERENCES Orphans(OrphanID), AcademicYear nvarchar(20), Grade nvarchar(50), AttendancePercent decimal(5,2), Performance nvarchar(100), SchoolFees decimal(18,2), Needs nvarchar(500), IsDropout bit NOT NULL DEFAULT 0, RecordDate date NOT NULL DEFAULT CAST(GETDATE() AS date));
CREATE TABLE HealthRecords(HealthRecordID int IDENTITY PRIMARY KEY, OrphanID int NOT NULL REFERENCES Orphans(OrphanID), ExaminationDate date NOT NULL, HealthCondition nvarchar(500), ChronicDiseases nvarchar(500), Disability nvarchar(250), Treatment nvarchar(500), Medication nvarchar(500), Referral nvarchar(500), SupportCost decimal(18,2));
CREATE TABLE SocialAssessments(AssessmentID int IDENTITY PRIMARY KEY, OrphanID int NOT NULL REFERENCES Orphans(OrphanID), AssessmentDate date NOT NULL, EconomicCondition nvarchar(500), HousingStatus nvarchar(500), ProtectionConcerns nvarchar(500), PsychosocialCondition nvarchar(500), EducationRisks nvarchar(500), ChildLabourRisk nvarchar(500), PriorityNeeds nvarchar(1000), Recommendations nvarchar(1000), AssessedBy int NULL REFERENCES Users(UserID));
CREATE TABLE Documents(DocumentID int IDENTITY PRIMARY KEY, OrphanID int NOT NULL REFERENCES Orphans(OrphanID), DocumentType nvarchar(80) NOT NULL, FileName nvarchar(255) NOT NULL, FilePath nvarchar(500) NOT NULL, UploadedBy int NULL REFERENCES Users(UserID), UploadedDate datetime2 NOT NULL DEFAULT SYSDATETIME());
CREATE TABLE AuditLogs(AuditLogID bigint IDENTITY PRIMARY KEY, UserID int NULL REFERENCES Users(UserID), ActionName nvarchar(80) NOT NULL, EntityName nvarchar(80), EntityID int, Details nvarchar(1000), ActionDate datetime2 NOT NULL DEFAULT SYSDATETIME(), MachineName nvarchar(128) NOT NULL DEFAULT HOST_NAME());
GO

INSERT INTO Roles(RoleName) VALUES ('Administrator'),('Registration Officer'),('Social Worker'),('Sponsorship Officer'),('Finance Officer'),('Distribution Officer'),('Manager'),('Auditor');
INSERT INTO Users(Username,PasswordHash,DisplayName,RoleID) SELECT 'admin',CONVERT(varchar(64),HASHBYTES('SHA2_256','Admin@123'),2),'System Administrator',RoleID FROM Roles WHERE RoleName='Administrator';
INSERT INTO AssistanceTypes(TypeName) VALUES ('Cash assistance'),('Food parcel'),('Clothing'),('School supplies'),('Medical assistance'),('Shelter assistance'),('Eid support'),('Vocational training'),('Psychological and social support');
GO

CREATE VIEW vw_DashboardStatistics AS
SELECT (SELECT COUNT(*) FROM Orphans) TotalOrphans,
       (SELECT COUNT(*) FROM Orphans WHERE OrphanStatus='Active') ActiveOrphans,
       (SELECT COUNT(DISTINCT OrphanID) FROM Sponsorships WHERE SponsorshipStatus='Active' AND (EndDate IS NULL OR EndDate>=CAST(GETDATE() AS date))) SponsoredOrphans,
       (SELECT COUNT(*) FROM Orphans o WHERE o.OrphanStatus='Active' AND NOT EXISTS(SELECT 1 FROM Sponsorships s WHERE s.OrphanID=o.OrphanID AND s.SponsorshipStatus='Active' AND (s.EndDate IS NULL OR s.EndDate>=CAST(GETDATE() AS date)))) UnsponsoredOrphans,
       (SELECT COUNT(*) FROM Sponsors WHERE IsActive=1) ActiveSponsors;
GO
CREATE VIEW vw_ExpiringSponsorships AS
SELECT sp.SponsorName,o.FullName,s.EndDate,s.SponsorshipAmount
FROM Sponsorships s JOIN Sponsors sp ON sp.SponsorID=s.SponsorID JOIN Orphans o ON o.OrphanID=s.OrphanID
WHERE s.SponsorshipStatus='Active' AND s.EndDate BETWEEN CAST(GETDATE() AS date) AND DATEADD(day,30,CAST(GETDATE() AS date));
GO
