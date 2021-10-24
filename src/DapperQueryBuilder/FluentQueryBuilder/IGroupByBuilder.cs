using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more groupby clauses, which can still add more clauses to groupby
    /// </summary>
    public interface IGroupByBuilder : ICompleteCommand
    {
        /// <summary>
        /// Adds one or more column(s) to groupby clauses.
        /// </summary>
        IGroupByBuilder GroupBy(FormattableString groupBy);

        /// <summary>
        /// Adds one or more condition(s) to having clauses.
        /// </summary>
        IGroupByHavingBuilder Having(FormattableString having);

        /// <summary>
        /// Adds one or more column(s) to orderby clauses.
        /// </summary>
        IOrderByBuilder OrderBy(FormattableString orderBy);
    }
}
