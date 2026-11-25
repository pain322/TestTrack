using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;
using TestTrack.Data;

namespace TestTrack
{
    public partial class AddBugWindow : Window
    {
        private readonly int _userId;

        public class ModuleItem
        {
            public int Id { get; set; }

            public string Name { get; set; } = "";
        }

        public AddBugWindow(int userId)
        {
            InitializeComponent();

            _userId = userId;

            LoadModules();
        }

        private void LoadModules()
        {
            var modules = new List<ModuleItem>();

            using var connection = Database.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = """
                SELECT Id, Name
                FROM Modules
                ORDER BY Name;
                """;

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                modules.Add(new ModuleItem
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }

            ModuleBox.ItemsSource = modules;

            if (modules.Count > 0)
            {
                ModuleBox.SelectedIndex = 0;
            }
        }

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string title = TitleBox.Text.Trim();
            string description = DescriptionBox.Text.Trim();
            string steps = StepsBox.Text.Trim();
            string expected = ExpectedBox.Text.Trim();
            string actual = ActualBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show(
                    "Введите название ошибки.",
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                TitleBox.Focus();
                return;
            }

            if (ModuleBox.SelectedValue == null)
            {
                MessageBox.Show(
                    "Выберите модуль приложения.",
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            if (_userId <= 0)
            {
                MessageBox.Show(
                    "Не удалось определить пользователя.",
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            int moduleId = (int)ModuleBox.SelectedValue;

            string priority =
                (PriorityBox.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Средний";

            try
            {
                using var connection = Database.GetConnection();
                connection.Open();

                using var command = connection.CreateCommand();

                command.CommandText = """
                    INSERT INTO Bugs
                    (
                        Title,
                        Description,
                        StepsToReproduce,
                        ExpectedResult,
                        ActualResult,
                        Priority,
                        ModuleId,
                        ReportedBy
                    )
                    VALUES
                    (
                        $title,
                        $description,
                        $steps,
                        $expected,
                        $actual,
                        $priority,
                        $moduleId,
                        $userId
                    );
                    """;

                command.Parameters.AddWithValue("$title", title);
            
command.Parameters.AddWithValue("$description", description);
                command.Parameters.AddWithValue("$steps", steps);
                command.Parameters.AddWithValue("$expected", expected);
                command.Parameters.AddWithValue("$actual", actual);
                command.Parameters.AddWithValue("$priority", priority);
                command.Parameters.AddWithValue("$moduleId", moduleId);
                command.Parameters.AddWithValue("$userId", _userId);

                int result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    MessageBox.Show(
                        "Ошибка успешно добавлена!",
                        "TestTrack",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(
                        "Ошибка не была добавлена.",
                        "TestTrack",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (SqliteException ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить ошибку:\n" + ex.Message,
                    "Ошибка базы данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Произошла ошибка:\n" + ex.Message,
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}