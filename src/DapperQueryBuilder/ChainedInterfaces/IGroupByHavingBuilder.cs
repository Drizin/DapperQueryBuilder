using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more having clauses, which can still add more clauses to having
    /// </summary>
    public interface IGroupByHavingBuilder<T> : ICommandBuilder, ICompleteCommand<T>
    {

        /// <summary>
        /// Adds a new condition to having clauses.
        /// </summary>
        /// <param name="having"></param>
        /// <returns></returns>
        IGroupByHavingBuilder<T> Having(FormattableString having);

        /// <summary>
        /// Adds a new condition to orderby clauses.
        /// </summary>
        IOrderByBuilder<T> OrderBy(FormattableString orderBy);
    }
}
