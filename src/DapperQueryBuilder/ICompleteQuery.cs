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
    public interface ICompleteQuery : ICommandBuilder
    {

        #region Dapper (ICompleteQuery.Execute())
        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        int Execute(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        Task<int> ExecuteAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        #endregion

        #region Dapper (ICompleteQuery<T>.Query<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        IEnumerable<T> Query<T>(IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        T QueryFirst<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        T QueryFirstOrDefault<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        T QuerySingle<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        #endregion

        #region Dapper (ICompleteQuery<T>.Query() dynamic)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        IEnumerable<dynamic> Query(IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        dynamic QueryFirst(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        dynamic QueryFirstOrDefault(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        dynamic QuerySingle(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        #endregion

        #region Dapper (ICompleteQuery<T>.Query<object>())
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        IEnumerable<object> Query(Type type, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        object QueryFirst(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        object QueryFirstOrDefault(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        object QuerySingle(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        #endregion

        #region Dapper (ICompleteQuery<T>.QueryAsync<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        Task<IEnumerable<T>> QueryAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        Task<T> QueryFirstAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>

        Task<T> QueryFirstOrDefaultAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        Task<T> QuerySingleAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        #endregion

        #region Dapper (ICompleteQuery<T>.QueryAsync() dynamic)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        Task<IEnumerable<dynamic>> QueryAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        Task<dynamic> QueryFirstAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        Task<dynamic> QueryFirstOrDefaultAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        Task<dynamic> QuerySingleAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        #endregion

        #region Dapper (ICompleteQuery<T>.QueryAsync<object>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        Task<IEnumerable<object>> QueryAsync(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        Task<object> QueryFirstAsync(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        Task<object> QueryFirstOrDefaultAsync(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        Task<object> QuerySingleAsync(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null);
        #endregion


    }
}
