using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public interface IOrderByBuilder : ICompleteQuery
    {
        IOrderByBuilder OrderBy(string column);
        ICompleteQuery Limit(int offset, int rowCount);
    }
}
