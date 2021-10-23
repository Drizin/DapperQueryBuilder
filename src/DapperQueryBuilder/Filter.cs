using Dapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Filter statement defined in a single statement <br />
    /// It can include multiple conditions (if defined in a single statement during constructor), <br />
    /// but usually this is used as one condition (one column, one comparison operator, and one parameter).
    /// </summary>
    [DebuggerDisplay("{Sql} ({_parametersStr,nq})")]
    public class Filter : IFilter
    {
        #region Members
        /// <summary>
        /// Formatted SQL statement using parameters (@p0, @p1, etc)
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// Dictionary of Dapper parameters
        /// </summary>
        public ParameterInfos Parameters { get; set; }
        
        private string _parametersStr;
        #endregion

        #region ctor
        /// <summary>
        /// New Filter statement. <br />
        /// Example: $"[CategoryId] = {categoryId}" <br />
        /// Example: $"[Name] LIKE {productName}"
        /// </summary>
        public Filter(FormattableString filter)
        {
            var parsedStatement = new InterpolatedStatementParser(filter);
            Sql = parsedStatement.Sql;
            Parameters = parsedStatement.Parameters;
            _parametersStr = string.Join(", ", Parameters.ParameterNames.ToList().Select(n => DapperQueryBuilderOptions.DatabaseParameterSymbol + n + "='" + Convert.ToString(Parameters.Get<dynamic>(n)) + "'"));
        }
        #endregion

        #region IFilter
        /// <inheritdoc/>
        public void WriteFilter(StringBuilder sb)
        {
            sb.Append(Sql);
        }

        /// <inheritdoc/>
        public void MergeParameters(ParameterInfos target)
        {
            string newSql = target.MergeParameters(Parameters, Sql);
            if (newSql != null)
            {
                Sql = newSql;
                //_parametersStr = string.Join(", ", Parameters.ParameterNames.ToList().Select(n => "@" + n + "='" + Convert.ToString(Parameters.Get<dynamic>(n)) + "'"));
                _parametersStr = string.Join(", ", Parameters.ParameterNames.ToList().Select(n => "'" + Convert.ToString(Parameters.Get<dynamic>(n)) + "'"));
                // filter parameters in Sql were renamed and won't match the previous passed filters - discard original parameters to avoid reusing wrong values
                Parameters = null;
            }
        }
        #endregion
    }
}
