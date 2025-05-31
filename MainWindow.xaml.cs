using System;
using System.Data.SQLite;
using System.Windows;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SecretGame
{
    public partial class MainWindow : Window
    {
        private readonly string connectionString;

        public MainWindow()
        {
            // Set a portable data directory (e.g., in the application folder)
            string appDataDir = AppDomain.CurrentDomain.BaseDirectory;
            string dbPath = Path.Combine(appDataDir, "users.db");
            connectionString = $"Data Source={dbPath};Version=3;";

            InitializeComponent();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.db");
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");

                if (!File.Exists(dbPath))
                {
                    SQLiteConnection.CreateFile(dbPath);
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Created database file: {dbPath}\n");
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "CREATE TABLE IF NOT EXISTS Users (Username TEXT PRIMARY KEY, Password TEXT)";
                        using (var command = new SQLiteCommand(sql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                    File.AppendAllText(logPath, $"[{DateTime.Now}] Initialized Users table\n");
                    MessageBox.Show("Database file 'users.db' created successfully.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    using (var connection = new SQLiteConnection(connectionString))
                    {
                        connection.Open();
                        string sql = "CREATE TABLE IF NOT EXISTS Users (Username TEXT PRIMARY KEY, Password TEXT)";
                        using (var command = new SQLiteCommand(sql, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Error: {ex.Message}\n{ex.StackTrace}\n");
                MessageBox.Show($"Database initialization failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UsernameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text == "Пайдаланушы аты")
            {
                UsernameTextBox.Text = "";
            }
        }

        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Password == "Құпия сөз")
            {
                PasswordBox.Password = "";
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = HashPassword(PasswordBox.Password.Trim());

            if (string.IsNullOrWhiteSpace(username) || username == "Пайдаланушы аты" ||
                string.IsNullOrWhiteSpace(password) || PasswordBox.Password == "Құпия сөз")
            {
                MessageTextBlock.Text = "Пожалуйста, введите имя пользователя и пароль!";
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT COUNT(*) FROM Users WHERE Username = @username AND Password = @password";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        long count = (long)command.ExecuteScalar();

                        if (count > 0)
                        {
                            GameWindow gameWindow = new GameWindow(username);
                            gameWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageTextBlock.Text = "Неверное имя пользователя или пароль!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Login Error: {ex.Message}\n{ex.StackTrace}\n");
                MessageTextBlock.Text = "Ошибка при входе. Проверьте лог-файл.";
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = HashPassword(PasswordBox.Password.Trim());

            if (string.IsNullOrWhiteSpace(username) || username == "Пайдаланушы аты" ||
                string.IsNullOrWhiteSpace(password) || PasswordBox.Password == "Құпия сөз")
            {
                MessageTextBlock.Text = "Пожалуйста, введите имя пользователя и пароль!";
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT OR IGNORE INTO Users (Username, Password) VALUES (@username, @password)";
                    using (var command = new SQLiteCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageTextBlock.Text = "Регистрация успешна! Теперь вы можете войти.";
                        }
                        else
                        {
                            MessageTextBlock.Text = "Имя пользователя уже существует!";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] Registration Error: {ex.Message}\n{ex.StackTrace}\n");
                MessageTextBlock.Text = "Ошибка при регистрации. Проверьте лог-файл.";
            }
        }
    }
}