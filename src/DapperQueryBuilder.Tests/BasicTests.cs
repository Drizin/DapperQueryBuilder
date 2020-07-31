using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DapperQueryBuilder.Tests
{
    public class CodegenContextTests
    {
        IDbConnection cn;

        #region Setup
        [SetUp]
        public void Setup()
        {
            string connectionString = @"Data Source=MYWORKSTATION\SQLEXPRESS;
                            Initial Catalog=AdventureWorks;
                            Integrated Security=True;";
            cn = new SqlConnection(connectionString);
        }
        #endregion

        [Test]
        public void Test1()
        {
            var q = cn.QueryBuilder()
                .SelectDistinct("ProductId")
                .SelectDistinct("Name")
                .From("[Production].[Product]")
                .Where($"[ProductId] > {10}")
                .OrderBy("ProductId")
                ;

            Assert.AreEqual(@"SELECT DISTINCT ProductId, Name
FROM [Production].[Product]
WHERE [ProductId] > @p0
ORDER BY ProductId
", q.Sql);
            Assert.That(q.Parameters.ParameterNames.Contains("p0"));
            Assert.AreEqual(q.Parameters.Get<int>("p0"), 10);

            var products = q.Query<Product>();
            
            Assert.That(products.Any());

        }
        public class Product
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
        }


    }
}
