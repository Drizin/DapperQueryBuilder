using InterpolatedSql.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace InterpolatedSql.Dapper.SqlBuilders
{
    /// <summary>
    /// Exactly like <see cref="global::InterpolatedSql.SqlBuilders.QueryBuilder{U, RB, R}"/> but also wraps a (required) underlying IDbConnection, 
    /// has a "Filters" property which can track a list of filters which are later combined (by default with AND) and will replace the keyword /**where**/,
    /// provides facades (as extension-methods) to invoke Dapper extensions (see <see cref="IDapperSqlCommandExtensions"/>),
    /// and maps <see cref="IInterpolatedSql.SqlParameters"/> and <see cref="IInterpolatedSql.ExplicitParameters"/>
    /// into Dapper <see cref="global::Dapper.DynamicParameters"/> type.
    /// </summary>
    public abstract class QueryBuilder<U, RB, R> : global::InterpolatedSql.SqlBuilders.QueryBuilder<U, RB, R>, IQueryBuilder<U, RB, R>
        where U : IQueryBuilder<U, RB, R>, ISqlBuilder<U, R>, IBuildable<R>
        where RB : IDapperSqlBuilder, IBuildable<R>
        where R : class, IInterpolatedSql, IDapperSqlCommand
    {
        #region ctors
        /// <inheritdoc/>
        protected QueryBuilder(
            Func<InterpolatedSqlBuilderOptions?, RB> combinedBuilderFactory1,
            Func<InterpolatedSqlBuilderOptions?, StringBuilder?, List<InterpolatedSqlParameter>?, RB> combinedBuilderFactory2,
            IDbConnection connection
            ) : base(combinedBuilderFactory1, combinedBuilderFactory2)
        {
            DbConnection = connection;
            Options.CalculateAutoParameterName = (parameter, pos) => InterpolatedSqlDapperOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }

        /// <inheritdoc/>
        protected QueryBuilder(
            Func<InterpolatedSqlBuilderOptions?, RB> combinedBuilderFactory1,
            Func<InterpolatedSqlBuilderOptions?, StringBuilder?, List<InterpolatedSqlParameter>?, RB> combinedBuilderFactory2,
            IDbConnection connection,
            FormattableString query
            ) : base(combinedBuilderFactory1, combinedBuilderFactory2, query)
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

        #region Legacy compatibility (DapperQueryBuilder)
        public new string Sql => base.Build().Sql;

        public QueryBuilderParameters Parameters => new QueryBuilderParameters(this);
        public class QueryBuilderParameters
        {
            public HashSet<string> ParameterNames;
            protected ParametersDictionary _dapperParameters;
            public QueryBuilderParameters(QueryBuilder<U, RB, R> builder)
            {
                _dapperParameters = builder.Build().DapperParameters;
                ParameterNames = _dapperParameters.ParameterNames;
            }
            public T Get<T>(string parameterName)
            {
                return _dapperParameters.Get<T>(parameterName);
            }
            public SqlParameterInfo this[string parameterName]
            {
                get { return _dapperParameters[parameterName]; }
            }
            public int Count => _dapperParameters.Count;
        }
        #endregion
    }
}
