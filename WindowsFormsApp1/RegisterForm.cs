using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;
using System.Drawing;

namespace WindowsFormsApp1
{
    public partial class RegisterForm : Form
    {
        private MySqlConnection connection;

        public RegisterForm()
        {
            InitializeComponent();
            connection = DatabaseInitializer.InitializeDatabase();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (fullnamefield.Text == "" || datefield.Text == "" || postfield.Text == "" || loginfield.Text == "" || passfield.Text == "")
            {
                MessageBox.Show("Пожалуйста, заполните все поля");
                return;
            }

            if (isUserExist())
                return;

            MySqlCommand command = new MySqlCommand("INSERT INTO `stufff` (`s_id`, `s_fullname`, `s_date`, `s_gender`, " +
                "`s_passport`, `s_inn`, `s_snils`, `s_post`, `s_sal`, `s_phone`, `s_login`, `s_password`, `s_feducation`," +
                " `s_seducation`, `s_comment`, `reg_date`) VALUES (NULL, @fullname, @date, '', '', '', '', @post, '', " +
                "'', @login, @password, NULL, NULL, NULL, NOW());", connection);

            command.Parameters.Add("@fullname", MySqlDbType.VarChar).Value = fullnamefield.Text;
            command.Parameters.Add("@date", MySqlDbType.Date).Value = DateTime.ParseExact(datefield.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            command.Parameters.Add("@post", MySqlDbType.VarChar).Value = postfield.Text;
            command.Parameters.Add("@login", MySqlDbType.VarChar).Value = loginfield.Text;
            command.Parameters.Add("@password", MySqlDbType.VarChar).Value = passfield.Text;

            if (command.ExecuteNonQuery() == 1)
            {
                MessageBox.Show("Аккаунт создан");
                this.Hide();
                MainForm mainForm = new MainForm(connection);
                mainForm.Show();
            }
            else
                MessageBox.Show("Аккаунт не создан");
        }

        public Boolean isUserExist()
        {
            MySqlCommand command = new MySqlCommand("SELECT COUNT(*) FROM stufff WHERE s_login = @sl", connection);
            command.Parameters.Add("@sl", MySqlDbType.VarChar).Value = loginfield.Text;

            int count = Convert.ToInt32(command.ExecuteScalar());
            if (count > 0)
            {
                MessageBox.Show("Такой логин уже занят");
                return true;
            }
            return false;
        }

        private void back_MouseEnter(object sender, EventArgs e)
        {
            back.Font = new System.Drawing.Font(back.Font, FontStyle.Underline);
        }

        private void back_MouseLeave(object sender, EventArgs e)
        {
            back.Font = new System.Drawing.Font(back.Font, FontStyle.Regular);
        }
        private void back_Click(object sender, EventArgs e)
        {
            this.Hide();
            LoginForm loginForm = new LoginForm();
            loginForm.Show();
        }

     
        private void RegisterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }


        
    }
}