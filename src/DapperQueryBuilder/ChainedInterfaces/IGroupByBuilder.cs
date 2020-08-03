using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder with one or more groupby clauses, which can still add more clauses to groupby
    /// </summary>
    public interface IGroupByBuilder<T> : ICommandBuilder, ICompleteCommand<T>
    {
        IGroupByBuilder<T> GroupBy(FormattableString groupBy);
        IGroupByHavingBuilder<T> Having(FormattableString having);
        IOrderByBuilder<T> OrderBy(FormattableString orderBy);
    }
}
