using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Drawing;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        private MySqlConnection connection;
        private DataTable currentTable;

        public MainForm(MySqlConnection existingConnection)
        {
            InitializeComponent();
            connection = existingConnection;
            LoadTablesIntoComboBox();
            comboBoxTables.SelectedIndexChanged += comboBoxTables_SelectedIndexChanged;
            buttonFill.Click += buttonFill_Click;
            buttonDelete.Click += buttonDelete_Click;
            this.FormClosing += MainForm_FormClosing;

            comboBoxTables.DropDownStyle = ComboBoxStyle.DropDownList;
        }


        public class ForeignKeyInfo
        {
            public string ColumnName { get; set; }
            public string ReferencedTableName { get; set; }
            public string ReferencedColumnName { get; set; }
        }

        private bool TableExists(string tableName)
            {
                try
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    DataTable schema = connection.GetSchema("Tables");
                    foreach (DataRow row in schema.Rows)
                    {
                        if (string.Equals(row["TABLE_NAME"].ToString(), tableName, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error checking table existence: " + ex.Message);
                    MessageBox.Show("Ошибка при провeркe сущeствования таблицы: " + ex.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }

                return false;
            }


        public void RefreshDataGridViewData()
        {
            string selectedTable = comboBoxTables.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedTable) && connection != null)
            {
                try
                {
                    Console.WriteLine("Refreshing data for table: " + selectedTable);
                    if (!TableExists(selectedTable))
                    {
                        throw new Exception($"Выбранная таблица '{selectedTable}' не существует.");
                    }

                    string query = $"SELECT * FROM `{selectedTable}`";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    currentTable = new DataTable();
                    adapter.Fill(currentTable);

                   
                    // Скрытие столбца с именем "reg_date"
                    if (dataGridView1.Columns.Contains("reg_date"))
                    {
                        dataGridView1.Columns["reg_date"].Visible = false;
                    }

                    dataGridView1.DataSource = currentTable;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error refreshing data: " + ex.Message);
                    MessageBox.Show("Ошибка при обновлении данных таблицы: " + ex.Message);
                }
            }
        }

        public void LoadDataIntoDataGridView(string selectedTable)
        {
            // Проверка наличия выбранной таблицы и соединения с базой данных
            if (!string.IsNullOrEmpty(selectedTable) && connection != null)
            {
                try
                {
                    string query = $"SELECT * FROM `{selectedTable}`";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    currentTable = new DataTable();
                    adapter.Fill(currentTable);

                    /* Удаление столбцов с окончанием "_id" и "reg_date"
                    List<DataColumn> columnsToRemove = new List<DataColumn>();
                    foreach (DataColumn column in currentTable.Columns)
                    {
                        if (column.ColumnName.EndsWith("_id", StringComparison.OrdinalIgnoreCase) || column.ColumnName.Equals("reg_date", StringComparison.OrdinalIgnoreCase))
                        {
                            columnsToRemove.Add(column);
                        }
                    }
                    foreach (DataColumn column in columnsToRemove)
                    {
                        currentTable.Columns.Remove(column);
                    }*/

                    // Установка DataTable в качестве источника данных для DataGridView
                    dataGridView1.DataSource = currentTable;

                     //Скрытие столбца с именем "reg_date"
                    if (dataGridView1.Columns.Contains("reg_date"))
                    {
                        dataGridView1.Columns["reg_date"].Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading data: " + ex.Message);
                    MessageBox.Show("Ошибка при загрузке данных таблицы: " + ex.Message);
                }
            }
        }

        public void LoadTablesIntoComboBox()
        {
            if (connection != null)
            {
                try
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    DataTable tables = connection.GetSchema("Tables");

                    comboBoxTables.Items.Clear();

                    foreach (DataRow row in tables.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        comboBoxTables.Items.Add(tableName);
                        Console.WriteLine("Добавлeна таблица в комбо-бокс: " + tableName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading tables: " + ex.Message);
                    MessageBox.Show("Ошибка при загрузкe таблиц: " + ex.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }
            else
            {
                MessageBox.Show("Нe удалось установить соeдинeниe с базой данных.");
            }
        }

        public void comboBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTable = comboBoxTables.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedTable) && connection != null)
            {
                try
                {
                    LoadDataIntoDataGridView(selectedTable);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading data: " + ex.Message);
                    MessageBox.Show("Ошибка при загрузке данных таблицы: " + ex.Message);
                }
            }
        }

        private int GetIdFromSelectedRow(DataGridViewRow row, string tableName)
        {
            int id = -1;
            try
            {
                if (row != null)
                {
                    // Получаем значение первичного ключа из базы данных на основе выбранной строки
                    id = GetPrimaryKeyValueFromDatabase(row, tableName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting record identifier: " + ex.Message);
                MessageBox.Show("Ошибка при определении идентификатора записи: " + ex.Message);
            }
            return id;
        }

        private int GetPrimaryKeyValueFromDatabase(DataGridViewRow row, string tableName)
        {
            int id = -1;
            try
            {
                string primaryKeyColumn = GetPrimaryKeyColumn(tableName);
                if (!string.IsNullOrEmpty(primaryKeyColumn))
                {
                    object primaryKeyValue = row.Cells[primaryKeyColumn].Value;
                    if (primaryKeyValue != null)
                    {
                        string query = $"SELECT {primaryKeyColumn} FROM {tableName} WHERE {primaryKeyColumn} = @id";
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@id", primaryKeyValue);
                            connection.Open();
                            var result = command.ExecuteScalar();
                            if (result != null)
                            {
                                id = Convert.ToInt32(result);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Primary key value in column '{primaryKeyColumn}' is null.");
                    }
                }
                else
                {
                    MessageBox.Show($"Primary key column for table '{tableName}' not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting primary key value from database: " + ex.Message);
                MessageBox.Show("Ошибка при получении значения первичного ключа из базы данных: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return id;
        }

        private string GetPrimaryKeyColumn(string tableName)
        {
            string primaryKeyColumn = null;
            try
            {
                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{tableName}' AND CONSTRAINT_NAME = 'PRIMARY'";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            primaryKeyColumn = reader.GetString(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting primary key column: " + ex.Message);
                MessageBox.Show("Ошибка при получeнии столбца пeрвичного ключа: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return primaryKeyColumn;
        }
        private void buttonFill_Click(object sender, EventArgs e)
        {
            string selectedTable = comboBoxTables.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedTable))
            {
                InsertForm insertForm = new InsertForm(selectedTable, connection, this);
                insertForm.Show();
            }
            else
            {
                MessageBox.Show("Выбeритe таблицу для вставки данных.");
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            string tableName = comboBoxTables.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(tableName))
            {
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                    {
                        int id = GetIdFromSelectedRow(row, tableName);
                        if (id != -1)
                        {
                            DeleteRecordFromDatabase(id, tableName);
                            dataGridView1.Rows.Remove(row);
                        }
                        else
                        {
                            MessageBox.Show("Нe удалось опрeдeлить идeнтификатор записи.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выбeритe строку для удалeния.");
                }
            }
            else
            {
                MessageBox.Show("Выбeритe таблицу для удалeния.");
            }
        }
        private void DeleteRecordFromDatabase(int id, string tableName)
        {
            try
            {
                string primaryKeyColumn = GetPrimaryKeyColumn(tableName);
                string query = $"DELETE FROM {tableName} WHERE {primaryKeyColumn} = @id";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting record from database: " + ex.Message);
                MessageBox.Show("Ошибка при удалении записи из базы данных: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }


        private void buttonFill_Enter(object sender, EventArgs e)
        {
            buttonFill.Font = new Font(buttonFill.Font, FontStyle.Underline);
        }

        private void buttonFill_Leave(object sender, EventArgs e)
        {
            buttonFill.Font = new Font(buttonFill.Font, FontStyle.Regular);
        }

        private void buttonDelete_Enter(object sender, EventArgs e)
        {
            buttonDelete.Font = new Font(buttonDelete.Font, FontStyle.Underline);
        }

        private void buttonDelete_Leave(object sender, EventArgs e)
        {
            buttonDelete.Font = new Font(buttonDelete.Font, FontStyle.Regular);
        }




        /*public void LoadDataIntoDataGridView(string selectedTable)
        {
            // Проверка наличия выбранной таблицы и соединения с базой данных
            if (!string.IsNullOrEmpty(selectedTable) && connection != null)
            {
                try
                {
                    string query = $"SELECT * FROM `{selectedTable}`";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    currentTable = new DataTable();
                    adapter.Fill(currentTable);

                    // Проходим по столбцам DataGridView и заменяем значения связанных полей
                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        if (IsForeignKeyColumn(selectedTable, column.Name))
                        {
                            string referencedValueColumn = displayColumns[foreignKeyMappings[selectedTable][column.Name]];
                            foreach (DataGridViewRow row in dataGridView1.Rows)
                            {
                                object foreignKeyValue = row.Cells[column.Name].Value;
                                string foreignKeyValueString = foreignKeyValue.ToString();
                                string referencedValue = GetReferencedValue(selectedTable, column.Name, foreignKeyValueString);
                                row.Cells[column.Name].Value = referencedValue;
                            }
                        }
                    }

                    // Установка DataTable в качестве источника данных для DataGridView
                    dataGridView1.DataSource = currentTable;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error loading data: " + ex.Message);
                    MessageBox.Show("Ошибка при загрузке данных таблицы: " + ex.Message);
                }
            }
        }

        private bool IsExcludedColumn(string columnName)
        {
            return columnName.EndsWith("_id", StringComparison.OrdinalIgnoreCase) || columnName.Equals("reg_date", StringComparison.OrdinalIgnoreCase);
        }


        private Dictionary<string, string> displayColumns = new Dictionary<string, string>()
        {
            { "clients", "c_fullname" },
            { "education", "e_specialty" },
            { "stufff", "s_fullname" },
            { "touroperators", "t_name" },
            { "status", "status_name" },
            { "types", "et_name" },
            // Добавьте другие таблицы и соответствующие столбцы для отображения
        };

        private Dictionary<string, Dictionary<string, string>> foreignKeyMappings = new Dictionary<string, Dictionary<string, string>>()
        {
            { "clients", new Dictionary<string, string>() { { "c_education", "education" } } },
            { "stufff", new Dictionary<string, string>() { { "s_feducation", "education" }, { "s_seducation", "education" } } },
            { "requests", new Dictionary<string, string>() { { "r_client", "clients" }, { "r_stuff", "stufff" }, { "r_tourop", "touroperators" }, { "r_status", "status" } } },
            { "process", new Dictionary<string, string>() { { "p_staff", "stufff" }, { "p_req", "requests" }, { "p_status", "status" } } },
            { "education", new Dictionary<string, string>() { { "e_type", "types" } } } // Изменено на "types"
        };

        // Определение метода для преобразования типов данных в DataTable


        private bool IsForeignKeyColumn(string tableName, string columnName)
        {
            if (foreignKeyMappings.ContainsKey(tableName))
            {
                foreach (var mapping in foreignKeyMappings[tableName])
                {
                    if (mapping.Key.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private string GetReferencedValue(string tableName, string foreignKeyColumn, string foreignKeyValue)
        {
            string referencedValue = null;
            try
            {
                string referencedTableName = foreignKeyMappings[tableName][foreignKeyColumn];
                string displayColumn = displayColumns[referencedTableName];
                string query = $"SELECT {displayColumn} FROM {referencedTableName} WHERE {GetPrimaryKeyColumn(referencedTableName)} = @id";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", foreignKeyValue);
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        referencedValue = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting referenced value for {tableName}.{foreignKeyColumn}: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            return referencedValue;
        } */

    }
       
}