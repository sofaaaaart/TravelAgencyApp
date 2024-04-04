using System;
using System.IO;
using MySql.Data.MySqlClient;

public static class DatabaseInitializer
{
    private static string connectionString = "host=localhost;port=3306;user=root;password=;";
    private static string databaseName = "db_touragency";

    public static MySqlConnection InitializeDatabase()
    {
        // Получаем путь к базовой директории проекта
        string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

        // Формируем путь к файлу базы данных относительно базовой директории проекта
        string scriptFileName = Path.Combine(projectDirectory, "database.sql");

        if (!DatabaseExists(databaseName))
        {
            CreateDatabase();
            ExecuteScript(scriptFileName);
        }
        else
        {
            Console.WriteLine("База данных уже существует.");
        }

        MySqlConnection connection = new MySqlConnection(connectionString + $"database={databaseName}");

        try
        {
            connection.Open();
            Console.WriteLine("Соединение установлено!");
            return connection;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при подключении к базе данных: " + ex.Message);
            return null;
        }
    }

    private static bool DatabaseExists(string databaseName)
    {
        string checkDatabaseQuery = $"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}'";

        using (MySqlConnection tempConnection = new MySqlConnection(connectionString))
        {
            tempConnection.Open();
            MySqlCommand command = new MySqlCommand(checkDatabaseQuery, tempConnection);
            object result = command.ExecuteScalar();
            return (result != null && result.ToString() == databaseName);
        }
    }

    private static void CreateDatabase()
    {
        string createDatabaseQuery = $"CREATE DATABASE IF NOT EXISTS {databaseName}";

        using (MySqlConnection tempConnection = new MySqlConnection(connectionString))
        {
            tempConnection.Open();
            MySqlCommand command = new MySqlCommand(createDatabaseQuery, tempConnection);
            command.ExecuteNonQuery();
            Console.WriteLine("База данных успешно создана.");
        }
    }

    private static void ExecuteScript(string scriptFileName)
    {
        try
        {
            string scriptPath = Path.Combine(Environment.CurrentDirectory, scriptFileName);
            if (!File.Exists(scriptPath))
            {
                Console.WriteLine($"Файл {scriptFileName} не найден.");
                return;
            }

            string script = File.ReadAllText(scriptPath);
            Console.WriteLine("Executing SQL script:");
            Console.WriteLine(script);

            using (MySqlConnection tempConnection = new MySqlConnection(connectionString + $"database={databaseName}"))
            {
                tempConnection.Open();
                MySqlCommand command = new MySqlCommand(script, tempConnection);
                command.ExecuteNonQuery();
                Console.WriteLine("Скрипт успешно выполнен.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при выполнении скрипта: " + ex.Message);
        }
    }
}
