using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public class Filter : IFilter
    {
        #region Members
        public string Sql { get; set; }
        public Dapper.DynamicParameters Parameters { get; set; }
        #endregion

        #region ctor
        public Filter(FormattableString filter)
        {
            var parser = new InterpolatedQueryParser(filter);
            Sql = parser.Sql;
            Parameters = parser.Parameters;
        }
        public Filter(RawString filter)
        {
            Sql = filter;
        }
        
        #endregion

        #region IFilter
        void IFilter.Build(StringBuilder sb, DynamicParameters parms)
        {
        }
        #endregion
    }
}
