using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public interface ISelectDistinctBuilder
    {
        ISelectDistinctBuilder SelectDistinct(string select);
        IFromBuilder From(string from);
    }
}
