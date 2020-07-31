using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public interface IEmptyQueryBuilder
    {
        ISelectBuilder Select(string select);
        ISelectDistinctBuilder SelectDistinct(string select);
    }
}
