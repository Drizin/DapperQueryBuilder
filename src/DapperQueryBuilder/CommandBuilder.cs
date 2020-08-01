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
        public CommandBuilder(IDbConnection cnn, FormattableString command) : this(cnn)
        {
            var parsedStatement = new InterpolatedStatementParser(command);
            string sql = parsedStatement.MergeParameters(this) ?? parsedStatement.Sql;
            if (!string.IsNullOrEmpty(sql)) // if it's empty command we don't add a linebreak
                _command.AppendLine(sql);
        }
        #endregion


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
        /// If parameter is renamed the new name will be returned, else returns null.
        /// </summary>
        protected string MergeParameter(string parameterName, object parameterValue)
        {
            string newParameterName = parameterName;
            if (_parameters.ParameterNames.Contains(parameterName) || Regex.IsMatch(parameterName, "p(\\d)*"))
            {
                newParameterName = "p" + _autoNamedParametersCount.ToString();
                _autoNamedParametersCount++;
            }
            AddParameter(newParameterName, parameterValue);
            if (newParameterName != parameterName)
                return newParameterName;
            return null;
        }

        /// <summary>
        /// Merges multiple parameters into this CommandBuilder. <br />
        /// Checks for name clashes, and will rename parameters (in the CommandBuilder only) if necessary. <br />
        /// If some parameter is renamed the returned Sql statement will containg the original sql replaced with new names, else (if nothing changed) returns null. <br />
        /// This method does NOT append Parser SQL to CommandBuilder SQL (you may want to save this SQL statement elsewhere)
        /// </summary>
        public string MergeParameters(DynamicParameters parameters, string sql)
        {
            string newSql = sql;
            foreach (var parameterName in parameters.ParameterNames)
            {
                string newParameterName = MergeParameter(parameterName, parameters.Get<object>(parameterName));
                if (newParameterName != null)
                    newSql = newSql.Replace("@" + parameterName, "@" + newParameterName);
            }
            if (newSql != sql)
                return newSql;
            return null;
        }

        ///// <summary>
        ///// Merges parameters from the query/statement into this CommandBuilder. <br />
        ///// Checks for name clashes, and will rename parameters if necessary. <br />
        ///// If some parameter is renamed the Parser Sql statement will also be replaced with new names. <br />
        ///// This method does NOT append Parser SQL to CommandBuilder SQL (you may want to save this SQL statement elsewhere)
        ///// </summary>
        //public void MergeParameters(InterpolatedStatementParser parsedStatement)
        //{
        //    string newSql = MergeParameters(parsedStatement.Parameters, parsedStatement.Sql);
        //    if (newSql != parsedStatement.Sql)
        //        parsedStatement.Sql = newSql;
        //}



        /// <summary>
        /// Appends a statement to the current command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="statement">SQL command</param>
        public CommandBuilder Append(FormattableString statement)
        {
            var parsedStatement = new InterpolatedStatementParser(statement);
            string sql = parsedStatement.MergeParameters(this) ?? parsedStatement.Sql;
            _command.Append(sql);
            return this;
        }

        /// <summary>
        /// Appends a statement to the current command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="statement">SQL command</param>
        public CommandBuilder AppendLine(FormattableString statement)
        {
            this.Append(statement);
            _command.AppendLine();
            return this;
        }


        /// <summary>
        /// SQL of Command
        /// </summary>
        public virtual string Sql => _command.ToString().TrimEnd(); // base CommandBuilder will just have a single variable for the statement; TrimEnd because linebreaks at end may break commands like CommandType.StoreProcedure ("procedureName\n")

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
