using System;
using System.Data.SQLite;
using System.Windows;
using System.Security.Cryptography;
using System.Text;

namespace SecretGame
{
    public partial class MainWindow : Window
    {
        private readonly string connectionString = "Data Source=users.db;Version=3;";

        public MainWindow()
        {
            InitializeComponent();
            InitializeDatabase();
        }

        private void InitializeDatabase()
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

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = HashPassword(PasswordBox.Password);

            if (string.IsNullOrWhiteSpace(username) || username == "Пайдаланушы аты" || string.IsNullOrWhiteSpace(password) || password == "Құпия сөз")
            {
                MessageTextBlock.Text = "Пожалуйста, введите имя пользователя и пароль!";
                return;
            }

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

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = HashPassword(PasswordBox.Password);

            if (string.IsNullOrWhiteSpace(username) || username == "Пайдаланушы аты" || string.IsNullOrWhiteSpace(password) || password == "Құпия сөз")
            {
                MessageTextBlock.Text = "Пожалуйста, введите имя пользователя и пароль!";
                return;
            }

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
    }
}