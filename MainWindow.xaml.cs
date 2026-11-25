using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;
using TestTrack.Data;
using TestTrack.Models;

namespace TestTrack
{
    public partial class MainWindow : Window
    {
        private readonly int _currentUserId;

        public MainWindow(int userId)
        {
            InitializeComponent();

            _currentUserId = userId;

            LoadBugs();
        }

        private void LoadBugs()
        {
            try
            {
                var bugs = new List<BugRow>();

                using var connection = Database.GetConnection();
                connection.Open();

                using var command = connection.CreateCommand();

                string searchText = SearchBox.Text.Trim();
                string selectedStatus =
                    (StatusFilterBox.SelectedItem as ComboBoxItem)
                    ?.Content?.ToString() ?? "Все";

                command.CommandText = """
                    SELECT
                        b.Id,
                        b.Title,
                        b.Status,
                        b.Priority,
                        COALESCE(m.Name, 'Не указан') AS ModuleName,
                        COALESCE(reporter.FullName, 'Не указан') AS ReporterName,
                        COALESCE(assignee.FullName, 'Не назначен') AS AssigneeName
                    FROM Bugs b
                    LEFT JOIN Modules m ON b.ModuleId = m.Id
                    LEFT JOIN Users reporter ON b.ReportedBy = reporter.Id
                    LEFT JOIN Users assignee ON b.AssignedTo = assignee.Id
                    WHERE
                        ($search = ''
                         OR b.Title LIKE $pattern
                         OR b.Description LIKE $pattern)
                        AND
                        ($status = 'Все' OR b.Status = $status)
                    ORDER BY b.Id DESC;
                    """;

                command.Parameters.AddWithValue("$search", searchText);
                command.Parameters.AddWithValue(
                    "$pattern", "%" + searchText + "%");
                command.Parameters.AddWithValue("$status", selectedStatus);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    bugs.Add(new BugRow
                    {
                        Id = reader.GetInt32(0),
                        Title = reader.GetString(1),
                        Status = reader.GetString(2),
                        Priority = reader.GetString(3),
                        ModuleName = reader.GetString(4),
                        ReporterName = reader.GetString(5),
                        AssigneeName = reader.GetString(6)
                    });
                }

                BugsGrid.ItemsSource = bugs;

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить ошибки:\n" + ex.Message,
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void UpdateStatistics()
        {
            try
            {
                using var connection = Database.GetConnection();
                connection.Open();

                using var command = connection.CreateCommand();

                command.CommandText = """
                    SELECT
                        COUNT(*),
                        SUM(CASE WHEN Status = 'Новая' THEN 1 ELSE 0 END),
                        SUM(CASE WHEN Status = 'Исправлена' THEN 1 ELSE 0 END)
                    FROM Bugs;
                    """;

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    int total = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);

                    int newBugs = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                    int fixedBugs = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                    TotalBugsText.Text = total.ToString();
                    NewBugsText.Text = newBugs.ToString();
                    FixedBugsText.Text = fixedBugs.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось обновить статистику:\n" + ex.Message,
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBugs();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBugs();
        }

        private void AddBugButton_Click(object sender, RoutedEventArgs e)
        {
            AddBugWindow window = new AddBugWindow(_currentUserId);

            if (window.ShowDialog() == true)
            {
                LoadBugs();
            }
        }

        private void EditBugButton_Click(object sender, RoutedEventArgs e)
        {
            if (BugsGrid.SelectedItem is not BugRow selectedBug)
            {
                MessageBox.Show(
                    "Сначала выбери ошибку в таблице.",
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            EditBugsWindow window = new EditBugsWindow(
                selectedBug.Id,
                _currentUserId);

            if (window.ShowDialog() == true)
            {
                LoadBugs();
            }

        }
    
    
private void DeleteBugButton_Click(object sender, RoutedEventArgs e)
        {
            if (BugsGrid.SelectedItem is not BugRow selectedBug)
            {
                MessageBox.Show(
                    "Сначала выбери ошибку в таблице.",
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            MessageBoxResult result = MessageBox.Show(
                $"Ты действительно хочешь удалить ошибку:\n\n" +
                $"{selectedBug.Title}?\n\n" +
                "Все записи истории этой ошибки также будут удалены.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                using var connection = Database.GetConnection();
                connection.Open();

                using var transaction = connection.BeginTransaction();

                using var historyCommand = connection.CreateCommand();
                historyCommand.Transaction = transaction;

                historyCommand.CommandText =
                    "DELETE FROM BugHistory WHERE BugId = $id;";

                historyCommand.Parameters.AddWithValue(
                    "$id", selectedBug.Id);

                historyCommand.ExecuteNonQuery();

                using var bugCommand = connection.CreateCommand();
                bugCommand.Transaction = transaction;

                bugCommand.CommandText =
                    "DELETE FROM Bugs WHERE Id = $id;";

                bugCommand.Parameters.AddWithValue(
                    "$id", selectedBug.Id);

                bugCommand.ExecuteNonQuery();

                transaction.Commit();

                MessageBox.Show(
                    "Ошибка успешно удалена.",
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadBugs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось удалить ошибку:\n" + ex.Message,
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

    }
}