using NUnit.Framework;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DapperQueryBuilder.Tests
{
	public class CommandBuilderTests
	{
		IDbConnection cn;

		#region Setup
		[SetUp]
		public void Setup()
		{
			string connectionString = @"Data Source=LENOVOFLEX5;
                            Initial Catalog=AdventureWorks;
                            Integrated Security=True;";
			cn = new SqlConnection(connectionString);
		}
		#endregion

		int businessEntityID = 1;
		string nationalIDNumber = "295847284";
		DateTime birthDate = new DateTime(1969, 01, 29);
		string maritalStatus = "S"; // single
		string gender = "M";

		public class Product
		{
			public int ProductId { get; set; }
			public string Name { get; set; }
		}

		[Test]
		public void TestBareCommand()
		{
			string productName = "%mountain%";
			int subCategoryId = 12;

			var query = cn
				.QueryBuilder($@"
                SELECT * FROM [Production].[Product]
                WHERE
                [Name] LIKE {productName}
                AND [ProductSubcategoryID] = {subCategoryId}
                ORDER BY [ProductId]");

			Assert.AreEqual(@"
SELECT * FROM [Production].[Product]
WHERE
[Name] LIKE @p0
AND [ProductSubcategoryID] = @p1
ORDER BY [ProductId]".TrimStart(), query.Sql);

			var products = query.Query<Product>();
		}


		[Test]
		public void TestNameof()
		{
			string productName = "%mountain%";
			int subCategoryId = 12;

			var query = cn
				.QueryBuilder($@"
                SELECT * FROM [Production].[Product]
                WHERE
                [{nameof(Product.Name):raw}] LIKE {productName}
                AND [ProductSubcategoryID] = {subCategoryId}
                ORDER BY [ProductId]");

			Assert.AreEqual(@"
SELECT * FROM [Production].[Product]
WHERE
[Name] LIKE @p0
AND [ProductSubcategoryID] = @p1
ORDER BY [ProductId]".TrimStart(), query.Sql);

			var products = query.Query<Product>();
		}

		[Test]
		public void TestAppends()
		{
			string productName = "%mountain%";
			int subCategoryId = 12;

			var query = cn
				.CommandBuilder($@"SELECT * FROM [Production].[Product]")
				.AppendLine($"WHERE")
				.AppendLine($"[Name] LIKE {productName}")
				.AppendLine($"AND [ProductSubcategoryID] = {subCategoryId}")
				.AppendLine($"ORDER BY [ProductId]");
			Assert.AreEqual(
@"SELECT * FROM [Production].[Product]
WHERE
[Name] LIKE @p0
AND [ProductSubcategoryID] = @p1
ORDER BY [ProductId]", query.Sql);

			var products = query.Query<Product>();
		}

		[Test]
		public void TestAutoSpacing()
		{
			string productName = "%mountain%";
			int subCategoryId = 12;

			var query = cn
				.CommandBuilder($@"SELECT * FROM [Production].[Product]")
				.Append($"WHERE")
				.Append($"[Name] LIKE {productName}")
				.Append($"AND [ProductSubcategoryID] = {subCategoryId}")
				.Append($"ORDER BY [ProductId]");

			Assert.AreEqual(@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0 AND [ProductSubcategoryID] = @p1 ORDER BY [ProductId]", query.Sql);

			var products = query.Query<Product>();
		}

		[Test]
		public void TestStoredProcedure()
		{
			var q = cn.CommandBuilder($"[HumanResources].[uspUpdateEmployeePersonalInfo]")
				.AddParameter("BusinessEntityID", businessEntityID)
				.AddParameter("NationalIDNumber", nationalIDNumber)
				.AddParameter("BirthDate", birthDate)
				.AddParameter("MaritalStatus", maritalStatus)
				.AddParameter("Gender", gender);
			int affected = q.Execute(commandType: CommandType.StoredProcedure);
		}

		[Test]
		public void TestStoredProcedureExec()
		{
			var q = cn.CommandBuilder($@"
                DECLARE @ret INT;
                EXEC @RET = [HumanResources].[uspUpdateEmployeePersonalInfo] 
                   @BusinessEntityID={businessEntityID}
                  ,@NationalIDNumber={nationalIDNumber}
                  ,@BirthDate={birthDate}
                  ,@MaritalStatus={maritalStatus}
                  ,@Gender={gender};
                SELECT @RET;
                ");

			int affected = q.Execute();
		}

		public class MyPoco { public int MyValue { get; set; } }

		[Test]
		public void TestStoredProcedureOutput()
		{
			/*
                CREATE PROCEDURE [Test]
                    @Input1 [int], 
                    @Output1 [int] OUTPUT
                AS
                BEGIN
	                SET @Output1 = 2
                END;
                GO
            */

			MyPoco poco = new MyPoco();

			var cmd = cn.CommandBuilder($"[dbo].[Test]")
				.AddParameter("Input1", dbType: DbType.Int32);
			//.AddParameter("Output1",  dbType: DbType.Int32, direction: ParameterDirection.Output);
			//var getter = ParameterInfos.GetSetter((MyPoco p) => p.MyValue);
			cmd.Parameters.Add(ParameterInfo.CreateOutputParameter("Output1", poco, p => p.MyValue, ParameterInfo.OutputParameterDirection.Output, size: 4));
			int affected = cmd.Execute(commandType: CommandType.StoredProcedure);

			string outputValue = cmd.Parameters.Get<string>("Output1"); // why is this being returned as string? just because I didn't provide type above?
			Assert.AreEqual(outputValue, "2");

			Assert.AreEqual(poco.MyValue, 2);
		}

		[Test]
		public void TestCRUD()
		{
			string id = "123";
			int affected = cn.CommandBuilder($@"UPDATE [HumanResources].[Employee] SET")
				.Append($"NationalIDNumber={id}")
				.Append($"WHERE BusinessEntityID={businessEntityID}")
				.Execute();
		}

		/// <summary>
		/// Quotes around interpolated arguments should be automtaically detected and ignored
		/// </summary>
		[Test]
		public void TestQuotes1()
		{
			string search = "%mountain%";
			string expected = "SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0";
			var cmd = cn.CommandBuilder($@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE {search}");
			var cmd2 = cn.CommandBuilder($@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE '{search}'");

			Assert.AreEqual(expected, cmd.Sql);
			Assert.AreEqual(expected, cmd2.Sql);

			var products = cmd.Query<Product>();
			Assert.That(products.Any());
			var products2 = cmd2.Query<Product>();
			Assert.That(products2.Any());
		}


		/// <summary>
		/// Quotes around interpolated arguments should be automtaically detected and ignored
		/// </summary>
		[Test]
		public void TestQuotes2()
		{
			string productNumber = "AR-5381";
			string expected = "SELECT * FROM [Production].[Product] WHERE [ProductNumber]=@p0";
			var cmd = cn.CommandBuilder($@"SELECT * FROM [Production].[Product] WHERE [ProductNumber]='{productNumber}'");
			var cmd2 = cn.CommandBuilder($@"SELECT * FROM [Production].[Product] WHERE [ProductNumber]={productNumber}");

			Assert.AreEqual(expected, cmd.Sql);
			Assert.AreEqual(expected, cmd2.Sql);

			var products = cmd.Query<Product>();
			Assert.That(products.Any());
			var products2 = cmd2.Query<Product>();
			Assert.That(products2.Any());
		}


		/// <summary>
		/// Quotes around interpolated arguments should be automtaically detected and ignored
		/// </summary>
		[Test]
		public void TestQuotes3()
		{
			string productNumber = "AR-5381";
			string expected = "SELECT * FROM [Production].[Product] WHERE @p0<=[ProductNumber]";
			var cmd = cn.CommandBuilder($@"SELECT * FROM [Production].[Product] WHERE '{productNumber}'<=[ProductNumber]");
			var cmd2 = cn.CommandBuilder($@"SELECT * FROM [Production].[Product] WHERE {productNumber}<=[ProductNumber]");

			Assert.AreEqual(expected, cmd.Sql);
			Assert.AreEqual(expected, cmd2.Sql);

			var products = cmd.Query<Product>();
			Assert.That(products.Any());
			var products2 = cmd2.Query<Product>();
			Assert.That(products2.Any());
		}


		/// <summary>
		/// Quotes around interpolated arguments should not be ignored if it's raw string
		/// </summary>
		[Test]
		public void TestQuotes4()
		{
			string literal = "Hello";
			string search = "%mountain%";

			string expected = "SELECT 'Hello' FROM [Production].[Product] WHERE [Name] LIKE @p0";
			var cmd = cn.CommandBuilder($@"SELECT '{literal:raw}' FROM [Production].[Product] WHERE [Name] LIKE {search}"); // quotes will be preserved

			string expected2 = "SELECT @p0 FROM [Production].[Product] WHERE [Name] LIKE @p1";
			var cmd2 = cn.CommandBuilder($@"SELECT '{literal}' FROM [Production].[Product] WHERE [Name] LIKE {search}"); // quotes will be striped

			Assert.AreEqual(expected, cmd.Sql);
			Assert.AreEqual(expected2, cmd2.Sql);

			var products = cmd.Query<Product>();
			Assert.That(products.Any());

			var products2 = cmd2.Query<Product>();
			Assert.That(products2.Any());
		}


		[Test]
		public void TestAutospacing()
		{
			string search = "%mountain%";
			var cmd = cn.CommandBuilder($@"SELECT * FROM [Production].[Product]");
			cmd.Append($"WHERE [Name] LIKE {search}");
			cmd.Append($"AND 1=1");
			Assert.AreEqual("SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0 AND 1=1", cmd.Sql);
		}

		[Test]
		public void TestAutospacing2()
		{
			string search = "%mountain%";
			var cmd = cn.CommandBuilder($@"
                            SELECT * FROM [Production].[Product]
                            WHERE [Name] LIKE {search}
                            AND 1=2");
			Assert.AreEqual(
				"SELECT * FROM [Production].[Product]" + Environment.NewLine +
				"WHERE [Name] LIKE @p0" + Environment.NewLine +
				"AND 1=2", cmd.Sql);
		}

		[Test]
		public void TestAutospacing3()
		{
			string productNumber = "EC-M092";
			int productId = 328;
			var cmd = cn.CommandBuilder($@"
                UPDATE [Production].[Product]
                SET [ProductNumber]={productNumber}
                WHERE [ProductId]={productId}");

			string expected =
				"UPDATE [Production].[Product]" + Environment.NewLine +
				"SET [ProductNumber]=@p0" + Environment.NewLine +
				"WHERE [ProductId]=@p1";

			Assert.AreEqual(expected, cmd.Sql);
		}

		[Test]
		public void TestAutospacing4()
		{
			string productNumber = "EC-M092";
			int productId = 328;

			var cmd = cn.CommandBuilder($@"UPDATE [Production].[Product]")
				.Append($"SET [ProductNumber]={productNumber}")
				.Append($"WHERE [ProductId]={productId}");

			string expected = "UPDATE [Production].[Product] SET [ProductNumber]=@p0 WHERE [ProductId]=@p1";

			Assert.AreEqual(expected, cmd.Sql);
		}


		[Test]
		public void TestQueryBuilderWithAppends()
		{
			string productName = "%mountain%";
			int subCategoryId = 12;

			var query = cn
				.QueryBuilder($@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE {productName}");
			query.AppendLine($"AND [ProductSubcategoryID] = {subCategoryId} ORDER BY {2}");
			Assert.AreEqual(@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0
AND [ProductSubcategoryID] = @p1 ORDER BY @p2", query.Sql);

			//var products = query.Query<Product>();
		}

		[Test]
		public void TestQueryBuilderWithAppends2()
		{
			string productName = "%mountain%";
			int subCategoryId = 12;

			var query = cn
				.QueryBuilder($@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE {productName}");
			query.AppendLine($"AND [ProductSubcategoryID]={subCategoryId} ORDER BY {2}");
			Assert.AreEqual(@"SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0
AND [ProductSubcategoryID]=@p1 ORDER BY @p2", query.Sql);

			//var products = query.Query<Product>();
		}


		[Test]
		public void TestRepeatedParameters()
		{
			string username = "rdrizin";
			int subCategoryId = 12;
			int? categoryId = null;

			var query = cn.QueryBuilder($@"SELECT * FROM [table1] WHERE ([Name]={username} or [Author]={username}");
			query.Append($"or [Creator]={username})");
			query.Append($"AND ([ProductSubcategoryID]={subCategoryId}");
			query.Append($"OR [ProductSubcategoryID]={categoryId}");
			query.Append($"OR [ProductCategoryID]={subCategoryId}");
			query.Append($"OR [ProductCategoryID]={categoryId})");

			Assert.AreEqual(@"SELECT * FROM [table1] WHERE ([Name]=@p0 or [Author]=@p0"
				+ " or [Creator]=@p0)"
				+ " AND ([ProductSubcategoryID]=@p1"
				+ " OR [ProductSubcategoryID]=@p2"
				+ " OR [ProductCategoryID]=@p1"
				+ " OR [ProductCategoryID]=@p2)"
				, query.Sql);
			int? val = query.Parameters.Get<int?>("p2");
			Assert.AreEqual(val, null);

			int val2 = query.Parameters.Get<int>("p1");
			Assert.AreEqual(val2, 12);
		}

		[Test]
		public void TestRepeatedParameters2()
		{
			int? fileId = null;
			string backupFileName = null;
			var folderKey = 93572;
			var secureFileName = "upload.txt";
			var uploadDate = DateTime.Now;
			string description = null;
			int? size = null;
			var cacheId = new Guid("{95b94695-b7ec-4e3a-9260-f815cceb5ff1}");
			var version = new Guid("{95b94695-b7ec-4e3a-9260-f815cceb5ff1}");
			var user = "terry.aney";
			var contentType = "application/pdf";
			var folder = "btr.aney.terry";

			var query = cn.QueryBuilder($@"DECLARE @fKey int; SET @fKey = {fileId};
DECLARE @backupFileName VARCHAR(1); SET @backupFileName = {backupFileName};

IF @backupFileName IS NOT NULL BEGIN
	UPDATE [File] SET Name = @backupFileName WHERE UID = @fKey;
	SET @fKey = NULL;
END

IF @fKey IS NULL BEGIN
	INSERT INTO [File] ( Folder, Name, CreateTime, Description )
	VALUES ( {folderKey}, {secureFileName}, {uploadDate}, {description} )
	SELECT @fKey = SCOPE_IDENTITY();
END ELSE BEGIN
	-- File Existed
	UPDATE [File] SET Deleted = 0, Description = ISNULL({description}, Description) WHERE UID = @fKey
END

DECLARE @size int; SET @size = {size};

-- File was compressed during upload so the 'original' file size is wrong and need to query the length of the content
IF @size IS NULL BEGIN
	SELECT @size = DATALENGTH( Content )
	FROM Cache
	WHERE UID = {cacheId}
END

INSERT INTO Version ( [File], VersionID, Time, UploadedBy, ContentType, Size, VersionIndex, DataLockerToken )
VALUES ( @fKey, {version}, {uploadDate}, {user}, {contentType}, @size, 0, {cacheId} )

INSERT INTO [Log] ( Action, FolderName, FileName, VersionId, VersionIndex, [User], Size, Time )
VALUES ( 'I', {folder}, {secureFileName}, {version}, 0, {user}, @size, {uploadDate} )

SELECT @fKey");

			Assert.AreEqual(@"DECLARE @fKey int; SET @fKey = @p0;
DECLARE @backupFileName VARCHAR(1); SET @backupFileName = @p0;

IF @backupFileName IS NOT NULL BEGIN
	UPDATE [File] SET Name = @backupFileName WHERE UID = @fKey;
	SET @fKey = NULL;
END

IF @fKey IS NULL BEGIN
	INSERT INTO [File] ( Folder, Name, CreateTime, Description )
	VALUES ( @p1, @p2, @p3, @p0 )
	SELECT @fKey = SCOPE_IDENTITY();
END ELSE BEGIN
	-- File Existed
	UPDATE [File] SET Deleted = 0, Description = ISNULL(@p0, Description) WHERE UID = @fKey
END

DECLARE @size int; SET @size = @p0;

-- File was compressed during upload so the 'original' file size is wrong and need to query the length of the content
IF @size IS NULL BEGIN
	SELECT @size = DATALENGTH( Content )
	FROM Cache
	WHERE UID = @p4
END

INSERT INTO Version ( [File], VersionID, Time, UploadedBy, ContentType, Size, VersionIndex, DataLockerToken )
VALUES ( @fKey, @p4, @p3, @p5, @p6, @size, 0, @p4 )

INSERT INTO [Log] ( Action, FolderName, FileName, VersionId, VersionIndex, [User], Size, Time )
VALUES ( 'I', @p7, @p2, @p4, 0, @p5, @size, @p3 )

SELECT @fKey", query.Sql);

			Assert.AreEqual(query.Parameters.Get<int?>("p0"), null);
			Assert.AreEqual(query.Parameters.Get<int>("p1"), folderKey);
			Assert.AreEqual(query.Parameters.Get<string>("p2"), secureFileName);
			Assert.AreEqual(query.Parameters.Get<DateTime>("p3"), uploadDate);
			Assert.AreEqual(query.Parameters.Get<Guid>("p4"), cacheId);
			Assert.AreEqual(query.Parameters.Get<string>("p5"), user);
			Assert.AreEqual(query.Parameters.Get<string>("p6"), contentType);
			Assert.AreEqual(query.Parameters.Get<string>("p7"), folder);
		}

	}
}
