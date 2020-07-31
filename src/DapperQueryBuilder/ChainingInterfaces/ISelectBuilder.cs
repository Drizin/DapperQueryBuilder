using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public interface ISelectBuilder
    {
        ISelectBuilder Select(string select);
        IFromBuilder From(string from);
    }
}
