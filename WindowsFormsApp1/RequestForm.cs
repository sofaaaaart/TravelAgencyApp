using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using UniversalCardApp;
using System.IO;
using Guna.UI2.WinForms;
using WindowsFormsApp1.Properties;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;

namespace WindowsFormsApp1
{

    public partial class RequestForm : Form
    {
        private readonly SqlConnection connection;

        // Поля для изображений
        private Image normalHideImage;
        private Image hoverHideImage;
        private Image normalCloseImage;
        private Image hoverCloseImage;


        public RequestForm(SqlConnection existingConnection, Form parentForm)
        {
            if (existingConnection is null)
            {
                throw new ArgumentNullException(nameof(existingConnection));
            }

            if (parentForm is null)
            {
                throw new ArgumentNullException(nameof(parentForm));
            }

            InitializeComponent();
            InitializeImages();
            searchBox.Tag = "  Введите фразу для поиска";

            // Присваиваем существующее подключение
            this.connection = existingConnection;

            // Проверка на открытое соединение, если оно закрыто, откроем новое
            if (this.connection.State != ConnectionState.Open)
            {
                this.connection.Open();
            }
            LoadUserInfo();
            allFilterButton.FillColor = Color.FromArgb(217, 217, 217); // Серый цвет

            allFilterButton.BorderThickness = 0;

            // Оставляем кнопку "Только мои" неактивной
            myFilterButton.FillColor = Color.White;

            // Загружаем все заявки при запуске
            // Загружаем данные в форму
            LoadDataAndCreateCards(cardsPanel, patternCard);
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


        private void LoadUserInfo()
        {
            try
            {
                if (connection == null)
                {
                    MessageBox.Show("Ошибка: подключение не установлено.");
                    return;
                }

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
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
        private void LoadDataAndCreateCards(Guna2Panel cardsPanel, Guna2Panel patternCard)
        {
            try
            {
                // Проверяем, что шаблон карточки правильный
                if (patternCard.Name != "patternCard")
                {
                    throw new InvalidOperationException("Only the patternCard can be cloned.");
                }

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = @"
                SELECT r.*, s.status_name, (r_agent_cost - r_tourop_cost) AS profit, 
                       staff.s_lastName + ' ' + LEFT(staff.s_name, 1) + '. ' AS staff_name       
                FROM requests r 
                JOIN status s ON r.r_status = s.status_id
                LEFT JOIN staff ON r.r_staff = staff.s_id;";

                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();

                int maxCards = 3;
                int count = 0;
                int cardSpacing = 6;
                bool hasRequests = false;

                // 🔥 Удаляем старые карточки и сам patternCard из cardsPanel
                var existingCards = cardsPanel.Controls.OfType<Guna2Panel>()
                    .Where(c => c.Name.StartsWith("patternCard")).ToList();

                foreach (var card in existingCards)
                {
                    cardsPanel.Controls.Remove(card);
                    card.Dispose();
                }

                // Читаем данные из базы и создаем карточки
                while (reader.Read() && count < maxCards)
                {
                    hasRequests = true; // Найдены заявки

                    // Клонируем шаблон карты
                    var card = ClonePanel(patternCard);
                    card.Name = $"patternCard_clone_{count}"; // Уникальное имя для каждой карточки

                    var requestLabel = (Label)card.Controls["request"];
                    var countryDepartmentLabel = (Label)card.Controls["countryDepartment"];
                    var fullNameClientLabel = (Label)card.Controls["fullNameClient"];
                    var phoneNumberClientLabel = (Label)card.Controls["phoneNumberClient"];
                    var departureCityLabel = (Label)card.Controls["departureCity"];
                    var statusLabel = (Label)card.Controls["status"];
                    var startTourLabel = (Label)card.Controls["startTour"];
                    var endTourLabel = (Label)card.Controls["endTour"];
                    var dayNightLabel = (Label)card.Controls["dayNight"];
                    var personsLabel = (Label)card.Controls["persons"];
                    var staffLabel = (Label)card.Controls["staff"];
                    var costTOLabel = (Label)card.Controls["costTO"];
                    var patternCardLabel = (Label)card.Controls["patternCard"];
                    var paidLabel = (Label)card.Controls["paid"];
                    var proffitLabel = (Label)card.Controls["proffit"];

                    // Заполняем данные
                    requestLabel.Text = $"№{reader["r_id"]}";
                    countryDepartmentLabel.Text = reader["r_country"].ToString();
                    fullNameClientLabel.Text = FormatFullName(reader["r_client"].ToString());
                    phoneNumberClientLabel.Text = FormatPhoneNumber(reader["r_phone"].ToString());
                    departureCityLabel.Text = $"Вылет из {reader["r_departure_city"]}";
                    statusLabel.Text = reader["status_name"].ToString();
                    statusLabel.ForeColor = GetStatusColor(reader["status_name"].ToString());
                    startTourLabel.Text = FormatDate(reader["r_start_date"]);
                    endTourLabel.Text = FormatDate(reader["r_end_date"]);
                    dayNightLabel.Text = reader["r_days_nights"].ToString();
                    personsLabel.Text = reader["r_adults_children"].ToString();
                    staffLabel.Text = reader["staff_name"].ToString();
                    costTOLabel.Text = $"{reader["r_tourop_cost"]} Руб";
                    patternCardLabel.Text = $"{reader["r_agent_cost"]} Руб";
                    paidLabel.Text = $"{(Convert.ToBoolean(reader["r_paid"]) ? "Оплачено" : "Не оплачено")}";
                    proffitLabel.Text = $"{reader["profit"]} Руб";

                    // Устанавливаем позицию карточки
                    card.Location = new Point(0, (120 + cardSpacing) * count);

                    // Добавляем карточку в cardsPanel
                    cardsPanel.Controls.Add(card);

                    count++;
                }

                reader.Close();

                // Если заявок нет, добавляем надпись "Заявок нет"
                if (!hasRequests)
                {
                    Label noRequestsLabel = new Label
                    {
                        Text = "Заявок нет",
                        Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                        ForeColor = Color.Gray,
                        AutoSize = true
                    };

                    // Вычисляем позицию по центру панели
                    noRequestsLabel.Location = new Point(
                        (cardsPanel.Width - noRequestsLabel.Width) / 2,
                        (cardsPanel.Height - noRequestsLabel.Height) / 2
                    );

                    cardsPanel.Controls.Add(noRequestsLabel);
                }
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


        private void LoadUserRequests(Guna2Panel cardsPanel, Guna2Panel patternCard)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // Получаем ID текущего пользователя
                string sessionToken = File.ReadAllText("session.txt").Trim();
                int userId = -1;

                using (SqlCommand cmd = new SqlCommand("SELECT user_id FROM user_sessions WHERE token = @token AND expires_at > GETDATE()", connection))
                {
                    cmd.Parameters.AddWithValue("@token", sessionToken);
                    using (SqlDataReader reader = cmd.ExecuteReader())  // Используем один и тот же reader
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

                string query = @"
                        SELECT r.*, s.status_name, (r_agent_cost - r_tourop_cost) AS profit, 
                               staff.s_lastName + ' ' + LEFT(staff.s_name, 1) + '. ' AS staff_name       
                        FROM requests r 
                        JOIN status s ON r.r_status = s.status_id
                        LEFT JOIN staff ON r.r_staff = staff.s_id
                        WHERE r.r_staff = @userId;";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    using (SqlDataReader reader = command.ExecuteReader())  // Используем тот же reader
                    {
                        int maxCards = 3;
                        int count = 0;
                        int cardSpacing = 6;
                        bool hasRequests = false;

                        // Удаляем старые карточки
                        var existingCards = cardsPanel.Controls.OfType<Guna2Panel>()
                            .Where(c => c.Name.StartsWith("patternCard")).ToList();

                        foreach (var card in existingCards)
                        {
                            cardsPanel.Controls.Remove(card);
                            card.Dispose();
                        }

                        // Создание карточек с заявками
                        while (reader.Read() && count < maxCards)
                        {
                            hasRequests = true;

                            var card = ClonePanel(patternCard);
                            card.Name = $"patternCard_clone_{count}";

                            var requestLabel = (Label)card.Controls["request"];
                            var countryDepartmentLabel = (Label)card.Controls["countryDepartment"];
                            var fullNameClientLabel = (Label)card.Controls["fullNameClient"];
                            var phoneNumberClientLabel = (Label)card.Controls["phoneNumberClient"];
                            var departureCityLabel = (Label)card.Controls["departureCity"];
                            var statusLabel = (Label)card.Controls["status"];
                            var startTourLabel = (Label)card.Controls["startTour"];
                            var endTourLabel = (Label)card.Controls["endTour"];
                            var dayNightLabel = (Label)card.Controls["dayNight"];
                            var personsLabel = (Label)card.Controls["persons"];
                            var staffLabel = (Label)card.Controls["staff"];
                            var costTOLabel = (Label)card.Controls["costTO"];
                            var patternCardLabel = (Label)card.Controls["patternCard"];
                            var paidLabel = (Label)card.Controls["paid"];
                            var proffitLabel = (Label)card.Controls["proffit"];

                            // Заполняем карточку данными
                            requestLabel.Text = $"№{reader["r_id"]}";
                            countryDepartmentLabel.Text = reader["r_country"].ToString();
                            fullNameClientLabel.Text = FormatFullName(reader["r_client"].ToString());
                            phoneNumberClientLabel.Text = FormatPhoneNumber(reader["r_phone"].ToString());
                            departureCityLabel.Text = $"Вылет из {reader["r_departure_city"]}";
                            statusLabel.Text = reader["status_name"].ToString();
                            statusLabel.ForeColor = GetStatusColor(reader["status_name"].ToString());
                            startTourLabel.Text = FormatDate(reader["r_start_date"]);
                            endTourLabel.Text = FormatDate(reader["r_end_date"]);
                            dayNightLabel.Text = reader["r_days_nights"].ToString();
                            personsLabel.Text = reader["r_adults_children"].ToString();
                            staffLabel.Text = reader["staff_name"].ToString();
                            costTOLabel.Text = $"{reader["r_tourop_cost"]} Руб";
                            patternCardLabel.Text = $"{reader["r_agent_cost"]} Руб";
                            paidLabel.Text = $"{(Convert.ToBoolean(reader["r_paid"]) ? "Оплачено" : "Не оплачено")}";
                            proffitLabel.Text = $"{reader["profit"]} Руб";

                            // Устанавливаем позицию карточки
                            card.Location = new Point(0, (120 + cardSpacing) * count);
                            cardsPanel.Controls.Add(card);

                            count++;
                        }

                        // Если заявок нет, добавляем надпись "Заявок нет"
                        if (!hasRequests)
                        {
                            Label noRequestsLabel = new Label
                            {
                                Text = "Заявок нет",
                                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                                ForeColor = Color.Gray,
                                AutoSize = true
                            };

                            noRequestsLabel.Location = new Point(
                                (cardsPanel.Width - noRequestsLabel.Width) / 2,
                                (cardsPanel.Height - noRequestsLabel.Height) / 2
                            );

                            cardsPanel.Controls.Add(noRequestsLabel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void searchButton_Click(object sender, EventArgs e)
        {
            string searchQuery = searchBox.Text.Trim(); // Получаем текст из поля поиска

            // Если строка поиска пуста, загружаем все заявки
            if (string.IsNullOrEmpty(searchQuery))
            {
                LoadDataAndCreateCards(cardsPanel, patternCard);
                return;
            }

            // Загружаем только заявки, содержащие искомую строку
            LoadFilteredRequests(searchQuery);
        }

        private void LoadFilteredRequests(string searchQuery)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                string query = @"
        SELECT r.*, s.status_name, (r_agent_cost - r_tourop_cost) AS profit, 
               staff.s_lastName + ' ' + LEFT(staff.s_name, 1) + '. ' AS staff_name
        FROM requests r
        JOIN status s ON r.r_status = s.status_id
        LEFT JOIN staff ON r.r_staff = staff.s_id
        WHERE 
            LOWER(r.r_country) LIKE LOWER(@search) OR
            LOWER(r.r_client) LIKE LOWER(@search) OR
            LOWER(r.r_phone) LIKE LOWER(@search) OR
            LOWER(r.r_departure_city) LIKE LOWER(@search) OR
            LOWER(s.status_name) LIKE LOWER(@search);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@search", $"%{searchQuery}%");

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        cardsPanel.Controls.Clear(); // Очищаем старые карточки

                        bool hasRequests = false;
                        int count = 0;
                        int cardSpacing = 6;

                        while (reader.Read())
                        {
                            hasRequests = true;
                            var card = ClonePanel(patternCard);
                            card.Name = $"patternCard_clone_{count}";

                            var requestLabel = (Label)card.Controls["request"];
                            requestLabel.Text = $"№{reader["r_id"]}";

                            card.Location = new Point(0, (120 + cardSpacing) * count);
                            cardsPanel.Controls.Add(card);

                            count++;
                        }

                        if (!hasRequests)
                        {
                            Label noRequestsLabel = new Label
                            {
                                Text = "Ничего не найдено",
                                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                                ForeColor = Color.Gray,
                                AutoSize = true
                            };

                            noRequestsLabel.Location = new Point(
                                (cardsPanel.Width - noRequestsLabel.Width) / 2,
                                (cardsPanel.Height - noRequestsLabel.Height) / 2
                            );

                            cardsPanel.Controls.Add(noRequestsLabel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }


        private string FormatCost(decimal cost)
        {
            return $"{cost:N0} Руб"; // Пример: "120 000 Руб"
        }
        private string FormatFullName(string fullName)
        {
            var parts = fullName.Split(' ');
            return parts.Length >= 3 ? $"{parts[0]} {parts[1][0]}. {parts[2][0]}." : fullName;
        }

        private string FormatPhoneNumber(string phone)
        {
            return phone.Length == 11 ? $"+7 ({phone.Substring(1, 3)}) {phone.Substring(4, 3)}-{phone.Substring(7, 2)}-{phone.Substring(9, 2)}" : phone;
        }

        private string FormatDate(object date)
        {
            return date != DBNull.Value ? Convert.ToDateTime(date).ToString("dd.MM.yyyy") : "Не указано";
        }

        private static readonly Dictionary<string, Color> StatusColors = new Dictionary<string, Color>
        {
            { "Не обработана", Color.FromArgb(230, 128, 136) },
            { "1", Color.FromArgb(230, 128, 136) },
            { "В работе", Color.FromArgb(71, 137, 120) },
            { "2", Color.FromArgb(71, 137, 120) },
            { "Отослана", Color.FromArgb(252, 213, 136) },
            { "3", Color.FromArgb(252, 213, 136) },
            { "Аннулирована", Color.FromArgb(255, 192, 192) },
            { "4", Color.FromArgb(255, 192, 192) },
            { "ОК", Color.FromArgb(249, 160, 119) },
            { "5", Color.FromArgb(249, 160, 119) },
            { "Wait list", Color.FromArgb(167, 192, 241) },
            { "6", Color.FromArgb(167, 192, 241) }
        };


        private Guna2Panel ClonePanel(Guna2Panel original)
        {
            if (original.Name != "patternCard")
            {
                throw new InvalidOperationException("Only the patternCard can be cloned.");
            }

            // Создаем новый экземпляр панели
            var clone = new Guna2Panel
            {
                Size = original.Size,
                Location = original.Location,
                BackColor = original.BackColor,
                BorderRadius = original.BorderRadius,
                BorderThickness = original.BorderThickness,
                FillColor = original.FillColor,
                Visible = original.Visible,
                Name = original.Name + "_clone" // Изменяем имя, чтобы избежать конфликтов
            };

            // Копируем все дочерние элементы
            foreach (Control control in original.Controls)
            {
                Control newControl = CloneControl(control);
                clone.Controls.Add(newControl);
            }

            return clone;
        }


        // Метод для копирования отдельных элементов управления
        private Control CloneControl(Control original)
        {
            Control clone = (Control)Activator.CreateInstance(original.GetType());

            clone.Size = original.Size;
            clone.Location = original.Location;
            clone.BackColor = original.BackColor;
            clone.ForeColor = original.ForeColor;
            clone.Font = original.Font;
            clone.Text = original.Text;
            clone.Name = original.Name + "_clone"; // Изменяем имя для уникальности

            // Если у элемента есть дочерние элементы, рекурсивно копируем их
            foreach (Control child in original.Controls)
            {
                Control childClone = CloneControl(child);
                clone.Controls.Add(childClone);
            }

            return clone;
        }

        private Color GetStatusColor(string status)
        {
            return StatusColors.ContainsKey(status) ? StatusColors[status] : Color.Gray;
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
                requestButton.CustomBorderThickness = new Padding(0);
                requestButton.CustomBorderColor = Color.Transparent;  // Убираем цвет границы
                // Устанавливаем границу только слева
                button.CustomBorderThickness = new Padding(4, 0, 0, 0); // Левая граница 3px
                button.CustomBorderColor = Color.White; // Цвет границы слева
            }
        }
        private void Button_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Guna2Button button)
            {
                // Убираем границу при уходе мыши
                button.CustomBorderThickness = new Padding(0);
                button.CustomBorderColor = Color.Transparent;  // Убираем цвет границы
                                                               // Оставляем границу только на mainButton
                requestButton.CustomBorderThickness = new Padding(4, 0, 0, 0); // Левая граница 4px
                requestButton.CustomBorderColor = Color.White; // Цвет границы слева для mainButton
            }
        }

        private void ProfileButton_MouseEnter(object sender, EventArgs e)
        {

            requestButton.CustomBorderThickness = new Padding(0);
            requestButton.CustomBorderColor = Color.Transparent;  // Убираем цвет границы

        }
        private void ProfileButton_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Guna2Button)
            {
                requestButton.CustomBorderThickness = new Padding(4, 0, 0, 0); // Левая граница 4px
                requestButton.CustomBorderColor = Color.White; // Цвет границы слева для mainButton
            }
        }


        private void AddButton_MouseEnter(object sender, EventArgs e)
        {
            this.addRequestButton.FillColor = Color.FromArgb(57, 109, 96);
        }
        private void AddButton_MouseLeave(object sender, EventArgs e)
        {
            this.addRequestButton.FillColor = Color.FromArgb(71, 137, 120);
        }

        private void Field_Enter(object sender, EventArgs e)
        {
            var field = sender as Guna2TextBox;
            if (field != null)
            {
                if (field.Text == "Введите фразу для поиска") // Если в поле стоит плейсхолдер
                {
                    field.Text = ""; // Очищаем поле
                    field.ForeColor = Color.Black; // Делаем текст черным
                }
            }
        }

        private void Field_Leave(object sender, EventArgs e)
        {
            var field = sender as Guna2TextBox;
            if (field != null)
            {
                field.Text = "Введите фразу для поиска"; // Возвращаем плейсхолдер из Tag
                field.ForeColor = Color.FromArgb(125, 137, 149); // Серый цвет плейсхолдера
                
            }
        }

        private void allFilterButton_Click(object sender, EventArgs e)
        {
            // Изменяем цвет кнопок (для Guna2Button используем FillColor)
            allFilterButton.FillColor = Color.FromArgb(217, 217, 217);
            myFilterButton.FillColor = Color.White;
            allFilterButton.BorderThickness = 0;
            myFilterButton.BorderThickness = 1;

            // Обновляем заявки
            LoadDataAndCreateCards(cardsPanel, patternCard);
        }

        private void myFilterButton_Click(object sender, EventArgs e)
        {
            // Изменяем цвет кнопок
            myFilterButton.FillColor = Color.FromArgb(217, 217, 217);
            allFilterButton.FillColor = Color.White;
            allFilterButton.BorderThickness = 1;
            myFilterButton.BorderThickness = 0;

            // Обновляем заявки
            LoadUserRequests(cardsPanel, patternCard);
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            // Use 'this.connection' to refer to the class-level connection
            using (SqlConnection connection = new SqlConnection(this.connection.ConnectionString))
            {
                connection.Open(); // Ensure that the connection is open
                AddRequestForm addForm = new AddRequestForm(connection);
                addForm.StartPosition = FormStartPosition.CenterScreen;
                addForm.ShowDialog();
            }
        }
        private void MainButton_Click(object sender, EventArgs e)
        {
            this.Hide();
            MainForm mainForm = new MainForm(connection); // Убедитесь, что MainForm определен и доступен
            mainForm.Show();
                
        }
        private void HideButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void RequestForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }


    }
}






/* private Guna2Panel CreateCard(int id, string country, string departureCity, string status, DateTime? startDate, DateTime? endDate,
                                 string daysNights, string adultsChildren, decimal touropCost, decimal agentCost, string fullName, string phone, int staffName, bool paid, decimal profit)
 {
     Console.WriteLine($"Creating card for ID: {id}, Country: {country}, Departure City: {departureCity}, Status: {status}");

     // Шрифты для разных элементов
     Font commonFont = new Font("Segoe UI Light", 12.25f, FontStyle.Regular);
     Font boldFont = new Font("Segoe UI", 12.25f, FontStyle.Bold);
     Font headerFont = new Font("Segoe UI", 14.25f, FontStyle.Bold);

     // Создаем основную панель (карточку)
     Guna2Panel card = new Guna2Panel
     {
         Size = new Size(952, 380), // Размер карточки
         Location = new Point(0, 100), // Позиция карточки
         BackColor = Color.FromArgb(252, 252, 252), // Цвет фона карточки
         BorderThickness = 0, // Толщина границы (убираем стандартную)
         BorderColor = Color.Transparent, // Цвет границы (делаем невидимой)
         CustomBorderThickness = new Padding(4, 0, 0, 0), // Левая граница 4px
         CustomBorderColor = GetStatusColor(status), // Цвет границы зависит от статуса
     };

     // Создаем метки и добавляем их в карточку с помощью вспомогательной функции
     int yOffset = AddLabel(card, $"№ {id:000000}", headerFont, Color.FromArgb(0, 50, 73), 34, 10);
     yOffset = AddLabel(card, country, boldFont, Color.FromArgb(0, 50, 73), 34, yOffset + 6);
     yOffset = AddLabel(card, fullName, boldFont, Color.FromArgb(0, 50, 73), 34, yOffset + 6);

     // Добавляем иконку телефона и номер телефона
     AddPhoneLabel(card, phone, commonFont, 34, yOffset + 10);
     yOffset += 40; // Обновляем yOffset для следующих элементов

     // Добавляем иконку города вылета и текст
     AddDepartureLabel(card, departureCity, commonFont, 34, yOffset + 10);
     yOffset += 40;

     // Добавляем иконку статуса и сам статус
     AddStatusLabel(card, status, boldFont, 34, yOffset + 10);

     // Добавляем разделительную линию
     AddDivider(card, 170);

     // Добавляем информацию о туре (даты, количество дней/ночей и т. д.)
     int dividerRight = AddDivider(card, 170);
     //AddTripDetails(card, startDate, endDate, daysNights, adultsChildren, staffName, dividerRight + 22, 10);

     // Вторая разделительная линия
     AddDivider(card, 700);

     // Добавляем информацию о стоимости тура
     //AddCostDetails(card, touropCost, agentCost, paid, profit, dividerRight + 22, 10);

     // Возвращаем созданную карточку
     return card;
 }

         // Вспомогательная функция для создания метки с текстом
        private int AddLabel(Guna2Panel card, string text, Font font, Color color, int x, int y)
        {
            Guna2HtmlLabel label = new Guna2HtmlLabel
            {
                Text = text, // Устанавливаем текст
                Font = font, // Устанавливаем шрифт
                ForeColor = color, // Устанавливаем цвет текста
                Location = new Point(x, y), // Устанавливаем позицию
                AutoSize = true // Автоматический размер для метки
            };
            card.Controls.Add(label); // Добавляем метку в карточку
            return label.Bottom; // Возвращаем нижнюю позицию метки для дальнейших отступов
        }

 
         // Функция для добавления метки с номером телефона и иконкой
        private void AddPhoneLabel(Guna2Panel card, string phone, Font font, int x, int y)
        {
            // Иконка телефона
            PictureBox phoneIcon = new PictureBox
            {
                Size = new Size(16, 16),
                Location = new Point(x, y),
                Image = Resources.phone, // Задаем иконку телефона
                SizeMode = PictureBoxSizeMode.Normal
            };
            card.Controls.Add(phoneIcon); // Добавляем иконку в карточку

            // Текстовый лейбл с номером телефона
            Guna2HtmlLabel phoneLabel = new Guna2HtmlLabel
            {
                Text = phone, // Устанавливаем номер телефона
                Font = font, // Устанавливаем шрифт
                ForeColor = Color.FromArgb(0, 50, 73), // Устанавливаем цвет текста
                Location = new Point(phoneIcon.Right + 6, y), // Устанавливаем позицию (правее иконки)
                AutoSize = true // Автоматический размер для метки
            };
            card.Controls.Add(phoneLabel); // Добавляем метку в карточку
        }

        // Функция для добавления метки с городом вылета и иконкой
        private void AddDepartureLabel(Guna2Panel card, string departureCity, Font font, int x, int y)
        {
            // Иконка города вылета
            PictureBox departureIcon = new PictureBox
            {
                Size = new Size(16, 16),
                Location = new Point(x, y),
                Image = Resources.departureIcon, // Задаем иконку города вылета
                SizeMode = PictureBoxSizeMode.Normal
            };
            card.Controls.Add(departureIcon); // Добавляем иконку в карточку

            // Текстовый лейбл с информацией о городе вылета
            Guna2HtmlLabel departureLabel = new Guna2HtmlLabel
            {
                Text = $"Вылет из {departureCity}", // Устанавливаем текст с городом
                Font = font, // Устанавливаем шрифт
                ForeColor = Color.FromArgb(0, 50, 73), // Устанавливаем цвет текста
                Location = new Point(departureIcon.Right + 6, y), // Устанавливаем позицию (правее иконки)
                AutoSize = true // Автоматический размер для метки
            };
            card.Controls.Add(departureLabel); // Добавляем метку в карточку
        }

        // Функция для добавления метки со статусом и иконкой
        private void AddStatusLabel(Guna2Panel card, string status, Font font, int x, int y)
        {
            // Иконка статуса
            PictureBox statusIcon = new PictureBox
            {
                Size = new Size(16, 16),
                Location = new Point(x, y),
                Image = Resources.statusIcon, // Задаем иконку статуса
                SizeMode = PictureBoxSizeMode.Normal
            };
            card.Controls.Add(statusIcon); // Добавляем иконку в карточку

            // Текстовый лейбл с информацией о статусе
            Guna2HtmlLabel statusLabel = new Guna2HtmlLabel
            {
                Text = status, // Устанавливаем текст со статусом
                Font = font, // Устанавливаем шрифт
                ForeColor = GetStatusColor(status), // Устанавливаем цвет текста в зависимости от статуса
                Location = new Point(statusIcon.Right + 6, y), // Устанавливаем позицию (правее иконки)
                AutoSize = true // Автоматический размер для метки
            };
            card.Controls.Add(statusLabel); // Добавляем метку в карточку
        }

        // Функция для добавления разделителя
        private int AddDivider(Guna2Panel card, int xPosition)
        {
            PictureBox dividerLine = new PictureBox
            {
                Size = new Size(1, 120), // Размер картинки (подберите нужный)
                Image = Resources.line_1, // Загружаем изображение
                SizeMode = PictureBoxSizeMode.Zoom, // Масштабируем изображение
                Location = new Point(xPosition, 0), // Устанавливаем позицию
                BackColor = Color.Transparent // Прозрачный фон
            };
            card.Controls.Add(dividerLine); // Добавляем разделитель в карточку
            return dividerLine.Right;
        }

        // Функция для добавления информации о туре (даты, дни/ночи, сотрудники)
        private void AddTripDetails(Guna2Panel card, DateTime? startDate, DateTime? endDate, string daysNights, string adultsChildren, string staffName, int x, int y)
        {
            // Устанавливаем шрифт Segoe UI размером 12 и стиль Regular для текста
            using (Font regularFont = new Font("Segoe UI", 12, FontStyle.Regular))
            {
            }

            Font boldFont = new Font("Segoe UI", 12, FontStyle.Bold);

            // Добавляем метки с информацией о туре
            y = AddLabel(card, "Начало тура", Font, Color.FromArgb(0, 50, 73), x, y);
            y = AddLabel(card, startDate.HasValue ? $"{startDate:dd.MM.yyyy}" : "Не указано", Font, Color.FromArgb(0, 50, 73), x, y + 6);

            y = AddLabel(card, "Конец тура", Font, Color.FromArgb(0, 50, 73), x, y + 6);
            y = AddLabel(card, endDate.HasValue ? $"{endDate:dd.MM.yyyy}" : "Не указано", Font, Color.FromArgb(0, 50, 73), x, y + 6);

            y = AddLabel(card, "Дней/Ночей", Font, Color.FromArgb(0, 50, 73), x, y + 6);
            y = AddLabel(card, daysNights, Font, Color.FromArgb(0, 50, 73), x, y + 6);

            y = AddLabel(card, "Человек", Font, Color.FromArgb(0, 50, 73), x, y + 6);
            y = AddLabel(card, adultsChildren, Font, Color.FromArgb(0, 50, 73), x, y + 6);

            y = AddLabel(card, "Сотрудник", Font, Color.FromArgb(0, 50, 73), x, y + 6);
            y = AddLabel(card, staffName, Font, Color.FromArgb(0, 50, 73), x, y + 6);
        }

        // Функция для добавления информации о стоимости
        private void AddCostDetails(Guna2Panel card, decimal touropCost, decimal agentCost, decimal paid, decimal profit, int x, int y)
        {
            // Устанавливаем шрифт Segoe UI размером 12 и стиль Regular для текста
            Font regularFont = new Font("Segoe UI", 12, FontStyle.Regular);
            Font boldFont = new Font("Segoe UI", 12, FontStyle.Bold);


            y = AddLabel(card, "Полная стоимость у ТО", Font, Color.FromArgb(0, 50, 73), x, y);
            y = AddLabel(card, $"{touropCost:F2} Руб", Font, Color.FromArgb(0, 50, 73), x, y + 6);

            y = AddLabel(card, "Стоимость клиенту", Font, Color.FromArgb(0, 50, 73), x, y + 6);
            y = AddLabel(card, $"{agentCost:F2} Руб", Font, Color.FromArgb(71, 137, 120), x, y + 6);

            y = AddLabel(card, "Оплачено", Font, Color.FromArgb(0, 50, 73), x, y + 6);
            y = AddLabel(card, $"{paid:F2} Руб", Font, Color.FromArgb(0, 50, 73), x, y + 6);

            y = AddLabel(card, "Прибыль", Font, Color.FromArgb(0, 50, 73), x, y + 6);
            y = AddLabel(card, $"{profit:F2} Руб", Font, Color.FromArgb(0, 50, 73), x, y + 6);
        }*/
