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
        private Dictionary<string, int> idMap;
        private Button insertButton; // Добавляем поле insertButton

        public InsertForm(string tableName, MySqlConnection connection, MainForm mainForm)
        {
            InitializeComponent();
            this.tableName = tableName;
            this.connection = connection;
            this.mainForm = mainForm;
            InitializeForm();
            insertButton = new Button(); // Инициализируем поле insertButton
            insertButton.Enter += insertButton_Enter;
            insertButton.Leave += insertButton_Leave;
        

    }

        private void PopulateComboBoxWithForeignKeyData(ComboBox comboBox, string columnName)
        {
            
            try
            {
                string query = $"SELECT REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string referencedTableName = reader["REFERENCED_TABLE_NAME"].ToString();
                            string referencedColumnName = reader["REFERENCED_COLUMN_NAME"].ToString();

                            if (string.IsNullOrEmpty(referencedTableName) || string.IsNullOrEmpty(referencedColumnName))
                            {
                                Console.WriteLine("Ошибка: Связанная таблица или столбец не найдены.");
                                return;
                            }

                            idMap = new Dictionary<string, int>(); // Инициализируем idMap здесь
                            string dataQuery = $"SELECT * FROM {referencedTableName}";

                            comboBox.DisplayMember = referencedColumnName;
                            comboBox.ValueMember = referencedColumnName;

                            // Закрываем первый DataReader после использования
                            reader.Close();

                            using (MySqlCommand dataCmd = new MySqlCommand(dataQuery, connection))
                            using (MySqlDataReader dataReader = dataCmd.ExecuteReader())
                            {
                                while (dataReader.Read())
                                {
                                    string displayText = "";
                                    foreach (var columnNameFromReader in Enumerable.Range(0, dataReader.FieldCount)
                                        .Select(dataReader.GetName)
                                        .Where(name => !name.EndsWith("_id") && name != "reg_date"))
                                    {
                                        displayText += dataReader[columnNameFromReader].ToString() + " ";
                                    }
                                    int referenceId = Convert.ToInt32(dataReader[referencedColumnName]);
                                    idMap.Add(displayText.Trim(), referenceId);
                                    comboBox.Items.Add(displayText.Trim());
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Ошибка: Данные для комбо-бокса не найдены.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных для комбо-бокса: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private void InitializeForm()
        {
            Controls.Clear();

            DataTable tableSchema = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                tableSchema = GetTableSchema(tableName);

                if (tableSchema != null && tableSchema.Rows.Count > 0)
                {
                    int labelY = 20;
                    int controlY = 20;

                    foreach (DataRow row in tableSchema.Rows)
                    {
                        string columnName = row["COLUMN_NAME"].ToString();
                        bool isAutoIncrement = row["COLUMN_KEY"].ToString().ToLower() == "auto_increment";

                        if (!isAutoIncrement && !columnName.EndsWith("_id") && columnName != "reg_date")
                        {
                            Control control;

                            if (IsForeignKey(tableName, columnName))
                            {
                                ComboBox comboBox = new ComboBox();
                                comboBox.Name = columnName;
                                comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                                comboBox.Location = new Point(120, controlY);
                                comboBox.Width = 150; // Устанавливаем ширину ComboBox
                                comboBox.Visible = true; // Делаем ComboBox видимым

                                Controls.Add(comboBox);

                                try
                                {
                                    PopulateComboBoxWithForeignKeyData(comboBox, columnName);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Ошибка при загрузке данных для комбо-бокса '{columnName}': {ex.Message}");
                                }
                                control = comboBox;
                            }

                            if (columnName.ToLower() == "e_diplom")
                            {
                                TextBox textBox_e_diplom = new TextBox();
                                textBox_e_diplom.Name = columnName;
                                textBox_e_diplom.Location = new Point(120, controlY);
                                textBox_e_diplom.Width = 150;
                                textBox_e_diplom.MaxLength = 15; // Ограничение длины вводимых символов
                                textBox_e_diplom.Visible = true;
                                Controls.Add(textBox_e_diplom);
                                control = textBox_e_diplom;
                            }

                            else if (columnName.EndsWith("_year"))
                            {
                                TextBox textBox_e_year = new TextBox();
                                textBox_e_year.Name = columnName;
                                textBox_e_year.Location = new Point(120, controlY);
                                textBox_e_year.Width = 150;
                                textBox_e_year.MaxLength = 4; // Ограничение длины вводимых символов
                                textBox_e_year.Visible = true;
                                textBox_e_year.KeyPress += (sender, e) =>
                                {
                                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                                    {
                                        e.Handled = true;
                                    }
                                };
                                Controls.Add(textBox_e_year);
                                control = textBox_e_year;
                            }
                            else if (columnName.ToLower() == "s_inn")
                            {
                                TextBox textBox_s_inn = new TextBox();
                                textBox_s_inn.Name = columnName;
                                textBox_s_inn.Location = new Point(120, controlY);
                                textBox_s_inn.Width = 150;
                                textBox_s_inn.MaxLength = 12; // Ограничение длины вводимых символов
                                textBox_s_inn.Visible = true;
                                Controls.Add(textBox_s_inn);
                                control = textBox_s_inn;
                            }
                            else if (columnName.ToLower() == "s_snils")
                            {
                                TextBox textBox_s_snils = new TextBox();
                                textBox_s_snils.Name = columnName;
                                textBox_s_snils.Location = new Point(120, controlY);
                                textBox_s_snils.Width = 150;
                                textBox_s_snils.MaxLength = 14; // Ограничение длины вводимых символов
                                textBox_s_snils.Visible = true;
                                Controls.Add(textBox_s_snils);
                                control = textBox_s_snils;
                            }
                            else
                            {
                                MaskedTextBox maskedTextBox = new MaskedTextBox();
                                maskedTextBox.Name = columnName;
                                maskedTextBox.Location = new Point(120, controlY);
                                maskedTextBox.Width = 150;
                                maskedTextBox.MaxLength = 12; // Ограничение длины вводимых символов

                                // Применяем маску в зависимости от названия столбца
                                if (columnName.ToLower().Contains("date"))
                                {
                                    maskedTextBox.Mask = "0000-00-00";
                                }
                                else if (columnName.ToLower().Contains("phone"))
                                {
                                    maskedTextBox.Mask = "0(000)000-00-00";
                                }

                                // Другие условия для применения масок

                                Controls.Add(maskedTextBox);
                                control = maskedTextBox;
                            }

                            Label label = new Label();
                            label.Text = columnName;
                            label.Location = new Point(20, labelY);
                            Controls.Add(label);

                            labelY += 30;
                            controlY += 30;
                            Console.WriteLine("Добавлен элемент управления: " + columnName);
                        }
                    }

                    Button insertButton = new Button();
                    insertButton.Text = "Insert Data";
                    insertButton.Location = new Point(20, controlY);
                    insertButton.FlatStyle = FlatStyle.Flat; // Устанавливаем стиль кнопки на Flat
                    insertButton.Click += InsertButton_Click;
                    insertButton.Enter += insertButton_Enter;
                    insertButton.Leave += insertButton_Leave;
                    Controls.Add(insertButton);

                    int formHeight = insertButton.Bottom + 50; // Добавляем 20 пикселей от нижнего края кнопки
                    this.Height = formHeight;
                }
                else
                {
                    Console.WriteLine("Схема таблицы не найдена или пуста.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при загрузке схемы таблиц: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private bool IsForeignKey(string tableName, string columnName)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке внешнего ключа: {ex.Message}");
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        public void RefreshData()
        {
            mainForm.RefreshDataGridViewData(); // Перезагрузка данных в комбо-боксе
        
            
        }

        private DataTable GetTableSchema(string tableName)
        {
            DataTable schemaTable = null;
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = $"SELECT DISTINCT COLUMN_NAME, COLUMN_KEY FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND TABLE_SCHEMA = DATABASE()";
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
                connection.Open();
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
                    else if (control is ComboBox comboBox)
                    {
                        if (!firstParameter)
                        {
                            command.CommandText += ", ";
                            valuesCommand.CommandText += ", ";
                        }
                        command.CommandText += control.Name;
                        valuesCommand.CommandText += $"@{control.Name}";
                        string selectedText = comboBox.SelectedItem.ToString();
                        int selectedId = idMap[selectedText];
                        command.Parameters.AddWithValue($"@{control.Name}", selectedId);
                        firstParameter = false;
                    }
                }

                command.CommandText += ") " + valuesCommand.CommandText + ")";
                command.ExecuteNonQuery();
                RefreshData();
                MessageBox.Show("Данные успешно вставлены!");

                // Очистка полей формы вставки данных
                foreach (Control control in Controls)
                {
                    if (control is TextBox)
                    {
                        ((TextBox)control).Clear();
                    }
                }

                Close(); // Закрытие формы вставки данных
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при вставке данных: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        private void InsertForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            RefreshData();
            Console.WriteLine("Форма закрыта.");
        }

        private void insertButton_Enter(object sender, EventArgs e)
        {
            insertButton.Font = new Font(insertButton.Font, FontStyle.Underline);
        }

        private void insertButton_Leave(object sender, EventArgs e)
        {
            insertButton.Font = new Font(insertButton.Font, FontStyle.Regular);
        }
    }
}

