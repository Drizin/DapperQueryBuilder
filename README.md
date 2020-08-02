# Dapper Query Builder

**Dapper Query Builder using Fluent API and String Interpolation**

We all love Dapper and how Dapper is a minimalist library.

This library is a wrapper around Dapper mostly for helping building dynamic SQL queries and commands. It's based on a few fundamentals:

**1. String interpolation instead of manually using DynamicParameters**

By using interpolated strings we can pass parameters to Dapper without having to worry about creating and managing DynamicParameters manually.
You can build your queries with interpolated strings, and this library will automatically "parametrize" your values.

(If you just passed an interpolated string to Dapper, you would have to manually sanitize your inputs [against SQL-injection attacks](https://stackoverflow.com/a/7505842/3606250), 
and on top of that your queries wouldn't benefit from cached execution plan).

So instead of writing like this:
```cs
sql += " AND Name LIKE @productName"; 
dynamicParams.Add("productName", productName);
var products = cn.Query<Product>(sql, dynamicParams);
```

Or like this:
```cs
sql += $" AND Name LIKE {productName.Replace("'", "''")}"; 
var products = cn.Query<Product>(sql); // pray that you sanitized correctly against sql-injection
```

You can just write like this:
```cs
cmd.Append($" AND Name LIKE {productName}"); 
// query and parameters are wrapped inside CommandBuilder or QueryBuilder and passed automatically to Dapper
var products = cmd.Query<Product>(); 

```

**2. Combining Filters**

Like other QueryBuilders you can create your filters dynamically (**with interpolated strings**, which is our mojo and charm), and combine AND/OR filters.
Different from other builders, we don't try to reinvent SQL syntax or create a limited abstraction over SQL language, which is powerful, comprehensive, and vendor-specific.

You can still write your queries on your own, and yet benefit from string interpolation and from dynamically building a list of parametrized filters.

```cs
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
	FROM [Production].[Product]
	/**where**/
	ORDER BY ProductId
	");

q.Where(new Filters()
{
	new Filter($"[ListPrice] >= {minPrice}"),
	new Filter($"[ListPrice] <= {maxPrice}")
});
q.Where(new Filters(Filters.FiltersType.OR)
{
	new Filter($"[Weight] <= {maxWeight}"),
	new Filter($"[Name] LIKE {search}")
});

// Query() will automatically build your SQL query, and will replace your /**where**/ (if any filter was added)
// it will also pass an underlying DynamicParameters object, with all parameters you passed using string interpolation
var products = q.Query<Product>();	

```


**3. Fluent API (Chained-methods)**

For those who like method-chaining guidance, there's a Fluent API which allows you to build queries step-by-step mimicking dynamic SQL concatenation.

```cs
var q = cn.QueryBuilder()
	.Select("ProductId") // you could also use nameof(Product.ProductId) to use "find references" and refactor(rename)
	.Select("Name")
	.Select("ListPrice")
	.Select("Weight")
	.From("[Production].[Product]")
	.Where($"[ListPrice] <= {maxPrice}")
	.Where($"[Weight] <= {maxWeight}")
	.Where($"[Name] LIKE {search}")
	.OrderBy("ProductId");
	
var products = q.Query<Product>();	
```

You would get this query:

```sql
SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE [ListPrice] <= @p0 AND [Weight] <= @p1 AND [Name] LIKE @p2
ORDER BY ProductId
```

# Quickstart / NuGet Package

1) Download [NuGet package Dapper-QueryBuilder](https://www.nuget.org/packages/Dapper-QueryBuilder)

2) Start using like this:

```cs
using DapperQueryBuilder;

// ...
cn = new SqlConnection(connectionString);

var q = cn.QueryBuilder()
	.Select($"ProductId, Name, ListPrice, Weight")
	.From($"[Production].[Product]")
	.Where($"[ListPrice] <= {maxPrice}")
	.Where($"[Weight] <= {maxWeight}")
	.Where($"[Name] LIKE {search}")
	.OrderBy($"ProductId");
var products = q.Query<Product>();
```

# Documentation / More Examples

**Manual Query (Templating) With Type-Safe Dynamic Filters:**

You can still write your queries on your own, and yet benefit from string interpolation and from dynamically building a list of filters.

All filters added to QueryBuilder are automatically added to the underlying DynamicParameters object, 
and when you invoke Query we build the filter statements and replace the `/**where**/` keyword which you used in your templated-query.

```cs
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

You would get this query:

```sql
SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE [ListPrice] <= @p0 AND [Weight] <= @p1 AND [Name] LIKE @p2
ORDER BY ProductId
```

**Passing IN (lists) and building joins dynamically using Fluent API:**

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

**Using Type-Safe Filters without QueryBuilder**
If for any reason you don't want to use our QueryBuilder, you can still use type-safe dynamic filters:

```cs
Dapper.DynamicParameters parms = new Dapper.DynamicParameters();

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

string where = filters.BuildFilters(parms);
// "WHERE ([ListPrice] >= @p0 AND [ListPrice] <= @p1) AND ([Weight] <= @p2 OR [Name] LIKE @p3)"
// parms contains @p0 as minPrice, @p1 as maxPrice, etc..
```

# How was life before this library? :-) 

**Building dynamic filters in Dapper was a little cumbersome / ugly:**
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

**Now with DapperQueryBuilder it's much easier to write queries with dynamic filters:**
```cs
var query = cn.QueryBuilder(@"
	SELECT * FROM [Production].[Product] 
	/**where**/ 
	ORDER BY [ProductId]")
	.Where($"[Name] LIKE {productName}")
	.Where($"[CategoryId] = {categoryId}");

var products = query.Query<Product>();	
```


# Collaborate

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate?**  

Please submit a pull-request or if you prefer you can [create an issue](https://github.com/Drizin/DapperQueryBuilder/issues) or [contact me](http://drizin.io/pages/Contact/) to discuss your idea.


## License
MIT License
