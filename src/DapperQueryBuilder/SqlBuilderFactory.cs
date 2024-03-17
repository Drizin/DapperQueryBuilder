using InterpolatedSql.Dapper.SqlBuilders;
using InterpolatedSql.SqlBuilders;
using System;
using System.Data;
using SqlBuilder = InterpolatedSql.Dapper.SqlBuilders.SqlBuilder;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// Creates <see cref="ISqlBuilder"/>
    /// </summary>
    public class SqlBuilderFactory
    {
        /// <summary>
        /// Creates a new IInterpolatedSqlBuilderBase of type B
        /// </summary>
        public virtual B Create<B>(IDbConnection connection)
            where B : IDapperSqlBuilder
        {
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(IDbConnection) });
            B builder = (B)ctor.Invoke(new object[] { connection });
            return builder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilderBase of type B
        /// </summary>
        public virtual B Create<B>(IDbConnection connection, InterpolatedSqlBuilderOptions options)
            where B : IDapperSqlBuilder
        {
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(IDbConnection), typeof(InterpolatedSqlBuilderOptions) });
            B builder = (B)ctor.Invoke(new object[] { connection, options });
            return builder;
        }

        /// <summary>
        /// Creates the default IInterpolatedSqlBuilder, which by default is SqlBuilder
        /// </summary>
        public virtual SqlBuilder Create(IDbConnection connection, InterpolatedSqlBuilderOptions? options = null)
        {
            SqlBuilder builder = new SqlBuilder(connection, options);
            return builder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilderBase of type B
        /// </summary>
        public virtual B Create<B>(IDbConnection connection, FormattableString command)
            where B : IDapperSqlBuilder
        {
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(IDbConnection), typeof(FormattableString) });
            B builder = (B)ctor.Invoke(new object[] { connection, command });
            return builder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilderBase of type B
        /// </summary>
        public virtual B Create<B>(IDbConnection connection, FormattableString command, InterpolatedSqlBuilderOptions? options = null)
            where B : IDapperSqlBuilder
        {
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(IDbConnection), typeof(FormattableString), typeof(InterpolatedSqlBuilderOptions) });
            B builder = (B)ctor.Invoke(new object[] { connection, command, options });
            return builder;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Creates a new IInterpolatedSqlBuilderBase of type B
        /// </summary>
        public virtual B Create<B>(IDbConnection connection, int literalLength, int formattedCount)
            where B : IDapperSqlBuilder
        {
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(IDbConnection), typeof(int), typeof(int) });
            B builder = (B)ctor.Invoke(new object[] { connection, literalLength, formattedCount });
            return builder;
        }

        /// <summary>
        /// Creates a new IInterpolatedSqlBuilder of type B
        /// </summary>
        public virtual B Create<B>(IDbConnection connection, int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null)
            where B : IDapperSqlBuilder
        {
            var ctor = typeof(B).GetConstructor(new Type[] { typeof(IDbConnection), typeof(int), typeof(int), typeof(InterpolatedSqlBuilderOptions) });
            B builder = (B)ctor.Invoke(new object?[] { connection, literalLength, formattedCount, options });
            return builder;
        }

        /// <summary>
        /// Creates new SqlBuilder
        /// </summary>
        public virtual SqlBuilder Create(IDbConnection connection, int literalLength, int formattedCount, InterpolatedSqlBuilderOptions? options = null)
        {
            SqlBuilder builder = new SqlBuilder(connection, literalLength, formattedCount, options);
            return builder;
        }
#endif


        /// <summary>
        /// Default Factory
        /// </summary>
        public static SqlBuilderFactory Default = new SqlBuilderFactory();
    }
}
