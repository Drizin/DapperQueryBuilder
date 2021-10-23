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
    SELECT * FROM Product
    WHERE
    Name LIKE @productName
    AND ProductSubcategoryID = @subCategoryId
    ORDER BY ProductId",
    new { productName, subCategoryId });
```

**... you can just write like this:**
```cs
var products = cn
    .QueryBuilder($@"
    SELECT * FROM Product
    WHERE
    Name LIKE {productName}
    AND ProductSubcategoryID = {subCategoryId}
    ORDER BY ProductId").Query<Product>;
```
The underlying query will be fully parametrized (`Name LIKE @p0 AND ProductSubcategoryID = @p1`), without risk of SQL-injection, even though it looks like you're just building dynamic sql.

## Fundamental 2: Query and Parameters walk side-by-side

QueryBuilder basically wraps 2 things that should always stay together: the query which you're building, and the parameters which must go together with your query.  
This is a simple concept but it allows us to add new sql clauses (parametrized) in a single statement.

Let's say you're building a query with a variable number of conditions. **Instead of appending multiple conditions like this**:
```cs
var dynamicParams = new DynamicParameters();
string sql = "SELECT * FROM Product WHERE 1=1";
sql += " AND Name LIKE @productName"; 
dynamicParams.Add("productName", productName);
sql += " AND ProductSubcategoryID = @subCategoryId"; 
dynamicParams.Add("subCategoryId", subCategoryId);
var products = cn.Query<Product>(sql, dynamicParams);

// or like this:
string sql = "SELECT * FROM Product WHERE 1=1";
sql += $" AND Name LIKE {productName.Replace("'", "''")}"; 
sql += $" AND ProductSubcategoryID = {subCategoryId.Replace("'", "''")}"; 
// here is where you pray that you've correctly sanitized inputs against sql-injection
var products = cn.Query<Product>(sql);
```

**... you can just write like this:**
```cs
var query = cn.QueryBuilder($"SELECT * FROM Product WHERE 1=1");
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

// Build your query with interpolated parameters
// which are automagically converted into safe SqlParameters
var products = cn.QueryBuilder($@"
    SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    WHERE ListPrice <= {maxPrice}
    AND Weight <= {maxWeight}
    AND Name LIKE {search}
    ORDER BY ProductId").Query<Product>();
```

Or building dynamic conditions like this:
```cs
using DapperQueryBuilder;
// ...

cn = new SqlConnection(connectionString);

// Build initial query
var q = cn.QueryBuilder($@"
    SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    WHERE 1=1 ");

// and dynamically append extra filters
q.AppendLine($"AND ListPrice <= {maxPrice}");
q.AppendLine($"AND Weight <= {maxWeight}");
q.AppendLine($"AND Name LIKE {search}");
q.AppendLine($"ORDER BY ProductId");

var products = q.Query<Product>();
```

# Full Documentation and Features

## Manual command building

```cs
// start your basic query
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight FROM Product WHERE 1=1");

// Dynamically append whatever statements you need
// and QueryBuilder will automatically convert interpolated parameters to Dapper parameters (injection-safe)
q.Append($"AND ListPrice <= {maxPrice}");
q.Append($"AND Weight <= {maxWeight}");
q.Append($"AND Name LIKE {search}");
q.Append($"ORDER BY ProductId");

// Query<T>() will automatically pass your query and injection-safe SqlParameters to Dapper
var products = q.Query<Product>(); 
// all other Dapper extensions are also available: QueryAsync, QueryMultiple, ExecuteScalar, etc..
```

So, basically you pass parameters as interpolated strings, but they are converted to safe SqlParameters.

This is our mojo :-) 


## \*\*where\*\* filters 

The **\*\*where\*\*** is a special keyword which acts as a placeholder to render dynamically-defined filters:

- You can append filters to QueryBuilder object using .Where() method, and those filters are saved internally.
- When you send your query to Dapper, QueryBuilder will search for a `/**where**/` statement in your query and will replace with the filters you defined.

To sum, DapperQueryBuilder keeps track of filters in this special structure and during query execution the \*\*where\*\* keyword is replaced with those filters.

```cs
int maxPrice = 1000;
int maxWeight = 15;
string search = "%Mountain%";

var cn = new SqlConnection(connectionString);

// You can build the query manually and just use QueryBuilder to replace "where" filters (if any)
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    /**where**/
    ORDER BY ProductId
    ");
    
// You just pass the parameters as if it was an interpolated string, 
// and QueryBuilder will automatically convert them to Dapper parameters (injection-safe)
q.Where($"ListPrice <= {maxPrice}");
q.Where($"Weight <= {maxWeight}");
q.Where($"Name LIKE {search}");

// Query() will automatically build your query and replace your /**where**/ (if any filter was added)
var products = q.Query<Product>();
```

You would get this query:

```sql
SELECT ProductId, Name, ListPrice, Weight
FROM Product
WHERE ListPrice <= @p0 AND Weight <= @p1 AND Name LIKE @p2
ORDER BY ProductId
```
If you don't need the `WHERE` keyword (if you already have other fixed conditions before), you can use `/**filters**/` instead:
```cs
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    WHERE Price>{minPrice} /**filters**/
    ORDER BY ProductId
    ");
```

## Combining AND/OR Filters

QueryBuilder contains an internal property called "Filters" which just keeps track of all conditions you've added using `.Where()` method.  
By default those conditions are combined using the `AND` operator.  

If you want to write more complex filters (combining multiple AND/OR filters) we have a typed structure for that, like other query builders do.
But differently from other builders, we don't try to reinvent SQL syntax or create a limited abstraction over SQL language, which is powerful, comprehensive, and vendor-specific, so you should still write your raw filters as if they were regular strings, and we do the rest (structuring AND/OR filters, and extracting parameters from interpolated strings):

```cs
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    /**where**/
    ORDER BY ProductId
    ");

q.Where(new Filters()
{
    new Filter($"ListPrice >= {minPrice}"),
    new Filter($"ListPrice <= {maxPrice}")
});
q.Where(new Filters(Filters.FiltersType.OR)
{
    new Filter($"Weight <= {maxWeight}"),
    new Filter($"Name LIKE {search}")
});

var products = q.Query<Product>();

// Query() will automatically build your SQL query, 
// and will replace your /**where**/ (if any filter was added)
// "WHERE (ListPrice >= @p0 AND ListPrice <= @p1) AND (Weight <= @p2 OR Name LIKE @p3)"
// it will also pass an underlying DynamicParameters object, 
// with all parameters you passed using string interpolation 
// (@p0 as minPrice, @p1 as maxPrice, etc..)
```

## Multiple statements in a single command
```cs
// In a single roundtrip we can run multiple SQL commands
var cmd = cn.CommandBuilder();
cmd.Append($"DELETE FROM Orders WHERE OrderId = {orderId}; ");
cmd.Append($"INSERT INTO Logs (Action, UserId, Description) VALUES ({action}, {orderId}, {description}); ");
cmd.Execute();
```


## raw strings

If you want to embed raw strings in your queries (don't want them to be parametrized), you can use the **raw modifier**:

```cs
string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
string name = "Rick";
cn.QueryBuilder($@"
    CREATE TABLE #tmpTable{uniqueId:raw}
    (
        Name nvarchar(200)
    );
    INSERT INTO #tmpTable{uniqueId:raw} (Name) VALUES ({name});
").Execute();
```

One good reason to use the **raw** modifier is when using **nameof expression**, which allows us to "find references" for a column, "rename", etc:

```cs
var q = cn.QueryBuilder($@"
    SELECT
        c.{nameof(Category.Name):raw} as Category, 
        sc.{nameof(Subcategory.Name):raw} as Subcategory, 
        p.{nameof(Product.Name):raw}, p.ProductNumber"
    FROM Product p
    INNER JOIN ProductSubcategory sc ON p.ProductSubcategoryID=sc.ProductSubcategoryID
    INNER JOIN ProductCategory c ON sc.ProductCategoryID=c.ProductCategoryID");
```

## Inner Queries

It's possible to add sql-safe queries inside other queries (e.g. to use as subqueries) as long as you declare them as FormattableString.
This makes it easier to break very complex queries into smaller methods/blocks.
The parameters are fully preserved and safe:

```cs
int orgId = 123;
FormattableString innerQuery = $"SELECT Id, Name FROM SomeTable where OrganizationId={orgId}";
var q = cn.QueryBuilder($"SELECT FROM ({innerQuery}) a join AnotherTable b on a.Id=b.Id where b.OrganizationId={321}");

// q.Sql == "SELECT FROM (SELECT Id, Name FROM SomeTable where OrganizationId=@p0) a join AnotherTable b on a.Id=b.Id where b.OrganizationId=@p1"
```


## varchar vs nvarchar

Dapper has an issue with strings because they are assumed to be unicode strings (nvarchar) by default.  
That works, but does not give the best performance - in some cases you may prefer to explicitly describe if your strings are unicode or ansi (nvarchar or varchar), and also describe their exact sizes.

Instead of using Dapper `DbString` class, you can just pass explicit type for your parameters, like this:

```cs
// start your basic query
string productName = "Mountain%";

var query = cn.QueryBuilder($@"
    SELECT * FROM Production.Product p 
    WHERE Name LIKE {productName:nvarchar(20)}");
```

You can use sql types like `varchar(size)`, `nvarchar(size)`, `char(size)`, `nchar(size)`, `varchar(MAX)`, `nvarchar(MAX)`.
(If your database does not use this exact types, Dapper will convert them to your database. We pass DbStrings to Dapper and use the hints above to define if they `IsAnsi` and `IsFixedLength`.  

`nvarchar` and `nchar` are unicode strings, while `varchar` and `char` are ansi strings.  
`nvarchar` and `varchar` are variable-length strings, while `nchar` and `char` are fixed-length strings.



## IN lists

Dapper allows us to use IN lists magically. And it also works with our string interpolation:

```cs
var q = cn.QueryBuilder($@"
    SELECT c.Name as Category, sc.Name as Subcategory, p.Name, p.ProductNumber
    FROM Product p
    INNER JOIN ProductSubcategory sc ON p.ProductSubcategoryID=sc.ProductSubcategoryID
    INNER JOIN ProductCategory c ON sc.ProductCategoryID=c.ProductCategoryID");

var categories = new string[] { "Components", "Clothing", "Acessories" };
q.Append($"WHERE c.Name IN {categories}");
```



## Fluent API (Chained-methods)

For those who like method-chaining guidance (or for those who allow end-users to build their own queries), there's a Fluent API which allows you to build queries step-by-step mimicking dynamic SQL concatenation.  

So, basically, instead of starting with a full query and just appending new filters (`.Where()`), the FluentQueryBuilder will build the whole query for you:

```cs
var q = cn.FluentQueryBuilder()
    .Select($"ProductId")
    .Select($"Name")
    .Select($"ListPrice")
    .Select($"Weight")
    .From($"Product")
    .Where($"ListPrice <= {maxPrice}")
    .Where($"Weight <= {maxWeight}")
    .Where($"Name LIKE {search}")
    .OrderBy($"ProductId");
    
var products = q.Query<Product>();
```

You would get this query:

```sql
SELECT ProductId, Name, ListPrice, Weight
FROM Product
WHERE ListPrice <= @p0 AND Weight <= @p1 AND Name LIKE @p2
ORDER BY ProductId
```
Or more elaborated:

```cs
var q = cn.FluentQueryBuilder()
    .SelectDistinct($"ProductId, Name, ListPrice, Weight")
    .From("Product")
    .Where($"ListPrice <= {maxPrice}")
    .Where($"Weight <= {maxWeight}")
    .Where($"Name LIKE {search}")
    .OrderBy("ProductId");
```

Building joins dynamically using Fluent API:

```cs
var categories = new string[] { "Components", "Clothing", "Acessories" };

var q = cn.FluentQueryBuilder()
    .SelectDistinct($"c.Name as Category, sc.Name as Subcategory, p.Name, p.ProductNumber")
    .From($"Product p")
    .From($"INNER JOIN ProductSubcategory sc ON p.ProductSubcategoryID=sc.ProductSubcategoryID")
    .From($"INNER JOIN ProductCategory c ON sc.ProductCategoryID=c.ProductCategoryID")
    .Where($"c.Name IN {categories}");
```

There are also chained-methods for adding GROUP BY, HAVING, ORDER BY, and paging (OFFSET x ROWS / FETCH NEXT x ROWS ONLY).



## Using Type-Safe Filters without QueryBuilder

If for any reason you don't want to use our QueryBuilder, you can still use type-safe dynamic filters:

```cs
Dapper.DynamicParameters parms = new Dapper.DynamicParameters();

var filters = new Filters(Filters.FiltersType.AND);
filters.Add(new Filters()
{
    new Filter($"ListPrice >= {minPrice}"),
    new Filter($"ListPrice <= {maxPrice}")
});
filters.Add(new Filters(Filters.FiltersType.OR)
{
    new Filter($"Weight <= {maxWeight}"),
    new Filter($"Name LIKE {search}")
});

string where = filters.BuildFilters(parms);
// "WHERE (ListPrice >= @p0 AND ListPrice <= @p1) AND (Weight <= @p2 OR Name LIKE @p3)"
// parms contains @p0 as minPrice, @p1 as maxPrice, etc..
```

## Invoking Stored Procedures
```cs
// This is basically Dapper, but with a FluentAPI where you can append parameters dynamically.
var q = cn.CommandBuilder($"HumanResources.uspUpdateEmployeePersonalInfo")
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

# Database Support

QueryBuilder is database agnostic (like Dapper) - it should work with all ADO.NET providers (including Microsoft SQL Server, PostgreSQL, MySQL, SQLite, Firebird, SQL CE and Oracle), since it's basically a wrapper around the way parameters are passed to the database provider.  

DapperQueryBuilder doesn't generate SQL statements (except for simple clauses which should work in all databases like `WHERE`/`AND`/`OR` - if you're using `/**where**/` keyword).  


It was tested with **Microsoft SQL Server** and with **PostgreSQL** (using Npgsql driver), and works fine in both.  

## Parameters prefix

By default the parameters are generated using "at-parameters" format (the first parameter is named `@p0`, the next is `@p1`, etc), and that should work with most database providers (including PostgreSQL Npgsql).  
If your provider doesn't accept at-parameters (like Oracle) you can modify `DapperQueryBuilderOptions.DatabaseParameterSymbol`:

```cs
// Default database-parameter-symbol is "@", which mean the underlying query will use @p0, @p1, etc..
// Some database vendors (like Oracle) expect ":" parameters instead of "@" parameters
DapperQueryBuilderOptions.DatabaseParameterSymbol = ":";

OracleConnection cn = new OracleConnection("DATA SOURCE=server;PASSWORD=password;PERSIST SECURITY INFO=True;USER ID=user");

string search = "%Dinosaur%";
var cmd = cn.QueryBuilder($"SELECT * FROM film WHERE title like {search}");
// Underlying SQL will be: SELECT * FROM film WHERE title like :p0
```

If for any reason you don't want parameters to be named `p0`, `p1`, etc, you can change the auto-naming prefix by setting `AutoGeneratedParameterName`:

```cs
DapperQueryBuilderOptions.AutoGeneratedParameterName = "PARAM_";

// your parameters will be named @PARAM_0, @PARAM_1, etc..
```


# How was life before this library? :-) 

**Building dynamic filters in Dapper was a little cumbersome / ugly:**
```cs
var parms = new DynamicParameters();
List<string> filters = new List<string>();

filters.Add("Name LIKE @productName"); 
parms.Add("productName", productName);
filters.Add("CategoryId = @categoryId"); 
parms.Add("categoryId", categoryId);

string where = (filters.Any() ? " WHERE " + string.Join(" AND ", filters) : "");

var products = cn.Query<Product>($@"
    SELECT * FROM Product"
    {where}
    ORDER BY ProductId", parms);

```

**Now with DapperQueryBuilder it's much easier to write queries with dynamic filters:**
```cs
var query = cn.QueryBuilder(@"
    SELECT * FROM Product 
    /**where**/ 
    ORDER BY ProductId");

query.Where($"Name LIKE {productName}");
query.Where($"CategoryId = {categoryId}");

var products = query.Query<Product>();
```



# Collaborate

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate?**  

Please submit a pull-request or if you prefer you can [create an issue](https://github.com/Drizin/DapperQueryBuilder/issues) or [contact me](http://drizin.io/pages/Contact/) to discuss your idea.


## License
MIT License
