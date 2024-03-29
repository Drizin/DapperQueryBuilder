﻿using System.Data;
using InterpolatedSql.SqlBuilders.FluentQueryBuilder;
using InterpolatedSql.Dapper.SqlBuilders.FluentQueryBuilder;
using InterpolatedSql.Dapper.SqlBuilders;
using InterpolatedSql.Dapper;
namespace DapperQueryBuilder
{
    /// <summary>
    /// Extends IDbConnection to easily build FluentQueryBuilder
    /// </summary>
    public static partial class IDbConnectionExtensions
    {
        /// <summary>
        /// Creates a new empty FluentQueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        public static IEmptyQueryBuilder<
            InterpolatedSql.Dapper.SqlBuilders.FluentQueryBuilder.IFluentQueryBuilder,
            SqlBuilder, 
            IDapperSqlCommand
            > FluentQueryBuilder(this IDbConnection cnn)
        {
            return new FluentQueryBuilder((options) => new SqlBuilder(cnn, options), (opts, format, arguments) => new SqlBuilder(cnn, opts, format, arguments), cnn);
        }
    }
}
