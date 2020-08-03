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
    [DebuggerDisplay("{Sql} ({_parametersStr,nq})")]
    public abstract class BaseCommandBuilder<T> where T : BaseCommandBuilder<T>
    {
        #region Members
        protected readonly IDbConnection _cnn;
        protected readonly DynamicParameters _parameters;
        private string _parametersStr;

        private readonly StringBuilder _command;
        private int _autoNamedParametersCount = 0;
        #endregion

        #region ctors
        /// <summary>
        /// New empty QueryBuilder. Should be constructed using .Select(), .From(), .Where(), etc.
        /// </summary>
        /// <param name="cnn"></param>
        public BaseCommandBuilder(IDbConnection cnn)
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
        public BaseCommandBuilder(IDbConnection cnn, FormattableString command) : this(cnn)
        {
            var parsedStatement = new InterpolatedStatementParser(command);
            parsedStatement.MergeParameters(this.Parameters);
			_command.Append(parsedStatement.Sql);
        }
        #endregion


        /// <summary>
        /// Adds single parameter to current Command Builder. <br />
        /// </summary>
        public T AddParameter(string parameterName, object parameterValue = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            _parameters.Add(parameterName, parameterValue, dbType, direction, size, precision, scale);
            _parametersStr = string.Join(", ", _parameters.ParameterNames.ToList().Select(n => "@" + n + "='" + Convert.ToString(Parameters.Get<dynamic>(n)) + "'"));
            return (T)this;
        }


        /// <summary>
        /// Adds the properties of an object (like a POCO) to current Command Builder. Does not check for name clashes.
        /// </summary>
        /// <param name="param"></param>
        public void AddDynamicParams(object param)
        {
            _parameters.AddDynamicParams(param);
            _parametersStr = string.Join(", ", _parameters.ParameterNames.ToList().Select(n => "@" + n + "='" + Convert.ToString(Parameters.Get<dynamic>(n)) + "'"));
        }

        /// <summary>
        /// Adds single parameter to current Command Builder. <br />
        /// Checks for name clashes, and will rename parameter if necessary. <br />
        /// If parameter is renamed the new name will be returned, else returns null.
        /// </summary>
        protected string MergeParameter(string parameterName, object parameterValue)
        {
            return _parameters.MergeParameter(parameterName, parameterValue);
        }


        /// <summary>
        /// Appends a statement to the current command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="statement">SQL command</param>
        public T Append(FormattableString statement)
        {
            var parsedStatement = new InterpolatedStatementParser(statement);
            parsedStatement.MergeParameters(this.Parameters);
            string sql = parsedStatement.Sql;
            if (!string.IsNullOrWhiteSpace(sql))
            {
                // we assume that a single word will always be rendered in a single statement,
                // so if there is no whitespace (or line break) immediately before this new statement, we add a space
                string currentLine = _command.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).LastOrDefault();
                if (currentLine != null && currentLine.Length > 0 && currentLine.Last() != ' ')
                {
                    _command.Append(" ");
                }
            }
            _command.Append(sql);
            return (T)this;
        }

        /// <summary>
        /// Appends a statement to the current command, but before statement adds a linebreak. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="statement">SQL command</param>
        public T AppendLine(FormattableString statement)
        {
            // instead of appending line AFTER the statement it makes sense to add BEFORE, just to ISOLATE the new line from previous one
            // there's no point in having linebreaks at the end of a query
            _command.AppendLine();

            this.Append(statement);
            return (T)this;
        }


        /// <summary>
        /// SQL of Command
        /// </summary>
        public virtual string Sql => _command.ToString(); // base CommandBuilder will just have a single variable for the statement;

        /// <summary>
        /// Parameters of Command
        /// </summary>
        public virtual DynamicParameters Parameters => _parameters;

        #region Dapper (ICompleteQuery<T>.Execute)
        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        public int Execute(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.Execute(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }

        /// <summary>
        /// Executes the query (using Dapper), returning the number of rows affected.
        /// </summary>
        public Task<int> ExecuteAsync(IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.ExecuteAsync(Sql, param: _parameters, transaction: transaction, commandTimeout: commandTimeout, commandType: commandType);
        }
        #endregion

    }
}
