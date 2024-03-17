using InterpolatedSql.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.Dapper.SqlBuilders.FluentQueryBuilder
{
    /// <summary>
    /// Exactly like <see cref="global::InterpolatedSql.SqlBuilders.FluentQueryBuilder.FluentQueryBuilder{U, RB, R}"/> 
    /// (an injection-safe dynamic SQL builder with a Fluent API that helps to build the query step by step)
    /// but also wraps an underlying IDbConnection, and there are extensions to invoke Dapper methods
    /// </summary>
    public class FluentQueryBuilder : global::InterpolatedSql.SqlBuilders.FluentQueryBuilder.FluentQueryBuilder<IFluentQueryBuilder, SqlBuilder, IDapperSqlCommand>,
        IFluentQueryBuilder,
        IBuildable<IDapperSqlCommand>,
        IDapperSqlBuilder
    {
        #region ctors
        /// <inheritdoc/>
        public FluentQueryBuilder(
            Func<InterpolatedSqlBuilderOptions?, SqlBuilder> combinedBuilderFactory1,
            Func<InterpolatedSqlBuilderOptions?, StringBuilder?, List<InterpolatedSqlParameter>?, SqlBuilder> combinedBuilderFactory2,
            IDbConnection connection) : base(combinedBuilderFactory1, combinedBuilderFactory2, connection)
        {
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
