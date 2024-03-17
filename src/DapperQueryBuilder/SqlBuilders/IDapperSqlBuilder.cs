namespace InterpolatedSql.Dapper.SqlBuilders
{
    /// <summary>
    /// Any Builder that creates a <see cref="IDapperSqlCommand"/>
    /// </summary>
    public interface IDapperSqlBuilder : InterpolatedSql.SqlBuilders.IInterpolatedSqlBuilderBase
    {
        /// <summary>
        /// Builds the SQL statement
        /// </summary>
        IDapperSqlCommand Build();
    }
}
