using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more clause in where, which can still add more clauses to where
    /// </summary>
    public interface IWhereBuilder<T> : ICommandBuilder, ICompleteCommand<T>
    {
        /// <summary>
        /// Adds a new condition to where clauses.
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
        /// Adds a new condition to groupby clauses.
        /// </summary>
        IGroupByBuilder<T> GroupBy(FormattableString groupBy);

        /// <summary>
        /// Adds a new condition to orderby clauses.
        /// </summary>
        IOrderByBuilder<T> OrderBy(FormattableString orderBy);
    }
}
