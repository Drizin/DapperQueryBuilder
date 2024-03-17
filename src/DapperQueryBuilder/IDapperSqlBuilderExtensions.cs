using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using InterpolatedSql.Dapper.SqlBuilders;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// IDapperSqlBuilder are "statements ready to run" - this is where we extend the builders with Dapper facades
    /// <see cref="InterpolatedSql.Dapper.SqlBuilders.SqlBuilder"/> is the most generic builder - and is always ready to run.
    /// <see cref="InterpolatedSql.Dapper.SqlBuilders.QueryBuilder"/> is a builder with some helpers to build SELECT queries - and is always ready to run.
    /// <see cref="InterpolatedSql.Dapper.SqlBuilders.FluentQueryBuilder.FluentQueryBuilder"/> is a step-by-step fluent builder - it is ready to run only in some stages of the builder
    /// </summary>
    public static partial class IDapperSqlBuilderExtensions
    {

        #region Dapper (IDapperSqlBuilder.Execute())
        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        public static int Execute(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Execute(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        public static Task<int> ExecuteAsync(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.ExecuteAsync(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        #endregion

        #region Dapper (IDapperSqlBuilder.ExecuteScalar())
        /// <summary>
        /// Executes the query (using Dapper), returning the first cell returned, as object.
        /// </summary>
        public static object ExecuteScalar(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.ExecuteScalar(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the first cell returned, as T.
        /// </summary>
        public static T ExecuteScalar<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.ExecuteScalar<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the first cell returned, as T.
        /// </summary>
        public static Task<T> ExecuteScalarAsync<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.ExecuteScalarAsync<T>(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the first cell returned, as object.
        /// </summary>
        public static Task<object> ExecuteScalarAsync(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.ExecuteScalarAsync(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        #endregion

        #region Dapper (IDapperSqlBuilder.QueryMultiple())
        /// <summary>
        /// Executes the query (using Dapper), returning multiple result sets, and access each in turn.
        /// </summary>
        public static SqlMapper.GridReader QueryMultiple(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            // DynamicParameters because QueryMultiple with Stored Procedures doesn't work with Dictionary<string, object> - see https://github.com/DapperLib/Dapper/issues/1580#issuecomment-889813797
            return command.DbConnection.QueryMultiple(sql: command.Sql, param: ParametersDictionary.LoadFrom(command).DynamicParameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning multiple result sets, and access each in turn.
        /// </summary>
        public static Task<SqlMapper.GridReader> QueryMultipleAsync(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            // DynamicParameters because QueryMultiple with Stored Procedures doesn't work with Dictionary<string, object> - see https://github.com/DapperLib/Dapper/issues/1580#issuecomment-889813797
            return command.DbConnection.QueryMultipleAsync(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command).DynamicParameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        #endregion

        #region Dapper (IDapperSqlBuilder.Query<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static IEnumerable<T> Query<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Query<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QueryFirst<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirst<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QueryFirstOrDefault<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirstOrDefault<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QuerySingle<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingle<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static T QuerySingleOrDefault<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingleOrDefault<T>(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (IDapperSqlBuilder.Query() dynamic)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static IEnumerable<dynamic> Query(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Query(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QueryFirst(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirst(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QueryFirstOrDefault(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirstOrDefault(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QuerySingle(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingle(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static dynamic QuerySingleOrDefault(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingleOrDefault(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (IDapperSqlBuilder.Query<object>())
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static IEnumerable<object> Query(this IDapperSqlBuilder builder, Type type, IDbTransaction? transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Query(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QueryFirst(this IDapperSqlBuilder builder, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirst(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QueryFirstOrDefault(this IDapperSqlBuilder builder, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirstOrDefault(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QuerySingle(this IDapperSqlBuilder builder, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingle(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static object QuerySingleOrDefault(this IDapperSqlBuilder builder, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingleOrDefault(type: type, sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

        #region Dapper (IDapperSqlBuilder.QueryAsync<T>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<IEnumerable<T>> QueryAsync<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryAsync<T>(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QueryFirstAsync<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirstAsync<T>(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QueryFirstOrDefaultAsync<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirstOrDefaultAsync<T>(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QuerySingleAsync<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingleAsync<T>(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public static Task<T> QuerySingleOrDefaultAsync<T>(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingleOrDefaultAsync<T>(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }
        #endregion

        #region Dapper (IDapperSqlBuilder.QueryAsync() dynamic)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<IEnumerable<dynamic>> QueryAsync(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryAsync(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QueryFirstAsync(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirstAsync(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QueryFirstOrDefaultAsync(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirstOrDefaultAsync(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QuerySingleAsync(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingleAsync(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as dynamic objects.
        /// </summary>
        public static Task<dynamic> QuerySingleOrDefaultAsync(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingleOrDefaultAsync(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }
        #endregion

        #region Dapper (IDapperSqlBuilder.QueryAsync<object>)
        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<IEnumerable<object>> QueryAsync(this IDapperSqlBuilder builder, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryAsync(type: type, command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QueryFirstAsync(this IDapperSqlBuilder builder, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirstAsync(type: type, command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QueryFirstOrDefaultAsync(this IDapperSqlBuilder builder, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryFirstOrDefaultAsync(type: type, command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QuerySingleAsync(this IDapperSqlBuilder builder, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingleAsync(type: type, command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as type.
        /// </summary>
        public static Task<object> QuerySingleOrDefaultAsync(this IDapperSqlBuilder builder, Type type, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QuerySingleOrDefaultAsync(type: type, new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }
        #endregion

        #region Dapper (IDapperSqlBuilder.ExecuteReader())
        /// <summary>
        /// Executes the query (using Dapper), returning an System.Data.IDataReader
        /// </summary>
        public static IDataReader ExecuteReader(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.ExecuteReader(sql: command.Sql, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning an System.Data.IDataReader
        /// </summary>
        public static Task<IDataReader> ExecuteReaderAsync(this IDapperSqlBuilder builder, IDbTransaction? transaction = null, int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.ExecuteReaderAsync(new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, cancellationToken: cancellationToken));
        }
        #endregion

        #region Dapper Multi-mapping query
        /// <summary>
        /// Perform a multi-mapping query with 2 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Query<TFirst, TSecond, TReturn>(sql: command.Sql, map: map, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, buffered: buffered, splitOn: splitOn);
        }

        /// <summary>
        /// Perform a multi-mapping query with 3 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TThird, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Query<TFirst, TSecond, TThird, TReturn>(sql: command.Sql, map: map, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, buffered: buffered, splitOn: splitOn);
        }

        /// <summary>
        /// Perform a multi-mapping query with 4 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Query<TFirst, TSecond, TThird, TFourth, TReturn>(sql: command.Sql, map: map, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, buffered: buffered, splitOn: splitOn);
        }

        /// <summary>
        /// Perform a multi-mapping query with 5 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
        /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(sql: command.Sql, map: map, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, buffered: buffered, splitOn: splitOn);
        }

        /// <summary>
        /// Perform a multi-mapping query with 6 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
        /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
        /// <typeparam name="TSixth">The sixth type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(sql: command.Sql, map: map, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, buffered: buffered, splitOn: splitOn);
        }

        /// <summary>
        /// Perform a multi-mapping query with 7 input types. If you need more types -> use Query with Type[] parameter.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
        /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
        /// <typeparam name="TSixth">The sixth type in the recordset.</typeparam>
        /// <typeparam name="TSeventh">The seventh type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(sql: command.Sql, map: map, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, buffered: buffered, splitOn: splitOn);
        }

        /// <summary>
        /// Perform a multi-mapping query with an arbitrary number of input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="types">Array of types in the recordset.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static IEnumerable<TReturn> Query<TReturn>(this IDapperSqlBuilder builder, Type[] types, Func<object[], TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            var command = builder.Build();
            return command.DbConnection.Query<TReturn>(sql: command.Sql, types, map: map, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, buffered: buffered, splitOn: splitOn);
        }

        #endregion

        #region Dapper Multi-mapping query (async)
        /// <summary>
        /// Perform a multi-mapping query with 2 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryAsync(map: map, splitOn: splitOn, command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, flags: buffered ? CommandFlags.Buffered : CommandFlags.None, cancellationToken: cancellationToken));
        }


        /// <summary>
        /// Perform a multi-mapping query with 3 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TThird, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryAsync(map: map, splitOn: splitOn, command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, flags: buffered ? CommandFlags.Buffered : CommandFlags.None, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Perform a multi-mapping query with 4 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryAsync(map: map, splitOn: splitOn, command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, flags: buffered ? CommandFlags.Buffered : CommandFlags.None, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Perform a multi-mapping query with 5 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
        /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryAsync(map: map, splitOn: splitOn, command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, flags: buffered ? CommandFlags.Buffered : CommandFlags.None, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Perform a multi-mapping query with 6 input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
        /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
        /// <typeparam name="TSixth">The sixth type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryAsync(map: map, splitOn: splitOn, command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, flags: buffered ? CommandFlags.Buffered : CommandFlags.None, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Perform a multi-mapping query with 7 input types. If you need more types -> use Query with Type[] parameter.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TFirst">The first type in the recordset.</typeparam>
        /// <typeparam name="TSecond">The second type in the recordset.</typeparam>
        /// <typeparam name="TThird">The third type in the recordset.</typeparam>
        /// <typeparam name="TFourth">The fourth type in the recordset.</typeparam>
        /// <typeparam name="TFifth">The fifth type in the recordset.</typeparam>
        /// <typeparam name="TSixth">The sixth type in the recordset.</typeparam>
        /// <typeparam name="TSeventh">The seventh type in the recordset.</typeparam>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDapperSqlBuilder builder, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            return command.DbConnection.QueryAsync(map: map, splitOn: splitOn, command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, flags: buffered ? CommandFlags.Buffered : CommandFlags.None, cancellationToken: cancellationToken));
        }

        /// <summary>
        /// Perform a multi-mapping query with an arbitrary number of input types.
        /// This returns a single type, combined from the raw types via <paramref name="map"/>.
        /// </summary>
        /// <typeparam name="TReturn">The combined type to return.</typeparam>
        /// <param name="builder">IDapperSqlBuilder used to query the DbConnection.</param>
        /// <param name="types">Array of types in the recordset.</param>
        /// <param name="map">The function to map row types to the return type.</param>
        /// <param name="transaction">The transaction to use for this query.</param>
        /// <param name="buffered">Whether to buffer the results in memory.</param>
        /// <param name="splitOn">The field we should split and read the second object from (default: "Id").</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout.</param>
        /// <param name="commandType">Is it a stored proc or a batch?</param>
        /// <param name="cancellationToken">The cancellation token for this command.</param>
        /// <returns>An enumerable of <typeparamref name="TReturn"/>.</returns>
        public static Task<IEnumerable<TReturn>> QueryAsync<TReturn>(this IDapperSqlBuilder builder, Type[] types, Func<object[], TReturn> map, IDbTransaction? transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null, CancellationToken cancellationToken = default)
        {
            var command = builder.Build();
            // return command.DbConnection.QueryAsync(command: new CommandDefinition(commandText: command.Sql, parameters: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, flags: buffered ? CommandFlags.Buffered : CommandFlags.None, cancellationToken: cancellationToken), types: types, map: map, splitOn: splitOn);
            return command.DbConnection.QueryAsync(sql: command.Sql, types, map: map, param: ParametersDictionary.LoadFrom(command), transaction: transaction, commandTimeout: commandTimeout, commandType: commandType, buffered: buffered, splitOn: splitOn);
        }

        #endregion
    }
}
