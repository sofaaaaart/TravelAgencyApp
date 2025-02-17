using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        private SqlConnection connection; // Заменяем MySqlConnection на SqlConnection

        private Image normalHideImage;
        private Image hoverHideImage;
        private Image normalCloseImage;
        private Image hoverCloseImage;

        public LoginForm(SqlConnection connection) // Используем SqlConnection
        {
            InitializeComponent();
            this.connection = connection; // Передаём подключение
            InitializeDatabase();
            InitializeImages();
        }

        private void InitializeDatabase()
        {
            if (connection == null)
            {
                MessageBox.Show("Не удалось установить соединение с базой данных.");
            }
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

        private void ProcessLogin()
        {
            string login = this.loginField.Text.Trim();

            if (string.IsNullOrEmpty(login) || login == "Введите логин")
            {
                this.messageLabel.Text = "Введите логин";
                this.messageLabel.ForeColor = Color.Red;
                return;
            }

            if (IsLoginExists(login))
            {
                this.Hide();
                LoginFormPass loginPassForm = new LoginFormPass(login, connection); // Передаём SqlConnection
                loginPassForm.Show();
            }
            else
            {
                this.messageLabel.Text = "Такого логина не существует";
                this.messageLabel.ForeColor = Color.White;
            }
        }

        private bool IsLoginExists(string login)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM staff WHERE s_login = @login";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@login", login);

                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    connection.Close();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при проверке логина: " + ex.Message);
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                return false;
            }
        }
        private void LoginField_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Если нажата клавиша Enter, вызываем метод для обработки логина
            if (e.KeyChar == (char)Keys.Enter)
            {
                ProcessLogin();
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

        private void RegisterButton_MouseEnter(object sender, EventArgs e)
        {
            this.registerButton.FillColor = Color.FromArgb(57, 109, 96);
        }
        private void RegisterButton_MouseLeave(object sender, EventArgs e)
        {
            this.registerButton.FillColor = Color.FromArgb(71, 137, 120);
        }


        private void RegisterButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();
        }
        private void EnterButton_Click(object sender, EventArgs e)
        {
            ProcessLogin();
        }
        private void HideButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }


    }
}