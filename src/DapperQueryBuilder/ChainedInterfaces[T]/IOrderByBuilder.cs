using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more orderby clauses, which can still add more clauses to orderby
    /// </summary>
    public interface IOrderByBuilder<T> : ICommandBuilder, ICompleteCommand<T>
    {
        /// <summary>
        /// Adds a new condition to orderby clauses.
        /// </summary>
        IOrderByBuilder<T> OrderBy(FormattableString column);

        /// <summary>
        /// Adds offset and rowcount clauses
        /// </summary>
        ICompleteCommand<T> Limit(int offset, int rowCount);
    }
}
