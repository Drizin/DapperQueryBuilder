using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more from clauses, which can still add more clauses to from
    /// </summary>
    public interface IFromBuilder : ICommandBuilder, ICompleteQuery
    {
        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        IFromBuilder From(string from);

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        IWhereBuilder Where(FormattableString filter);

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        //IWhereBuilder Where(RawString filter);

        /// <summary>
        /// Adds a new column to orderby clauses.
        /// </summary>
        IOrderByBuilder OrderBy(string orderBy);
    }
}
