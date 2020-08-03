using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more from clauses, which can still add more clauses to from
    /// </summary>
    public interface IFromBuilder<T> : ICommandBuilder, ICompleteCommand<T>
    {
        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        IFromBuilder<T> From(FormattableString from);

        /// <summary>
        /// Adds a new group of conditions to where clauses.
        /// </summary>
        IWhereBuilder<T> Where(Filter filter);

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        IWhereBuilder<T> Where(Filters filter);

        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        IWhereBuilder<T> Where(FormattableString filter);

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        //IWhereBuilder<T> Where(RawString filter);

        /// <summary>
        /// Adds a new column to orderby clauses.
        /// </summary>
        IOrderByBuilder<T> OrderBy(FormattableString orderBy);
    }
}
