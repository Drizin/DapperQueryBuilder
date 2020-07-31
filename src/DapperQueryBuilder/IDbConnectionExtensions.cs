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
        public static IEmptyQueryBuilder QueryBuilder(this IDbConnection cnn)
        {
            return new QueryBuilder(cnn);
        }


        /// <summary>
        /// Creates a new QueryBuilder over current connection
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="rawSql">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "filters" (without where) (if any filter is defined).
        /// </param>
        /// <returns></returns>
        public static IFromBuilder QueryBuilder(this IDbConnection cnn, string rawSql)
        {
            return new QueryBuilder(cnn, rawSql);
        }
    }
}
