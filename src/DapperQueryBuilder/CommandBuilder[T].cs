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
    public class CommandBuilder<T> : BaseCommandBuilder<CommandBuilder<T>>, ICommandBuilder, ICompleteCommand<T>
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
        public IEnumerable<T> Query(IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.Query<T>(Sql, param: _parameters, transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public T QueryFirst(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirst<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public T QueryFirstOrDefault(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstOrDefault<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public T QuerySingle(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QuerySingle<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (IcompleteQuery<T>.QueryAsync<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public Task<IEnumerable<T>> QueryAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryAsync<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public Task<T> QueryFirstAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstAsync<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public Task<T> QueryFirstOrDefaultAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QueryFirstOrDefaultAsync<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public Task<T> QuerySingleAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.QuerySingleAsync<T>(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

    }
}
