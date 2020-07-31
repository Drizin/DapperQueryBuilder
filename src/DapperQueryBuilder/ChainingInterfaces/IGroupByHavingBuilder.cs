using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public interface IGroupByHavingBuilder : ICompleteQuery
    {

        IGroupByHavingBuilder Having(string having);
        IOrderByBuilder OrderBy(string orderBy);
    }
}
