# Dapper Query Builder

Dapper Query Builder using Fluent API and String Interpolation

By using String Interpolation we can easily pass parameters to Dapper (like building interpolated strings, but SQL-injection-safe)

By using Fluent API we can easily add dynamic filters to queries (or even dynamically select columns and join new tables).

**Sample usage**:

```cs
string connectionString = @"Data Source=LENOVOFLEX5\SQLEXPRESS;
				Initial Catalog=AdventureWorks;
				Integrated Security=True;";
cn = new SqlConnection(connectionString);

int maxPrice = 1000;
int maxWeight = 15;
string search = "%Mountain%";

// You can build the full query and just replace "where" filters (if any)
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

products = q.Query<Product>();	


// Or you can use chained-methods that help to build dynamic queries
var q2 = cn.QueryBuilder()
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
	
var products = q2.Query<Product>();	
```

Both queries above would provide the same result:

```sql
SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE [ListPrice] <= @p0 AND [Weight] <= @p1 AND [Name] LIKE @p2
ORDER BY ProductId
```


# Collaborate

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate?**  

Please submit a pull-request or if you prefer you can [contact me](http://drizin.io/pages/Contact/) to discuss your idea.


## License
MIT License
