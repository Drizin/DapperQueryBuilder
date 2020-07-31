using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public interface IFromBuilder : ICompleteQuery
    {
        IFromBuilder From(string from);
        IWhereBuilder Where(FormattableString filter);
        IWhereBuilder Where(RawString filter);
        IOrderByBuilder OrderBy(string orderBy);
    }
}
