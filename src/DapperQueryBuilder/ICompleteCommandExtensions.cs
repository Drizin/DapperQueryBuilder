using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DapperQueryBuilder
{
    /// <summary>
    /// ICompleteCommands are "ready to run" - this is where we extend those commands with Dapper facades
    /// </summary>
    public static class ICompleteCommandExtensions
    {

        #region Dapper (ICompleteQuery.Execute())
        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        public static int Execute(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.Execute(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        public static Task<int> ExecuteAsync(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.ExecuteAsync(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        #endregion

        #region Dapper (ICompleteQuery.ExecuteScalar())
        /// <summary>
        /// Executes the query (using Dapper), returning the first cell returned, as T.
        /// </summary>
        public static Task<T> ExecuteScalarAsync<T>(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.ExecuteScalarAsync<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the first cell returned, as object.
        /// </summary>
        public static Task<object> ExecuteScalarAsync(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.ExecuteScalarAsync(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        #endregion

        #region Dapper (ICompleteQuery.QueryMultipleAsync())
        /// <summary>
        /// Executes the query (using Dapper), returning multiple result sets.
        /// </summary>
        public static Task<SqlMapper.GridReader> QueryMultipleAsync(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryMultipleAsync(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        #endregion

        #region Dapper (ICompleteQuery<T>.Query<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static IEnumerable<T> Query<T>(this ICompleteCommand command, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.Query<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QueryFirst<T>(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirst<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QueryFirstOrDefault<T>(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirstOrDefault<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QuerySingle<T>(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingle<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QuerySingleOrDefault<T>(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingleOrDefault<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.Query() dynamic)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static IEnumerable<dynamic> Query(this ICompleteCommand command, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.Query(sql: command.Sql, param: command.Parameters, transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QueryFirst(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirst(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QueryFirstOrDefault(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirstOrDefault(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QuerySingle(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingle(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QuerySingleOrDefault(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingleOrDefault(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.Query<object>())
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static IEnumerable<object> Query(this ICompleteCommand command, Type type, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.Query(type: type, sql: command.Sql, param: command.Parameters, transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QueryFirst(this ICompleteCommand command, Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirst(type: type, sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QueryFirstOrDefault(this ICompleteCommand command, Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirstOrDefault(type: type, sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QuerySingle(this ICompleteCommand command, Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingle(type: type, sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QuerySingleOrDefault(this ICompleteCommand command, Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingleOrDefault(type: type, sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.QueryAsync<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<IEnumerable<T>> QueryAsync<T>(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryAsync<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QueryFirstAsync<T>(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirstAsync<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QueryFirstOrDefaultAsync<T>(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirstOrDefaultAsync<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QuerySingleAsync<T>(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingleAsync<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QuerySingleOrDefaultAsync<T>(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingleOrDefaultAsync<T>(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.QueryAsync() dynamic)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<IEnumerable<dynamic>> QueryAsync(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryAsync(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QueryFirstAsync(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirstAsync(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QueryFirstOrDefaultAsync(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirstOrDefaultAsync(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QuerySingleAsync(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingleAsync(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QuerySingleOrDefaultAsync(this ICompleteCommand command, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingleOrDefaultAsync(sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (ICompleteQuery<T>.QueryAsync<object>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<IEnumerable<object>> QueryAsync(this ICompleteCommand command, Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryAsync(type: type, sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QueryFirstAsync(this ICompleteCommand command, Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirstAsync(type: type, sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QueryFirstOrDefaultAsync(this ICompleteCommand command, Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QueryFirstOrDefaultAsync(type: type, sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QuerySingleAsync(this ICompleteCommand command, Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingleAsync(type: type, sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QuerySingleOrDefaultAsync(this ICompleteCommand command, Type type, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return command.Connection.QuerySingleOrDefaultAsync(type: type, sql: command.Sql, param: command.Parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

    }
}
