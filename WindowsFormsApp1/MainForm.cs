using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Data.SqlClient;
using System.IO;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.Generic;
using WindowsFormsApp1.Properties;
using WindowsFormsApp1;

namespace UniversalCardApp
{
    public partial class MainForm : Form
    {
        private readonly SqlConnection connection;

        // Поля для изображений
        private Image normalHideImage;
        private Image hoverHideImage;
        private Image normalCloseImage;
        private Image hoverCloseImage;

        public MainForm(SqlConnection existingConnection)
        {
            InitializeComponent(); 

            connection = existingConnection;
            InitializeImages();
            LoadDataAndCreateCards(cardsPanel);
            LoadUserInfo();
            LoadChartData();
            UpdateRequestLabel();
            LoadStatusLabels();
        }

        private void InitializeImages()
        {
            // Установите базовый путь
            string basePath = AppDomain.CurrentDomain.BaseDirectory; // Путь к исполняемому файлу
            string imagePath = Path.Combine(basePath, "img"); // Путь к папке "img"

            try
            {
                // Загрузите изображения для HideButton
                normalHideImage = Image.FromFile(Path.Combine(imagePath, "ButtonHide.png"));
                hoverHideImage = Image.FromFile(Path.Combine(imagePath, "ButtonHide1.png"));

                // Загрузите изображения для CloseButton
                normalCloseImage = Image.FromFile(Path.Combine(imagePath, "ButtonClose.png"));
                hoverCloseImage = Image.FromFile(Path.Combine(imagePath, "ButtonClose1.png"));
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

        private void UpdateRequestLabel()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM requests";
                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                int requestCount = Convert.ToInt32(command.ExecuteScalar());
                connection.Close();

                requestLabel.Text = $"{requestCount}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStatusLabels()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT status_name FROM status"; // Загружаем статусы
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    List<string> statusNames = new List<string>();

                    while (reader.Read())
                    {
                        statusNames.Add(reader["status_name"].ToString());
                    }

                    Guna2HtmlLabel[] labels =
                    {
                statusLabel1, statusLabel2, statusLabel3,
                statusLabel4, statusLabel5, statusLabel6
            };

                    for (int i = 0; i < labels.Length; i++)
                    {
                        if (i < statusNames.Count)
                            labels[i].Text = statusNames[i];
                        else
                            labels[i].Text = "-"; // Если статусов меньше 6, заполняем дефолтным значением
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки статусов: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void LoadChartData()
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT r_status, COUNT(*) AS count FROM requests GROUP BY r_status";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    var pieSeries = new SeriesCollection();
                    int totalCount = 0;
                    List<(string status, int count)> data = new List<(string, int)>();

                    while (reader.Read())
                    {
                        string status = reader["r_status"].ToString();
                        int count = Convert.ToInt32(reader["count"]);
                        data.Add((status, count));
                        totalCount += count;
                    }

                    foreach (var (status, count) in data)
                    {
                        Color drawingColor = GetStatusColor(status);
                        var brush = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromRgb(drawingColor.R, drawingColor.G, drawingColor.B));

                        var pieItem = new PieSeries
                        {
                            Title = status,
                            Values = new ChartValues<int> { count },
                            Fill = brush,
                            StrokeThickness = 0
                        };

                        pieSeries.Add(pieItem);
                    }

                    pieChart.Series = pieSeries;
                    pieChart.DataTooltip = null;
                    pieChart.DisableAnimations = true;
                    pieChart.InnerRadius = 68;
                }
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        private void LoadUserInfo()
        {
            try
            {
                string sessionToken = File.ReadAllText("session.txt").Trim();

                if (string.IsNullOrEmpty(sessionToken))
                {
                    MessageBox.Show("Ошибка: токен сессии не найден.");
                    return;
                }

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query1 = "SELECT user_id FROM user_sessions WHERE token = @token AND expires_at > GETDATE()";
                int userId = -1;

                using (SqlCommand cmd = new SqlCommand(query1, connection))
                {
                    cmd.Parameters.AddWithValue("@token", sessionToken);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userId = Convert.ToInt32(reader["user_id"]);
                        }
                    }
                }

                if (userId == -1)
                {
                    MessageBox.Show("Ошибка: сессия недействительна или истекла.");
                    return;
                }

                string query2 = "SELECT s_name, s_lastName FROM staff WHERE s_id = @userId";
                using (SqlCommand cmd = new SqlCommand(query2, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string firstName = reader["s_name"].ToString();
                            string lastName = reader["s_lastName"].ToString();

                            staffLabel.Text = $"{lastName} {firstName}";
                        }
                        else
                        {
                            staffLabel.Text = "Пользователь не найден";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки пользователя: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void LoadDataAndCreateCards(Guna2Panel cardsPanel)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = "SELECT r_id, r_country, r_departure_city, s.status_name, r_start_date, r_end_date, r_days_nights, r_adults_children, r_tourop_cost, r_agent_cost " +
                               "FROM requests r JOIN status s ON r.r_status = s.status_id";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                int maxCards = 5;
                int count = 0;
                int cardSpacing = 6;

                cardsPanel.Controls.Clear();

                while (reader.Read() && count < maxCards)
                {
                    int id = reader.GetInt32(reader.GetOrdinal("r_id"));
                    string country = reader.IsDBNull(reader.GetOrdinal("r_country")) ? "Unknown" : reader.GetString(reader.GetOrdinal("r_country"));
                    string departureCity = reader.IsDBNull(reader.GetOrdinal("r_departure_city")) ? "Unknown" : reader.GetString(reader.GetOrdinal("r_departure_city"));
                    string status = reader.IsDBNull(reader.GetOrdinal("status_name")) ? "Unknown" : reader.GetString(reader.GetOrdinal("status_name"));

                    DateTime? startDate = reader.IsDBNull(reader.GetOrdinal("r_start_date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("r_start_date"));
                    DateTime? endDate = reader.IsDBNull(reader.GetOrdinal("r_end_date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("r_end_date"));

                    string daysNights = reader.IsDBNull(reader.GetOrdinal("r_days_nights")) ? "0/0" : reader.GetString(reader.GetOrdinal("r_days_nights"));
                    string adultsChildren = reader.IsDBNull(reader.GetOrdinal("r_adults_children")) ? "0/0" : reader.GetString(reader.GetOrdinal("r_adults_children"));

                    decimal touropCost = reader.IsDBNull(reader.GetOrdinal("r_tourop_cost")) ? 0 : reader.GetDecimal(reader.GetOrdinal("r_tourop_cost"));
                    decimal agentCost = reader.IsDBNull(reader.GetOrdinal("r_agent_cost")) ? 0 : reader.GetDecimal(reader.GetOrdinal("r_agent_cost"));

                    var card = CreateCard(id, country, departureCity, status, startDate, endDate, daysNights, adultsChildren, touropCost, agentCost);

                    if (card == null)
                    {
                        MessageBox.Show("Ошибка при создании карточки!");
                        return;
                    }

                    card.Location = new Point(0, (120 + cardSpacing) * count);
                    cardsPanel.Controls.Add(card);
                    count++;
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private Guna2Panel CreateCard(int id, string country, string departureCity, string status, DateTime? startDate, DateTime? endDate,
                                        string daysNights, string adultsChildren, decimal touropCost, decimal agentCost)
        {

            Console.WriteLine($"Creating card for ID: {id}, Country: {country}, Departure City: {departureCity}, Status: {status}");

            Font commonFont = new Font("Segoe UI Light", 12.25f, FontStyle.Regular);

            Guna2Panel card = new Guna2Panel
            {
                Size = new Size(532, 120),
                BackColor = Color.FromArgb(254, 254, 254),
                BorderThickness = 0, // Отключаем стандартную границу
                BorderColor = Color.Transparent, // Делаем границу невидимой (если не нужно)

                CustomBorderThickness = new Padding(3, 0, 0, 0), // Левая граница 3px
                CustomBorderColor = GetStatusColor(status), // Цвет границы по статусу
            };

            Guna2HtmlLabel requestIdLabel = new Guna2HtmlLabel
            {
                Text = $"№ {id:000000}",
                Font = commonFont, // Применяем общий шрифт
                ForeColor = Color.FromArgb(0, 50, 73),
                Location = new Point(14, 8),
                AutoSize = true
            };
            card.Controls.Add(requestIdLabel);

            Console.WriteLine("Card created for ID " + id);

            Guna2HtmlLabel countryLabel = new Guna2HtmlLabel
            {
                Text = country,
                Font = new Font("Segoe UI", 14.25f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 50, 73),
                Location = new Point(14, requestIdLabel.Bottom + 4),
                AutoSize = true
            };
            card.Controls.Add(countryLabel);

            PictureBox departureIcon = new PictureBox
            {
                Size = new Size(16, 16),
                Location = new Point(14, countryLabel.Bottom + 10),
                Image = Resources.departureIcon,
                SizeMode = PictureBoxSizeMode.Normal
            };
            card.Controls.Add(departureIcon);

            Guna2HtmlLabel departureCityLabel = new Guna2HtmlLabel
            {
                Text = departureCity,
                Font = commonFont, // Применяем общий шрифт
                ForeColor = Color.FromArgb(0, 50, 73),
                Location = new Point(departureIcon.Right + 6, countryLabel.Bottom + 8),
                AutoSize = true
            };
            card.Controls.Add(departureCityLabel);

            PictureBox statusIcon = new PictureBox
            {
                Size = new Size(16, 16),
                Location = new Point(14, departureCityLabel.Bottom + 4),
                Image = Resources.statusIcon,
                SizeMode = PictureBoxSizeMode.Normal
            };
            card.Controls.Add(statusIcon);

            Guna2HtmlLabel statusLabel = new Guna2HtmlLabel
            {
                Text = status,
                Font = new Font("Segoe UI", 12.25f, FontStyle.Bold),
                Location = new Point(statusIcon.Right + 6, departureCityLabel.Bottom),
                ForeColor = GetStatusColor(status), 
                AutoSize = true
            };
            card.Controls.Add(statusLabel);

            Guna2Panel dividerLine = new Guna2Panel
            {
                Size = new Size(1, 120),
                BackColor = Color.LightGray,
                Location = new Point(170, 0)
            };
            card.Controls.Add(dividerLine);

            string dateText = (startDate.HasValue && endDate.HasValue)
                ? $"{startDate:dd.MM} - {endDate:dd.MM.yyyy}"
                : "Не указано";

            Guna2HtmlLabel datesLabel = new Guna2HtmlLabel
            {
                Text = dateText,
                Font = commonFont, // Применяем общий шрифт
                ForeColor = Color.FromArgb(0, 50, 73),
                Location = new Point(dividerLine.Right + 14, 8),
                AutoSize = true
            };
            card.Controls.Add(datesLabel);

            int tripDays = (startDate.HasValue && endDate.HasValue)
                ? (endDate.Value - startDate.Value).Days
                : 0;
            int tripNights = tripDays > 0 ? tripDays - 1 : 0;

            Guna2HtmlLabel tripDaysLabel = new Guna2HtmlLabel
            {
                Text = (tripDays > 0) ? $"{tripDays}/{tripNights} дн/нч" : "Не указано",
                Font = commonFont, // Применяем общий шрифт
                ForeColor = Color.FromArgb(0, 50, 73),
                Location = new Point(dividerLine.Right + 14, datesLabel.Bottom + 1),
                AutoSize = true
            };
            card.Controls.Add(tripDaysLabel);

            string[] parts = adultsChildren.Split('/');
            string formattedTravelers = "Не указано";

            if (parts.Length == 2 && int.TryParse(parts[0], out int adults) && int.TryParse(parts[1], out int children))
            {
                formattedTravelers = $"{adults} {(adults == 1 ? "взрослый" : "взрослых")} {children} {(children == 1 ? "ребенок" : "детей")}";
            }

            Guna2HtmlLabel travelersLabel = new Guna2HtmlLabel
            {
                Text = formattedTravelers,
                Font = commonFont, // Применяем общий шрифт
                ForeColor = Color.FromArgb(0, 50, 73),
                Location = new Point(dividerLine.Right + 14, tripDaysLabel.Bottom + 1),
                AutoSize = true
            };
            card.Controls.Add(travelersLabel);

            Guna2Panel dividerLine2 = new Guna2Panel
            {
                Size = new Size(1, 120),
                BackColor = Color.LightGray,
                Location = new Point(362, 0)
            };
            card.Controls.Add(dividerLine2);

            // Блок 3: Стоимость
            Guna2HtmlLabel touropCostLabelText = new Guna2HtmlLabel
            {
                Text = "стоимость у ТО",
                Font = commonFont, // Применяем общий шрифт
                ForeColor = Color.FromArgb(0, 50, 73),
                Location = new Point(dividerLine2.Right + 14, 6)
            };
            card.Controls.Add(touropCostLabelText);

            Guna2HtmlLabel touropCostLabel = new Guna2HtmlLabel
            {
                Text = (touropCost > 0) ? $"{touropCost:F2} EUR" : "Не указано",
                Font = new Font("Segoe UI", 14.25f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 50, 73),
                Location = new Point(dividerLine2.Right + 14, touropCostLabelText.Bottom),
                AutoSize = true
            };
            card.Controls.Add(touropCostLabel);

            Guna2HtmlLabel agentCostLabelText = new Guna2HtmlLabel
            {
                Text = "стоимость клиенту",
                Font = commonFont, // Применяем общий шрифт
                ForeColor = Color.FromArgb(0, 50, 73),
                Location = new Point(dividerLine2.Right + 14, touropCostLabel.Bottom + 8)
            };
            card.Controls.Add(agentCostLabelText);

            Guna2HtmlLabel agentCostLabel = new Guna2HtmlLabel
            {
                Text = (agentCost > 0) ? $"{agentCost:F2} EUR" : "Не указано",
                Font = new Font("Segoe UI", 14.25f, FontStyle.Bold),
                ForeColor = Color.FromArgb(71, 137, 120),
                Location = new Point(dividerLine2.Right + 14, agentCostLabelText.Bottom),
                AutoSize = true
            };
            card.Controls.Add(agentCostLabel);

            return card;
        }
        private Color GetStatusColor(string status)
        {
            switch (status)
            {
                case "Не обработана":
                case "1":
                    return Color.FromArgb(230, 128, 136); // Цвет для "Не обработана" и "1"
                case "В работе":
                case "2":
                    return Color.FromArgb(71, 137, 120); // Цвет для "В работе" и "2"
                case "Отослана":
                case "3":
                    return Color.FromArgb(252, 213, 136); // Цвет для "Отослана" и "3"
                case "Аннулирована":
                case "4":
                    return Color.FromArgb(255, 192, 192); // Цвет для "Аннулирована" и "4"
                case "ОК":
                case "5":
                    return Color.FromArgb(249, 160, 119); // Цвет для "ОК" и "5"
                case "Wait list":
                case "6":
                    return Color.FromArgb(167, 192, 241); // Цвет для "Wait list" и "6"
                default:
                    return Color.Gray; // Цвет по умолчанию
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            Console.WriteLine("MainForm_Load event triggered.");
            LoadDataAndCreateCards(cardsPanel);
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
            if (sender is Guna2Button button)
            {
                mainButton.CustomBorderThickness = new Padding(0);
                mainButton.CustomBorderColor = Color.Transparent;  // Убираем цвет границы
                button.CustomBorderThickness = new Padding(4, 0, 0, 0); // Левая граница 3px
                button.CustomBorderColor = Color.White; // Цвет границы слева
            }
        }
        private void Button_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Guna2Button button)
            {
                button.CustomBorderThickness = new Padding(0);
                button.CustomBorderColor = Color.Transparent;
                mainButton.CustomBorderThickness = new Padding(4, 0, 0, 0);
                mainButton.CustomBorderColor = Color.White;
            }
        }

        private void ProfileButton_MouseEnter(object sender, EventArgs e)
        {
            mainButton.CustomBorderThickness = new Padding(0);
            mainButton.CustomBorderColor = Color.Transparent; 

        }
        private void ProfileButton_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Guna2Button)
            {
                mainButton.CustomBorderThickness = new Padding(4, 0, 0, 0);
                mainButton.CustomBorderColor = Color.White;
            }
        }

        private void RequestButton_Click(object sender, EventArgs e)
        {
            try
            {
                string sessionToken = File.ReadAllText("session.txt").Trim();

                if (string.IsNullOrEmpty(sessionToken))
                {
                    MessageBox.Show("Ошибка: токен сессии не найден.");
                    return;
                }

                int userId = -1;
                string login = string.Empty;

                // Проверка, открыто ли соединение, если нет, открываем
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                // Запрос для получения user_id
                string query1 = "SELECT user_id FROM user_sessions WHERE token = @token AND expires_at > GETDATE()";
                using (SqlCommand cmd = new SqlCommand(query1, connection))
                {
                    cmd.Parameters.AddWithValue("@token", sessionToken);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userId = Convert.ToInt32(reader["user_id"]);
                        }
                    }
                }

                if (userId == -1)
                {
                    MessageBox.Show("Ошибка: сессия недействительна или истекла.");
                    return;
                }

                // Запрос для получения логина
                string query2 = "SELECT s_login FROM staff WHERE s_id = @userId";
                using (SqlCommand cmd = new SqlCommand(query2, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            login = reader["s_login"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Пользователь не найден.");
                            return;
                        }
                    }
                }

                // Скрываем текущую форму и передаем подключение в RequestForm
                this.Hide();
                RequestForm requestForm = new RequestForm(connection, this);
                requestForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при обработке запроса: " + ex.Message);
            }
            finally
            {
                // Закрываем соединение, если оно открыто
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void HideButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            Logout();
            LoginForm loginForm = new LoginForm(connection);
            loginForm.Show();
        }
        private void Logout()
        {
            if (File.Exists("session.txt"))
            {
                File.Delete("session.txt"); 
            }

            this.Hide();
            LoginForm loginForm = new LoginForm(connection);  
            loginForm.Show();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }


    }
}