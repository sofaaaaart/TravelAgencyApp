using System;
using System.Collections.Generic; // Добавляем эту директиву
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Text;

namespace WindowsFormsApp1
{
    public partial class MainForm : Form
    {
        private MySqlConnection connection;
        private DataTable currentTable;
        private ComboBox comboBoxForeignKeyValues;

        public MainForm(MySqlConnection existingConnection)
        {
            InitializeComponent();
            connection = existingConnection;
            LoadTablesIntoComboBox();
            comboBoxTables.SelectedIndexChanged += comboBoxTables_SelectedIndexChanged;
            buttonFill.Click += buttonFill_Click;
            buttonDelete.Click += buttonDelete_Click;
            comboBoxForeignKeyValues = new ComboBox();
            // Присваиваем нужные свойства ComboBox
            comboBoxForeignKeyValues.Name = "comboBoxForeignKeyValues";
            comboBoxForeignKeyValues.Location = new System.Drawing.Point(10, 10); // Укажите нужные координаты
                                                                                  // Добавляем ComboBox на форму
            this.Controls.Add(comboBoxForeignKeyValues);
            this.FormClosing += MainForm_FormClosing;
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
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке таблиц: " + ex.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }
            else
            {
                MessageBox.Show("Не удалось установить соединение с базой данных.");
            }
        }

        private bool IsExcludedColumn(string columnName)
        {
            return columnName.EndsWith("_id", StringComparison.OrdinalIgnoreCase) || columnName.Equals("reg_date", StringComparison.OrdinalIgnoreCase);
        }

        public void comboBoxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedTable = comboBoxTables.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedTable) && connection != null)
            {
                try
                {
                    LoadData(selectedTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных таблицы: " + ex.Message);
                }
            }
        }

        private void LoadData(string selectedTable)
        {
            string query = $"SELECT * FROM {selectedTable}";
            MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
            currentTable = new DataTable();
            adapter.Fill(currentTable);

            foreach (DataColumn column in currentTable.Columns.OfType<DataColumn>().ToArray())
            {
                if (IsExcludedColumn(column.ColumnName))
                {
                    currentTable.Columns.Remove(column);
                }
                else if (column.DataType == typeof(DateTime) && column.ColumnName == "s_date")
                {
                    foreach (DataRow dataRow in currentTable.Rows)
                    {
                        string dateString = dataRow["s_date"].ToString();
                        DateTime dateValue;
                        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dateValue))
                        {
                            dataRow["s_date"] = dateValue;
                        }
                        else
                        {
                            dataRow["s_date"] = DBNull.Value;
                        }
                    }
                }
            }

            dataGridView1.DataSource = currentTable;
            LoadForeignKeyValuesIntoComboBox(selectedTable, connection);
        }

        private void LoadForeignKeyValuesIntoComboBox(string selectedTable, MySqlConnection connection)
        {
            List<string> foreignKeyColumns = GetForeignKeyColumns(selectedTable);

            foreach (string foreignKeyColumn in foreignKeyColumns)
            {
                try
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    string query = $"SELECT DISTINCT {foreignKeyColumn} FROM {selectedTable}";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBoxForeignKeyValues.Items.Add(reader[foreignKeyColumn].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке значений внешнего ключа {foreignKeyColumn}: " + ex.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }
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
                MessageBox.Show("Выберите таблицу для вставки данных.");
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

        private void buttonInsert_Click(object sender, EventArgs e)
        {
            // Ваш код обработчика кнопки buttonInsert
        }

        private void RefreshData()
        {
            // Ваш код обновления данных
        }

        private int GetIdFromSelectedRow(DataGridViewRow row, string tableName)
        {
            int id = -1;
            try
            {
                string primaryKeyColumn = GetPrimaryKeyColumn(tableName);
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
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return primaryKeyColumn;
        }

        private List<string> GetForeignKeyColumns(string tableName)
        {
            List<string> foreignKeyColumns = new List<string>();

            try
            {
                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{tableName}' AND REFERENCED_TABLE_NAME IS NOT NULL";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foreignKeyColumns.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при получении столбцов с внешними ключами: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }

            return foreignKeyColumns;
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