using InterpolatedSql.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.Dapper.SqlBuilders
{
    /// <summary>
    /// Exactly like <see cref="global::InterpolatedSql.SqlBuilders.SqlBuilder{U, R}"/> but also wraps a (required) underlying IDbConnection, 
    /// has a "Filters" property which can track a list of filters which are later combined (by default with AND) and will replace the keyword /**where**/,
    /// provides facades (as extension-methods) to invoke Dapper extensions (see <see cref="IDapperSqlCommandExtensions"/>),
    /// and maps <see cref="IInterpolatedSql.SqlParameters"/> and <see cref="IInterpolatedSql.ExplicitParameters"/>
    /// into Dapper <see cref="global::Dapper.DynamicParameters"/> type.
    /// </summary>
    public abstract class SqlBuilder<U, R> : global::InterpolatedSql.SqlBuilders.SqlBuilder<U, R>
        where U : ISqlBuilder<U, R>
        where R : class, IInterpolatedSql, IDapperSqlCommand
    {

        #region ctors
        /// <inheritdoc />
        protected SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments) : base(options, format, arguments)
        {
            DbConnection = connection;
        }

        /// <inheritdoc />
        public SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options = null) : base(options)
        {
            DbConnection = connection;
        }


        /// <inheritdoc />
        public SqlBuilder(IDbConnection connection, FormattableString value, InterpolatedSqlBuilderOptions? options = null) : base(value, options)
        {
            DbConnection = connection;
        }

#if NET6_0_OR_GREATER
        /// <inheritdoc />
        public SqlBuilder(IDbConnection connection, int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null) : base(literalLength, formattedCount, options)
        {
            DbConnection = connection;
        }
#endif
        #endregion

        #region Overrides
        /// <summary>
        /// Associated DbConnection
        /// </summary>
        public new IDbConnection DbConnection
        {
            get => base.DbConnection!;
            set => base.DbConnection = value;
        }
        #endregion

    }
}
