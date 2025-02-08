/* Artemyeva Sofya */
/* 3-MD-2 */

/* --- Database --- */
CREATE DATABASE IF NOT EXISTS db_ta;
USE db_ta;

/* --- Tables --- */

/* --- Tables --- */


CREATE TABLE clients (
    c_id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    c_fullname VARCHAR(50) NOT NULL,
    c_date DATE NOT NULL,
    c_passport VARCHAR(50) NOT NULL,
    c_phone VARCHAR(30) NOT NULL,
    c_email VARCHAR(30),
    c_children VARCHAR(20),
    c_comment VARCHAR(50),
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE types (
    et_id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    et_name VARCHAR(30),
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE education (
    e_id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    e_specialty VARCHAR(40) NOT NULL,
	e_type INT(6) UNSIGNED,
    e_diplom VARCHAR(15) NOT NULL,
    e_year INT(4) NOT NULL,
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (e_type) REFERENCES types (et_id) ON DELETE CASCADE ON UPDATE RESTRICT
);

CREATE TABLE stufff (
    s_id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    s_fullname VARCHAR(50) NOT NULL,
    s_date DATE NOT NULL,
    s_passport VARCHAR(50) NOT NULL,
    s_inn CHAR(12) NOT NULL,
    s_snils CHAR(14) NOT NULL,
    s_post VARCHAR(30) NOT NULL,
    s_sal DECIMAL(8,2) NOT NULL,
    s_phone VARCHAR(30) NOT NULL,
    s_login VARCHAR(30) NOT NULL,
    s_password VARCHAR(30) NOT NULL,
    s_feducation INT(6) UNSIGNED,
    s_seducation INT(6) UNSIGNED,
    s_comment VARCHAR(50),
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (s_feducation) REFERENCES education (e_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (s_seducation) REFERENCES education (e_id) ON DELETE CASCADE ON UPDATE RESTRICT
);

CREATE TABLE touroperators (
    t_id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    t_name VARCHAR(50),
    t_comm VARCHAR(200),
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE status (
    status_id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    status_name VARCHAR(20),
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE requests (
    r_id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    r_client INT(6) UNSIGNED,
    r_phone CHAR(11) NOT NULL,
    r_stuff INT(6) UNSIGNED,
    r_date DATE NOT NULL,
    r_country VARCHAR(30) NOT NULL,
    r_tourop INT(6) UNSIGNED,
    r_status INT(6) UNSIGNED,
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (r_stuff) REFERENCES stufff (s_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (r_client) REFERENCES clients (c_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (r_tourop) REFERENCES touroperators (t_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (r_status) REFERENCES status (status_id) ON DELETE CASCADE ON UPDATE RESTRICT
);

CREATE TABLE process (
    p_id INT(6) UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    p_staff INT(6) UNSIGNED,
    p_req INT(6) UNSIGNED,
    p_status INT(6) UNSIGNED,
    reg_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (p_staff) REFERENCES stufff (s_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (p_req) REFERENCES requests (r_id) ON DELETE CASCADE ON UPDATE RESTRICT,
    FOREIGN KEY (p_status) REFERENCES status (status_id) ON DELETE CASCADE ON UPDATE RESTRICT
);



