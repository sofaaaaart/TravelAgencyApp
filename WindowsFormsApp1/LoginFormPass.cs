using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using UniversalCardApp;

namespace WindowsFormsApp1
{
    public partial class LoginFormPass : Form
    {
        private SqlConnection connection; // Изменено с SqlConnection на MySqlConnection

        // Поля для изображений
        private Image normalHideImage;
        private Image hoverHideImage;
        private Image normalCloseImage;
        private Image hoverCloseImage;

        public LoginFormPass(string userLogin, SqlConnection existingConnection)
        {
            InitializeComponent();
            this.connection = existingConnection; // Подключение передаётся в конструктор
            InitializeImages();
            InitializeDatabase();
            this.loginField.Text = userLogin;
            this.loginField.ForeColor = Color.Black;
        }

        private void InitializeImages()
        {
            // Установите базовый путь
            string basePath = AppDomain.CurrentDomain.BaseDirectory; // Путь к исполняемому файлу
            string imagePath = Path.Combine(basePath, "img"); // Путь к папке "img"

            try
            {
                // Загрузите изображения для HideButton
                normalHideImage = Image.FromFile(Path.Combine(imagePath, "ButtonHide.png"));
                hoverHideImage = Image.FromFile(Path.Combine(imagePath, "ButtonHide1.png"));

                // Загрузите изображения для CloseButton
                normalCloseImage = Image.FromFile(Path.Combine(imagePath, "ButtonClose.png"));
                hoverCloseImage = Image.FromFile(Path.Combine(imagePath, "ButtonClose1.png"));
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"Ошибка: изображение не найдено. {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
            }
        }

        private void InitializeDatabase()
        {
            if (connection == null)
            {
                MessageBox.Show("Не удалось установить соединение с базой данных.");
            }
        }

        private void ProcessLogin()
        {
            string enteredPassword = this.passwordField.Text.Trim();
            string enteredLogin = this.loginField.Text.Trim();

            if (string.IsNullOrEmpty(enteredPassword))
            {
                this.messageLabel.Text = "Введите пароль";
                this.messageLabel.ForeColor = Color.Red;
                return;
            }

            int userId = GetUserIdByLogin(enteredLogin);
            if (userId != -1)
            {
                string storedPasswordHash = GetStoredPasswordHash(userId);
                if (VerifyPassword(enteredPassword, storedPasswordHash))
                {
                    string token = CreateSessionToken(userId);
                    File.WriteAllText("session.txt", token);

                    this.Hide();
                    MainForm mainForm = new MainForm(connection);
                    mainForm.Show();
                }
                else
                {
                    this.messageLabel.Text = "Пароль неверный";
                    this.messageLabel.ForeColor = Color.Red;
                }
            }
            else
            {
                this.messageLabel.Text = "Пользователь не найден";
                this.messageLabel.ForeColor = Color.Red;
            }
        }

        private string GetStoredPasswordHash(int userId)
        {
            string query = "SELECT s_password FROM stufff WHERE s_id = @userId";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@userId", userId);

                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                object result = cmd.ExecuteScalar();
                connection.Close();

                return result != null ? result.ToString() : string.Empty;
            }
        }


        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }

        private int GetUserIdByLogin(string login)
        {
            string query = "SELECT s_id FROM stufff WHERE s_login = @login";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@login", login);

                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                object result = cmd.ExecuteScalar();
                connection.Close();

                return result != null ? Convert.ToInt32(result) : -1;
            }
        }

        private string CreateSessionToken(int userId)
        {
            string token = Guid.NewGuid().ToString();
            DateTime expiresAt = DateTime.UtcNow.AddYears(1);

            string query = "INSERT INTO user_sessions (user_id, token, expires_at) VALUES (@user_id, @token, @expires_at)";
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@user_id", userId);
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@expires_at", expiresAt);

                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                cmd.ExecuteNonQuery();
                connection.Close();
            }

            return token;
        }
        private string GenerateSessionToken()
        {
            return Guid.NewGuid().ToString(); // Генерация уникального токена
        }

        private void PasswordField_Enter(object sender, EventArgs e)
        {
            if (this.passwordField.Text == "Введите пароль")
            {
                this.passwordField.Text = string.Empty;
                this.passwordField.UseSystemPasswordChar = true;
                this.passwordField.ForeColor = Color.Black;
            }
        }
        private void PasswordField_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.passwordField.Text))
            {
                this.passwordField.UseSystemPasswordChar = false;
                this.passwordField.Text = "Введите пароль";
                this.passwordField.ForeColor = Color.FromArgb(125, 137, 149);
            }
        }

        private void LoginField_Enter(object sender, EventArgs e)
        {
            if (this.loginField.Text == "Введите логин")
            {
                this.loginField.Text = string.Empty;
                this.loginField.ForeColor = Color.Black;
            }
        }
        private void LoginField_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.loginField.Text))
            {
                this.loginField.Text = "Введите логин";
                this.loginField.ForeColor = Color.FromArgb(125, 137, 149);
            }
        }

        private void HideButton_MouseEnter(object sender, EventArgs e)
        {
            hideButton.Image = hoverHideImage;
        }
        private void HideButton_MouseLeave(object sender, EventArgs e)
        {
            hideButton.Image = normalHideImage;
        }

        private void CloseButton_MouseEnter(object sender, EventArgs e)
        {
            closeButton.Image = hoverCloseImage;
        }
        private void CloseButton_MouseLeave(object sender, EventArgs e)
        {
            closeButton.Image = normalCloseImage;
        }

        private void EnterButton_MouseEnter(object sender, EventArgs e)
        {
            this.enterButton.FillColor = Color.FromArgb(57, 109, 96);
        }
        private void EnterButton_MouseLeave(object sender, EventArgs e)
        {
            this.enterButton.FillColor = Color.FromArgb(71, 137, 120);
        }

        private void BackLabel_MouseEnter(object sender, EventArgs e)
        {
            backLabel.Font = new Font(backLabel.Font, FontStyle.Underline); // Подчеркиваем текст
        }
        private void BackLabel_MouseLeave(object sender, EventArgs e)
        {
            backLabel.Font = new Font(backLabel.Font, FontStyle.Regular); // Убираем подчеркивание
        }

        private void EnterButton_Click(object sender, EventArgs e)
        {
            ProcessLogin();
        }

        private void BackLabel_Click(object sender, EventArgs e)
        {
            // Переход на предыдущую форму
            this.Hide(); // Скрываем текущую форму
            LoginForm loginForm = new LoginForm(connection); // Создаем экземпляр формы, передавая логин
            loginForm.Show(); // Отображаем предыдущую форму
        }

        private void HideButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LoginFormPass_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}
