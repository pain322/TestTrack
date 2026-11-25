using System;
using System.Windows;
using TestTrack.Data;

namespace TestTrack
{
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
               
                Database.Initialize();

                
                DatabaseSeeder.Seed();


                DatabaseSeeder.EnsureTestAccount();
                DatabaseSeeder.EnsureCredentialsForAllUsers();
                DatabaseSeeder.EnsureRealModules();
                RealisticDataSeeder.Seed();

                LoginWindow loginWindow = new LoginWindow();
                MainWindow = loginWindow;
                loginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка запуска приложения:\n" + ex.Message,
                    "TestTrack",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }

        }

    }
}