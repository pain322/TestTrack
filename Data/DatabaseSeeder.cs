
using System;
using System.Collections.Generic;

namespace TestTrack.Data
{
    public static class DatabaseSeeder
    {
        public static void Seed()
        {
            using var connection = Database.GetConnection();
            connection.Open();

            using var check = connection.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM Users;";
            long count = (long)check.ExecuteScalar()!;

            if (count > 0)
                return;

            using var transaction = connection.BeginTransaction();

            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            // Создание 100 пользователей
            for (int i = 1; i <= 100; i++)
            {
                command.CommandText = """
                    INSERT INTO Users (FullName, Email, Role)
                    VALUES ($name, $email, $role);
                    """;

                command.Parameters.Clear();
                command.Parameters.AddWithValue(
                    "$name", $"Пользователь {i}");
                command.Parameters.AddWithValue(
                    "$email", $"user{i}@testtrack.local");
                command.Parameters.AddWithValue(
                    "$role",
                    i % 3 == 0 ? "Тестировщик" :
                    i % 3 == 1 ? "Разработчик" :
                    "Администратор");

                command.ExecuteNonQuery();
            }

            // Создание 100 модулей
            for (int i = 1; i <= 100; i++)
            {
                command.CommandText = """
                    INSERT INTO Modules (Name, Description)
                    VALUES ($name, $description);
                    """;

                command.Parameters.Clear();
                command.Parameters.AddWithValue(
                    "$name", $"Модуль {i}");
                command.Parameters.AddWithValue(
                    "$description",
                    $"Описание программного модуля {i}");

                command.ExecuteNonQuery();
            }

            // Создание 100 тест-кейсов
            for (int i = 1; i <= 100; i++)
            {
                command.CommandText = """
                    INSERT INTO TestCases
                    (ModuleId, Title, Steps, ExpectedResult)
                    VALUES ($module, $title, $steps, $expected);
                    """;

                command.Parameters.Clear();
                command.Parameters.AddWithValue("$module", i);
                command.Parameters.AddWithValue(
                    "$title", $"Тест-кейс {i}");
                command.Parameters.AddWithValue(
                    "$steps", $"Выполнить проверку модуля {i}");
                command.Parameters.AddWithValue(
                    "$expected",
                    "Операция выполняется без ошибок");

                command.ExecuteNonQuery();
            }

            // Создание 100 ошибок
            for (int i = 1; i <= 100; i++)
            {
                command.CommandText = """
                    INSERT INTO Bugs
                    (Title, Description, StepsToReproduce,
                     ExpectedResult, ActualResult, Status,
                     Priority, ModuleId, TestCaseId,
                     ReportedBy, AssignedTo)
                    VALUES
                    ($title, $description, $steps,
                     $expected, $actual, $status,
                     $priority, $module, $testCase,
                     $reportedBy, $assignedTo);
                    """;

                command.Parameters.Clear();
                command.Parameters.AddWithValue(
                    "$title", $"Ошибка {i}");
                command.Parameters.AddWithValue(
                    "$description",
                    $"Описание обнаруженной ошибки {i}");
                command.Parameters.AddWithValue(
                    "$steps",
                    "Открыть модуль и выполнить проверку");
                command.Parameters.AddWithValue(
                    "$expected",
                    "Программа работает корректно");
                command.Parameters.AddWithValue(
                    "$actual", $"Обнаружено отклонение №{i}");
                command.Parameters.AddWithValue(
                    "$status",
                    i % 3 == 0 ? "Исправлена" :
                    i % 3 == 1 ? "Новая" : "В работе");
                command.Parameters.AddWithValue(
                    "$priority",
                    i % 3 == 0 ? "Низкий" :
                    i % 3 == 1 ? "Средний" : "Высокий");
                command.Parameters.AddWithValue("$module", i);
                command.Parameters.AddWithValue("$testCase", i);
                command.Parameters.AddWithValue("$reportedBy", i);
                command.Parameters.AddWithValue(
                    "$assignedTo", i % 2 == 0 ? i : 100);

                command.ExecuteNonQuery();
            }

            // Создание 100 записей истории ошибок
            for (int i = 1; i <= 100; i++)
            {
                command.CommandText = """
                    INSERT INTO BugHistory
                    (BugId, ChangedBy, OldStatus, NewStatus, Comment)
                    VALUES
                    ($bug, $user, $oldStatus, $newStatus, $comment);
                    """;

                command.Parameters.Clear();
                command.Parameters.AddWithValue("$bug", i);
                command.Parameters.AddWithValue("$user", i);
                command.Parameters.AddWithValue(
                    "$oldStatus",
                    i % 2 == 0 ? "Новая" : "В работе");
                command.Parameters.AddWithValue(
                    "$newStatus",
                    i % 2 == 0 ? "В работе" : "Исправлена");
                command.Parameters.AddWithValue(
                    "$comment", $"Изменение статуса ошибки {i}");

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        // Создание тестовой учётной записи
        public static void EnsureTestAccount()
        {
            using var connection = Database.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
                INSERT OR IGNORE INTO Users
                    (FullName, Email, Role)
                VALUES
                    ('Тестовый пользователь',
                     'user1@testtrack.local',
                     'Администратор');
                """;

            command.ExecuteNonQuery();

            command.CommandText = """
                INSERT OR IGNORE INTO UserCredentials
                    (UserId, Username, PasswordHash)
                SELECT Id, 'user1', $hash
                FROM Users
                WHERE Email = 'user1@testtrack.local';
                """;

            command.Parameters.Clear();
            command.Parameters.AddWithValue(
                "$hash",
                AuthService.HashPassword("DemoPass1!"));

            command.ExecuteNonQuery();
        }

        // Создание учётных записей для всех пользователей
        public static void EnsureCredentialsForAllUsers()
        {
            using var connection = Database.GetConnection();
            connection.Open();

            var users = new List<int>();

            using (var select = connection.CreateCommand())
            {
                select.CommandText = """
                    SELECT Id FROM Users ORDER BY Id;
                    """;

                using var reader = select.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(reader.GetInt32(0));
                }
            }

            using var transaction = connection.BeginTransaction();

            foreach (int userId in users)
            {
                string username = $"user{userId}";
                string password = $"DemoPass{userId}!";

                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = """
                    INSERT OR IGNORE INTO UserCredentials
                        (UserId, Username, PasswordHash)
                    VALUES
                        ($userId, $username, $hash);
                    """;

                command.Parameters.AddWithValue("$userId", userId);
                command.Parameters.AddWithValue("$username", username);
                command.Parameters.AddWithValue(
                    "$hash",
                    AuthService.HashPassword(password));

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        // Добавление дополнительных модулей
        public static void EnsureRealModules()
        {
            using var connection = Database.GetConnection();
            connection.Open();

            string[] modules =
            {
                "Авторизация и регистрация",
                "Профиль пользователя",
                "Управление паролем",
                "Поиск и фильтрация",
                "Загрузка файлов",
                "Управление тест-кейсами",
                "Отчёты о тестировании",
                "Управление ошибками",
                "Уведомления",
                "Настройки приложения",
                "Управление пользователями",
                "Экспорт данных",
                "Импорт данных",
                "Панель администратора",
                "Работа с базой данных"
            };

            foreach (string name in modules)
            {
                using var command = connection.CreateCommand();

                command.CommandText = """
                    INSERT INTO Modules (Name, Description)
                    SELECT $name, $description
                    WHERE NOT EXISTS (
                        SELECT 1 FROM Modules WHERE Name = $name
                    );
                    """;

                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue(
                    "$description",
                    $"Функциональный модуль «{name}» приложения TestTrack.");

                command.ExecuteNonQuery();
            }
        }
    }
}
