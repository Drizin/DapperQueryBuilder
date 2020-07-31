# Dapper Query Builder

Dapper Query Builder using Fluent API and String Interpolation

By using String Interpolation we can pass parameters to Dapper without risk of SQL Injections.

By using Fluent API we can easily add dynamic filters to queries (or even dynamically select columns and join new tables).

**Sample usage**:

```cs
string connectionString = @"Data Source=LENOVOFLEX5\SQLEXPRESS;
				Initial Catalog=AdventureWorks;
				Integrated Security=True;";
cn = new SqlConnection(connectionString);

var q = cn.QueryBuilder()
	.SelectDistinct("ProductId")
	.SelectDistinct("Name")
	.From("[Production].[Product]")
	.Where($"[ProductId] > {10}")
	.OrderBy("ProductId")
	;
	
var products = q.Query<Product>();	

```

# Collaborate

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate?**  

Please submit a pull-request or if you prefer you can [contact me](http://drizin.io/pages/Contact/) to discuss your idea.


## License
MIT License
