using Microsoft.Data.Sqlite;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TestTrack.Data;

namespace TestTrack
{
    public partial class EditBugsWindow : Window
    {
        private readonly int _bugId;
        private readonly int _userId;

        public EditBugsWindow(int bugId, int userId)
        {
            InitializeComponent();

            _bugId = bugId;
            _userId = userId;

            LoadBug();
        }

        private void LoadBug()
        {
            using var connection = Database.GetConnection();
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                SELECT Title, Description, Status, Priority
                FROM Bugs
                WHERE Id = $id;
                """;

            command.Parameters.AddWithValue("$id", _bugId);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                MessageBox.Show("Ошибка не найдена.");
                Close();
                return;
            }

            TitleBox.Text = reader["Title"]?.ToString() ?? "";
            DescriptionBox.Text =
                reader["Description"]?.ToString() ?? "";

            SelectItem(StatusBox, reader["Status"]?.ToString());
            SelectItem(PriorityBox, reader["Priority"]?.ToString());
        }

        private static void SelectItem(ComboBox comboBox, string? value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Content?.ToString() == value)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }

            comboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleBox.Text.Trim();
            string description = DescriptionBox.Text.Trim();

            string status =
                (StatusBox.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Новая";

            string priority =
                (PriorityBox.SelectedItem as ComboBoxItem)
                ?.Content?.ToString() ?? "Средний";

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Введите название ошибки.");
                return;
            }

            try
            {
                using var connection = Database.GetConnection();
                connection.Open();

                using var transaction = connection.BeginTransaction();

                string oldStatus;

                using (var readCommand = connection.CreateCommand())
                {
                    readCommand.Transaction = transaction;
                    readCommand.CommandText =
                        "SELECT Status FROM Bugs WHERE Id = $id;";
                    readCommand.Parameters.AddWithValue("$id", _bugId);

                    oldStatus = readCommand.ExecuteScalar()?.ToString()
                                ?? "Новая";
                }

                using (var updateCommand = connection.CreateCommand())
                {
                    updateCommand.Transaction = transaction;
                    updateCommand.CommandText = """
                        UPDATE Bugs
                        SET Title = $title,
                            Description = $description,
                            Status = $status,
                            Priority = $priority
                        WHERE Id = $id;
                        """;

                    updateCommand.Parameters.AddWithValue("$title", title);
                    updateCommand.Parameters.AddWithValue("$description", description);
                    updateCommand.Parameters.AddWithValue("$status", status);
                    updateCommand.Parameters.AddWithValue("$priority", priority);
                

updateCommand.Parameters.AddWithValue("$id", _bugId);

                    updateCommand.ExecuteNonQuery();
                }

                if (oldStatus != status)
                {
                    using var historyCommand = connection.CreateCommand();
                    historyCommand.Transaction = transaction;
                    historyCommand.CommandText = """
                        INSERT INTO BugHistory
                            (BugId, ChangedBy, OldStatus, NewStatus, ChangedAt, Comment)
                        VALUES
                            ($bugId, $userId, $oldStatus, $newStatus,
                             CURRENT_TIMESTAMP, $comment);
                        """;

                    historyCommand.Parameters.AddWithValue("$bugId", _bugId);
                    historyCommand.Parameters.AddWithValue("$userId", _userId);
                    historyCommand.Parameters.AddWithValue("$oldStatus", oldStatus);
                    historyCommand.Parameters.AddWithValue("$newStatus", status);
                    historyCommand.Parameters.AddWithValue(
                        "$comment", "Статус изменён через форму редактирования.");

                    historyCommand.ExecuteNonQuery();
                }

                transaction.Commit();

                MessageBox.Show("Изменения сохранены.");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить изменения:\n" + ex.Message);
            }
        }
    }
}
