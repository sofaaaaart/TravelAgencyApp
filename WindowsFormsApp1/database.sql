/* --- Создание базы данных --- */
CREATE DATABASE IF NOT EXISTS db_ta;
USE db_ta;

/* --- Таблица clients --- */
CREATE TABLE clients (
    c_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    c_fullname VARCHAR(50) NOT NULL,
    c_date DATE NOT NULL,
    c_passport VARCHAR(50) NOT NULL,
    c_phone VARCHAR(30) NOT NULL,
    c_email VARCHAR(30),
    c_children JSON DEFAULT NULL,
    c_comment VARCHAR(50),
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

/* --- Таблица сотрудников (stufff) --- */
CREATE TABLE stufff (
    s_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    s_lastName VARCHAR(50) NOT NULL,
    s_name VARCHAR(50) NOT NULL,
    s_date DATE NOT NULL,
    s_passport VARCHAR(50) NOT NULL,
    s_post VARCHAR(30) NOT NULL,
    s_phone VARCHAR(30) NOT NULL,
    s_login VARCHAR(30) NOT NULL UNIQUE,
    s_password VARCHAR(255) NOT NULL, 
    s_comment VARCHAR(50),
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

/* --- Таблица туроператоров --- */
CREATE TABLE touroperators (
    t_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    t_name VARCHAR(50),
    t_comm VARCHAR(200),
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

/* --- Таблица статусов --- */
CREATE TABLE status (
    status_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    status_name VARCHAR(20) NOT NULL UNIQUE,
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

/* --- Таблица заявок (requests) --- */
CREATE TABLE requests (
    r_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    r_client INT UNSIGNED,
    r_phone CHAR(11) NOT NULL,
    r_stufff INT UNSIGNED,
    r_start_date DATE NOT NULL,
    r_end_date DATE NOT NULL,
    r_days_nights VARCHAR(10) GENERATED ALWAYS AS (CONCAT(DATEDIFF(r_end_date, r_start_date), '/', DATEDIFF(r_end_date, r_start_date) - 1)) STORED,
    r_country VARCHAR(30) NOT NULL,
    r_tourop INT UNSIGNED,
    r_tourop_cost DECIMAL(10, 2) NOT NULL DEFAULT 0,
    r_agent_cost DECIMAL(10, 2) GENERATED ALWAYS AS (r_tourop_cost * 1.06) STORED,
    r_status INT UNSIGNED,
    r_departure_city VARCHAR(50) NOT NULL,
    r_adults_children VARCHAR(30) NOT NULL, 
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (r_stufff) REFERENCES stufff (s_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (r_client) REFERENCES clients (c_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (r_tourop) REFERENCES touroperators (t_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (r_status) REFERENCES status (status_id) ON DELETE CASCADE ON UPDATE RESTRICT
);

/* --- Таблица процессов (process) --- */
CREATE TABLE process (
    p_id INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    p_staff INT UNSIGNED,
    p_req INT UNSIGNED,
    p_status INT UNSIGNED,
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (p_staff) REFERENCES stufff (s_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (p_req) REFERENCES requests (r_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (p_status) REFERENCES status (status_id) ON DELETE CASCADE ON UPDATE RESTRICT
);
/* --- Таблица user_sessions --- */
CREATE TABLE user_sessions (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT UNSIGNED NOT NULL,  -- Делаем UNSIGNED, чтобы совпадало с stufff.s_id
    token VARCHAR(255) NOT NULL,
    expires_at DATETIME NOT NULL,
    FOREIGN KEY (user_id) REFERENCES stufff(s_id) ON DELETE CASCADE ON UPDATE RESTRICT
);