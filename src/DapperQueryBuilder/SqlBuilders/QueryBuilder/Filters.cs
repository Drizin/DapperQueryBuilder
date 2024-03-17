using InterpolatedSql.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DapperQueryBuilder
{
    public class Filters : InterpolatedSql.SqlBuilders.Filters
    {
        #region ctor

        /// <summary>
        /// Create a new group of filters.
        /// </summary>
        public Filters(FiltersType type, IEnumerable<IFilter> filters)
        {
            Type = type;
            this.AddRange(filters);
        }

        /// <summary>
        /// Create a new group of filters which are combined with AND operator.
        /// </summary>
        public Filters(IEnumerable<IFilter> filters) : this(FiltersType.AND, filters)
        {
        }

        /// <summary>
        /// Create a new group of filters from formattable strings
        /// </summary>
        public Filters(FiltersType type, params FormattableString[] filters) :
            this(type, filters.Select(fiString => new Filter(fiString)))
        {
        }

        /// <summary>
        /// Create a new group of filters from formattable strings which are combined with AND operator.
        /// </summary>
        public Filters(params FormattableString[] filters) : this(FiltersType.AND, filters)
        {
        }
        #endregion
    }
}
