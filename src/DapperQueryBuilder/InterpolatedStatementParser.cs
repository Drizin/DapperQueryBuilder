using Dapper;
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
    [DebuggerDisplay("{Sql} ({_parametersStr,nq})")]
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
        public DynamicParameters Parameters { get; set; }


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

        private static Regex likeQuotesStart = new Regex("\\s+\\bLIKE\\b \\s*'$",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        private static Regex likeQuotesEnd = new Regex("^'",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

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
            Parameters = new DynamicParameters();

            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(format))
                return;
            var matches = _formattableArgumentRegex.Matches(format);
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                // unescape escaped curly braces
                string sql = format.Substring(lastPos, matches[i].Index - lastPos).Replace("{{", "{").Replace("}}", "}");
                lastPos = matches[i].Index + matches[i].Length;

                // If user passes " column LIKE '{variable}' ", we assume that he used single quotes incorrectly as if interpolated string was a sql literal
                if (likeQuotesStart.IsMatch(sql) && likeQuotesEnd.IsMatch(format.Substring(lastPos)))
                {
                    sql = sql.Substring(0, sql.Length - 1); // skil starting quote
                    lastPos++; // skip closing quote
                }

                sb.Append(sql);
                // arguments[i] may not work because same argument can be used multiple times
                int argPos = int.Parse(matches[i].Groups["ArgPos"].Value);
                string argFormat = matches[i].Groups["Format"].Value;
                object arg = arguments[argPos];

                if (arg is string && argFormat == "raw") // example: {nameof(Product.Name):raw}  -> won't be parametrized, we just emit raw string!
                {
                    sb.Append(arg);
                    continue;
                }

                string parmName = "@p" + _autoNamedParametersCount.ToString();
                _autoNamedParametersCount++;
                //var direction = System.Data.ParameterDirection.Input;
                //if (argFormat == "out")
                //    direction = System.Data.ParameterDirection.Output;
                Parameters.Add(parmName, arg);
                sb.Append(parmName);
            }
            string lastPart = format.Substring(lastPos).Replace("{{", "{").Replace("}}", "}");
            sb.Append(lastPart);
            Sql = sb.ToString();
            _parametersStr = string.Join(", ", Parameters.ParameterNames.ToList().Select(n => "@" + n + "='" + Convert.ToString(Parameters.Get<dynamic>(n)) + "'"));
        }
        #endregion

        /// <summary>
        /// Merges parameters from this query/statement into a CommandBuilder. <br />
        /// Checks for name clashes, and will rename parameters (in CommandBuilder) if necessary. <br />
        /// If some parameter is renamed the underlying Sql statement will have the new parameter names replaced by their new names.<br />
        /// This method does NOT append Parser SQL to CommandBuilder SQL (you may want to save this SQL statement elsewhere)
        /// </summary>
        public void MergeParameters(DynamicParameters target)
        {
            //TODO: write a separate class that implements IDynamicParameters
            string newSql = target.MergeParameters(Parameters, Sql);
            if (newSql != null)
            {
                Sql = newSql;
                _parametersStr = string.Join(", ", Parameters.ParameterNames.ToList().Select(n => "@" + n + "='" + Convert.ToString(Parameters.Get<dynamic>(n)) + "'"));
                // filter parameters in Sql were renamed and won't match the previous passed filters - discard original parameters to avoid reusing wrong values
                Parameters = null;
            }
        }

    }
}
