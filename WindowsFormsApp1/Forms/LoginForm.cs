using MySql.Data.MySqlClient;
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

                MySqlCommand command = new MySqlCommand("SELECT * FROM stufff WHERE s_login = @sl AND s_password = @sp", connection);
                command.Parameters.Add("@sl", MySqlDbType.VarChar).Value = loginUser;
                command.Parameters.Add("@sp", MySqlDbType.VarChar).Value = passUser;

                adapter.SelectCommand = command;
                adapter.Fill(table);

                if (table.Rows.Count > 0)
                {
                    MessageBox.Show("Yes");
                    //this.Hide();
                   //MainForm mainForm = new MainForm();
                    //mainForm.Show();
                }
                else
                    MessageBox.Show("No");
            }
            else
            {
                MessageBox.Show("Не удалось установить соединение с базой данных.");
            }
        }

        private void registerLabel_Click(object sender, EventArgs e)
        {
            this.Hide();
            RegisterForm registerForm = new RegisterForm();
            registerForm.Show();

        }
    }
}