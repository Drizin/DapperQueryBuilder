using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Empty QueryBuilder (initialized without a template), which can start both with Select() or SelectDistinct()
    /// </summary>
    public interface IEmptyQueryBuilder : ICommandBuilder
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
        /// Adds one column to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder SelectDistinct(string select);

        /// <summary>
        /// Adds one or more columns to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        ISelectDistinctBuilder SelectDistinct(string select, params string[] moreColumns);
    }
}
