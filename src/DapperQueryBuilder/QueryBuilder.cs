using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder
    /// </summary>
    public class QueryBuilder : QueryBuilder<dynamic>
    {
        #region ctors
        /// <summary>
        /// New empty QueryBuilder. Should be constructed using .Select(), .From(), .Where(), etc.
        /// </summary>
        /// <param name="cnn"></param>
        public QueryBuilder(IDbConnection cnn) : base(cnn)
        {
        }

        /// <summary>
        /// New QueryBuilder based on an initial query. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "AND filters" (without where) (if any filter is defined).
        /// </param>
        public QueryBuilder(IDbConnection cnn, FormattableString query) : base(cnn, query)
        {
        }
        #endregion
    }

}
