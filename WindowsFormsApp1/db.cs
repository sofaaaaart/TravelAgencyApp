using System;
using System.Data.SqlClient;
using System.IO;

public static class DatabaseInitializer
{
    private static readonly string connectionString = @"Server=.\SQLEXPRESS;Integrated Security=True;";
    private static readonly string databaseName = "db_ta";

    public static SqlConnection InitializeDatabase()
    {
        if (!DatabaseExists(databaseName))
        {
            CreateDatabase();
            ExecuteScript("database.sql");
        }
        else
        {
            Console.WriteLine("✅ База данных уже существует.");
        }

        SqlConnection connection = new SqlConnection($"{connectionString}Database={databaseName};");

        try
        {
            connection.Open();
            Console.WriteLine("✅ Соединение с БД установлено!");
            return connection;
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Ошибка при подключении: " + ex.Message);
            return null;
        }
    }

    private static bool DatabaseExists(string databaseName)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = $"SELECT database_id FROM sys.databases WHERE name = '{databaseName}'";
            SqlCommand cmd = new SqlCommand(query, conn);
            return cmd.ExecuteScalar() != null;
        }
    }

    private static void CreateDatabase()
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();
            string query = $"CREATE DATABASE {databaseName}";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();
            Console.WriteLine("✅ База данных создана.");
        }
    }

    private static void ExecuteScript(string scriptFileName)
    {
        try
        {
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptFileName);
            if (!File.Exists(scriptPath))
            {
                Console.WriteLine($"❌ Файл {scriptFileName} не найден.");
                return;
            }

            string script = File.ReadAllText(scriptPath);
            using (SqlConnection conn = new SqlConnection($"{connectionString}Database={databaseName};"))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(script, conn);
                cmd.ExecuteNonQuery();
                Console.WriteLine("✅ Скрипт успешно выполнен.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Ошибка при выполнении скрипта: " + ex.Message);
        }
    }
}
