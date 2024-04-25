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
            this.passfield.Size = new Size(this.passfield.Size.Width, 64);
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

                MySqlCommand command = new MySqlCommand("SELECT s_login, s_password FROM stufff WHERE s_login = @sl AND s_password = @sp", connection);
                command.Parameters.Add("@sl", MySqlDbType.VarChar).Value = loginUser;
                command.Parameters.Add("@sp", MySqlDbType.VarChar).Value = passUser;
                adapter.SelectCommand = command;

                try
                {
                    adapter.Fill(table);
                    // Проверка, если такого пользователя в таблице нет
                    MySqlCommand checkCommand = new MySqlCommand("SELECT COUNT(*) FROM stufff WHERE s_login = @sl", connection);
                    checkCommand.Parameters.Add("@sl", MySqlDbType.VarChar).Value = loginUser;
                    int userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (userCount == 0)
                    {
                        MessageBox.Show("Пользователь с таким логином не существует.");
                    }
                    else
                    {
                        if (table.Rows.Count > 0)
                        {
                            this.Hide();
                            MainForm mainForm = new MainForm(connection);
                            mainForm.Show();
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль.");
                        }
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