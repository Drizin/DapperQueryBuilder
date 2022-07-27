/*********** How to setup a MySQL for tests ***********/

-- 1) Download MariaDB for Windows https://dlm.mariadb.com/browse/mariadb_server/200/1374/winx64-packages/

CREATE DATABASE DapperQueryBuilderTests;

USE DapperQueryBuilderTests;

CREATE TABLE authors
(author_id INT AUTO_INCREMENT PRIMARY KEY,
name_last VARCHAR(50),
name_first VARCHAR(50),
country VARCHAR(50) );

INSERT INTO authors
(name_last, name_first, country)
VALUES('Kafka', 'Franz', 'Czech Republic');

INSERT INTO authors
(name_last, name_first, country)
VALUES('de Assis', 'Machado', 'Brazil');


CREATE TABLE books (
isbn CHAR(20) PRIMARY KEY, 
title VARCHAR(50),
author_id INT,
publisher_id INT,
year_pub CHAR(4),
description TEXT );

INSERT INTO books
(title, author_id, isbn, year_pub)
VALUES('The Castle', '1', '0805211063', '1998');

INSERT INTO books
(title, author_id, isbn, year_pub)
VALUES('The Trial', '1', '0805210407', '1995'),
('The Metamorphosis', '1', '0553213695', '1995'),
('America', '1', '0805210644', '1995');
