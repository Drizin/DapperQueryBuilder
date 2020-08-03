using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperQueryBuilder
{
    public static class IDbConnectionExtensions
    {
        /// <summary>
        /// Creates a new QueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public static IEmptyQueryBuilder<dynamic> QueryBuilder(this IDbConnection cnn)
        {
            return new QueryBuilder(cnn);
        }

        /// <summary>
        /// Creates a new QueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <returns></returns>
        public static IEmptyQueryBuilder<T> QueryBuilder<T>(this IDbConnection cnn)
        {
            return new QueryBuilder<T>(cnn);
        }


        /// <summary>
        /// Creates a new QueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "filters" (without where) (if any filter is defined).
        /// </param>
        /// <returns></returns>
        public static IFromBuilder<dynamic> QueryBuilder(this IDbConnection cnn, FormattableString query)
        {
            return new QueryBuilder(cnn, query);
        }

        /// <summary>
        /// Creates a new QueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "filters" (without where) (if any filter is defined).
        /// </param>
        /// <returns></returns>
        public static IFromBuilder<T> QueryBuilder<T>(this IDbConnection cnn, FormattableString query)
        {
            return new QueryBuilder<T>(cnn, query);
        }

        /// <summary>
        /// Creates a new CommandBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        /// <returns></returns>
        public static CommandBuilder CommandBuilder(this IDbConnection cnn, FormattableString command)
        {
            return new CommandBuilder(cnn, command);
        }

        /// <summary>
        /// Creates a new CommandBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        /// <returns></returns>
        public static CommandBuilder<T> CommandBuilder<T>(this IDbConnection cnn, FormattableString command)
        {
            return new CommandBuilder<T>(cnn, command);
        }

    }
}
