using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder in a state that is ready to execute Query
    /// </summary>
    public interface ICompleteQuery : ICommandBuilder
    {
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        IEnumerable<T> Query<T>(IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
    }
}
