using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder in a state that is ready to execute Query
    /// </summary>
    public interface ICompleteCommand<T> : ICommandBuilder
    {

        #region Dapper calls (Query<T>, Execute, etc..)

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        IEnumerable<T> Query(IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        T QueryFirst(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        T QueryFirstOrDefault(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        T QuerySingle(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);


        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        int Execute(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        #endregion

        #region Dapper Async calls

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        Task<IEnumerable<T>> QueryAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        Task<T> QueryFirstAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        Task<T> QueryFirstOrDefaultAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        Task<T> QuerySingleAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);


        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        Task<int> ExecuteAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        #endregion


    }
}
