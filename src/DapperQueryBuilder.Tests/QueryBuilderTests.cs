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
        public void TestFluentAPI()
        {
            int maxPrice = 1000;
            int maxWeight = 15;
            string search = "%Mountain%";

            var q = cn.QueryBuilder()
                .Select($"ProductId")
                .Select($"Name")
                .Select($"ListPrice")
                .Select($"Weight")
                .From($"[Production].[Product]")
                .Where($"[ListPrice] <= {maxPrice}")
                .Where($"[Weight] <= {maxWeight}")
                .Where($"[Name] LIKE {search}")
                .OrderBy($"ProductId")
                ;

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
        public void JoinsTest()
        {
            var categories = new string[] { "Components", "Clothing", "Acessories" };
            var q = cn.QueryBuilder()
                .SelectDistinct($"c.[Name] as [Category], sc.[Name] as [Subcategory], p.[Name], p.[ProductNumber]")
                .From($"[Production].[Product] p")
                .From($"INNER JOIN [Production].[ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]")
                .From($"INNER JOIN [Production].[ProductCategory] c ON sc.[ProductCategoryID]=c.[ProductCategoryID]")
                .Where($"c.[Name] IN {categories}");
            var prods = q.Query<ProductCategories>();
        }


    }
}
