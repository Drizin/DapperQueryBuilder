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
            cn = new SqlConnection(TestHelper.GetMSSQLConnectionString());
        }
        #endregion

        string expected = @"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE [ListPrice] <= @p0 AND [Weight] <= @p1 AND [Name] LIKE @p2
ORDER BY ProductId
";

        public class Product
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
        }

        int maxPrice = 1000;
        int maxWeight = 15;
        string search = "%Mountain%";


        [Test]
        public void TestTemplateAPI()
        {

            var q = cn.QueryBuilder(
$@"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
/**where**/
ORDER BY ProductId
");
            q.Where($"[ListPrice] <= {maxPrice}");
            q.Where($"[Weight] <= {maxWeight}");
            q.Where($"[Name] LIKE {search}");

            Assert.AreEqual(expected, q.Sql);
            Assert.That(q.Parameters.ParameterNames.Contains("p0"));
            Assert.That(q.Parameters.ParameterNames.Contains("p1"));
            Assert.That(q.Parameters.ParameterNames.Contains("p2"));
            Assert.AreEqual(q.Parameters.Get<int>("p0"), maxPrice);
            Assert.AreEqual(q.Parameters.Get<int>("p1"), maxWeight);
            Assert.AreEqual(q.Parameters.Get<string>("p2"), search);

            var products = q.Query<Product>();

            Assert.That(products.Any());
        }

        public class ProductCategories
        {
            public string Category { get; set; }
            public string Subcategory { get; set; }
            public string Name { get; set; }
            public string ProductNumber { get; set; }
        }



        [Test]
        public void TestDetachedFilters()
        {
            int minPrice = 200;
            int maxPrice = 1000;
            int maxWeight = 15;
            string search = "%Mountain%";

            var filters = new Filters(Filters.FiltersType.AND);
            filters.Add(new Filters()
            {
                new Filter($"[ListPrice] >= {minPrice}"),
                new Filter($"[ListPrice] <= {maxPrice}")
            });
            filters.Add(new Filters(Filters.FiltersType.OR)
            {
                new Filter($"[Weight] <= {maxWeight}"),
                new Filter($"[Name] LIKE {search}")
            });

            Dapper.DynamicParameters parms = new Dapper.DynamicParameters();
            string where = filters.BuildFilters(parms);

            Assert.AreEqual(@"WHERE ([ListPrice] >= @p0 AND [ListPrice] <= @p1) AND ([Weight] <= @p2 OR [Name] LIKE @p3)", where);
        }

        [Test]
        public void TestQueryBuilderWithNestedFormattableString()
        {
            int orgId = 123;
            FormattableString innerQuery = $"SELECT Id, Name FROM SomeTable where OrganizationId={orgId}";
            var q = cn.QueryBuilder($"SELECT FROM ({innerQuery}) a join AnotherTable b on a.Id=b.Id where b.OrganizationId={321}");

            Assert.AreEqual("SELECT FROM (SELECT Id, Name FROM SomeTable where OrganizationId=@p0) a join AnotherTable b on a.Id=b.Id where b.OrganizationId=@p1", q.Sql);

            Assert.AreEqual(2, q.Parameters.Count);
            var p0 = q.Parameters["p0"];
            var p1 = q.Parameters["p1"];
            Assert.AreEqual(123, p0.Value);
            Assert.AreEqual(321, p1.Value);
        }
    }
}
