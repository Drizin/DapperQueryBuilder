using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Extends IDbConnection to easily build QueryBuilder or FluentQueryBuilder
    /// </summary>
    public static class IDbConnectionExtensions
    {
        /// <summary>
        /// Creates a new empty FluentQueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        public static IEmptyQueryBuilder FluentQueryBuilder(this IDbConnection cnn)
        {
            return new FluentQueryBuilder(cnn);
        }


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

        /// <summary>
        /// Creates a new CommandBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public static CommandBuilder CommandBuilder(this IDbConnection cnn, FormattableString command)
        {
            return new CommandBuilder(cnn, command);
        }

        /// <summary>
        /// Creates a new empty CommandBuilder over current connection
        /// </summary>
        public static CommandBuilder CommandBuilder(this IDbConnection cnn)
        {
            return new CommandBuilder(cnn);
        }

    }
}
