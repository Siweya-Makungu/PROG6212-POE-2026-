/* =========================================================
   RaceDay Database
   South African Road Running, Walking and Cycling Events
   SQL Server / SSMS
   ========================================================= */

-- Create database
IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END;
GO

USE RaceDayDB;
GO

/* =========================================================
   DROP TABLES
   Allows the script to be tested repeatedly
   ========================================================= */

IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL
    DROP TABLE dbo.Results;

IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL
    DROP TABLE dbo.Enrollments;

IF OBJECT_ID('dbo.Weather', 'U') IS NOT NULL
    DROP TABLE dbo.Weather;

IF OBJECT_ID('dbo.Routes', 'U') IS NOT NULL
    DROP TABLE dbo.Routes;

IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL
    DROP TABLE dbo.Categories;

IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL
    DROP TABLE dbo.Events;

IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;
GO

/* =========================================================
   USERS
   ========================================================= */

CREATE TABLE Users
(
    UserID INT IDENTITY(1,1) PRIMARY KEY,

    FirstName VARCHAR(50) NOT NULL,

    LastName VARCHAR(50) NOT NULL,

    Email VARCHAR(100) NOT NULL UNIQUE,

    PasswordHash VARCHAR(255) NOT NULL,

    Phone VARCHAR(20),

    Role VARCHAR(20) NOT NULL
        CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Participant', 'Organiser')),

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Users_CreatedAt
        DEFAULT SYSDATETIME()
);
GO

/* =========================================================
   EVENTS
   ========================================================= */

CREATE TABLE Events
(
    EventID INT IDENTITY(1,1) PRIMARY KEY,

    OrganiserID INT NOT NULL,

    EventName VARCHAR(150) NOT NULL,

    Description VARCHAR(500),

    EventDate DATE NOT NULL,

    Location VARCHAR(150) NOT NULL,

    Status VARCHAR(30) NOT NULL
        CONSTRAINT DF_Events_Status
        DEFAULT 'Upcoming',

    CreatedAt DATETIME2 NOT NULL
        CONSTRAINT DF_Events_CreatedAt
        DEFAULT SYSDATETIME(),

    CONSTRAINT FK_Events_Organiser
        FOREIGN KEY (OrganiserID)
        REFERENCES Users(UserID)
);
GO

/* =========================================================
   CATEGORIES
   ========================================================= */

CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,

    EventID INT NOT NULL,

    CategoryName VARCHAR(100) NOT NULL,

    DistanceKM DECIMAL(6,2) NOT NULL,

    EntryFee DECIMAL(10,2) NOT NULL
        CONSTRAINT DF_Categories_EntryFee
        DEFAULT 0,

    MaximumParticipants INT,

    CONSTRAINT FK_Categories_Event
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
        ON DELETE CASCADE,

    CONSTRAINT CK_Categories_Distance
        CHECK (DistanceKM > 0),

    CONSTRAINT CK_Categories_EntryFee
        CHECK (EntryFee >= 0),

    CONSTRAINT CK_Categories_MaxParticipants
        CHECK (
            MaximumParticipants IS NULL
            OR MaximumParticipants > 0
        )
);
GO

/* =========================================================
   ENROLLMENTS
   ========================================================= */

CREATE TABLE Enrollments
(
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,

    ParticipantID INT NOT NULL,

    CategoryID INT NOT NULL,

    EnrollmentDate DATETIME2 NOT NULL
        CONSTRAINT DF_Enrollments_Date
        DEFAULT SYSDATETIME(),

    Status VARCHAR(30) NOT NULL
        CONSTRAINT DF_Enrollments_Status
        DEFAULT 'Registered',

    CONSTRAINT FK_Enrollments_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES Users(UserID),

    CONSTRAINT FK_Enrollments_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID)
        ON DELETE CASCADE,

    CONSTRAINT CK_Enrollments_Status
        CHECK (
            Status IN
            ('Registered', 'Cancelled', 'Completed')
        ),

    CONSTRAINT UQ_Enrollment_Participant_Category
        UNIQUE (ParticipantID, CategoryID)
);
GO

/* =========================================================
   RESULTS
   ========================================================= */

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,

    EnrollmentID INT NOT NULL UNIQUE,

    FinishTime TIME NULL,

    Position INT NULL,

    Pace DECIMAL(6,2) NULL,

    ResultStatus VARCHAR(30) NOT NULL
        CONSTRAINT DF_Results_Status
        DEFAULT 'Completed',

    CONSTRAINT FK_Results_Enrollment
        FOREIGN KEY (EnrollmentID)
        REFERENCES Enrollments(EnrollmentID)
        ON DELETE CASCADE,

    CONSTRAINT CK_Results_Position
        CHECK (Position IS NULL OR Position > 0),

    CONSTRAINT CK_Results_Pace
        CHECK (Pace IS NULL OR Pace > 0)
);
GO

/* =========================================================
   ROUTES
   ========================================================= */

CREATE TABLE Routes
(
    RouteID INT IDENTITY(1,1) PRIMARY KEY,

    EventID INT NOT NULL,

    RouteName VARCHAR(150) NOT NULL,

    DistanceKM DECIMAL(6,2) NOT NULL,

    RouteDescription VARCHAR(500),

    CONSTRAINT FK_Routes_Event
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
        ON DELETE CASCADE,

    CONSTRAINT CK_Routes_Distance
        CHECK (DistanceKM > 0)
);
GO

/* =========================================================
   WEATHER
   ========================================================= */

CREATE TABLE Weather
(
    WeatherID INT IDENTITY(1,1) PRIMARY KEY,

    EventID INT NOT NULL,

    WeatherDate DATE NOT NULL,

    Temperature DECIMAL(5,2),

    Conditions VARCHAR(100),

    WindSpeed DECIMAL(5,2),

    CONSTRAINT FK_Weather_Event
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
        ON DELETE CASCADE,

    CONSTRAINT CK_Weather_WindSpeed
        CHECK (
            WindSpeed IS NULL OR WindSpeed >= 0
        )
);
GO

/* =========================================================
   SAMPLE USERS
   2 Organisers + 2 Participants
   ========================================================= */

INSERT INTO Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Phone,
    Role
)
VALUES
(
    'Thabo',
    'Mokoena',
    'thabo@raceday.co.za',
    'HASHED_PASSWORD_001',
    '0821112233',
    'Organiser'
),
(
    'Lerato',
    'Dlamini',
    'lerato@raceday.co.za',
    'HASHED_PASSWORD_002',
    '0832223344',
    'Organiser'
),
(
    'Sipho',
    'Nkosi',
    'sipho@example.com',
    'HASHED_PASSWORD_003',
    '0843334455',
    'Participant'
),
(
    'Anele',
    'Mthembu',
    'anele@example.com',
    'HASHED_PASSWORD_004',
    '0854445566',
    'Participant'
);
GO

/* =========================================================
   SAMPLE EVENTS
   ========================================================= */

INSERT INTO Events
(
    OrganiserID,
    EventName,
    Description,
    EventDate,
    Location,
    Status
)
VALUES
(
    1,
    'Durban City Run',
    'Annual road running event in Durban.',
    '2026-10-18',
    'Durban, KwaZulu-Natal',
    'Upcoming'
),
(
    2,
    'Johannesburg Community Cycle',
    'Community cycling event for local riders.',
    '2026-11-08',
    'Johannesburg, Gauteng',
    'Upcoming'
),
(
    1,
    'Cape Town Road Challenge',
    'Road running challenge around Cape Town.',
    '2026-12-06',
    'Cape Town, Western Cape',
    'Upcoming'
);
GO

/* =========================================================
   SAMPLE CATEGORIES
   ========================================================= */

INSERT INTO Categories
(
    EventID,
    CategoryName,
    DistanceKM,
    EntryFee,
    MaximumParticipants
)
VALUES
(1, '10 KM Run', 10.00, 150.00, 1000),
(1, '21 KM Half Marathon', 21.10, 250.00, 1500),
(2, '40 KM Cycle', 40.00, 200.00, 800),
(2, '80 KM Cycle', 80.00, 300.00, 600),
(3, '10 KM Road Run', 10.00, 180.00, 1200),
(3, '42.20 KM Marathon', 42.20, 350.00, 1000);
GO

/* =========================================================
   SAMPLE ROUTES
   ========================================================= */

INSERT INTO Routes
(
    EventID,
    RouteName,
    DistanceKM,
    RouteDescription
)
VALUES
(
    1,
    'Durban 10 KM Route',
    10.00,
    'City route starting and finishing near the beachfront.'
),
(
    2,
    'Johannesburg 40 KM Cycle Route',
    40.00,
    'Community cycling route through Johannesburg.'
),
(
    3,
    'Cape Town Marathon Route',
    42.20,
    'Long-distance road route through Cape Town.'
);
GO

/* =========================================================
   SAMPLE WEATHER
   ========================================================= */

INSERT INTO Weather
(
    EventID,
    WeatherDate,
    Temperature,
    Conditions,
    WindSpeed
)
VALUES
(
    1,
    '2026-10-18',
    22.50,
    'Partly Cloudy',
    12.00
),
(
    2,
    '2026-11-08',
    24.00,
    'Sunny',
    9.50
),
(
    3,
    '2026-12-06',
    20.00,
    'Clear',
    15.00
);
GO

/* =========================================================
   SAMPLE ENROLLMENTS
   ========================================================= */

INSERT INTO Enrollments
(
    ParticipantID,
    CategoryID,
    Status
)
VALUES
(
    3,
    1,
    'Registered'
),
(
    4,
    2,
    'Registered'
),
(
    3,
    3,
    'Registered'
),
(
    4,
    5,
    'Registered'
);
GO

/* =========================================================
   SAMPLE RESULTS
   ========================================================= */

INSERT INTO Results
(
    EnrollmentID,
    FinishTime,
    Position,
    Pace,
    ResultStatus
)
VALUES
(
    1,
    '00:52:35',
    15,
    5.26,
    'Completed'
),
(
    2,
    '01:58:42',
    23,
    5.63,
    'Completed'
);
GO

/* =========================================================
   TEST QUERIES
   ========================================================= */

SELECT * FROM Users;

SELECT * FROM Events;

SELECT * FROM Categories;

SELECT * FROM Enrollments;

SELECT * FROM Results;

SELECT * FROM Routes;

SELECT * FROM Weather;
GO 