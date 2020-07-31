using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Parses an interpolated-string query into a string with parameters as @p0, @p1, etc, and a dictionary of parameter values.
    /// </summary>
    public class InterpolatedQueryParser
    {
        #region Members
        public string Sql { get; set; }
        public Dapper.DynamicParameters Parameters { get; set; }

        private static Regex _formattableArgumentRegex = new Regex(
              "{\\d(:(?<Format>[^}]*))?}",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );
        private int _parametersCount = 0;
        #endregion

        #region ctor
        /// <summary>
        /// Parses an interpolated-string query into a string with parameters as @p0, @p1, etc, and a dictionary of parameter values.
        /// </summary>
        /// <param name="query"></param>
        public InterpolatedQueryParser(FormattableString query) : this(query.Format, query.GetArguments())
        {
        }
        private InterpolatedQueryParser(string format, params object[] arguments)
        {
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
                string argPos = matches[i].Value;
                var arg = arguments[int.Parse(argPos.Substring(1, argPos.Length - 2))];
                //string argFormat = matches[i].Groups["Format"].Value;
                if (Parameters == null)
                    Parameters = new Dapper.DynamicParameters();
                string parmName = "@p" + _parametersCount.ToString();
                _parametersCount++;
                Parameters.Add(parmName, arg);
                sb.Append(parmName);

                lastPos = matches[i].Index + matches[i].Length;
            }
            string lastPart = format.Substring(lastPos).Replace("{{", "{").Replace("}}", "}");
            sb.Append(lastPart);
            Sql = sb.ToString();
        }
        #endregion
    }
}
