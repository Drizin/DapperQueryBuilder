using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public interface IGroupByBuilder : ICompleteQuery
    {
        IGroupByBuilder GroupBy(string groupBy);
        IGroupByHavingBuilder Having(string having);
        IOrderByBuilder OrderBy(string orderBy);
    }
}
