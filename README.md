# Dapper Query Builder

Dapper Query Builder using Fluent API and String Interpolation

By using String Interpolation we can easily pass parameters to Dapper (it's like building interpolated strings, but it's SQL-injection-safe).  

By using Fluent API we can easily add dynamic filters to queries (or even dynamically select columns and join new tables).

# What is the purpose of this library?

**Building dynamic filters in Dapper can get really cumbersome and ugly:**
```cs
var parms = new DynamicParameters();
List<string> filters = new List<string>();

filters.Add("[Name] LIKE @productName"); 
parms.Add("productName", productName);
filters.Add("[CategoryId] = @categoryId"); 
parms.Add("categoryId", categoryId);

string where = (filters.Any() ? " WHERE " + string.Join(" AND ", filters) : "");

var products = cn.Query<Product>($@"
	SELECT * FROM [Production].[Product]"
	{where}
	ORDER BY [ProductId]", parms);

```

**DapperQueryBuilder makes it much easier to write queries with dynamic filters:**
```cs
var query = cn.QueryBuilder(@"
	SELECT * FROM [Production].[Product] 
	/**where**/ 
	ORDER BY [ProductId]")
	.Where($"[Name] LIKE {productName}")
	.Where($"[CategoryId] = {categoryId}");

var products = query.Query<Product>();	
```

All filters added using the Fluent-API (method chaining) will be automatically 
added to DynamicParameters and will replace the `/**where**/` keyword.


# Documentation / Examples

**Manual Query With Type-Safe Dynamic Filters:**

```cs
using DapperQueryBuilder;
// ...

int maxPrice = 1000;
int maxWeight = 15;
string search = "%Mountain%";

var cn = new SqlConnection(connectionString);

// You can build the query manually and just use QueryBuilder to replace "where" filters (if any)
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
	FROM [Production].[Product]
	/**where**/
	ORDER BY ProductId
	");
	
// You just pass the parameters as if it was an interpolated string, 
// and QueryBuilder will automatically convert them to Dapper parameters (injection-safe)
q.Where($"[ListPrice] <= {maxPrice}");
q.Where($"[Weight] <= {maxWeight}");
q.Where($"[Name] LIKE {search}");

// Query() will automatically build your query and replace your /**where**/ (if any filter was added)
var products = q.Query<Product>();	
```

**Use Chained-methods (Fluent API) to build the whole query:**

```cs
// Or you can use chained-methods that help to build dynamic queries
var q2 = cn.QueryBuilder()
	.Select("ProductId") // you could also use nameof(pocoProperty)
	.Select("Name")
	.Select("ListPrice")
	.Select("Weight")
	.From("[Production].[Product]")
	.Where($"[ListPrice] <= {maxPrice}")
	.Where($"[Weight] <= {maxWeight}")
	.Where($"[Name] LIKE {search}")
	.OrderBy("ProductId");
	
var products = q2.Query<Product>();	
```


Both queries above would provide the same result:

```sql
SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE [ListPrice] <= @p0 AND [Weight] <= @p1 AND [Name] LIKE @p2
ORDER BY ProductId
```

**Fluent API to build joins dynamically:**

```cs
var categories = new string[] { "Components", "Clothing", "Acessories" };

var q = cn.QueryBuilder()
	.SelectDistinct("c.[Name] as [Category], sc.[Name] as [Subcategory], p.[Name], p.[ProductNumber]")
	.From("[Production].[Product] p")
	.From("INNER JOIN [Production].[ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]")
	.From("INNER JOIN [Production].[ProductCategory] c ON sc.[ProductCategoryID]=c.[ProductCategoryID]")
	.Where($"c.[Name] IN {categories}");
```

There are also chained-methods for adding GROUP BY, HAVING, ORDER BY, and paging (OFFSET x ROWS / FETCH NEXT x ROWS ONLY).

**Invoking Stored Procedures:**
```cs
// This is basically Dapper, but with a FluentAPI where you can append parameters dynamically.
var q = cn.CommandBuilder($"[HumanResources].[uspUpdateEmployeePersonalInfo]")
	.AddParameter("ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue)
	.AddParameter("ErrorLogID", dbType: DbType.Int32, direction: ParameterDirection.Output)
	.AddParameter("BusinessEntityID", businessEntityID)
	.AddParameter("NationalIDNumber", nationalIDNumber)
	.AddParameter("BirthDate", birthDate)
	.AddParameter("MaritalStatus", maritalStatus)
	.AddParameter("Gender", gender);
	
int affected = q.Execute(commandType: CommandType.StoredProcedure);
int returnValue = q.Parameters.Get<int>("ReturnValue");

```


# Collaborate

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate?**  

Please submit a pull-request or if you prefer you can [create an issue](https://github.com/Drizin/DapperQueryBuilder/issues) or [contact me](http://drizin.io/pages/Contact/) to discuss your idea.


## License
MIT License
