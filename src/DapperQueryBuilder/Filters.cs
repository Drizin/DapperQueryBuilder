using Dapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Multiple Filter statements which are grouped together. Can be grouped with ANDs or ORs.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Filters : List<IFilter>, IFilter
    {
        #region Members
        /// <summary>
        /// By default Filter Groups are combined with AND operator. But you can use OR.
        /// </summary>
        public FiltersType Type { get; set; } = FiltersType.AND;

        /// <summary>
        /// How a list of Filters are combined (AND operator or OR operator)
        /// </summary>
        public enum FiltersType
        {
            /// <summary>
            /// AND
            /// </summary>
            AND,

            /// <summary>
            /// OR
            /// </summary>
            OR
        }
        #endregion

        #region ctor
        /// <summary>
        /// Create a new group of filters.
        /// </summary>
        public Filters(FiltersType type, params FormattableString[] filters)
        {
            Type = type;
            foreach (var filter in filters)
                this.Add(new Filter(filter));
        }

        /// <summary>
        /// Create a new group of filters which are combined with AND operator.
        /// </summary>
        public Filters(params FormattableString[] filters) : this(FiltersType.AND, filters)
        {
        }
        #endregion

        #region IFilter
        /// <inheritdoc/>
        public void WriteFilter(StringBuilder sb)
        {
            //if (this.Count() > 1)
            //    sb.Append("(");
            for (int i = 0; i < this.Count(); i++)
            {
                if (i > 0 && Type == FiltersType.AND)
                    sb.Append(" AND ");
                else if (i > 0 && Type == FiltersType.OR)
                    sb.Append(" OR ");
                IFilter filter = this[i];
                if (filter is Filters && ((Filters)filter).Count() > 1) // only put brackets in groups after the first level
                {
                    sb.Append("(");
                    filter.WriteFilter(sb);
                    sb.Append(")");
                }
                else
                    filter.WriteFilter(sb);
            }
            //if (this.Count() > 1)
            //    sb.Append(")");
        }

        /// <inheritdoc/>
        public void MergeParameters(DynamicParameters target)
        {
            foreach(IFilter filter in this)
            {
                filter.MergeParameters(target);
            }
        }

        /// <summary>
        /// If you're using Filters in standalone structure (without QueryBuilder), <br />
        /// you can just "build" the filters over a DynamicParameters and get the string for the filters (with leading WHERE)
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public string BuildFilters(DynamicParameters target)
        {
            foreach (IFilter filter in this)
            {
                filter.MergeParameters(target);
            }
            StringBuilder sb = new StringBuilder();
            WriteFilter(sb);
            if (sb.Length > 0)
                return "WHERE " + sb.ToString();
            return "";
        }

        private string DebuggerDisplay { get { StringBuilder sb = new StringBuilder(); sb.Append($"({this.Count()} filters): "); WriteFilter(sb); return sb.ToString(); } }
        #endregion

    }
}
