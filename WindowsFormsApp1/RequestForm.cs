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

            // Присваиваем существующее подключение и родительскую форму
            this.connection = existingConnection; // Сохраняем родительскую форму для дальнейшего использования

            LoadDataAndCreateCards(cardsPanel);
        }


        private void InitializeImages()
        {
            // Установите базовый путь
            string basePath = AppDomain.CurrentDomain.BaseDirectory; // Путь к исполняемому файлу
            string imagePath = System.IO.Path.Combine(basePath, "img"); // Путь к папке "img"

            try
            {
                // Загрузите изображения для HideButton
                normalHideImage = Image.FromFile(System.IO.Path.Combine(imagePath, "ButtonHide.png"));
                hoverHideImage = Image.FromFile(System.IO.Path.Combine(imagePath, "ButtonHide1.png"));

                // Загрузите изображения для CloseButton
                normalCloseImage = Image.FromFile(System.IO.Path.Combine(imagePath, "ButtonClose.png"));
                hoverCloseImage = Image.FromFile(System.IO.Path.Combine(imagePath, "ButtonClose1.png"));
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

        private void LoadDataAndCreateCards(Guna2Panel cardsPanel)
        {
            try
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                // Пример запроса для SQL Server
                string query = @"
            SELECT r.*, s.status_name, (r_agent_cost - r_tourop_cost) AS profit 
            FROM requests r 
            JOIN status s ON r.r_status = s.status_id;
        ";

                // Используем SqlCommand вместо MySqlCommand
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();  // Используем SqlDataReader вместо MySqlDataReader

                int maxCards = 3;
                int count = 0;
                int cardSpacing = 6;

                cardsPanel.Controls.Clear();

                while (reader.Read() && count < maxCards)
                {
                    int id = reader.GetInt32(reader.GetOrdinal("r_id"));
                    string country = reader.IsDBNull(reader.GetOrdinal("r_country")) ? "Unknown" : reader.GetString(reader.GetOrdinal("r_country"));
                    string clientName = reader.IsDBNull(reader.GetOrdinal("r_client")) ? "Unknown" : reader.GetString(reader.GetOrdinal("r_client"));
                    string phone = reader.IsDBNull(reader.GetOrdinal("r_phone")) ? "Unknown" : reader.GetString(reader.GetOrdinal("r_phone"));
                    string departureCity = reader.IsDBNull(reader.GetOrdinal("r_departure_city")) ? "Unknown" : reader.GetString(reader.GetOrdinal("r_departure_city"));
                    string status = reader.IsDBNull(reader.GetOrdinal("status_name")) ? "Unknown" : reader.GetString(reader.GetOrdinal("status_name"));

                    DateTime? startDate = reader.IsDBNull(reader.GetOrdinal("r_start_date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("r_start_date"));
                    DateTime? endDate = reader.IsDBNull(reader.GetOrdinal("r_end_date")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("r_end_date"));

                    string daysNights = reader.IsDBNull(reader.GetOrdinal("r_days_nights")) ? "0/0" : reader.GetString(reader.GetOrdinal("r_days_nights"));
                    string adultsChildren = reader.IsDBNull(reader.GetOrdinal("r_adults_children")) ? "0/0" : reader.GetString(reader.GetOrdinal("r_adults_children"));

                    int stuff = reader.IsDBNull(reader.GetOrdinal("r_stufff")) ? 0 : reader.GetInt32(reader.GetOrdinal("r_stufff"));

                    decimal touropCost = reader.IsDBNull(reader.GetOrdinal("r_tourop_cost")) ? 0 : reader.GetDecimal(reader.GetOrdinal("r_tourop_cost"));
                    decimal agentCost = reader.IsDBNull(reader.GetOrdinal("r_agent_cost")) ? 0 : reader.GetDecimal(reader.GetOrdinal("r_agent_cost"));
                    bool paid = !reader.IsDBNull(reader.GetOrdinal("r_paid")) && reader.GetBoolean(reader.GetOrdinal("r_paid"));

                    // profit = agentCost - touropCost (если в SQL добавлено)
                    decimal profit = reader.IsDBNull(reader.GetOrdinal("profit")) ? 0 : reader.GetDecimal(reader.GetOrdinal("profit"));

                    // Создание карточки с дополнительной информацией
                    var card = CreateCard(id, country, departureCity, status, startDate, endDate,
                        daysNights, adultsChildren, touropCost, agentCost, clientName,
                        phone, stuff, paid, profit);

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
                Image = Resources.line, // Загружаем изображение
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

