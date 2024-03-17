using System;
using System.Data;

namespace InterpolatedSql.Dapper.SqlBuilders
{
    /// <inheritdoc/>
    public class QueryBuilder : QueryBuilder<QueryBuilder, ISqlBuilder, IDapperSqlCommand>, IQueryBuilder, IDapperSqlBuilder
    {
        #region ctors
        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection) : base(opts => new SqlBuilder(connection, opts), (opts, format, arguments) => new SqlBuilder(connection, opts, format, arguments), connection)
        {
        }

        /// <inheritdoc/>
        public QueryBuilder(IDbConnection connection, FormattableString query) : base(opts => new SqlBuilder(connection, opts), (opts, format, arguments) => new SqlBuilder(connection, opts, format, arguments), connection, query)
        {
        }
        #endregion
    }
}
