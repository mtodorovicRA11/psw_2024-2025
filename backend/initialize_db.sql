-- Kreiranje sheme (ako ne postoji)
CREATE SCHEMA IF NOT EXISTS tourapp;

-- USERS
CREATE TABLE tourapp."Users" (
    "Id" uuid PRIMARY KEY,
    "Username" varchar(100) NOT NULL UNIQUE,
    "PasswordHash" varchar(256) NOT NULL,
    "FirstName" varchar(100) NOT NULL,
    "LastName" varchar(100) NOT NULL,
    "Email" varchar(100) NOT NULL UNIQUE,
    "Role" varchar(20) NOT NULL,
    "Interests" varchar(200),
    "BonusPoints" int NOT NULL,
    "IsMalicious" boolean NOT NULL,
    "IsBlocked" boolean NOT NULL,
    "AwardPoints" int NOT NULL,
    "IsAwardedGuide" boolean NOT NULL
);

-- TOURS
CREATE TABLE tourapp."Tours" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(200) NOT NULL,
    "Description" text NOT NULL,
    "Difficulty" varchar(50) NOT NULL,
    "Category" varchar(20) NOT NULL,
    "Price" numeric NOT NULL,
    "Date" timestamp NOT NULL,
    "State" varchar(20) NOT NULL,
    "GuideId" uuid NOT NULL REFERENCES tourapp."Users"("Id")
);

-- KEYPOINTS
CREATE TABLE tourapp."KeyPoints" (
    "Id" uuid PRIMARY KEY,
    "TourId" uuid NOT NULL REFERENCES tourapp."Tours"("Id") ON DELETE CASCADE,
    "Name" varchar(100) NOT NULL,
    "Description" text NOT NULL,
    "Latitude" double precision NOT NULL,
    "Longitude" double precision NOT NULL,
    "ImageUrl" varchar(300)
);

-- PURCHASES
CREATE TABLE tourapp."Purchases" (
    "Id" uuid PRIMARY KEY,
    "TouristId" uuid NOT NULL REFERENCES tourapp."Users"("Id"),
    "TourId" uuid NOT NULL REFERENCES tourapp."Tours"("Id"),
    "PurchaseDate" timestamp NOT NULL,
    "UsedBonusPoints" int NOT NULL,
    "FinalPrice" numeric NOT NULL
);

-- TOUR RATINGS
CREATE TABLE tourapp."TourRatings" (
    "Id" uuid PRIMARY KEY,
    "TourId" uuid NOT NULL REFERENCES tourapp."Tours"("Id"),
    "TouristId" uuid NOT NULL REFERENCES tourapp."Users"("Id"),
    "Rating" int NOT NULL,
    "Comment" text,
    "RatedAt" timestamp NOT NULL
);

-- TOUR PROBLEMS
CREATE TABLE tourapp."TourProblems" (
    "Id" uuid PRIMARY KEY,
    "TourId" uuid NOT NULL REFERENCES tourapp."Tours"("Id"),
    "TouristId" uuid NOT NULL REFERENCES tourapp."Users"("Id"),
    "Title" varchar(200) NOT NULL,
    "Description" text NOT NULL,
    "Status" varchar(20) NOT NULL,
    "CreatedAt" timestamp NOT NULL,
    "UpdatedAt" timestamp NOT NULL
);

-- TOUR PROBLEM EVENTS
CREATE TABLE tourapp."TourProblemEvents" (
    "Id" uuid PRIMARY KEY,
    "ProblemId" uuid NOT NULL REFERENCES tourapp."TourProblems"("Id") ON DELETE CASCADE,
    "EventType" varchar(100) NOT NULL,
    "OccurredAt" timestamp NOT NULL,
    "Data" text
);

-- DEMO PODACI
-- Admin
INSERT INTO tourapp."Users" ("Id", "Username", "PasswordHash", "FirstName", "LastName", "Email", "Role", "Interests", "BonusPoints", "IsMalicious", "IsBlocked", "AwardPoints", "IsAwardedGuide") VALUES
    ('00000000-0000-0000-0000-000000000001', 'admin', 'adminhash', 'Admin', 'Admin', 'admin@demo.com', 'Admin', '', 0, false, false, 0, false);
-- Guide
INSERT INTO tourapp."Users" ("Id", "Username", "PasswordHash", "FirstName", "LastName", "Email", "Role", "Interests", "BonusPoints", "IsMalicious", "IsBlocked", "AwardPoints", "IsAwardedGuide") VALUES
    ('00000000-0000-0000-0000-000000000002', 'guide', 'guidehash', 'Guide', 'Demo', 'guide@demo.com', 'Guide', 'Nature,Art', 0, false, false, 3, true);
-- Tourist
INSERT INTO tourapp."Users" ("Id", "Username", "PasswordHash", "FirstName", "LastName", "Email", "Role", "Interests", "BonusPoints", "IsMalicious", "IsBlocked", "AwardPoints", "IsAwardedGuide") VALUES
    ('00000000-0000-0000-0000-000000000003', 'tourist', 'touristhash', 'Tourist', 'Demo', 'tourist@demo.com', 'Tourist', 'Nature,Food', 10, false, false, 0, false);

-- Tour
INSERT INTO tourapp."Tours" ("Id", "Name", "Description", "Difficulty", "Category", "Price", "Date", "State", "GuideId") VALUES
    ('10000000-0000-0000-0000-000000000001', 'Demo Tour', 'Test tour description', 'Easy', 'Nature', 50, '2024-08-01 10:00:00', 'Published', '00000000-0000-0000-0000-000000000002');
-- KeyPoints
INSERT INTO tourapp."KeyPoints" ("Id", "TourId", "Name", "Description", "Latitude", "Longitude", "ImageUrl") VALUES
    ('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'Start Point', 'Beginning of the tour', 44.8, 20.5, NULL),
    ('20000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000001', 'End Point', 'End of the tour', 44.81, 20.51, NULL);
-- Purchase
INSERT INTO tourapp."Purchases" ("Id", "TouristId", "TourId", "PurchaseDate", "UsedBonusPoints", "FinalPrice") VALUES
    ('30000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000001', '2024-07-01 12:00:00', 5, 45);
-- TourRating
INSERT INTO tourapp."TourRatings" ("Id", "TourId", "TouristId", "Rating", "Comment", "RatedAt") VALUES
    ('40000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000003', 5, 'Odlična tura!', '2024-08-02 15:00:00');
-- TourProblem
INSERT INTO tourapp."TourProblems" ("Id", "TourId", "TouristId", "Title", "Description", "Status", "CreatedAt", "UpdatedAt") VALUES
    ('50000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000003', 'Problem na turi', 'Nije bilo dovoljno vremena na lokaciji.', 'Pending', '2024-08-01 12:00:00', '2024-08-01 12:00:00');
-- TourProblemEvent
INSERT INTO tourapp."TourProblemEvents" ("Id", "ProblemId", "EventType", "OccurredAt", "Data") VALUES
    ('60000000-0000-0000-0000-000000000001', '50000000-0000-0000-0000-000000000001', 'Created', '2024-08-01 12:05:00', NULL); 