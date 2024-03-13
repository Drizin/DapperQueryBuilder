using InterpolatedSql;
using NUnit.Framework;
using System;
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
        public void TestOperatorOverload()
        {
            string search = "%mountain%";
            var cmd = cn.QueryBuilder()
                + $@"SELECT * FROM [Production].[Product]"
                + $"WHERE [Name] LIKE {search}";
            cmd += $"AND 1=1";
            Assert.AreEqual("SELECT * FROM [Production].[Product] WHERE [Name] LIKE @p0 AND 1=1", cmd.Sql);
        }

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

            Assert.AreEqual(4, parms.ParameterNames.Count());
            Assert.AreEqual(minPrice, parms.Get<int>("p0"));
            Assert.AreEqual(maxPrice, parms.Get<int>("p1"));
            Assert.AreEqual(maxWeight, parms.Get<int>("p2"));
            Assert.AreEqual(search, parms.Get<string>("p3"));
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

        [Test]
        public void TestQueryBuilderWithNestedFormattableString2()
        {
            int orgId = 123;
            FormattableString otherColumns = $"{"111111111"} AS {"ssn":raw}";
            FormattableString innerQuery = $"SELECT Id, Name, {otherColumns} FROM SomeTable where OrganizationId={orgId}";
            var q = cn.QueryBuilder($"SELECT FROM ({innerQuery}) a join AnotherTable b on a.Id=b.Id where b.OrganizationId={321}");

            Assert.AreEqual("SELECT FROM (SELECT Id, Name, @p0 AS ssn FROM SomeTable where OrganizationId=@p1) a join AnotherTable b on a.Id=b.Id where b.OrganizationId=@p2", q.Sql);

            Assert.AreEqual(3, q.Parameters.Count);
            Assert.AreEqual("111111111", q.Parameters["p0"].Value);
            Assert.AreEqual(123, q.Parameters["p1"].Value);
            Assert.AreEqual(321, q.Parameters["p2"].Value);
        }

        [Test]
        public void TestQueryBuilderWithNestedFormattableString3()
        {
            string val1 = "val1";
            string val2 = "val2";
            FormattableString condition = $"col3 = {val2}";

            var q = cn.QueryBuilder($@"SELECT col1, {val1} as col2 FROM Table1 WHERE {condition}");

            Assert.AreEqual("SELECT col1, @p0 as col2 FROM Table1 WHERE col3 = @p1", q.Sql);

            Assert.AreEqual(2, q.Parameters.Count);
            Assert.AreEqual(val1, q.Parameters["p0"].Value);
            Assert.AreEqual(val2, q.Parameters["p1"].Value);
        }

        [Test]
        public void TestMultipleFilterExtensions()
        {
            var storageFolder = "_CALCENGINES";
			var sqlTestNamePattern = "'%~_Test.%' ESCAPE '~'";
            var regularNames = new[] { "Conduent_1_CE" };

            var qb = cn.QueryBuilder(@$"
				SELECT COUNT(*) TotalCount
				FROM [File] f
					INNER JOIN Folder fo ON f.Folder = fo.UID
				WHERE fo.Name = {storageFolder:varchar(200)} AND f.Name NOT LIKE {sqlTestNamePattern:raw} AND f.Deleted != 1 /**filters**/

				SELECT COUNT(*) TotalCount
				FROM [File] f
					INNER JOIN Folder fo ON f.Folder = fo.UID
				WHERE fo.Name = {storageFolder:varchar(200)} AND f.Name NOT LIKE {sqlTestNamePattern:raw} AND f.Deleted != 1 /**filters**/
			");

			var filters = new Filters(Filters.FiltersType.AND)
            {
                new Filter($"f.Name IN {regularNames:varchar(200)}")
            };

            qb.Where(filters);

            Assert.AreEqual(@"
				SELECT COUNT(*) TotalCount
				FROM [File] f
					INNER JOIN Folder fo ON f.Folder = fo.UID
				WHERE fo.Name = @p0 AND f.Name NOT LIKE '%~_Test.%' ESCAPE '~' AND f.Deleted != 1 AND f.Name IN @parray2

				SELECT COUNT(*) TotalCount
				FROM [File] f
					INNER JOIN Folder fo ON f.Folder = fo.UID
				WHERE fo.Name = @p1 AND f.Name NOT LIKE '%~_Test.%' ESCAPE '~' AND f.Deleted != 1 AND f.Name IN @parray3
			", qb.Sql);
        }
    }
}
