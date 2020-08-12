using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DapperQueryBuilder
{
    /// <summary>
    /// CommandBuilder wraps an underlying SQL statement and the associated parameters. <br />
    /// Allows to easily add new clauses to underlying statement and also add new parameters.
    /// </summary>
    [DebuggerDisplay("{Sql} ({_parametersStr,nq})")]
    public class CommandBuilder : ICompleteCommand
    {
        #region Members
        private readonly IDbConnection _cnn;
        private readonly ParameterInfos _parameters;
        private string _parametersStr;

        private readonly StringBuilder _command;
        private int _autoNamedParametersCount = 0;
        
        /// <inheritdoc/>
        public IDbConnection Connection { get { return _cnn; } }

        #endregion

        #region statics/constants

        /// <summary>
        /// Identify all types of line-breaks
        /// </summary>
        protected static readonly Regex _lineBreaksRegex = new Regex(@"(\r\n|\n|\r)", RegexOptions.Compiled);

        #endregion

        #region ctors
        /// <summary>
        /// New CommandBuilder. 
        /// </summary>
        /// <param name="cnn"></param>
        public CommandBuilder(IDbConnection cnn)
        {
            _cnn = cnn;
            _command = new StringBuilder();
            _parameters = new ParameterInfos();
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
            parsedStatement.MergeParameters(this.Parameters);
            string sql = AdjustMultilineString(parsedStatement.Sql);
            _command.Append(sql);
        }
        #endregion

        #region Parameters Adding/Merging
        /// <summary>
        /// Adds single parameter to current Command Builder. <br />
        /// </summary>
        public CommandBuilder AddParameter(string parameterName, object parameterValue = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            _parameters.Add(parameterName, parameterValue, dbType, direction, size, precision, scale);
            _parametersStr = string.Join(", ", _parameters.ParameterNames.ToList().Select(n => "@" + n + "='" + Convert.ToString(Parameters.Get<dynamic>(n)) + "'"));
            return this;
        }


        /// <summary>
        /// Adds all public properties of an object (like a POCO) as parameters of the current Command Builder. <br />
        /// This is like Dapper templates: useful when you're passing an object with multiple properties and you'll reference those properties in the SQL statement. <br />
        /// This method does not check for name clashes against previously added parameters. <br />
        /// </summary>
        public void AddObjectProperties(object obj)
        {
            Dictionary<string, PropertyInfo> props = 
                obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(prop => prop.Name, prop => prop);

            foreach (var prop in props)
            {
                _parameters.Add(prop.Key, prop.Value.GetValue(obj, new object[] { }));
            }
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
        #endregion



        /// <summary>
        /// Appends a statement to the current command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="statement">SQL command</param>
        public CommandBuilder Append(FormattableString statement)
        {
            var parsedStatement = new InterpolatedStatementParser(statement);
            parsedStatement.MergeParameters(this.Parameters);
            string sql = AdjustMultilineString(parsedStatement.Sql);
            if (!string.IsNullOrWhiteSpace(sql))
            {
                // we assume that a single word will always be appended in a single statement (why would anyone split a single sql word in 2 appends?!),
                // so if there is no whitespace (or line break) between last text and new text, we add a space betwen them
                string currentLine = _command.ToString().Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).LastOrDefault();
                if (currentLine != null && currentLine.Length > 0 && !char.IsWhiteSpace(currentLine.Last()) && !char.IsWhiteSpace(sql[0]))
                    _command.Append(" ");
            }
            _command.Append(sql);
            return this;
        }

        /// <summary>
        /// Replaces a text by a replacement text<br />
        /// </summary>
        public CommandBuilder Replace(string oldValue, FormattableString newValue)
        {
            var parsedStatement = new InterpolatedStatementParser(newValue);
            parsedStatement.MergeParameters(this.Parameters);
            string sql = AdjustMultilineString(parsedStatement.Sql);
            _command.Replace(oldValue, sql);
            return this;
        }






        #region Multi-line blocks can be conveniently used with any indentation, and we will correctly adjust the indentation of those blocks (TrimLeftPadding and TrimFirstEmptyLine)
        /// <summary>
        /// Given a text block (multiple lines), this removes the left padding of the block, by calculating the minimum number of spaces which happens in EVERY line.
        /// Then, other methods writes the lines one by one, which in case will respect the current indent of the writer.
        /// </summary>
        protected string AdjustMultilineString(string block)
        {
            // copied from https://github.com/Drizin/CodegenCS/

            if (string.IsNullOrEmpty(block))
                return null;
            string[] parts = _lineBreaksRegex.Split(block);
            if (parts.Length <= 1) // no linebreaks at all
                return block;
            var nonEmptyLines = parts.Where(line => line.TrimEnd().Length > 0).ToList();
            if (nonEmptyLines.Count <= 1) // if there's not at least 2 non-empty lines, assume that we don't need to adjust anything
                return block;

            Match m = _lineBreaksRegex.Match(block);
            if (m != null && m.Success && m.Index == 0)
            {
                block = block.Substring(m.Length); // remove first empty line
                parts = _lineBreaksRegex.Split(block);
                nonEmptyLines = parts.Where(line => line.TrimEnd().Length > 0).ToList();
            }


            int minNumberOfSpaces = nonEmptyLines.Select(nonEmptyLine => nonEmptyLine.Length - nonEmptyLine.TrimStart().Length).Min();

            StringBuilder sb = new StringBuilder();

            var matches = _lineBreaksRegex.Matches(block);
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                string line = block.Substring(lastPos, matches[i].Index - lastPos);
                string lineBreak = block.Substring(matches[i].Index, matches[i].Length);
                lastPos = matches[i].Index + matches[i].Length;

                sb.Append(line.Substring(Math.Min(line.Length, minNumberOfSpaces)));
                sb.Append(lineBreak);
            }
            string lastLine = block.Substring(lastPos);
            sb.Append(lastLine.Substring(Math.Min(lastLine.Length, minNumberOfSpaces)));

            return sb.ToString();
        }
        #endregion


        /// <summary>
        /// Appends a statement to the current command, but before statement adds a linebreak. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="statement">SQL command</param>
        public CommandBuilder AppendLine(FormattableString statement)
        {
            // instead of appending line AFTER the statement it makes sense to add BEFORE, just to ISOLATE the new line from previous one
            // there's no point in having linebreaks at the end of a query
            _command.AppendLine();

            this.Append(statement);
            return this;
        }


        /// <summary>
        /// SQL of Command
        /// </summary>
        public virtual string Sql => _command.ToString(); // base CommandBuilder will just have a single variable for the statement;

        /// <summary>
        /// Parameters of Command
        /// </summary>
        public virtual ParameterInfos Parameters => _parameters;

    }
}
