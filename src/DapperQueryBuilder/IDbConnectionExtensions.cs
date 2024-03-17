using InterpolatedSql.SqlBuilders;
using System;
using System.Data;
using SqlBuilder = InterpolatedSql.Dapper.SqlBuilders.SqlBuilder;
using QueryBuilder = InterpolatedSql.Dapper.SqlBuilders.QueryBuilder;
using InterpolatedSql.Dapper.SqlBuilders;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// Extends IDbConnection to easily build QueryBuilder or SqlBuilder
    /// </summary>
    public static partial class IDbConnectionExtensions
    {
        public static SqlBuilderFactory SqlBuilderFactory { get; set; } = SqlBuilderFactory.Default;

        #region SqlBuilder
        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B over current connection
        /// </summary>
        public static B SqlBuilder<B>(this IDbConnection cnn)
            where B : IDapperSqlBuilder
        {
            return SqlBuilderFactory.Create<B>(cnn);
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B over current connection
        /// </summary>
        /// <param name="command">SQL command</param>
        public static B SqlBuilder<B>(this IDbConnection cnn, ref InterpolatedSqlHandler command)
            where B : IDapperSqlBuilder
        {
            if (command.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                command.AdjustMultilineString();
            return SqlBuilderFactory.Create<B>(cnn, command.InterpolatedSqlBuilder.AsFormattableString());
        }

        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, ref InterpolatedSqlHandler command)
        {
            if (command.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                command.AdjustMultilineString();
            return new SqlBuilder(cnn, command.InterpolatedSqlBuilder.AsFormattableString());
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B over current connection
        /// </summary>
        /// <param name="command">SQL command</param>
        public static B SqlBuilder<B>(this IDbConnection cnn, InterpolatedSqlBuilderOptions options, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler command)
            where B : IDapperSqlBuilder
        {
            if (command.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                command.AdjustMultilineString();
            return SqlBuilderFactory.Create<B>(cnn, command.InterpolatedSqlBuilder.AsFormattableString());
        }

        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, InterpolatedSqlBuilderOptions options, [System.Runtime.CompilerServices.InterpolatedStringHandlerArgument("options")] ref InterpolatedSqlHandler command)
        {
            if (command.InterpolatedSqlBuilder.Options.AutoAdjustMultilineString)
                command.AdjustMultilineString();
            return new SqlBuilder(cnn, command.InterpolatedSqlBuilder.AsFormattableString());
        }

#else
        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B over current connection
        /// </summary>
        /// <param name="command">SQL command</param>
        public static B SqlBuilder<B>(this IDbConnection cnn, FormattableString command)
            where B : IDapperSqlBuilder
        {
            return SqlBuilderFactory.Create<B>(cnn, command);
        }

        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
        /// <param name="command">SQL command</param>
        public static SqlBuilder SqlBuilder(this IDbConnection cnn, FormattableString command)
        {
            return new SqlBuilder(cnn, command);
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B over current connection
        /// </summary>
        /// <param name="command">SQL command</param>
        public static B SqlBuilder<B>(this IDbConnection cnn, InterpolatedSqlBuilderOptions options, FormattableString command)
            where B : IDapperSqlBuilder
        {
            return SqlBuilderFactory.Create<B>(cnn, command, options);
        }

        /// <summary>
        /// Creates a new SqlBuilder over current connection
        /// </summary>
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

        #region QueryBuilder
        /// <summary>
        /// Creates a new QueryBuilder over current connection
        /// </summary>
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
        public static QueryBuilder QueryBuilder(this IDbConnection cnn)
        {
            return new QueryBuilder(cnn);
        }
        #endregion
    }
}
