using Dapper;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace DapperQueryBuilder.Tests
{
    public class ExplicitTypeTests
    {
        UnitTestsDbConnection cn;

        #region Setup
        [SetUp]
        public void Setup()
        {
            string connectionString = @"Data Source=LENOVOFLEX5\SQLEXPRESS;
                            Initial Catalog=AdventureWorks;
                            Integrated Security=True;";
            cn = new UnitTestsDbConnection(new SqlConnection(connectionString));
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
        public void TestExplicitTypes()
        {
            decimal cost = 884.7083m;

            var cmd1 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [StandardCost]={cost}"); // int32 is matched against DbType.Int32 and will send this dbType to Dapper
            Assert.AreEqual("SELECT * FROM [Production].[Product] p WHERE [StandardCost]=@p0", cmd1.Sql);
            var products = cmd1.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.Decimal);

            var cmd2 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [StandardCost]={cost:int32}"); // int32 is matched against DbType.Int32 and will send this dbType to Dapper
            Assert.AreEqual("SELECT * FROM [Production].[Product] p WHERE [StandardCost]=@p0", cmd2.Sql);
            products = cmd2.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.Int);

            System.Diagnostics.Debug.WriteLine(cn.LastCommand.CommandText);
        }

        [Test]
        public void TestExplicitTypes2()
        {
            string productName = "Mountain%";

            // By default strings are Unicode (nvarchar) and size is max between DbString.DefaultLength (4000) or string
            var cmd1 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [Name] LIKE {productName}");
            var products = cmd1.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.NVarChar);
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).Size == Dapper.DbString.DefaultLength);


            // Unless we specify it's an Ansi (non-unicode) string
            var cmd2 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [Name] LIKE {productName:AnsiString}");
            products = cmd2.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.VarChar);
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).Size == Dapper.DbString.DefaultLength);

            // If string is larger than DbString.DefaultLength (4000), size will be string size
            productName = new string('c', 4010);
            var cmd3 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [Name] LIKE {productName:AnsiString}");
            products = cmd3.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.VarChar);
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).Size == 4010);
        }

        [Test]
        public void TestExplicitTypes3()
        {
            string productName = "Mountain%";

            var cmd1 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [Name] LIKE {productName:nvarchar(20)}");
            var products = cmd1.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.NVarChar);
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).Size == 20);


            var cmd2 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [Name] LIKE {productName:varchar(30)}");
            products = cmd2.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.VarChar);
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).Size == 30);
        }

        [Test]
        public void TestExplicitTypes4()
        {
            string productName = "Mountain%";

            var cmd1 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [Name] LIKE {productName:nvarchar()}");
            var products = cmd1.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.NVarChar);
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).Size == Dapper.DbString.DefaultLength);


            var cmd2 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [Name] LIKE {productName:varchar()}");
            products = cmd2.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.VarChar);
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).Size == Dapper.DbString.DefaultLength);
        }

        [Test]
        public void TestExplicitTypes5()
        {
            string productName = "Mountain%";

            var cmd1 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [Name] LIKE {productName:nchar()}");
            var products = cmd1.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.NChar);
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).Size == productName.Length);


            var cmd2 = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [Name] LIKE {productName:char(20)}");
            products = cmd2.Query<Product>();
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).SqlDbType == SqlDbType.Char);
            Assert.That(((SqlParameter)cn.LastCommand.Parameters[0]).Size == 20);
        }


    }
}
