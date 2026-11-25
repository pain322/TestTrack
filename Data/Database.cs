using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TestTrack.Data
{
    public static class Database
    {
        private static readonly string FolderPath =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "TestTrack");

        private static readonly string DatabasePath =
            Path.Combine(FolderPath, "testtrack.db");

        public static SqliteConnection GetConnection()
        {
            Directory.CreateDirectory(FolderPath);

            return new SqliteConnection(
                $"Data Source={DatabasePath}");
        }

        public static void Initialize()
        {
            using var connection = GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FullName TEXT NOT NULL,
                    Email TEXT NOT NULL UNIQUE,
                    Role TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Modules (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT
                );

                CREATE TABLE IF NOT EXISTS TestCases (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ModuleId INTEGER NOT NULL,
                    Title TEXT NOT NULL,
                    Steps TEXT NOT NULL,
                    ExpectedResult TEXT NOT NULL,
                    FOREIGN KEY (ModuleId) REFERENCES Modules(Id)
                );

                CREATE TABLE IF NOT EXISTS Bugs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Title TEXT NOT NULL,
                    Description TEXT NOT NULL,
                    StepsToReproduce TEXT NOT NULL,
                    ExpectedResult TEXT NOT NULL,
                    ActualResult TEXT NOT NULL,
                    Status TEXT NOT NULL DEFAULT 'Новая',
                    Priority TEXT NOT NULL DEFAULT 'Средний',
                    ModuleId INTEGER NOT NULL,
                    TestCaseId INTEGER,
                    ReportedBy INTEGER NOT NULL,
                    AssignedTo INTEGER,
                    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (ModuleId) REFERENCES Modules(Id),
                    FOREIGN KEY (TestCaseId) REFERENCES TestCases(Id),
                    FOREIGN KEY (ReportedBy) REFERENCES Users(Id),
                    FOREIGN KEY (AssignedTo) REFERENCES Users(Id)
                );

                CREATE TABLE IF NOT EXISTS BugHistory (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    BugId INTEGER NOT NULL,
                    ChangedBy INTEGER NOT NULL,
                    OldStatus TEXT,
                    NewStatus TEXT NOT NULL,
                    ChangedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    Comment TEXT,
                    FOREIGN KEY (BugId) REFERENCES Bugs(Id),
                    FOREIGN KEY (ChangedBy) REFERENCES Users(Id)
                );
                CREATE TABLE IF NOT EXISTS UserCredentials (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL UNIQUE,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );
                """;

            command.ExecuteNonQuery();
        }
    }
}







