using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

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
            // Добавление обработчика события comboBoxTables_SelectedIndexChanged
            comboBoxTables.SelectedIndexChanged += comboBoxTables_SelectedIndexChanged;
            // Добавление обработчика события buttonFill_Click
            buttonFill.Click += buttonFill_Click;
            // Добавление обработчика события buttonDelete_Click
            buttonDelete.Click += buttonDelete_Click;
            // Добавление обработчика события MainForm_FormClosing
            this.FormClosing += MainForm_FormClosing;
        }

        private void LoadTablesIntoComboBox()
        {
            if (connection != null)
            {
                try
                {
                    DataTable tables = connection.GetSchema("Tables");
                    foreach (DataRow row in tables.Rows)
                    {
                        string tableName = row["TABLE_NAME"].ToString();
                        comboBoxTables.Items.Add(tableName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке таблиц: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Не удалось установить соединение с базой данных.");
            }
        }

        private bool IsExcludedColumn(string columnName)
        {
            // Исключаем любые поля, содержащие в своем имени "_id" или "reg_date"
            return columnName.EndsWith("_id", StringComparison.OrdinalIgnoreCase) || columnName.Equals("reg_date", StringComparison.OrdinalIgnoreCase);
        }

        private void comboBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTable = comboBoxTables.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedTable) && connection != null)
            {
                try
                {
                    string query = "SELECT * FROM " + selectedTable;
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                    currentTable = new DataTable();
                    adapter.Fill(currentTable);

                    // Удаление исключаемых полей
                    foreach (DataColumn column in currentTable.Columns.OfType<DataColumn>().ToArray())
                    {
                        if (IsExcludedColumn(column.ColumnName))
                        {
                            currentTable.Columns.Remove(column);
                        }
                        else if (column.DataType == typeof(DateTime) && column.ColumnName == "s_date")
                        {
                            // Преобразовать строку в DateTime используя явное указание формата даты
                            foreach (DataRow dataRow in currentTable.Rows)
                            {
                                string dateString = dataRow["s_date"].ToString();
                                DateTime dateValue;
                                if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateValue))
                                {
                                    dataRow["s_date"] = dateValue;
                                }
                                else if (column.DataType == typeof(DateTime) && column.ColumnName == "s_date")
                                {
                                    // Пропускаем преобразование, если тип данных уже DateTime
                                    continue;
                                }
                                else
                                {
                                    // Обработка неверного формата даты
                                    // Например, установка значения по умолчанию или вывод сообщения об ошибке
                                    dataRow["s_date"] = DBNull.Value; // или другая обработка ошибки
                                }
                            }
                        }
                    }

                    dataGridView1.DataSource = currentTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных таблицы: " + ex.Message);
                }
            }
        }

        private void buttonFill_Click(object sender, EventArgs e)
        {
            // Здесь можно добавить логику для заполнения таблицы
            MessageBox.Show("Функционал заполнения таблицы еще не реализован.");
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
                            MessageBox.Show("Не удалось определить идентификатор записи.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите строку для удаления.");
                }
            }
            else
            {
                MessageBox.Show("Выберите таблицу для удаления.");
            }
        }

        private int GetIdFromSelectedRow(DataGridViewRow row, string tableName)
        {
            int id = -1;
            try
            {
                // Получаем имя столбца первичного ключа
                string primaryKeyColumn = GetPrimaryKeyColumn(tableName);

                // Получаем значение из ячейки столбца первичного ключа
                id = Convert.ToInt32(row.Cells[primaryKeyColumn].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при определении идентификатора записи: " + ex.Message);
            }
            return id;
        }

        private string GetPrimaryKeyColumn(string tableName)
        {
            string primaryKeyColumn = null;
            try
            {
                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{tableName}' AND CONSTRAINT_NAME = 'PRIMARY'";
                // Corrected query to specifically target primary key constraint

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
                MessageBox.Show("Ошибка при получении столбца первичного ключа: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }

            return primaryKeyColumn;
        }

        private void DeleteRecordFromDatabase(int id, string tableName)
{
    try
    {
        string query = $"DELETE FROM {tableName} WHERE {GetPrimaryKeyColumn(tableName)} = @id";
        using (MySqlCommand command = new MySqlCommand(query, connection))
        {
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
    catch (Exception ex)
    {
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
    }
}