using InterpolatedSql.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.Dapper.SqlBuilders
{
    /// <inheritdoc/>
    public class SqlBuilder : SqlBuilder<SqlBuilder, IDapperSqlCommand>, ISqlBuilder, IDapperSqlBuilder
    {
        #region ctors
        /// <inheritdoc />
        protected internal SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments) : base(connection, options, format, arguments)
        {
            DbConnection = connection;
        }

        /// <inheritdoc />
        public SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options = null) : base(connection, options)
        {
            DbConnection = connection;
        }


        /// <inheritdoc />
        public SqlBuilder(IDbConnection connection, FormattableString value, InterpolatedSqlBuilderOptions? options = null) : base(connection, value, options)
        {
            DbConnection = connection;
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public SqlBuilder(IDbConnection connection, int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null) : base(connection, literalLength, formattedCount, options)
        {
            DbConnection = connection;
        }
#endif
        #endregion

        #region Overrides
        /// <inheritdoc />
        public override IDapperSqlCommand Build()
        {
            return this.ToDapperSqlCommand();
        }

        /// <summary>
        /// Like <see cref="InterpolatedSqlBuilderBase.ToSql"/>
        /// </summary>
        /// <returns></returns>
        public IDapperSqlCommand ToDapperSqlCommand()
        {
            string format = _format.ToString();
            return new ImmutableDapperCommand(this.DbConnection, BuildSql(format, _sqlParameters), format, _sqlParameters, _explicitParameters);
        }


        #endregion

    }
}
