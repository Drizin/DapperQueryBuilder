# Dapper Query Builder

**Dapper Query Builder using String Interpolation and Fluent API**

We all love Dapper and how Dapper is a minimalist library.

This library is a wrapper around Dapper mostly for helping building dynamic SQL queries and commands. It's based on 2 fundamentals:

## Fundamental 1: String Interpolation instead of manually using DynamicParameters

By using interpolated strings we can pass parameters to Dapper without having to worry about creating and managing DynamicParameters manually.  
You can build your queries with interpolated strings, and this library will automatically "parametrize" your values.

(If you just passed an interpolated string to Dapper, you would have to manually sanitize your inputs [against SQL-injection attacks](https://stackoverflow.com/a/7505842/3606250), 
and on top of that your queries wouldn't benefit from cached execution plan).

Instead of writing like this:
```cs
var products = cn
    .Query<Product>($@"
    SELECT * FROM [Product]
    WHERE
    [Name] LIKE @productName
    AND [ProductSubcategoryID] = @subCategoryId
    ORDER BY [ProductId]",
    new { productName, subCategoryId });
```

**... you can just write like this:**
```cs
var products = cn
    .QueryBuilder($@"
    SELECT * FROM [Product]
    WHERE
    [Name] LIKE {productName}
    AND [ProductSubcategoryID] = {subCategoryId}
    ORDER BY [ProductId]").Query<Product>;
```
The underlying query will be fully parametrized (`[Name] LIKE @p0 AND [ProductSubcategoryID] = @p1`), without risk of SQL-injection, even though it looks like you're just building dynamic sql.

## Fundamental 2: Query and Parameters walk side-by-side

QueryBuilder basically wraps 2 things that should always stay together: the query which you're building, and the parameters which must go together with your query.  
This is a simple concept but it allows us to add new sql clauses (parametrized) in a single statement.

Let's say you're building a query with a variable number of conditions. **Instead of appending multiple conditions like this**:
```cs
var dynamicParams = new DynamicParameters();
string sql = "SELECT * FROM [Product] WHERE 1=1";
sql += " AND Name LIKE @productName"; 
dynamicParams.Add("productName", productName);
sql += " AND ProductSubcategoryID = @subCategoryId"; 
dynamicParams.Add("subCategoryId", subCategoryId);
var products = cn.Query<Product>(sql, dynamicParams);

// or like this:
string sql = "SELECT * FROM [Product] WHERE 1=1";
sql += $" AND Name LIKE {productName.Replace("'", "''")}"; 
sql += $" AND ProductSubcategoryID = {subCategoryId.Replace("'", "''")}"; 
// here is where you pray that you've correctly sanitized inputs against sql-injection
var products = cn.Query<Product>(sql);
```

**... you can just write like this:**
```cs
var query = cn.QueryBuilder($"SELECT * FROM [Product] WHERE 1=1");
query.Append($"AND Name LIKE {productName}"); 
query.Append($"AND ProductSubcategoryID = {subCategoryId}"); 
var products = query.Query<Product>(); 
```
QueryBuilder will wrap both the Query and the Parameters, so that you can easily append new sql statements (and parameters) easily.  
When you invoke Query, the underlying query and parameters are passed to Dapper.


# Quickstart / NuGet Package

1. Install the [NuGet package Dapper-QueryBuilder](https://www.nuget.org/packages/Dapper-QueryBuilder)
1. Start using like this:
```cs
using DapperQueryBuilder;
// ...

cn = new SqlConnection(connectionString);

var products = cn.QueryBuilder($@"
    SELECT ProductId, Name, ListPrice, Weight
    FROM [Product]
    WHERE [ListPrice] <= {maxPrice}
    AND [Weight] <= {maxWeight}
    AND [Name] LIKE {search}
    ORDER BY ProductId").Query<Product>();
```

Or building dynamic conditions like this:
```cs
using DapperQueryBuilder;
// ...

cn = new SqlConnection(connectionString);

var q = cn.QueryBuilder($@"
    SELECT ProductId, Name, ListPrice, Weight
    FROM [Product]
    WHERE 1=1 ");

q.AppendLine("AND [ListPrice] <= {maxPrice}");
q.AppendLine("AND [Weight] <= {maxWeight}");
q.AppendLine("AND [Name] LIKE {search}");
q.AppendLine("ORDER BY ProductId");

var products = q.Query<Product>();
```

# Full Documentation and Extra features

## Filters as First-class citizen

As shown above, you'll still write plain SQL, which is what we all love about Dapper.  
Since the most common use case for dynamic clauses is adding WHERE parameters, the library offers WHERE filters as a special structure:
- You can add filters to QueryBuilder using .Where() method, those filters are saved internally
- When you send your query to Dapper, QueryBuilder will search for a `/**where**/` statement in your query and will replace with the filters you defined.

So you can still write your queries on your own, and yet benefit from string interpolation (which is our mojo and charm) and from dynamically building a list of filters.

```cs
int maxPrice = 1000;
int maxWeight = 15;
string search = "%Mountain%";

var cn = new SqlConnection(connectionString);

// You can build the query manually and just use QueryBuilder to replace "where" filters (if any)
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM [Product]
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
FROM [Product]
WHERE [ListPrice] <= @p0 AND [Weight] <= @p1 AND [Name] LIKE @p2
ORDER BY ProductId
```
If you don't need the `WHERE` keyword (if you already have other fixed conditions before), you can use `/**filters**/` instead:
```cs
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM [Product]
    WHERE [Price]>{minPrice} /**filters**/
    ORDER BY ProductId
    ");
```

## Combining Filters

QueryBuilder contains an internal property called "Filters" which just keeps track of all conditions you've added using `.Where()` method.  
All those conditions by default are combined with `AND` operator.  

If you want to write more complex filters (combining multiple AND/OR filters) we have a typed structure for that, like other query builders do.
But differently from other builders, we don't try to reinvent SQL syntax or create a limited abstraction over SQL language, which is powerful, comprehensive, and vendor-specific, so you should still write your raw filters as if they were regular strings, and we do the rest (structuring AND/OR filters, and extracting parameters from interpolated strings):

```cs
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM [Product]
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

var products = q.Query<Product>();

// Query() will automatically build your SQL query, and will replace your /**where**/ (if any filter was added)
// "WHERE ([ListPrice] >= @p0 AND [ListPrice] <= @p1) AND ([Weight] <= @p2 OR [Name] LIKE @p3)"
// it will also pass an underlying DynamicParameters object, with all parameters you passed using string interpolation 
// (@p0 as minPrice, @p1 as maxPrice, etc..)
```

## Raw command building

If you don't like the "magic" of replacing `/**where**/` filters, you can do everything on your own.

```cs
// start your basic query
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight FROM [Product] WHERE 1=1");

// append whatever statements you need (.Append instead of .Where!)
q.Append($"AND [ListPrice] <= {maxPrice}");
q.Append($"AND [Weight] <= {maxWeight}");
q.Append($"AND [Name] LIKE {search}");
q.Append($"ORDER BY ProductId");

var products = q.Query<Product>();
```

## varchar vs nvarchar

Dapper has an issue with strings because they are assumed to be unicode strings (nvarchar) by default.  
That works, but does not give the best performance - in some cases you may prefer to explicitly describe if your strings are unicode or ansi (nvarchar or varchar), and also describe their exact sizes.

Instead of using Dapper `DbString` class, you can just pass explicit type for your parameters, like this:

```cs
// start your basic query
string productName = "Mountain%";

var query = cn.QueryBuilder($"SELECT * FROM [Production].[Product] p WHERE [Name] LIKE {productName:nvarchar(20)}");
```

You can use sql types like `varchar(size)`, `nvarchar(size)`, `char(size)`, `nchar(size)`, `varchar(MAX)`, `nvarchar(MAX)`.
(If your database does not use this exact types, Dapper will convert them to your database. We pass DbStrings to Dapper and use the hints above to define if they `IsAnsi` and `IsFixedLength`.


## IN lists

Dapper allows us to use IN lists magically. And it also works with our string interpolation:

```cs
var q = cn.QueryBuilder($@"
    SELECT c.[Name] as [Category], sc.[Name] as [Subcategory], p.[Name], p.[ProductNumber]
    FROM [Product] p
    INNER JOIN [ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]
    INNER JOIN [ProductCategory] c ON sc.[ProductCategoryID]=c.[ProductCategoryID]");

var categories = new string[] { "Components", "Clothing", "Acessories" };
q.Append($"WHERE c.[Name] IN {categories}");
```



## Fluent API (Chained-methods)

For those who like method-chaining guidance (or for those who allow end-users to build their own queries), there's a Fluent API which allows you to build queries step-by-step mimicking dynamic SQL concatenation.  

So, basically, instead of starting with a full query and just appending new filters (`.Where()`), the QueryBuilder will build the whole query for you:

```cs
var q = cn.QueryBuilder()
    .Select($"ProductId")
    .Select($"Name")
    .Select($"ListPrice")
    .Select($"Weight")
    .From($"[Product]")
    .Where($"[ListPrice] <= {maxPrice}")
    .Where($"[Weight] <= {maxWeight}")
    .Where($"[Name] LIKE {search}")
    .OrderBy($"ProductId");
    
var products = q.Query<Product>();
```

You would get this query:

```sql
SELECT ProductId, Name, ListPrice, Weight
FROM [Product]
WHERE [ListPrice] <= @p0 AND [Weight] <= @p1 AND [Name] LIKE @p2
ORDER BY ProductId
```
Or more elaborated:

```cs
var q = cn.QueryBuilder()
    .SelectDistinct($"ProductId, Name, ListPrice, Weight")
    .From("[Product]")
    .Where($"[ListPrice] <= {maxPrice}")
    .Where($"[Weight] <= {maxWeight}")
    .Where($"[Name] LIKE {search}")
    .OrderBy("ProductId");
```

Building joins dynamically using Fluent API:

```cs
var categories = new string[] { "Components", "Clothing", "Acessories" };

var q = cn.QueryBuilder()
    .SelectDistinct($"c.[Name] as [Category], sc.[Name] as [Subcategory], p.[Name], p.[ProductNumber]")
    .From($"[Product] p")
    .From($"INNER JOIN [ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]")
    .From($"INNER JOIN [ProductCategory] c ON sc.[ProductCategoryID]=c.[ProductCategoryID]")
    .Where($"c.[Name] IN {categories}");
```

There are also chained-methods for adding GROUP BY, HAVING, ORDER BY, and paging (OFFSET x ROWS / FETCH NEXT x ROWS ONLY).


## nameof() and raw strings

For those who like strongly typed queries, you can also use `nameof` expression, but you have to define format "raw" such that the string is preserved and it's not converted into a @parameter.

```cs
var q = cn.QueryBuilder($@"
    SELECT
        c.[{nameof(Category.Name):raw}] as [Category], 
        sc.[{nameof(Subcategory.Name):raw}] as [Subcategory], 
        p.[{nameof(Product.Name):raw}], p.[ProductNumber]"
    FROM [Product] p
    INNER JOIN [ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]
    INNER JOIN [ProductCategory] c ON sc.[ProductCategoryID]=c.[ProductCategoryID]");
```

And in case you can use "find references", "rename" (refactor), etc.

## Using Type-Safe Filters without QueryBuilder

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

## Invoking Stored Procedures
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
    SELECT * FROM [Product]"
    {where}
    ORDER BY [ProductId]", parms);

```

**Now with DapperQueryBuilder it's much easier to write queries with dynamic filters:**
```cs
var query = cn.QueryBuilder(@"
    SELECT * FROM [Product] 
    /**where**/ 
    ORDER BY [ProductId]")
    .Where($"[Name] LIKE {productName}")
    .Where($"[CategoryId] = {categoryId}");

var products = query.Query<Product>();
```

# Database Compatibility

QueryBuilder is database agnostic - it should work with any database, because it basically only helps to pass parameters - it does not generate SQL statements (except simple clauses like `WHERE`, `AND`, if you're using `/**where**/`). It was tested with Microsoft SQL Server and with PostgreSQL (using Npgsql driver), and works fine in both.  

If your database driver does not accept "at-parameters" (`@p0`, `@p1`, etc), then you can just modify InterpolatedStatementParser.AutoGeneratedParameterPrefix:

```cs
// Default value is "@p", some database vendors may not accept "@" parameters
InterpolatedStatementParser.AutoGeneratedParameterPrefix = ":p";

string search = "%Dinosaur%";
var cmd = cn.QueryBuilder($"SELECT * FROM film WHERE title like {search}");
// Underlying SQL will be: SELECT * FROM film WHERE title like :p0
```
PS: Npgsql accepts "at-parameters" (`@p0`, `@p1`, etc) and will convert/pass them correctly to PostgreSQL - so you don't need to use this for Npgsql.


# Collaborate

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate?**  

Please submit a pull-request or if you prefer you can [create an issue](https://github.com/Drizin/DapperQueryBuilder/issues) or [contact me](http://drizin.io/pages/Contact/) to discuss your idea.


## License
MIT License
