using Guna.UI2.WinForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class AddRequestForm : Form
    {
        private readonly SqlConnection connection;

        private Image normalHideImage;
        private Image hoverHideImage;
        private Image normalCloseImage;
        private Image hoverCloseImage;

        public AddRequestForm(SqlConnection connection)
        {
            InitializeComponent();
            this.connection = connection; // Передаём подключение в поле класса
            InitializeDatabase();
            InitializeImages();

            startDate.CustomFormat = "  dd.MM.yyyy"; // Добавляем пробелы
            startDate.Format = DateTimePickerFormat.Custom;

            endDate.CustomFormat = "  dd.MM.yyyy"; // Добавляем пробелы
            endDate.Format = DateTimePickerFormat.Custom;

            // Заполняем ComboBox числами от 1 до 10
            for (int i = 1; i <= 10; i++)
            {
                adultPersons.Items.Add(i.ToString());
                childPersons.Items.Add(i.ToString());
            }


            FillClientComboBox();
            FillStatusComboBox();
            FillTouroperatorComboBox();

            phoneNumber.Tag = "  Введите номер телефона";
            staffName.Tag = "Введите ФИО сотрудника";
            country.Tag = "Введите страну поездки";
            costTO.Tag = "  Введите стоимость";
            departureCityText.Tag = "Введите город вылета";
        }

        private void InitializeDatabase()
        {
            if (connection == null)
            {
                MessageBox.Show("Не удалось установить соединение с базой данных.");
            }
        }
        private void InitializeImages()
        {
            // Установите базовый путь
            string basePath = AppDomain.CurrentDomain.BaseDirectory; // Путь к исполняемому файлу
            string imagePath = Path.Combine(basePath, "img"); // Путь к папке "img"

            try
            {
                // Загрузите изображения для HideButton
                normalHideImage =Image.FromFile(Path.Combine(imagePath, "ButtonHide.png"));
                hoverHideImage = Image.FromFile(Path.Combine(imagePath, "ButtonHide1.png"));

                // Загрузите изображения для CloseButton
                normalCloseImage = System.Drawing.Image.FromFile(Path.Combine(imagePath, "ButtonClose.png"));
                hoverCloseImage = System.Drawing.Image.FromFile(Path.Combine(imagePath, "ButtonClose1.png"));
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"Ошибка: изображение не найдено. {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
            }
        }



        private void AddButton_Click(object sender, EventArgs e)
        {
            if (!ValidateFields())
            {
                MessageBox.Show("Пожалуйста, заполните все поля корректно.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int clientId = GetOrCreateClient(clientsName.Text, phoneNumber.Text);
            clientId = GetSelectedClientId();  // Получаем ID клиента из ComboBox
            if (clientId == -1)
            {
                MessageBox.Show("Пожалуйста, выберите клиента.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int staffId = GetStaffId(staffName.Text);
            if (staffId == -1)
            {
                MessageBox.Show("Сотрудник не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning); 
                return; // Прерываем выполнение, так как сотрудник не найден
            } 


            int touroperatorId = GetTouroperatorId(touroperator.Text);
            int statusId = GetStatusId(status.Text);
            
            string cleanedPhone = Regex.Replace(phoneNumber.Text, @"[^\d+]", ""); // Убираем все лишние символы
            if (cleanedPhone.StartsWith("  +7"))
            {
                cleanedPhone = "8" + cleanedPhone.Substring(2); // Заменяем +7 на 8
            }
            decimal costTo = decimal.Parse(costTOLabel.Text.Replace(" EUR", ""));
            string adultsChildren = $"{adultPersons.Text} взрослых, {childPersons.Text} детей";

            using (SqlCommand cmd = new SqlCommand(@"
    INSERT INTO requests 
    (r_client, r_phone, r_staff, r_start_date, r_end_date, r_country, r_tourop, r_tourop_cost, r_status, r_departure_city, r_adults_children)
    VALUES (@client, @phone, @staff, @startDate, @endDate, @country, @touroperator, @costTo, @status, @departureCity, @adultsChildren)", connection))
            {
                cmd.Parameters.AddWithValue("@client", clientId);
                cmd.Parameters.AddWithValue("@phone", cleanedPhone);
                cmd.Parameters.AddWithValue("@staff", staffId);
                cmd.Parameters.AddWithValue("@startDate", startDate.Value);
                cmd.Parameters.AddWithValue("@endDate", endDate.Value);
                cmd.Parameters.AddWithValue("@country", country.Text);
                cmd.Parameters.AddWithValue("@touroperator", touroperatorId);
                cmd.Parameters.AddWithValue("@costTo", costTo);
                cmd.Parameters.AddWithValue("@status", statusId);
                cmd.Parameters.AddWithValue("@departureCity", departureCityText.Text);
                cmd.Parameters.AddWithValue("@adultsChildren", adultsChildren);

                connection.Open();
                cmd.ExecuteNonQuery();
                connection.Close();
            }

            MessageBox.Show("Заявка успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool ValidateFields()
        {
            return !string.IsNullOrWhiteSpace(clientsName.Text) &&
                   !string.IsNullOrWhiteSpace(phoneNumber.Text) &&
                   !string.IsNullOrWhiteSpace(staffName.Text) &&  // Используем ComboBox вместо TextBox
                   !string.IsNullOrWhiteSpace(country.Text) &&
                   !string.IsNullOrWhiteSpace(touroperator.Text) &&
                   !string.IsNullOrWhiteSpace(departureCityText.Text) &&
                   !string.IsNullOrWhiteSpace(adultPersons.Text) &&
                   !string.IsNullOrWhiteSpace(childPersons.Text) &&
                   !string.IsNullOrWhiteSpace(status.Text) &&
                   decimal.TryParse(costTOLabel.Text.Replace(" EUR", ""), out _);
        }

        private int GetOrCreateClient(string name, string phone)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT c_id FROM clients WHERE c_fullname = @name", connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                connection.Open();
                var result = cmd.ExecuteScalar();
                connection.Close();
                if (result != null) return (int)result;
            }

            using (SqlCommand insertCmd = new SqlCommand("INSERT INTO clients (c_fullname, c_phone) OUTPUT INSERTED.c_id VALUES (@name, @phone)", connection))
            {
                insertCmd.Parameters.AddWithValue("@name", name);
                insertCmd.Parameters.AddWithValue("@phone", phone);
                connection.Open();
                int newId = (int)insertCmd.ExecuteScalar();
                connection.Close();
                return newId;
            }
        }

        private int GetStaffId(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return -1;

            // Разделяем ФИО на фамилию, имя и отчество
            string[] nameParts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);


            if (nameParts.Length < 2)
            {
                MessageBox.Show("Некорректный ввод ФИО сотрудника. Укажите как минимум фамилию и имя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return -1;
            }

            string lastName = nameParts[0];  // Фамилия
            string firstName = nameParts[1]; // Имя
            string middleName = nameParts.Length > 2 ? nameParts[2] : null; // Отчество (если есть)

            using (SqlCommand cmd = new SqlCommand(@"
        SELECT s_id FROM staff 
        WHERE s_lastName = @lastName AND s_firstName = @firstName 
        AND (s_middleName = @middleName OR s_middleName IS NULL AND @middleName IS NULL)", connection))
            {
                cmd.Parameters.AddWithValue("@lastName", lastName);
                cmd.Parameters.AddWithValue("@firstName", firstName);
                cmd.Parameters.AddWithValue("@middleName", (object)middleName ?? DBNull.Value);

                connection.Open();
                var result = cmd.ExecuteScalar();
                connection.Close();

                if (result != null)
                {
                    return (int)result; // Сотрудник найден — возвращаем его ID
                }
                else
                {
                    MessageBox.Show("Сотрудник с таким ФИО не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return -1; // Возвращаем -1, чтобы обработать ошибку выше
                }
            }
        }
        private int GetTouroperatorId(string name)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT t_id FROM touroperators WHERE t_name = @name", connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                connection.Open();
                var result = cmd.ExecuteScalar();
                connection.Close();
                return result != null ? (int)result : throw new Exception("Туроператор не найден");
            }
        }

        private int GetStatusId(string name)
        {
            using (SqlCommand cmd = new SqlCommand("SELECT status_id FROM status WHERE status_name = @name", connection))
            {
                cmd.Parameters.AddWithValue("@name", name);
                connection.Open();
                var result = cmd.ExecuteScalar();
                connection.Close();
                return result != null ? (int)result : throw new Exception("Статус не найден");
            }
        }

        //клиенты
        private void FillClientComboBox()
        {
            clientsName.Items.Clear(); // Очищаем перед добавлением

            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open(); // Открываем соединение, только если оно закрыто
                }

                using (SqlCommand cmd = new SqlCommand("SELECT c_id, c_fullname FROM clients", connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) // Если данных нет
                    {
                        clientsName.Items.Add("Записи отсутствуют");
                    }
                    else
                    {
                        status.Enabled = true;
                        while (reader.Read())
                        {
                            
                            clientsName.Items.Add(new ComboBoxItem
                            {
                                Id = (int)reader["c_id"],
                                FullName = reader["c_fullname"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close(); // Закрываем соединение только если оно было открыто
                }
            }
        }

        // Класс для хранения значения и ID клиента в ComboBox
        private class ComboBoxItem
        {
            public int Id { get; set; }
            public string FullName { get; set; }

            public override string ToString()
            {
                return FullName;
            }
        }
        private int GetSelectedClientId()
        {
            // Проверяем, что выбрано что-то кроме "Нет клиентов"
            if (clientsName.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Id;
            }
            return -1;  // Возвращаем -1, если ничего не выбрано
        }



        private void FillStatusComboBox()
        {
            status.Items.Clear();

            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                using (SqlCommand cmd = new SqlCommand("SELECT status_id, status_name FROM status", connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        status.Items.Add("Записи отсутствуют");
                    }
                    else
                    {
                        status.Enabled = true;
                        while (reader.Read())
                        {
                            status.Items.Add(reader["status_name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статусов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void FillTouroperatorComboBox()
        {
            touroperator.Items.Clear();

            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                using (SqlCommand cmd = new SqlCommand("SELECT t_id, t_name FROM touroperators", connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        touroperator.Items.Add("Записи отсутствуют");
                    }
                    else
                    {
                        touroperator.Enabled = true;
                        while (reader.Read())
                        {
                            touroperator.Items.Add(reader["t_name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки туроператоров: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }
        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                comboBox.ForeColor = Color.Black; // Меняем цвет текста на черный
            }
        }



        private void Field_Enter(object sender, EventArgs e)
        {
            if (sender is Guna2TextBox field && field.Text == field.Tag?.ToString())
            {
                field.Text = string.Empty;
                field.ForeColor = Color.Black;

            }
        }

        private void Field_Leave(object sender, EventArgs e)
        {
            var field = sender as Guna2TextBox;
            if (field != null && string.IsNullOrWhiteSpace(field.Text))
            {
                field.Text = field.Tag?.ToString();
                field.ForeColor = Color.FromArgb(125, 137, 149);

                
            }
        }

        
        private void MaskedField_Enter(object sender, EventArgs e)
        {
            var field = sender as MaskedTextBox;
            if (field != null && field.Text == field.Tag?.ToString())
            {
                field.Text = string.Empty;
                field.ForeColor = Color.Black;
            }

            // Проверяем, является ли поле MaskedTextBox (для номера телефона)
            if (field == phoneNumber)
            {
                field.Mask = "  +7 (000) 000-00-00";
            }
            // Проверяем, является ли поле MaskedTextBox (для номера телефона)
            if (field == costTO)
            {
                field.Mask = "  000 000 000 EUR";
            }
        }

        private void MaskedField_Leave(object sender, EventArgs e)
        {
            var field = sender as MaskedTextBox;
            if (field != null && string.IsNullOrWhiteSpace(field.Text.Replace("_", "").Replace("EUR", "").Replace("(", "").Replace("+7","").Replace(")", "").Replace("-", "").Replace(" ", "")))
            {
                field.Mask = ""; // Убираем маску, если поле пустое
                field.Text = field.Tag?.ToString(); // Возвращаем заглушку
                field.ForeColor = Color.FromArgb(125, 137, 149);
            }
        }





        private void HideButton_MouseEnter(object sender, EventArgs e)
        {
            hideButton.Image = hoverHideImage;
        }
        private void HideButton_MouseLeave(object sender, EventArgs e)
        {
            hideButton.Image = normalHideImage;
        }

        private void CloseButton_MouseEnter(object sender, EventArgs e)
        {
            closeButton.Image = hoverCloseImage;
        }
        private void CloseButton_MouseLeave(object sender, EventArgs e)
        {
            closeButton.Image = normalCloseImage;
        }



        private void Button_MouseEnter(object sender, EventArgs e)
        {
            this.enterButton.FillColor = Color.FromArgb(57, 109, 96);
        }
        private void Button_MouseLeave(object sender, EventArgs e)
        {
            this.enterButton.FillColor = Color.FromArgb(71, 137, 120);
        }


        private void HideButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }


    }
}
