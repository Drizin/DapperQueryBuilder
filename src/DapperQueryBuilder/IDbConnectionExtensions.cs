using InterpolatedSql;
using System;
using System.Data;
#if NET6_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace DapperQueryBuilder
{
    /// <summary>
    /// Extends IDbConnection to easily build QueryBuilder or FluentQueryBuilder
    /// </summary>
    public static class IDbConnectionExtensions //TODO: all factories here could be delegated to a Factory class, so that we can replace the factory
    {
        #region Fluent Query Builder
        /// <summary>
        /// Creates a new empty FluentQueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        public static IEmptyQueryBuilder FluentQueryBuilder(this IDbConnection cnn)
        {
            return new FluentQueryBuilder(cnn);
        }
        #endregion


        #region QueryBuilder
        /// <summary>
        /// Creates a new QueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "filters" (without where) (if any filter is defined).
        /// </param>
        public static QueryBuilder QueryBuilder(this IDbConnection cnn, FormattableString query)
        {
            return new QueryBuilder(cnn, query);
        }

        /// <summary>
        /// Creates a new empty QueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        public static QueryBuilder QueryBuilder(this IDbConnection cnn)
        {
            return new QueryBuilder(cnn);
        }
        #endregion

        #region SqlBuilder
#if NET6_0_OR_GREATER
        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, ref InterpolatedSqlHandler value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return new SqlBuilder(cnn, value.InterpolatedSqlBuilder);
        }
        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, InterpolatedSqlBuilderOptions options, [InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return new SqlBuilder(cnn, value.InterpolatedSqlBuilder);
        }

#else
        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, FormattableString command)
        {
            return new SqlBuilder(cnn, command);
        }
        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, InterpolatedSqlBuilderOptions options, FormattableString command)
        {
            return new SqlBuilder(cnn, command, options);
        }
#endif

        /// <summary>
        /// Creates a new empty SqlBuilder over current connection
        /// </summary>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, InterpolatedSqlBuilderOptions? options = null)
        {
            return new SqlBuilder(cnn, options);
        }
        #endregion

        #region SqlBuilder (backwards compatibility - legacy extension named CommandBuilder())
#if NET6_0_OR_GREATER
        [Obsolete("Please use new extension SqlBuilder()")]
        public static SqlBuilder CommandBuilder(this IDbConnection cnn, ref InterpolatedSqlHandler value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return new SqlBuilder(cnn, value.InterpolatedSqlBuilder);
        }

        [Obsolete("Please use new extension SqlBuilder()")]
        public static SqlBuilder CommandBuilder(this IDbConnection cnn, InterpolatedSqlBuilderOptions options, [InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler value)
        {
            if (value.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                value.AdjustMultilineString();
            return new SqlBuilder(cnn, value.InterpolatedSqlBuilder);
        }

#else
        [Obsolete("Please use new extension SqlBuilder()")]
        public static SqlBuilder CommandBuilder(this IDbConnection cnn, FormattableString command)
        {
            return new SqlBuilder(cnn, command);
        }

        [Obsolete("Please use new extension SqlBuilder()")]
        public static SqlBuilder CommandBuilder(this IDbConnection cnn, InterpolatedSqlBuilderOptions options, FormattableString command)
        {
            return new SqlBuilder(cnn, command, options);
        }
#endif

        [Obsolete("Please use new extension SqlBuilder()")]
        public static SqlBuilder CommandBuilder(this IDbConnection cnn, InterpolatedSqlBuilderOptions? options = null)
        {
            return new SqlBuilder(cnn, options);
        }
        #endregion

    }
}
