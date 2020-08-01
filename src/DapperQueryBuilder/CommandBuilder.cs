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
    /// CommandBuilder
    /// </summary>
    [DebuggerDisplay("{Sql} ({_parametersStr})")]
    public class CommandBuilder : ICommandBuilder, ICompleteQuery
    {
        #region Members
        private readonly IDbConnection _cnn;
        private readonly Dapper.DynamicParameters _parameters;
        private string _parametersStr;

        private readonly StringBuilder _command;
        private int _autoNamedParametersCount = 0;
        #endregion

        #region ctors
        /// <summary>
        /// New empty QueryBuilder. Should be constructed using .Select(), .From(), .Where(), etc.
        /// </summary>
        /// <param name="cnn"></param>
        public CommandBuilder(IDbConnection cnn)
        {
            _cnn = cnn;
            _command = new StringBuilder();
            _parameters = new DynamicParameters();
        }

        /// <summary>
        /// New CommandBuilder based on an initial command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="command">SQL command</param>
        public CommandBuilder(IDbConnection cnn, FormattableString command)
        {
            _cnn = cnn;
            var parser = new InterpolatedStatementParser(command);
            _command = new StringBuilder();
            _command.Append(parser.Sql);
            _parameters = parser.Parameters;
            _parametersStr = string.Join(", ", _parameters.ParameterNames.ToList().Select(n => n + "=" + Convert.ToString(Parameters.Get<dynamic>(n))));
        }
        #endregion


        /// <summary>
        /// Adds the properties of an object (like a POCO) to current Command Builder. Does not check for name clashes.
        /// </summary>
        /// <param name="param"></param>
        public void AddDynamicParams(object param)
        {
            _parameters.AddDynamicParams(param);
            _parametersStr = string.Join(", ", _parameters.ParameterNames.ToList().Select(n => n + "=" + Convert.ToString(Parameters.Get<dynamic>(n))));
        }

        /// <summary>
        /// Adds single parameter to current Command Builder. <br />
        /// Checks for name clashes, and will rename parameter if necessary. <br />
        /// If parameter is renamed will also replace sql statement with new names.
        /// </summary>
        protected void AddParameter(string parameterName, object parameterValue, ref string sql)
        {
            string newParameterName = parameterName;
            if (_parameters.ParameterNames.Contains(parameterName) || Regex.IsMatch(parameterName, "p(\\d)*"))
            {
                newParameterName = "p" + _autoNamedParametersCount.ToString();
                sql = sql.Replace("@" + parameterName, "@" + newParameterName);
                _autoNamedParametersCount++;
            }
            _parameters.Add(newParameterName, parameterValue);
            _parametersStr = string.Join(", ", _parameters.ParameterNames.ToList().Select(n => n + "=" + Convert.ToString(Parameters.Get<dynamic>(n))));
        }

        /// <summary>
        /// Adds single parameter to current Command Builder. <br />
        /// </summary>
        public CommandBuilder AddParameter(string parameterName, object parameterValue = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            _parameters.Add(parameterName, parameterValue, dbType, direction, size, precision, scale);
            _parametersStr = string.Join(", ", _parameters.ParameterNames.ToList().Select(n => n + "=" + Convert.ToString(Parameters.Get<dynamic>(n))));
            return this;
        }



        /// <summary>
        /// Appends a statement to the current command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="statement">SQL command</param>
        public CommandBuilder Append(FormattableString statement)
        {
            var parser = new InterpolatedStatementParser(statement);
            string statementSql = parser.Sql;
            foreach (var statementParameterName in parser.Parameters.ParameterNames)
                AddParameter(statementParameterName, parser.Parameters.Get<object>(statementParameterName), ref statementSql);
            _command.Append(statementSql);
            return this;
        }


        /// <summary>
        /// SQL of Command
        /// </summary>
        public virtual string Sql => _command.ToString(); // base CommandBuilder will just have a single variable for the statement and 

        /// <summary>
        /// Parameters of Command
        /// </summary>
        public virtual Dapper.DynamicParameters Parameters => _parameters;

        #region Query<T> - Invoke Dapper

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public IEnumerable<T> Query<T>(IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.Query<T>(Sql, param: _parameters, transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        public int Execute(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.Execute(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        //TODO: IEnumerable<object> Query, IEnumerable<dynamic> Query
        //TODO: Task<IEnumerable<dynamic>> QueryAsync, Task<IEnumerable<T>> QueryAsync<T>, Task<IEnumerable<object>> QueryAsync
        //TODO: QueryFirst, QueryFirstAsync, QueryFirstOrDefault, QueryFirstOrDefaultAsync, QuerySingle, QuerySingleAsync

        #endregion


    }
}
