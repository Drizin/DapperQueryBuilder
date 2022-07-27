using MySql.Data.MySqlClient;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace DapperQueryBuilder.Tests
{
    public class MySQLTests
    {
        UnitTestsDbConnection cn;

        #region Setup
        [SetUp]
        public void Setup()
        {
            cn = new UnitTestsDbConnection(new MySqlConnection(TestHelper.GetMySQLConnectionString()));
        }
        #endregion

        public class Author
        {
            public int author_id { get; set; }
            public string name_last { get; set; }
            public string name_first { get; set; }
            public string country { get; set; }
        }

        [Test]
        public void TestConnection()
        {
            var authors = Dapper.SqlMapper.Query<Author>(cn, "SELECT * FROM authors");
            Assert.That(authors.Any());
        }

        [Test]
        public void TestParameters()
        {
            string search = "%as%";
            var authors = cn.QueryBuilder($"SELECT * FROM authors WHERE name_last like {search}").Query<Author>();
            Assert.That(authors.Any());
            Assert.AreEqual(cn.PreviousCommands.Last().CommandText, "SELECT * FROM authors WHERE name_last like @p0");
        }

        [Test]
        public void TestArrays()
        {
            List<string> lastNames = new List<string>()
            {
                "Kafka",
                "de Assis",
            };

            var authors = cn.QueryBuilder($"SELECT * FROM authors WHERE name_last IN {lastNames}").Query<Author>();
            Assert.That(authors.Any());
            Assert.AreEqual(cn.PreviousCommands.Last().CommandText, "SELECT * FROM authors WHERE name_last IN (@parray01,@parray02)");
        }


    }
}
