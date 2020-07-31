using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public interface IWhereBuilder : ICompleteQuery
    {
        IWhereBuilder Where(FormattableString filter);
        IWhereBuilder Where(RawString filter);
        IGroupByBuilder GroupBy(string groupBy);
        IOrderByBuilder OrderBy(string orderBy);
    }
}
