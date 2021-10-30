/*********** How to setup a MSSQL for tests ***********/

-- 1) Download https://github.com/Microsoft/sql-server-samples/releases/download/adventureworks/AdventureWorks2019.bak
-- 2) Find your DATA FOLDER (if it's not C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\) using this query: 
  /*
	SELECT TOP 1 
    substring(f.physical_name, 1, len(f.physical_name)-charindex('\', reverse(f.physical_name))+1) 
    from sys.master_files f INNER JOIN sys.databases db ON f.database_id=db.database_id
    order by len ( substring(f.physical_name, 1, len(f.physical_name)-charindex('\', reverse(f.physical_name))+1)  )
  */
  -- Replace folders below with the correct locations.

RESTORE DATABASE [AdventureWorks2019] FROM DISK = N'C:\Users\youruser\Downloads\AdventureWorks2019.bak' WITH FILE=1,
MOVE 'AdventureWorks2017' TO 'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\AdventureWorks2019.mdf',
MOVE 'AdventureWorks2017_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL15.MSSQLSERVER\MSSQL\DATA\AdventureWorks2019.ldf'
GO

-- 3) Adjust Connection string in TestSettings.json 
-- e.g. remove "\SQLEXPRESS", and/or change "(local)" by some hostname
-- e.g. replace "Integrated Security=True" by "User Id=username;Password=password"

-- 4) Create some extra objects (AdventureWorks does not have everything we need):
USE [AdventureWorks2019]
GO

CREATE PROCEDURE [sp_TestOutput]
    @Input1 [int], 
    @Output1 [int] OUTPUT
AS
BEGIN
	SET @Output1 = 2
END;
GO
