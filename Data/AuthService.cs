using Microsoft.Data.Sqlite;
using System;
using System.Security.Cryptography;

namespace TestTrack.Data
{
    public static class AuthService
    {
        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                100_000,
                HashAlgorithmName.SHA256,
                32);

            return Convert.ToBase64String(salt) + ":" +
                   Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(
            string password,
            string storedHash)
        {
            string[] parts = storedHash.Split(':');

            if (parts.Length != 2)
                return false;

            try
            {
                byte[] salt = Convert.FromBase64String(parts[0]);
                byte[] expectedHash = Convert.FromBase64String(parts[1]);

                byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    100_000,
                    HashAlgorithmName.SHA256,
                    32);

                return CryptographicOperations.FixedTimeEquals(
                    actualHash,
                    expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool Login(string username, string password)
        {
            using var connection = Database.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT PasswordHash
                FROM UserCredentials
                WHERE Username = $username;
                """;

            command.Parameters.AddWithValue("$username", username);

            object? result = command.ExecuteScalar();

            if (result is not string storedHash)
                return false;

            return VerifyPassword(password, storedHash);
        }
        public static int GetUserId(string username)
        {
            using var connection = Database.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
        SELECT UserId
        FROM UserCredentials
        WHERE Username = $username;
        """;

            command.Parameters.AddWithValue("$username", username);

            object? result = command.ExecuteScalar();

            return result == null || result == DBNull.Value
                ? 0
                : Convert.ToInt32(result);
        }
    }
}