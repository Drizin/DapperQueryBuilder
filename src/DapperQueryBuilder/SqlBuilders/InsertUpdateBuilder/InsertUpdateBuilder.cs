using System;
using System.Data;

namespace InterpolatedSql.Dapper.SqlBuilders.InsertUpdateBuilder
{
    /// <inheritdoc/>
    public class InsertUpdateBuilder : InsertUpdateBuilder<InsertUpdateBuilder, ISqlBuilder, IDapperSqlCommand>, IInsertUpdateBuilder, IDapperSqlBuilder
    {
        #region ctors
        /// <inheritdoc/>
        public InsertUpdateBuilder(string tableName, IDbConnection connection) : base(tableName, opts => new SqlBuilder(connection, opts), connection)
        {
        }
        #endregion
    }
}
