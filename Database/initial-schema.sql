-- SQL Server schema corresponding to the EF Core model. Use EF migrations for ongoing changes.
CREATE TABLE Users (Id int IDENTITY PRIMARY KEY,FirstName nvarchar(80) NOT NULL,LastName nvarchar(80) NOT NULL,Email nvarchar(180) NOT NULL UNIQUE,PasswordHash nvarchar(max) NOT NULL,Role nvarchar(30) NOT NULL,Phone nvarchar(30) NULL);
CREATE TABLE Events (Id int IDENTITY PRIMARY KEY,EventName nvarchar(150) NOT NULL,Description nvarchar(2000) NOT NULL,EventDate datetime2 NOT NULL,Location nvarchar(180) NOT NULL,OrganiserId int NOT NULL REFERENCES Users(Id));
CREATE TABLE Categories (Id int IDENTITY PRIMARY KEY,EventId int NOT NULL REFERENCES Events(Id) ON DELETE CASCADE,Name nvarchar(100) NOT NULL,DistanceKm decimal(8,2) NOT NULL);
CREATE TABLE Enrollments (Id int IDENTITY PRIMARY KEY,ParticipantId int NOT NULL REFERENCES Users(Id),CategoryId int NOT NULL REFERENCES Categories(Id),EnrolledAt datetime2 NOT NULL,CONSTRAINT UQ_Enrollment UNIQUE(ParticipantId,CategoryId));
CREATE TABLE Results (Id int IDENTITY PRIMARY KEY,EnrollmentId int NOT NULL UNIQUE REFERENCES Enrollments(Id) ON DELETE CASCADE,FinishTime time NOT NULL,Position int NOT NULL,Pace decimal(8,2) NOT NULL);
