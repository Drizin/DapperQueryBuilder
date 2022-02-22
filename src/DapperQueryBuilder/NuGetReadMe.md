# Dapper Query Builder

**Dapper Query Builder using String Interpolation and Fluent API**

This library is a wrapper around Dapper mostly for helping building dynamic SQL queries and commands. 

## Parameters are passed using String Interpolation (but it's safe against SQL injection!)

By using interpolated strings we can pass parameters directly (embedded in the query) without having to use anonymous objects and without worrying about matching the property names with the SQL parameters. We can just build our queries with regular string interpolation and this library **will automatically "parameterize" our interpolated objects (sql-injection safe)**.

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

## Query and Parameters walk side-by-side

QueryBuilder basically wraps 2 things that should always stay together: the query which you're building, and the parameters which must go together with our query.
This is a simple concept but it allows us to dynamically add new parameterized SQL clauses/conditions in a single statement.

**Let's say you're building a query with a variable number of conditions**:
```cs
var query = cn.QueryBuilder($"SELECT * FROM Product WHERE 1=1");
query += $"AND Name LIKE {productName}"; 
query += $"AND ProductSubcategoryID = {subCategoryId}"; 
var products = query.Query<Product>(); 
```
QueryBuilder will wrap both the Query and the Parameters, so that you can easily append new sql statements (and parameters) easily.  
When you invoke Query, the underlying query and parameters are passed to Dapper.



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


## IN lists

Dapper allows us to use IN lists magically. And it also works with our string interpolation:

```cs
var q = cn.QueryBuilder($@"
	SELECT c.[Name] as [Category], sc.[Name] as [Subcategory], p.[Name], p.[ProductNumber]
	FROM [Product] p
	INNER JOIN [ProductSubcategory] sc ON p.[ProductSubcategoryID]=sc.[ProductSubcategoryID]
	INNER JOIN [ProductCategory] c ON sc.[ProductCategoryID]=c.[ProductCategoryID]");

var categories = new string[] { "Components", "Clothing", "Acessories" };
q += $"WHERE c.[Name] IN {categories}";
```



## Fluent API (Chained-methods)

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



See full documentation [here](https://github.com/Drizin/DapperQueryBuilder/)