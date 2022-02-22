# Dapper Query Builder

**Dapper Query Builder using String Interpolation and Fluent API**

We all love Dapper and how Dapper is a minimalist library.

This library is a tiny wrapper around Dapper to help manual building of dynamic SQL queries and commands. It's based on 2 fundamentals:

## Fundamental 1: Parameters are passed using String Interpolation (but it's safe against SQL injection!)

By using interpolated strings we can pass parameters directly (embedded in the query) without having to use anonymous objects and without worrying about matching the property names with the SQL parameters. We can just build our queries with regular string interpolation and this library **will automatically "parameterize" our interpolated objects (sql-injection safe)**.

With plain Dapper we would write a parameterized query like this:

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
Note that the the names of SQL parameter (`@productName` and `@subCategoryId`) must match the anonymous object (`new { productName, subCategoryId }`).

**With Dapper Query Builder we can just embed variables inside the query:**
```cs
var products = cn
    .QueryBuilder($@"
    SELECT * FROM Product
    WHERE
    Name LIKE {productName}
    AND ProductSubcategoryID = {subCategoryId}
    ORDER BY ProductId").Query<Product>;
```
When `.Query<T>()` is invoked `QueryBuilder` will basically invoke Dapper equivalent method (`Query<T>()`) and pass a fully parameterized query (without risk of SQL-injection) even though it looks like you're just building dynamic sql. Dapper would receive a statement like this:

```cs
var products = cn
    .Query<Product>($@"
    SELECT * FROM Product
    WHERE
    Name LIKE @p0
    AND ProductSubcategoryID = @p1
    ORDER BY ProductId",
    new { p0 = productName, p1 = subCategoryId }); 
```

... but without the risk of having name mismatches or even missing to pass some parameters.


## Fundamental 2: Query and Parameters walk side-by-side

QueryBuilder basically wraps 2 things that should always stay together: the query which you're building, and the parameters which must go together with our query.
This is a simple concept but it allows us to dynamically add new parameterized SQL clauses/conditions in a single statement.

This is how we would build a query with a variable number of conditions using plain Dapper:

```cs
var dynamicParams = new DynamicParameters();
string sql = "SELECT * FROM Product WHERE 1=1";
sql += " AND Name LIKE @productName"; 
dynamicParams.Add("productName", productName);
sql += " AND ProductSubcategoryID = @subCategoryId"; 
dynamicParams.Add("subCategoryId", subCategoryId);
var products = cn.Query<Product>(sql, dynamicParams);
``` 


**With Dapper Query Builder the SQL statement and the associated Parameters are kept together**, making it easy to append dynamic conditions:
```cs
var query = cn.QueryBuilder($"SELECT * FROM Product WHERE 1=1");
query += $"AND Name LIKE {productName}"; 
query += $"AND ProductSubcategoryID = {subCategoryId}"; 
var products = query.Query<Product>(); 
```

Our classes (`QueryBuilder` and `CommandBuilder`) wrap the SQL statement and the associated Parameters, and when we invoke the Query (or run the Command) the underlying statement and parameters are just passed to Dapper. So we don't have to keep statement and parameters separated and we don't have to manually use `DynamicParameters`.



# Quickstart / NuGet Package

1. Install the [NuGet package Dapper-QueryBuilder](https://www.nuget.org/packages/Dapper-QueryBuilder) (don't forget the dash to get the right package!)
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
    WHERE 1=1");

// and dynamically append extra filters
q += $"AND ListPrice <= {maxPrice}";
q += $"AND Weight <= {maxWeight}";
q += $"AND Name LIKE {search}";
q += $"ORDER BY ProductId";

var products = q.Query<Product>();
```

# Full Documentation and Features

## Static Query

```cs
// Create a QueryBuilder with a static query.
// QueryBuilder will automatically convert interpolated parameters to Dapper parameters (injection-safe)
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight FROM Product 
                          WHERE ListPrice <= {maxPrice}";
                          ORDER BY ProductId");

// Query<T>() will automatically pass our query and injection-safe SqlParameters to Dapper
var products = q.Query<Product>();
// all other Dapper extensions are also available: QueryAsync, QueryMultiple, ExecuteScalar, etc..
```

So, basically you pass parameters as interpolated strings, but they are converted to safe SqlParameters.

This is our mojo :-) 

## Dynamic Query

One of the top reasons for dynamically building SQL statements is to dynamically append new filters (`where` statements).  

```cs
// create a QueryBuilder with initial query
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight FROM Product WHERE 1=1");

// Dynamically append whatever statements you need, and QueryBuilder will automatically 
// convert interpolated parameters to Dapper parameters (injection-safe)
q += $"AND ListPrice <= {maxPrice}";
q += $"AND Weight <= {maxWeight}";
q += $"AND Name LIKE {search}";
q += $"ORDER BY ProductId";

var products = q.Query<Product>(); 
```

## Static Command

```cs
var cmd = cn.CommandBuilder($"DELETE FROM Orders WHERE OrderId = {orderId};");
int deletedRows = cmd.Execute();
```

```cs
cn.CommandBuilder($@"
   INSERT INTO Product (ProductName, ProductSubCategoryId)
   VALUES ({productName}, {ProductSubcategoryID})
").Execute();
```


## Command with Multiple statements

In a single roundtrip we can run multiple SQL commands:

```cs
var cmd = cn.CommandBuilder();
cmd += $"DELETE FROM Orders WHERE OrderId = {orderId}; ";
cmd += $"INSERT INTO Logs (Action, UserId, Description) VALUES ({action}, {orderId}, {description}); ";
cmd.Execute();
```


## Dynamic Query with \*\*where\*\* keyword

If you don't like the idea of using `WHERE 1=1` (even though it [doesn't hurt performance](https://dba.stackexchange.com/a/33958/85815)), you can use the special keyword **\*\*where\*\*** that act as a placeholder to render dynamically-defined filters.  

`QueryBuilder` maintains an internal list of filters (property called `Filters`) which keeps track of all filters you've added using `.Where()` method.
Then, when `QueryBuilder` invokes Dapper and sends the underlying query it will search for the keyword `/**where**/` in our query and if it exists it will replace it with the filters added (if any), combined using `AND` statements.


Example: 

```cs
// We can write the query structure and use QueryBuilder to render the "where" filters (if any)
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

// Query() will automatically render your query and replace /**where**/ keyword (if any filter was added)
var products = q.Query<Product>();

// In this case Dapper would get "WHERE ListPrice <= @p0 AND Weight <= @p1 AND Name LIKE @p2" and the associated values
```

When Dapper is invoked we replace the `/**where**/` by `WHERE <filter1> AND <filter2> AND <filter3...>` (if any filter was added).

## Dynamic Query with \*\*filters\*\* keyword

**\*\*filters\*\*** is exactly like **\*\*where\*\***, but it's used if we already have other fixed conditions before:
```cs
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    WHERE Price>{minPrice} /**filters**/
    ORDER BY ProductId
    ");
```

When Dapper is invoked we replace the `/**filters**/` by `AND <filter1> AND <filter2...>` (if any filter was added).


## Writing complex filters (combining AND/OR Filters) and using the Filters class

As explained above, `QueryBuilder` internally contains an instance of `Filters` class, which basically contains a list of filters and a combining operator (default is AND but can be changed to OR).
These filters are defined using `.Where()` and are rendered through the keywords `/**where**/` or `/**filters**/`.

Each filter (inside a parent list of `Filters`) can be a simple condition (using interpolated strings) or it can recursively be another list of filters (`Filters` class), 
and this can be used to write complex combinations of AND/OR conditions (inner filters filters are grouped by enclosing parentheses):

```cs
var q = cn.QueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    /**where**/
    ORDER BY ProductId
    ");

var priceFilters = new Filters(Filters.FiltersType.OR)
{
    new Filter($"ListPrice >= {minPrice}"),
    new Filter($"ListPrice <= {maxPrice}")
};
// Or we could add filters one by one like:  priceFilters.Add($"Weight <= {maxWeight}");

q.Where("Status={status}");
// /**where**/ would be replaced by "Status=@p0"

q.Where(priceFilters);
// /**where**/ would be replaced as "Status=@p0 AND (ListPrice >= @p0 OR ListPrice >= @p1)".
// Note that priceFilters is an inner Filter and it's enclosed with parentheses

// It's also possible to change the combining operator of the outer query or of inner filters:
// q.FiltersType = Filters.FiltersType.OR;
// priceFilters.FiltersType = Filters.FiltersType.AND;
// /**where**/ would be replaced as "Status=@p0 OR (ListPrice >= @p0 AND ListPrice >= @p1)".

var products = q.Query<Product>();
```

To sum, `Filters` class will render whatever conditions you define, conditions can be combined with `AND` or `OR`, and conditions can be defined as inner filters (will use parentheses).
This is all vendor-agnostic (`AND`/`OR`/parentheses are all SQL ANSI) so it should work with any vendor.

## IN lists

Dapper allows us to use IN lists magically. And it also works with our string interpolation:

```cs
var q = cn.QueryBuilder($@"
    SELECT c.Name as Category, sc.Name as Subcategory, p.Name, p.ProductNumber
    FROM Product p
    INNER JOIN ProductSubcategory sc ON p.ProductSubcategoryID=sc.ProductSubcategoryID
    INNER JOIN ProductCategory c ON sc.ProductCategoryID=c.ProductCategoryID");

var categories = new string[] { "Components", "Clothing", "Acessories" };
q += $"WHERE c.Name IN {categories}";
```

## Raw Modifier: Dynamic Column Names

When we want to use regular string interpolation for building up our queries/commands but the interpolated values are not supposed to be converted into SQL parameters we can use the **raw modifier**.

One popular example of the **raw modifier** is when we want to use **dynamic columns**:

```cs
var query = connection.QueryBuilder($@"SELECT * FROM Employee WHERE 1=1");
foreach(var filter in filters)
    query += $" AND {filter.ColumnName:raw} = {filter.Value}";
```

Or:

```cs
var query = connection.QueryBuilder($@"SELECT * FROM Employee /**where**/");
foreach(var filter in filters)
    query.Where($"{filter.ColumnName:raw} = {filter.Value}");
```

Whatever we pass as `:raw` should be either "trusted" or if it's untrusted (user-input) it should be sanitized correctly to avoid SQL-injection issues. (e.g. if `filter.ColumnName` comes from the UI we should validate it or sanitize it against SQL injection).


## Raw Modifier: Dynamic Table Names

Another common use for **raw modifier** is when we're creating a global temporary table and want a unique (random) name:

```cs
string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
string name = "Rick";

cn.QueryBuilder($@"
    CREATE TABLE ##tmpTable{uniqueId:raw}
    (
        Name nvarchar(200)
    );
    INSERT INTO ##tmpTable{uniqueId:raw} (Name) VALUES ({name});
").Execute();
```

Let's emphasize again: strings that you interpolate using `:raw` modifier are not passed as parameters and therefore you should ensure validade it or sanitize it against SQL injection.


## Raw Modifier: nameof
Another example of using the **raw modifier** is when we want to use **nameof expression** (which allows to "find references" for a column, "rename", etc):

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

## Explicitly describing varchar/char data types (to avoid varchar performance issues)

For Dapper (and consequently for us) strings are always are assumed to be unicode strings (nvarchar) by default.  

This causes a [known Dapper issue](https://jithilmt.medium.com/sql-server-hidden-load-evil-performance-issue-with-dapper-465a08f922f6): If the column datatype is varchar the query may not give the best performance and may even ignore existing indexed columns and do a full table scan.  
So for best performance we may want to explicitly describe if our strings are unicode (nvarchar) or ansi (varchar), and also describe their exact sizes.

Dapper's solution is to use the `DbString` class as a wrapper to describe the data type more explicitly, and QueryBuilder can also take this `DbString` in the interpolated values:

```cs
string productName = "Mountain%";

// This is how we declare a varchar(50) in plain Dapper
var productVarcharParm = new DbString { Value = productName, IsFixedLength = true, Length = 50, IsAnsi = true };

// DapperQueryBuilder understands Dapper DbString:
var query = cn.QueryBuilder($@"
    SELECT * FROM Production.Product p 
    WHERE Name LIKE {productVarcharParm}");
```

**But we can also specify the datatype (using the well-established SQL syntax) after the value (`{value:datatype}`):**

```cs
string productName = "Mountain%";

var query = cn.QueryBuilder($@"
    SELECT * FROM Production.Product p 
    WHERE Name LIKE {productName:varchar(50)}");
```

The library will parse the datatype specified after the colon, and it understands sql types like `varchar(size)`, `nvarchar(size)`, `char(size)`, `nchar(size)`, `varchar(MAX)`, `nvarchar(MAX)`.  

`nvarchar` and `nchar` are unicode strings, while `varchar` and `char` are ansi strings.  
`nvarchar` and `varchar` are variable-length strings, while `nchar` and `char` are fixed-length strings.

Don't worry if your database does not use those exact types - we basically convert from those formats back into Dapper `DbString` class (with the appropriate hints `IsAnsi` and `IsFixedLength`), and Dapper will convert that to your database.


## Inner Queries

It's possible to add sql-safe queries inside other queries (e.g. to use as subqueries) as long as you declare them as FormattableString.
This makes it easier to break very complex queries into smaller methods/blocks, or reuse queries as subqueries.
The parameters are fully preserved and safe:

```cs
int orgId = 123;
FormattableString innerQuery = $"SELECT Id, Name FROM SomeTable where OrganizationId={orgId}";
var q = cn.QueryBuilder($"SELECT FROM ({innerQuery}) a join AnotherTable b on a.Id=b.Id where b.OrganizationId={321}");

// q.Sql == "SELECT FROM (SELECT Id, Name FROM SomeTable where OrganizationId=@p0) a join AnotherTable b on a.Id=b.Id where b.OrganizationId=@p1"
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

If you want to use our type-safe dynamic filters but for some reason you don't want to use our QueryBuilder:

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


# FAQ

## Why should we use Parameterized Queries instead of Dynamic SQL?

The whole purpose of Dapper is to safely map our objects to the database (and to map database records back to our objects).  
If you build SQL statements by concatenating parameters as strings it means that:

- It would be more vulnerable to SQL injection.
- You would have to manually sanitize your inputs [against SQL-injection attacks](https://stackoverflow.com/a/7505842)
- You would have to manually convert null values
- Your queries wouldn't benefit from cached execution plan
- You would go crazy by not using Dapper like it was supposed to be used

Building dynamic SQL (**which is a TERRIBLE idea**) would be like this:

```cs 
string sql = "SELECT * FROM Product WHERE Name LIKE " 
   + "'" + productName.Replace("'", "''") + "'"; 
// now you pray that you've correctly sanitized inputs against sql-injection
var products = cn.Query<Product>(sql);
```

With plain Dapper it's safer:
```cs
string sql = "SELECT * FROM Product WHERE Name LIKE @productName";
var products = cn.Query<Product>(sql, new { productName });
``` 


**But with Dapper Query Builder it's even easier**:
```cs
var query = cn.QueryBuilder($"SELECT * FROM Product WHERE Name LIKE {productName}");
var products = query.Query<Product>(); 
```



## Why this library if we could just use interpolated strings directly with plain Dapper?

Dapper does not take interpolated strings, and therefore it doesn't do any kind of manipulation magic on interpolated strings (which is exactly what we do).  
This means that if you pass an interpolated string to Dapper it will be converted as a plain string (**so it would run as dynamic SQL, not as parameterized SQL**), meaning it has **the same issues as dynamic sql** (see previous question).  

So it WOULD be possible (but ugly) to use Dapper with interpolated strings (as long as you sanitize the inputs):

```cs
cn.Execute($@"
   INSERT INTO Product (ProductName, ProductSubCategoryId)
   VALUES ( 
      '{productName.Replace("'", "''")}', 
      {ProductSubcategoryID == null ? "NULL" : int.Parse(ProductSubcategoryID).ToString()}
   )");
// now you pray that you've correctly sanitized inputs against sql-injection
```

But with our library this is not only safer but also much simpler:

```cs
cn.CommandBuilder($@"
   INSERT INTO Product (ProductName, ProductSubCategoryId)
   VALUES ({productName}, {ProductSubcategoryID})
").Execute();
```

In other words, passing interpolated strings to Dapper is dangerous because you may forget to sanitize the inputs.  

Our library makes the use of interpolated strings easier and safer because:
- You don't have to sanitize the parameters (we rely on Dapper parameters)
- You don't have to convert from nulls (we rely on Dapper parameters)
- Our methods will never accept plain strings to avoid programmer mistakes
- If you want to embed in the interpolated statement a regular string a do NOT want it to be converted to a parameter you need to explicitly describe it with the `:raw` modifier

## How can I use Dapper async extensions?

This documentation is mostly using sync methods, but we have [facades](/src/DapperQueryBuilder/ICompleteCommandExtensions.cs) for **all** Dapper extensions, including `ExecuteAsync()`, `QueryAsync<T>`, etc. 

## How can I use Dapper Multi-Mapping?

We do not have facades for invoking Dapper Multi-mapper extensions directly, but you can combine QueryBuilder with Multi-mapper extensions by explicitly passing CommandBuillder `.Sql` and `.Parameters`:

```cs
// DapperQueryBuilder CommandBuilder
var orderAndDetailsQuery = cn
    .QueryBuilder($@"
    SELECT * FROM Orders AS A 
    INNER JOIN OrderDetails AS B ON A.OrderID = B.OrderID
    /**where**/
    ORDER BY A.OrderId, B.OrderDetailId");
// Dynamic Filters
orderAndDetailsQuery.Where($"[ListPrice] <= {maxPrice}");
orderAndDetailsQuery.Where($"[Weight] <= {maxWeight}");
orderAndDetailsQuery.Where($"[Name] LIKE {search}");

// Dapper Multi-mapping extensions
var orderAndDetails = cn.Query<Order, OrderDetail, Order>(
        /* orderAndDetailsQuery.Sql contains [ListPrice] <= @p0 AND [Weight] <= @p1 etc... */
        sql: orderAndDetailsQuery.Sql,
        map: (order, orderDetail) =>
        {
            // whatever..
        },
        /* orderAndDetailsQuery.Parameters contains @p0, @p1 etc... */
        param: orderAndDetailsQuery.Parameters,
        splitOn: "OrderDetailID")
    .Distinct()
    .ToList();
```

To sum, instead of using DapperQueryBuilder `.Query<T>` extensions (which invoke Dapper `IDbConnection.Query<T>`) you just invoke Dapper multimapper `.Query<T1, T2, T3>` directly, and use DapperQueryBuilder only for dynamically building your filters.  

## How does DapperQueryBuilder compares to plain Dapper?

**Building dynamic filters in plain Dapper is a little cumbersome / ugly:**
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

**With DapperQueryBuilder it's much easier to write queries with dynamic filters:**
```cs
var query = cn.QueryBuilder(@"
    SELECT * FROM Product 
    /**where**/ 
    ORDER BY ProductId");

query.Where($"Name LIKE {productName}");
query.Where($"CategoryId = {categoryId}");

// If any filter was added, Query() will automatically replace the /**where**/ keyword
var products = query.Query<Product>();
```

or without `/**where**/`:
```cs
var query = cn.QueryBuilder(@"SELECT * FROM Product WHERE 1=1");
query += $"AND Name LIKE {productName}";
query += $"AND CategoryId = {categoryId}";
query += $"ORDER BY ProductId";
var products = query.Query<Product>();
```


## How does DapperQueryBuilder compares to [Dapper.SqlBuilder](https://github.com/DapperLib/Dapper/tree/main/Dapper.SqlBuilder)?

Dapper.SqlBuilder combines the filters using `/**where**/` keyword (like we do) but requires some auxiliar classes, and filters have to be defined using Dapper syntax (no string interpolation):

```cs
// SqlBuilder and Template are helper classes
var builder = new SqlBuilder();

// We also use this /**where**/ syntax
var template = builder.AddTemplate(@"
    SELECT * FROM Product
    /**where**/
    ORDER BY ProductId");
    
// Define the filters using regular Dapper syntax:
builder.Where("Name LIKE @productName", new { productName });
builder.Where("CategoryId = @categoryId", new { categoryId });

// Use template to explicitly pass the rendered SQL and parameters to Dapper:
var products = cn.Query<Product>(template.RawSql, template.Parameters);
```


## Why don't you have Typed Filters using Lambda Expressions?

We believe that SQL syntax is powerful, comprehensive and vendor-specific. Dapper allows us to use the full SQL syntax (of our database vendor), and so does DapperQueryBuilder.
That's why we decided to focus on our magic (converting interpolated strings into SQL parameters), while keeping Dapper simplicity (you write your own filters).
In other words, we won't try to reinvent SQL syntax or create a limited abstraction over SQL language.


# Collaborate

Please submit a pull-request or if you want to make a sugestion you can [create an issue](https://github.com/Drizin/DapperQueryBuilder/issues) or [contact me](https://rdrizin.com/pages/Contact/).


## License
MIT License
