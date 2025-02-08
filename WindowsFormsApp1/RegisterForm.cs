using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Drawing;
using System.Data;
using System.IO;
using UniversalCardApp;
using System.Text;
using System.Security.Cryptography;
using System.Data.SqlClient;

namespace WindowsFormsApp1
{
    public partial class RegisterForm : Form
    {
        private readonly SqlConnection connection;


        // Поля для изображений
        private Image normalHideImage;
        private Image hoverHideImage;
        private Image normalCloseImage;
        private Image hoverCloseImage;


        public RegisterForm()
        {
            InitializeComponent();
            connection = InitializeDatabase();
            InitializeImages();
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
        private SqlConnection InitializeDatabase()
        {
            var conn = DatabaseInitializer.InitializeDatabase();
            if (conn == null)
            {
                MessageBox.Show("Не удалось установить соединение с базой данных.");
            }
            return conn;
        }



        private void RegisterButton_Click(object sender, EventArgs e)
        {

            // Проверяем поля на подсказки или пустые значения
            if (lastNameField.Text == "Фамилия" || string.IsNullOrWhiteSpace(lastNameField.Text) ||
                nameField.Text == "Имя" || string.IsNullOrWhiteSpace(nameField.Text) ||
                postField.Text == "Должность" || string.IsNullOrWhiteSpace(postField.Text) ||
                loginField.Text == "Логин" || string.IsNullOrWhiteSpace(loginField.Text) ||
                passwordField.Text == "Пароль" || string.IsNullOrWhiteSpace(passwordField.Text))
            {

                UpdateMessage("    Заполните все поля", Color.Red);
                return;
            }

            // Проверяем, существует ли пользователь
            if (IsUserExist())
            {

                UpdateMessage("Такой логин уже занят", Color.Red);
                return;
            }

            // Хэшируем пароль перед его сохранением
            string hashedPassword = HashPassword(passwordField.Text.Trim());


            // Используем SqlCommand для работы с SQL Server
            SqlCommand command = new SqlCommand("INSERT INTO stufff (s_id, s_lastName, s_name, s_post, s_login, s_password, reg_date) " +
                "VALUES (NULL, @lastName, @name, @post, @login, @password, GETDATE());", connection);

            command.Parameters.Add("@lastName", SqlDbType.VarChar).Value = lastNameField.Text.Trim();
            command.Parameters.Add("@name", SqlDbType.VarChar).Value = nameField.Text.Trim();
            command.Parameters.Add("@post", SqlDbType.VarChar).Value = postField.Text.Trim();
            command.Parameters.Add("@login", SqlDbType.VarChar).Value = loginField.Text.Trim();
            command.Parameters.Add("@password", SqlDbType.VarChar).Value = hashedPassword;

            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                if (command.ExecuteNonQuery() == 1)
                {
                    messageLabel.Text = "Аккаунт создан";
                    messageLabel.ForeColor = Color.Green;

                    // Переход к главной форме
                    this.Hide();
                    MainForm mainForm = new MainForm(connection);
                    mainForm.Show();
                }
                else
                {
                    messageLabel.Text = "Ошибка создания аккаунта.";
                    messageLabel.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                messageLabel.Text = "Ошибка: " + ex.Message;
                messageLabel.ForeColor = Color.Red;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        public bool IsUserExist()
        {
            try
            {
                SqlCommand command = new SqlCommand("SELECT COUNT(*) FROM stufff WHERE s_login = @sl", connection);
                command.Parameters.Add("@sl", SqlDbType.VarChar).Value = loginField.Text.Trim();


                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                messageLabel.Text = "Ошибка проверки существования пользователя: " + ex.Message;
                messageLabel.ForeColor = Color.Red;
                return true;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Преобразуем строку пароля в байты и хэшируем
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Преобразуем байты в строку (шестнадцатеричное представление)
                StringBuilder builder = new StringBuilder();
                foreach (byte byteValue in bytes)
                {
                    builder.Append(byteValue.ToString("x2"));
                }

                return builder.ToString(); // Возвращаем хэшированный пароль
            }
        }

        private void LastNameField_Enter(object sender, EventArgs e)
        {
            if (this.lastNameField.Text == "Фамилия")
            {
                this.lastNameField.Text = string.Empty;
                this.lastNameField.ForeColor = Color.Black;
            }
        }
        private void LastNameField_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.lastNameField.Text))
            {
                this.lastNameField.Text = "Фамилия";
                this.lastNameField.ForeColor = Color.FromArgb(125, 137, 149);
            }
        }

        private void NameField_Enter(object sender, EventArgs e)
        {
            if (this.nameField.Text == "Имя")
            {
                this.nameField.Text = string.Empty;
                this.nameField.ForeColor = Color.Black;
            }
        }
        private void NameField_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.nameField.Text))
            {
                this.nameField.Text = "Имя";
                this.nameField.ForeColor = Color.FromArgb(125, 137, 149);
            }
        }

        private void PostField_Enter(object sender, EventArgs e)
        {
            if (this.postField.Text == "Должность")
            {
                this.postField.Text = string.Empty;
                this.postField.ForeColor = Color.Black;
            }
        }
        private void PostField_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.postField.Text))
            {
                this.postField.Text = "Должность";
                this.postField.ForeColor = Color.FromArgb(125, 137, 149);
            }
        }

        private void LoginField_Enter(object sender, EventArgs e)
        {
            if (this.loginField.Text == "Логин")
            {
                this.loginField.Text = string.Empty;
                this.loginField.ForeColor = Color.Black;
            }
        }
        private void LoginField_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.loginField.Text))
            {
                this.loginField.Text = "Логин";
                this.loginField.ForeColor = Color.FromArgb(125, 137, 149);
            }
        }

        private void PasswordField_Enter(object sender, EventArgs e)
        {
            if (this.passwordField.Text == "Пароль")
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
                this.passwordField.Text = "Пароль";
                this.passwordField.ForeColor = Color.FromArgb(125, 137, 149);
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

        private void RegisterButton_MouseEnter(object sender, EventArgs e)
        {
            this.registerButton.FillColor = Color.FromArgb(57, 109, 96);
        }
        private void RegisterButton_MouseLeave(object sender, EventArgs e)
        {
            this.registerButton.FillColor = Color.FromArgb(71, 137, 120);
        }

        private void BackLabel_MouseEnter(object sender, EventArgs e)
        {
            backLabel.Font = new Font(backLabel.Font, FontStyle.Underline); // Подчеркиваем текст
        }
        private void BackLabel_MouseLeave(object sender, EventArgs e)
        {
            backLabel.Font = new Font(backLabel.Font, FontStyle.Regular); // Убираем подчеркивание
        }

        private void BackLabel_Click(object sender, EventArgs e)
        {
            // Переход на предыдущую форму
            this.Hide(); // Скрываем текущую форму
            LoginForm loginForm = new LoginForm(connection); // Создаем экземпляр предыдущей формы
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


        private void CenterMessageLabel()
        {
            // Обновляем размер метки на основе нового текста
            messageLabel.Update();

            // Вычисляем центр формы относительно ширины и высоты MessageLabel
            int x = (this.ClientSize.Width - messageLabel.Width) / 2;

            // Устанавливаем новую позицию
            messageLabel.Location = new Point(x, 508);
        }

        private void UpdateMessage(string message, Color color)
        {
            // Устанавливаем текст и цвет метки
            messageLabel.Text = message;
            messageLabel.ForeColor = color;

            // Принудительно обновляем размеры метки перед центровкой
            messageLabel.Update();
            CenterMessageLabel();
        }

        private void RegisterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }

}