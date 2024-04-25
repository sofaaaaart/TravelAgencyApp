using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace WindowsFormsApp1
{
    public partial class InsertForm : Form
    {
        private MySqlConnection connection;
        private string tableName;
        private MainForm mainForm;

        public InsertForm(string tableName, MySqlConnection connection, MainForm mainForm)
        {
            InitializeComponent();
            this.tableName = tableName;
            this.connection = connection;
            this.mainForm = mainForm;
            InitializeForm();
        }

        private void InitializeForm()
        {
            Controls.Clear();

            DataTable tableSchema = null;

            // Открытие соединения
            if (connection.State != ConnectionState.Open)
                connection.Open();

            try
            {
                tableSchema = GetTableSchema(tableName);

                // Остальной код...
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке схемы таблиц: " + ex.Message);
            }
            finally
            {
                // Закрытие соединения
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }


            if (tableSchema != null && tableSchema.Rows.Count > 0)
            {
                int labelY = 20;
                int textBoxY = 20;

                foreach (DataRow row in tableSchema.Rows)
                {
                    string columnName = row["COLUMN_NAME"].ToString();
                    bool isAutoIncrement = row["COLUMN_KEY"].ToString().ToLower() == "auto_increment";

                    if (!isAutoIncrement && !columnName.EndsWith("_id") && columnName != "reg_date")
                    {
                        if (!Controls.ContainsKey(columnName))
                        {
                            Label label = new Label();
                            label.Text = columnName;
                            label.Location = new Point(20, labelY);
                            Controls.Add(label);

                            TextBox textBox = new TextBox();
                            textBox.Name = columnName;
                            textBox.Location = new Point(120, textBoxY);
                            Controls.Add(textBox);
                        }

                        labelY += 30;
                        textBoxY += 30;
                        Console.WriteLine("Добавлен элемент управления: " + columnName);
                    }
                }

                ComboBox comboBoxTypes = new ComboBox();
                comboBoxTypes.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBoxTypes.Location = new Point(60, textBoxY);
                Controls.Add(comboBoxTypes);

                try
                {
                    string query = "SELECT et_id, et_name FROM types";
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int typeId = Convert.ToInt32(reader["et_id"]);
                            string typeName = reader["et_name"].ToString();
                            comboBoxTypes.Items.Add(new KeyValuePair<int, string>(typeId, typeName));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Ошибка при загрузке типов образования: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }

                Button insertButton = new Button();
                insertButton.Text = "Insert Data";
                insertButton.Location = new Point(90, textBoxY);
                insertButton.Click += InsertButton_Click;
                Controls.Add(insertButton);
            }
            else
            {
                MessageBox.Show("Схема таблицы не найдена или пуста.");
            }
        }

        public void RefreshData()
        {
            mainForm.LoadTablesIntoComboBox();
            mainForm.comboBoxTables_SelectedIndexChanged(null, null);
        }

        private void InsertForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            RefreshData();
            Console.WriteLine("Форма закрывается.");
        }

        private DataTable GetTableSchema(string tableName)
        {
            DataTable schemaTable = null;
            try
            {
                string query = $"SELECT COLUMN_NAME, COLUMN_KEY FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}'";
                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                schemaTable = new DataTable();
                adapter.Fill(schemaTable);

                Console.WriteLine("Получена схема таблицы:");
                foreach (DataRow row in schemaTable.Rows)
                {
                    Console.WriteLine(row["COLUMN_NAME"]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении схемы таблицы: " + ex.Message);
            }
            return schemaTable;
        }

        private void InsertButton_Click(object sender, EventArgs e)
        {
            try
            {
                MySqlCommand command = new MySqlCommand($"INSERT INTO {tableName} (", connection);
                MySqlCommand valuesCommand = new MySqlCommand("VALUES (", connection);
                bool firstParameter = true;

                foreach (Control control in Controls)
                {
                    if (control is TextBox)
                    {
                        if (!firstParameter)
                        {
                            command.CommandText += ", ";
                            valuesCommand.CommandText += ", ";
                        }
                        command.CommandText += control.Name;
                        valuesCommand.CommandText += $"@{control.Name}";
                        command.Parameters.AddWithValue($"@{control.Name}", ((TextBox)control).Text);
                        firstParameter = false;
                    }
                }

                command.CommandText += ") " + valuesCommand.CommandText + ")";
                command.ExecuteNonQuery();
                MessageBox.Show("Данные успешно вставлены!");
                mainForm.RefreshData();
                Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при вставке данных: " + ex.Message);
            }
        }
    }
}