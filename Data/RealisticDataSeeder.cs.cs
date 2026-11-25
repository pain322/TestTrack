
using Microsoft.Data.Sqlite;
using System;

namespace TestTrack.Data
{
    public static class RealisticDataSeeder
    {
        public static void Seed()
        {
            using var connection = Database.GetConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            string[] firstNames =
            {
                "Александр", "Дмитрий", "Максим", "Артём",
                "Иван", "Михаил", "Егор", "Андрей",
                "Кирилл", "Никита"
            };

            string[] lastNames =
            {
                "Иванов", "Смирнов", "Кузнецов", "Попов",
                "Соколов", "Лебедев", "Козлов", "Новиков",
                "Морозов", "Петров"
            };

            string[] patronymics =
            {
                "Александрович", "Дмитриевич",
                "Сергеевич", "Андреевич",
                "Иванович", "Михайлович",
                "Олегович", "Владимирович",
                "Алексеевич", "Игоревич"
            };

            string[] roles =
            {
                "Разработчик",
                "Тестировщик",
                "Администратор",
                "Аналитик"
            };

            // 1. Обновление пользователей
            for (int i = 1; i <= 100; i++)
            {
                int index = i - 1;

                string fullName =
                    $"{lastNames[index / 10]} " +
                    $"{firstNames[index % 10]} " +
                    $"{patronymics[index % 10]}";

                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = """
                    UPDATE Users
                    SET FullName = $name,
                        Email = $email,
                        Role = $role
                    WHERE Id = $id;
                    """;

                command.Parameters.AddWithValue("$id", i);
                command.Parameters.AddWithValue("$name", fullName);
                command.Parameters.AddWithValue(
                    "$email", $"employee{i:D3}@example.test");
                command.Parameters.AddWithValue(
                    "$role", roles[index % roles.Length]);

                command.ExecuteNonQuery();
            }

            // 2. Названия программных модулей
            string[] modules =
            {
                "Авторизация и регистрация",
                "Управление пользователями",
                "Профиль пользователя",
                "Управление паролями",
                "Поиск и фильтрация",
                "Управление ошибками",
                "Создание тест-кейсов",
                "Выполнение тестирования",
                "Формирование отчётов",
                "Уведомления",
                "Экспорт данных",
                "Импорт данных",
                "Настройки приложения",
                "Управление доступом",
                "Работа с базой данных"
            };

            for (int i = 1; i <= 115; i++)
            {
                string moduleName = modules[(i - 1) % modules.Length];

                if (i > modules.Length)
                    moduleName += $" — компонент {i}";

                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = """
                    UPDATE Modules
                    SET Name = $name,
                        Description = $description
                    WHERE Id = $id;
                    """;

                command.Parameters.AddWithValue("$id", i);
                command.Parameters.AddWithValue("$name", moduleName);
                command.Parameters.AddWithValue(
                    "$description",
                    $"Функциональный компонент системы TestTrack. " +
                    $"Обеспечивает работу подсистемы «{moduleName}».");

                command.ExecuteNonQuery();
            }

            // 3. Реалистичные тест-кейсы
            string[] testTitles =
            {
                "Проверка авторизации с корректными данными",
                "Проверка авторизации с неверным паролем",
                "Проверка регистрации нового пользователя",
                "Проверка сохранения изменений профиля",
                "Проверка поиска по названию",
                "Проверка фильтрации по статусу",
                "Проверка создания новой ошибки",
                "Проверка редактирования ошибки",
                "Проверка изменения приоритета",
                "Проверка назначения ответственного",
                "Проверка формирования отчёта",
                "Проверка экспорта данных",
                "Проверка загрузки данных",
                "Проверка удаления записи",
                "Проверка обработки пустых полей",
                "Проверка сохранения тест-кейса",
                "Проверка отображения уведомлений",
                "Проверка соединения с базой данных",
                "Проверка разграничения прав доступа",
                "Проверка корректности отображения таблицы"
            };

            for (int i = 1; i <= 100; i++)
            {
                string title = testTitles[(i - 1) % testTitles.Length];

                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = """
                    UPDATE TestCases
                    SET Title = $title,
                        Steps = $steps,
                        ExpectedResult = $expected
                    WHERE Id = $id;
                    """;

                command.Parameters.AddWithValue("$id", i);
                command.Parameters.AddWithValue(
                    "$title", $"{title} (сценарий {i})");
                command.Parameters.AddWithValue(
                    "$steps",
                    "Открыть соответствующий раздел приложения, " +
                    "выполнить необходимые действия и проверить результат.");
                command.Parameters.AddWithValue(
                    "$expected",
                    "Операция выполняется успешно, данные отображаются " +
                    "корректно, ошибок приложения не возникает.");

                command.ExecuteNonQuery();
            }

            // 4. Реалистичные описания ошибок
            string[] bugTitles =
            {
                "Не выполняется авторизация пользователя",
                "Не сохраняются изменения профиля",
                "Некорректно работает поиск записей",
                "Фильтр отображает лишние результаты",
                "Не отображается сообщение об ошибке",
                "Не сохраняется новый тест-кейс",
                "Ошибка при изменении статуса задачи",
                "Некорректно отображается приоритет ошибки",
                "Не работает назначение ответственного",
                "Отчёт формируется без части записей",
                "Не выполняется экспорт данных",
                "Ошибка при загрузке информации",
                "Некорректная обработка пустых полей",
                "Таблица не обновляется после изменения",
                "Не сохраняются комментарии пользователя",
                "Неверно отображается дата создания записи",
                "Ошибка при открытии карточки ошибки",
                "Некорректно работает сортировка",
                "Не обновляется статистика ошибок",
                "Возникает ошибка при подключении к БД"
            };

            string[] descriptions =
            {
                "При выполнении операции приложение работает не так, как предусмотрено требованиями.",
                "В процессе проверки обнаружено несоответствие фактического результата ожидаемому.",
                "Функция работает некорректно при определённых условиях использования.",
                "После выполнения действия данные отображаются неправильно."
            };

            string[] steps =
            {
                "1. Запустить приложение.\n2. Открыть соответствующий раздел.\n3. Выполнить операцию.\n4. Проверить результат.",
                "1. Авторизоваться в системе.\n2. Перейти к нужной записи.\n3. Изменить данные.\n4. Сохранить изменения.",
                "1. Открыть главное окно.\n2. Выбрать необходимый фильтр.\n3. Проверить отображаемые записи."
            };

            string[] expected =
            {
                "Операция должна выполняться успешно, данные должны сохраняться корректно.",
                "Система должна отображать актуальные сведения без ошибок.",
                "Пользователь должен получить корректный результат выполнения операции."
            };

            string[] actual =
            {
                "Операция завершается некорректно, изменения не отображаются.",
                "В результатах присутствуют лишние или отсутствующие записи.",
                "Приложение не отображает ожидаемое сообщение.",
                "После выполнения операции данные остаются без изменений."
            };

            string[] statuses =
            {
                "Новая", "В работе", "Исправлена", "Закрыта"
            };

            string[] priorities =
            {
                "Низкий", "Средний", "Высокий", "Критический"
            };

            // Обновляем все существующие ошибки
            for (int i = 1; i <= 101; i++)
            {
                int index = i - 1;

                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = """
                    UPDATE Bugs
                    SET Title = $title,
                        Description = $description,
                        StepsToReproduce = $steps,
                        ExpectedResult = $expected,
                        ActualResult = $actual,
                        Status = $status,
                        Priority = $priority
                    WHERE Id = $id;
                    """;

                command.Parameters.AddWithValue("$id", i);
                command.Parameters.AddWithValue(
                    "$title",
                    $"{bugTitles[index % bugTitles.Length]} (№{i})");

                command.Parameters.AddWithValue(
                    "$description",
                    descriptions[index % descriptions.Length]);

                command.Parameters.AddWithValue(
                    "$steps", steps[index % steps.Length]);

                command.Parameters.AddWithValue(
                    "$expected", expected[index % expected.Length]);

                command.Parameters.AddWithValue(
                    "$actual", actual[index % actual.Length]);

                command.Parameters.AddWithValue(
                    "$status", statuses[index % statuses.Length]);

                command.Parameters.AddWithValue(
                    "$priority", priorities[index % priorities.Length]);

                command.ExecuteNonQuery();
            }

            // 5. Обновление истории ошибок
            for (int i = 1; i <= 101; i++)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;

                command.CommandText = """
                    UPDATE BugHistory
                    SET OldStatus = $oldStatus,
                        NewStatus = $newStatus,
                        Comment = $comment
                    WHERE Id = $id;
                    """;

                command.Parameters.AddWithValue("$id", i);

                command.Parameters.AddWithValue(
                    "$oldStatus", statuses[(i + 1) % statuses.Length]);

                command.Parameters.AddWithValue(
                    "$newStatus", statuses[i % statuses.Length]);

                command.Parameters.AddWithValue(
                    "$comment",
                    "Статус задачи изменён в процессе тестирования. " +
                    "Выполнена проверка текущего состояния ошибки.");

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}
