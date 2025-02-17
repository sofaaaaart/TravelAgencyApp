/* --- Создание базы данных (если не существует) --- */
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'db_ta')
BEGIN
    CREATE DATABASE db_ta;
END;

USE db_ta;

/* --- Таблица clients --- */
CREATE TABLE clients (
    c_id INT IDENTITY(1,1) PRIMARY KEY,
    c_fullname NVARCHAR(50) NOT NULL,
    c_date DATE NOT NULL,
    c_passport NVARCHAR(50) NOT NULL,
    c_phone NVARCHAR(30) NOT NULL,
    c_email NVARCHAR(30),
    c_children NVARCHAR(MAX) DEFAULT NULL,  -- JSON хранится в NVARCHAR(MAX)
    c_comment NVARCHAR(50),
    reg_date DATETIME DEFAULT GETDATE()
);

/* --- Таблица сотрудников (stufff) --- */
CREATE TABLE staff (
    s_id INT IDENTITY(1,1) PRIMARY KEY,
    s_name NVARCHAR(50) NOT NULL,
    s_lastName NVARCHAR(50) NOT NULL,
    s_middleName NVARCHAR(50),
    s_date DATE,
    s_passport NVARCHAR(50),
    s_post NVARCHAR(30) NOT NULL,
    s_phone NVARCHAR(30),
    s_login NVARCHAR(30) NOT NULL UNIQUE,
    s_password NVARCHAR(255) NOT NULL, 
    s_comment NVARCHAR(50),
    reg_date DATETIME DEFAULT GETDATE()
);

/* --- Таблица туроператоров --- */
CREATE TABLE touroperators (
    t_id INT IDENTITY(1,1) PRIMARY KEY,
    t_name NVARCHAR(50),
    t_comm NVARCHAR(200),
    reg_date DATETIME DEFAULT GETDATE()
);

/* --- Таблица статусов --- */
CREATE TABLE status (
    status_id INT IDENTITY(1,1) PRIMARY KEY,
    status_name NVARCHAR(20) NOT NULL UNIQUE,
    reg_date DATETIME DEFAULT GETDATE()
);

/* --- Таблица заявок (requests) --- */
CREATE TABLE requests (
    r_id INT IDENTITY(1,1) PRIMARY KEY,
    r_client INT,
    r_phone CHAR(11) NOT NULL,
    r_staff INT,
    r_start_date DATE NOT NULL,
    r_end_date DATE NOT NULL,
    r_days_nights AS (CONCAT(DATEDIFF(DAY, r_start_date, r_end_date), '/', DATEDIFF(DAY, r_start_date, r_end_date) - 1)) PERSISTED,
    r_country NVARCHAR(30) NOT NULL,
    r_tourop INT,
    r_tourop_cost DECIMAL(10,2) NOT NULL DEFAULT 0,
    r_agent_cost AS (r_tourop_cost * 1.06) PERSISTED,
    r_status INT,
    r_departure_city NVARCHAR(50) NOT NULL,
    r_adults_children NVARCHAR(30) NOT NULL, 
    reg_date DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (r_staff) REFERENCES staff (s_id) ON DELETE CASCADE,
    FOREIGN KEY (r_client) REFERENCES clients (c_id) ON DELETE CASCADE,
    FOREIGN KEY (r_tourop) REFERENCES touroperators (t_id) ON DELETE CASCADE,
    FOREIGN KEY (r_status) REFERENCES status (status_id) ON DELETE CASCADE
);

/* --- Таблица процессов (process) --- */
CREATE TABLE process (
    p_id INT IDENTITY(1,1) PRIMARY KEY,
    p_staff INT,
    p_req INT,
    p_status INT,
    reg_date DATETIME DEFAULT GETDATE(),
    
    FOREIGN KEY (p_staff) REFERENCES staff (s_id) ON DELETE CASCADE,
    FOREIGN KEY (p_req) REFERENCES requests (r_id) ON DELETE NO ACTION,  -- Используем NO ACTION вместо CASCADE
    FOREIGN KEY (p_status) REFERENCES status (status_id) ON DELETE CASCADE
);

/* --- Таблица user_sessions --- */
CREATE TABLE user_sessions (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,  -- Ссылка на staff.s_id
    token NVARCHAR(255) NOT NULL,
    expires_at DATETIME NOT NULL,
    FOREIGN KEY (user_id) REFERENCES staff(s_id) ON DELETE CASCADE
);
