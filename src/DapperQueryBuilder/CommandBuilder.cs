using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DapperQueryBuilder
{
    /// <summary>
    /// CommandBuilder
    /// </summary>
    public class CommandBuilder : BaseCommandBuilder<CommandBuilder>, ICommandBuilder, ICompleteCommand
    {
        #region ctors
        /// <summary>
        /// New empty QueryBuilder. Should be constructed using .Select(), .From(), .Where(), etc.
        /// </summary>
        /// <param name="cnn"></param>
        public CommandBuilder(IDbConnection cnn) : base(cnn)
        {
        }

        /// <summary>
        /// New CommandBuilder based on an initial command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public CommandBuilder(IDbConnection cnn, FormattableString command) : base(cnn, command)
        {
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.Query<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public IEnumerable<T> Query<T>(IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.Query<T>(Sql, param: _parameters, transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public T QueryFirst<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirst<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public T QueryFirstOrDefault<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstOrDefault<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public T QuerySingle<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QuerySingle<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.Query() dynamic)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public IEnumerable<dynamic> Query(IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.Query(Sql, param: _parameters, transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public dynamic QueryFirst(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirst(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public dynamic QueryFirstOrDefault(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstOrDefault(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public dynamic QuerySingle(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QuerySingle(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.Query<object>())
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public IEnumerable<object> Query(Type type, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.Query(type: type, sql: Sql, param: _parameters, transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public object QueryFirst(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirst(type: type, sql: Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public object QueryFirstOrDefault(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstOrDefault(type: type, sql: Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public object QuerySingle(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QuerySingle(type: type, sql: Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.QueryAsync<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public Task<IEnumerable<T>> QueryAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryAsync<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public Task<T> QueryFirstAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstAsync<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public Task<T> QueryFirstOrDefaultAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstOrDefaultAsync<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public Task<T> QuerySingleAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QuerySingleAsync<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.QueryAsync() dynamic)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public Task<IEnumerable<dynamic>> QueryAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryAsync(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public Task<dynamic> QueryFirstAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstAsync(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public Task<dynamic> QueryFirstOrDefaultAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstOrDefaultAsync(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public Task<dynamic> QuerySingleAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QuerySingleAsync(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.QueryAsync<object>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public Task<IEnumerable<object>> QueryAsync(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryAsync(type: type, sql: Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public Task<object> QueryFirstAsync(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstAsync(type: type, sql: Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public Task<object> QueryFirstOrDefaultAsync(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstOrDefaultAsync(type: type, sql: Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public Task<object> QuerySingleAsync(Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QuerySingleAsync(type: type, sql: Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

    }
}
