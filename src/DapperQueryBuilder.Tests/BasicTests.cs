using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DapperQueryBuilder.Tests
{
    public class QueryBuilderTests
    {
        IDbConnection cn;

        #region Setup
        [SetUp]
        public void Setup()
        {
            string connectionString = @"Data Source=LENOVOFLEX5\SQLEXPRESS;
                            Initial Catalog=AdventureWorks;
                            Integrated Security=True;";
            cn = new SqlConnection(connectionString);
        }
        #endregion

        [Test]
        public void Test1()
        {
            int maxPrice = 1000;
            int maxWeight = 15;
            string search = "%Mountain%";

            var q = cn.QueryBuilder()
                .Select("ProductId")
                .Select("Name")
                .Select("ListPrice")
                .Select("Weight")
                .From("[Production].[Product]")
                .Where($"[ListPrice] <= {maxPrice}")
                .Where($"[Weight] <= {maxWeight}")
                .Where($"[Name] LIKE {search}")
                .OrderBy("ProductId")
                ;

            Assert.AreEqual(@"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE [ListPrice] <= @p0 AND [Weight] <= @p1 AND [Name] LIKE @p2
ORDER BY ProductId
", q.Sql);
            Assert.That(q.Parameters.ParameterNames.Contains("p0"));
            Assert.That(q.Parameters.ParameterNames.Contains("p1"));
            Assert.That(q.Parameters.ParameterNames.Contains("p2"));
            Assert.AreEqual(q.Parameters.Get<int>("p0"), maxPrice);
            Assert.AreEqual(q.Parameters.Get<int>("p1"), maxWeight);
            Assert.AreEqual(q.Parameters.Get<string>("p2"), search);

            var products = q.Query<Product>();
            
            Assert.That(products.Any());


            var q2 = cn.QueryBuilder(
@"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
/**where**/
ORDER BY ProductId
");
            q2.Where($"[ListPrice] <= {maxPrice}");
            q2.Where($"[Weight] <= {maxWeight}");
            q2.Where($"[Name] LIKE {search}");

            Assert.AreEqual(q.Sql, q2.Sql);
            Assert.AreEqual(q.Parameters.Get<object>("p0"), q2.Parameters.Get<object>("p0"));


        }
        public class Product
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
        }


    }
}
