using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder which is preparing a SELECT statement
    /// </summary>
    public interface ISelectBuilder<T> : ICommandBuilder
    {
        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        ISelectBuilder<T> Select(FormattableString column);

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        ISelectBuilder<T> Select(params FormattableString[] moreColumns);

        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        IFromBuilder<T> From(FormattableString from);
    }
}
