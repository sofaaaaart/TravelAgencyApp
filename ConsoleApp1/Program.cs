using System;
using System.Data.SqlClient;
using System.IO;

namespace SQLFileExecutor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Проверяем, передан ли путь к файлу SQL в аргументах командной строки
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: SQLFileExecutor database.sql");
                return;
            }

            string filePath = args[0];

            // Проверяем, существует ли файл
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return;
            }

            string sqlConnectionString = "host=localhost;port=3306;user=root;password=;"; // Замените на свою строку подключения

            try
            {
                // Чтение SQL-скрипта из файла
                string sqlScript = File.ReadAllText(filePath);

                // Выполнение SQL-скрипта
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    SqlCommand command = connection.CreateCommand();
                    command.CommandText = sqlScript;
                    command.ExecuteNonQuery();

                    Console.WriteLine("SQL script executed successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error executing SQL script: " + ex.Message);
            }
        }
    }
}