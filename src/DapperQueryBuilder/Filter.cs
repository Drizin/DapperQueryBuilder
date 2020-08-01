using Dapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DapperQueryBuilder
{
    [DebuggerDisplay("{sql} ({_parametersStr})")]
    public class Filter : IFilter
    {
        #region Members
        public string Sql { get; set; }
        public Dapper.DynamicParameters Parameters { get; set; }
        private string _parametersStr;
        #endregion

        #region ctor
        public Filter(FormattableString filter)
        {
            var parsedStatement = new InterpolatedStatementParser(filter);
            Sql = parsedStatement.Sql;
            Parameters = parsedStatement.Parameters;
            _parametersStr = string.Join(", ", Parameters.ParameterNames.ToList().Select(n => n + "=" + Convert.ToString(Parameters.Get<dynamic>(n))));
        }
        public Filter(RawString filter)
        {
            Sql = filter;
            Parameters = new DynamicParameters();
        }
        
        #endregion

        #region IFilter
        void IFilter.Build(StringBuilder sb, DynamicParameters parms)
        {
        }
        #endregion
    }
}
