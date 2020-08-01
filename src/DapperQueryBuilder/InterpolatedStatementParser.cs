using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Parses an interpolated-string SQL statement into a injection-safe statement (with parameters as @p0, @p1, etc) and a dictionary of parameter values.
    /// </summary>
    [DebuggerDisplay("{Sql} ({_parametersStr})")]
    public class InterpolatedStatementParser
    {
        #region Members
        /// <summary>
        /// Injection-safe statement, with parameters as @p0, @p1, etc.
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// Dictionary of Dapper parameters
        /// </summary>
        public Dapper.DynamicParameters Parameters { get; set; }


        private string _parametersStr;

        private static Regex _formattableArgumentRegex = new Regex(
              "{(?<ArgPos>\\d*)(:(?<Format>[^}]*))?}",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        private int _autoNamedParametersCount = 0;
        #endregion

        #region ctor
        /// <summary>
        /// Parses an interpolated-string SQL statement into a injection-safe statement (with parameters as @p0, @p1, etc) and a dictionary of parameter values.
        /// </summary>
        /// <param name="query"></param>
        public InterpolatedStatementParser(FormattableString query) : this(query.Format, query.GetArguments())
        {
        }
        private InterpolatedStatementParser(string format, params object[] arguments)
        {
            Parameters = new Dapper.DynamicParameters();

            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(format))
                return;
            var matches = _formattableArgumentRegex.Matches(format);
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                // unescape escaped curly braces
                string literal = format.Substring(lastPos, matches[i].Index - lastPos).Replace("{{", "{").Replace("}}", "}");
                sb.Append(literal);
                // arguments[i] may not work because same argument can be used multiple times
                int argPos = int.Parse(matches[i].Groups["ArgPos"].Value);
                string argFormat = matches[i].Groups["Format"].Value;
                object arg = arguments[argPos];
                string parmName = "@p" + _autoNamedParametersCount.ToString();
                _autoNamedParametersCount++;
                //var direction = System.Data.ParameterDirection.Input;
                //if (argFormat == "out")
                //    direction = System.Data.ParameterDirection.Output;
                Parameters.Add(parmName, arg);
                sb.Append(parmName);

                lastPos = matches[i].Index + matches[i].Length;
            }
            string lastPart = format.Substring(lastPos).Replace("{{", "{").Replace("}}", "}");
            sb.Append(lastPart);
            Sql = sb.ToString();
            _parametersStr = string.Join(", ", Parameters.ParameterNames.ToList().Select(n => n + "=" + Convert.ToString(Parameters.Get<dynamic>(n))));
        }
        #endregion

        /// <summary>
        /// Merges parameters from this query/statement into a CommandBuilder. <br />
        /// Checks for name clashes, and will rename parameters (in CommandBuilder) if necessary. <br />
        /// If some parameter is renamed the return string will be the original Sql statement replaced by new parameter names, else returns null.<br />
        /// This method does NOT append Parser SQL to CommandBuilder SQL (you may want to save this SQL statement elsewhere)
        /// </summary>
        public string MergeParameters(CommandBuilder commandBuilder)
        {
            string newSql = commandBuilder.MergeParameters(this.Parameters, this.Sql);
            //if (newSql != null)
            //    this.Sql = newSql;
            return newSql;
        }

    }
}
