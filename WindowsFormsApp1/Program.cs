using System;
using System.IO;
using System.Windows.Forms;
using System.Data.SqlClient;  // Используем SqlClient вместо MySQL
using UniversalCardApp;

namespace WindowsFormsApp1
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Инициализируем базу данных
            SqlConnection connection = DatabaseInitializer.InitializeDatabase();
            if (connection == null)
            {
                MessageBox.Show("Не удалось установить соединение с базой данных.");
                return;
            }

            // Проверка токена при запуске
            string savedToken = null;
            if (File.Exists("session.txt"))
            {
                savedToken = File.ReadAllText("session.txt");
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (savedToken != null && ValidateToken(savedToken, connection))
            {
                Application.Run(new MainForm(connection)); // Главная форма
            }
            else
            {
                Application.Run(new LoginForm(connection)); // Форма логина
            }
        }

        private static bool ValidateToken(string token, SqlConnection connection)
        {
            try
            {
                string query = "SELECT user_id, expires_at FROM user_sessions WHERE token = @token";

                using (SqlCommand cmd = new SqlCommand(query, connection)) // Используем SqlCommand вместо MySqlCommand
                {
                    cmd.Parameters.AddWithValue("@token", token);

                    if (connection.State == System.Data.ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    SqlDataReader reader = cmd.ExecuteReader(); // Используем SqlDataReader вместо MySqlDataReader

                    if (reader.Read())
                    {
                        DateTime expiresAt = Convert.ToDateTime(reader["expires_at"]);

                        if (expiresAt > DateTime.Now)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
            return false;
        }
    }
}
