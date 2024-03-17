using InterpolatedSql.SqlBuilders;
using System;
using System.Data;

namespace InterpolatedSql.Dapper.SqlBuilders.InsertUpdateBuilder
{
    /// <summary>
    /// Exactly like <see cref="global::InterpolatedSql.SqlBuilders.InsertUpdateBuilder.InsertUpdateBuilder{U, RB, R}"/> but also wraps a (required) underlying IDbConnection, 
    /// has a "Filters" property which can track a list of filters which are later combined (by default with AND) and will replace the keyword /**where**/,
    /// provides facades (as extension-methods) to invoke Dapper extensions (see <see cref="IDapperSqlCommandExtensions"/>),
    /// and maps <see cref="IInterpolatedSql.SqlParameters"/> and <see cref="IInterpolatedSql.ExplicitParameters"/>
    /// into Dapper <see cref="global::Dapper.DynamicParameters"/> type.
    /// </summary>
    public abstract class InsertUpdateBuilder<U, RB, R> : global::InterpolatedSql.SqlBuilders.InsertUpdateBuilder.InsertUpdateBuilder<U, RB, R>, IInsertUpdateBuilder<U, RB, R>
        where U : IInsertUpdateBuilder<U, RB, R>, ISqlBuilder<U, R>, IBuildable<R>
        where RB : IDapperSqlBuilder, IBuildable<R>
        where R : class, IInterpolatedSql, IDapperSqlCommand
    {
        #region ctors
        /// <inheritdoc/>
        protected InsertUpdateBuilder(string tableName, Func<InterpolatedSqlBuilderOptions?, RB> combinedBuilderFactory, IDbConnection connection) : base(tableName, combinedBuilderFactory)
        {
            DbConnection = connection;
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }
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
