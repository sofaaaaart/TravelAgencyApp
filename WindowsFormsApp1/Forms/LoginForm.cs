using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public partial class LoginForm : Form
    {
        private MySqlConnection connection;

        public LoginForm()
        {
            InitializeComponent();
            this.passfield.AutoSize = false;
            this.passfield.Size = new Size(this.passfield.Size.Width, 30);
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            connection = DatabaseInitializer.InitializeDatabase();
            if (connection == null)
            {
                MessageBox.Show("Не удалось установить соединение с базой данных.");
            }
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string loginUser = loginfield.Text;
            string passUser = passfield.Text;

            if (connection != null)
            {
                DataTable table = new DataTable();
                MySqlDataAdapter adapter = new MySqlDataAdapter();

                MySqlCommand command = new MySqlCommand("SELECT s_login, s_password FROM stufff WHERE s_login = @sl", connection);
                command.Parameters.Add("@sl", MySqlDbType.VarChar).Value = loginUser;
                adapter.SelectCommand = command;

                try
                {
                    adapter.Fill(table);

                    if (table.Rows.Count > 0)
                    {
                        // Пользователь с таким логином существует в базе данных
                        // Теперь проверим совпадение паролей
                        string passwordFromDB = table.Rows[0]["s_password"].ToString();
                        if (passwordFromDB == passUser)
                        {
                            // Пароль совпадает, открываем MainForm
                            this.Hide();
                            MainForm mainForm = new MainForm(connection);
                            mainForm.Show();
                        }
                        else
                        {
                            // Пароль не совпадает
                            MessageBox.Show("Неверный пароль");
                        }
                    }
                    else
                    {
                        // Пользователя с таким логином нет в базе данных
                        MessageBox.Show("Пользователь с таким логином не существует");
                    }
                }
                catch (MySqlConversionException ex)
                {
                    MessageBox.Show("Ошибка конвертации значения: " + ex.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Произошла ошибка: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Не удалось установить соединение с базой данных.");
            }
        }
        private void registerLabel_MouseEnter(object sender, EventArgs e)
        {
            registerLabel.Font = new Font(registerLabel.Font, FontStyle.Underline);
        }

        private void registerLabel_MouseLeave(object sender, EventArgs e)
        {
            registerLabel.Font = new Font(registerLabel.Font, FontStyle.Regular);
        }
        private void registerLabel_Click(object sender, EventArgs e)
        {
            this.Hide();
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();

        }

        private void LoginForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }
    }
}