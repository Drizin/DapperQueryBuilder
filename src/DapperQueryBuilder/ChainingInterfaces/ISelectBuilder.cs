using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder which is preparing a SELECT statement
    /// </summary>
    public interface ISelectBuilder : ICommandBuilder
    {
        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        ISelectBuilder Select(string column);

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        /// <param name="column"></param>
        /// <param name="moreColumns"></param>
        /// <returns></returns>
        ISelectBuilder Select(string column, params string[] moreColumns);

        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        IFromBuilder From(string from);
    }
}
