using System;
using System.Windows;
using TestTrack.Data;

namespace TestTrack
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrEmpty(password))
            {
                MessageText.Text = "Введите логин и пароль.";
                return;
            }

            try
            {
                if (AuthService.Login(username, password))
                {
                    int userId = AuthService.GetUserId(username);

                    if (userId == 0)
                    {
                        MessageText.Text =
                            "Не удалось определить пользователя.";
                        return;
                    }

                    MainWindow mainWindow = new MainWindow(userId);

                    Application.Current.MainWindow = mainWindow;
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    MessageText.Text = "Неверный логин или пароль.";
                }
            }
            catch (Exception)
            {
                MessageText.Text =
                    "Не удалось выполнить вход. Проверьте базу данных.";
            }
        }
    }
}