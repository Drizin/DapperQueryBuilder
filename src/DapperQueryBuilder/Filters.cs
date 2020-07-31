using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public class Filters : List<Filter>, IFilter
    {
        #region Members
        public string Sql { get; set; }
        public FiltersType Type { get; set; } = FiltersType.AND;
        public enum FiltersType
        {
            AND,
            OR
        }
        #endregion

        #region ctor
        public Filters(params FormattableString[] filter)
        {
        }
        public Filters(FiltersType type, params FormattableString[] filter)
        {
        }
        #endregion

        #region IFilter
        void IFilter.Build(StringBuilder sb, DynamicParameters parms)
        {
        }
        #endregion

    }
}
