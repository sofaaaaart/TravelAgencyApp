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
        using (SqlConnection conn = new SqlConnection($"{connectionString}Database={databaseName};"))
        {
            conn.Open();
            using (SqlTransaction transaction = conn.BeginTransaction())
            {
                try
                {
                    string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, scriptFileName);
                    Console.WriteLine($"Путь к файлу скрипта: {scriptPath}");
                    if (!File.Exists(scriptPath))
                    {
                        Console.WriteLine($"❌ Файл {scriptFileName} не найден.");
                        return;
                    }

                    string script = File.ReadAllText(scriptPath);
                    using (SqlCommand cmd = new SqlCommand(script, conn, transaction))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Вставка начальных данных
                    InsertInitialData(conn, transaction);

                    // Коммитим транзакцию, если все успешно
                    transaction.Commit();
                    Console.WriteLine("✅ Все операции успешно выполнены.");
                }
                catch (SqlException ex)
                {
                    // Откатываем транзакцию в случае ошибки
                    transaction.Rollback();
                    Console.WriteLine($"❌ Ошибка: {ex.Message}");
                }
            }
        }
    }

    private static void InsertInitialData(SqlConnection conn, SqlTransaction transaction)
    {
        try
        {
            string insertQuery = @"
            INSERT INTO status (status_name) 
            VALUES 
            ('Не обработана'),
            ('В работе'),
            ('Отослана'),
            ('Аннулирована'),
            ('ОК'),
            ('Wait list');

            INSERT INTO touroperators (t_name) 
            VALUES 
            ('Pegas Touristik'),
            ('Fun & Sun'),
            ('НТК Интурист'),
            ('Библио Глобус'),
            ('Anex Tour'),
            ('Sunmar'),
            ('Coral Travel'),
            ('Tez Tour'),
            ('Алеан'),
            ('Русский Экспресс'),
            ('Амботис'),
            ('SpaceTravel');
        ";

            using (SqlCommand cmd = new SqlCommand(insertQuery, conn, transaction))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("✅ Начальные данные добавлены.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Ошибка при добавлении начальных данных: " + ex.Message);
        }
    }

}
