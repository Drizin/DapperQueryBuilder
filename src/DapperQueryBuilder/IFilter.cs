using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public interface IFilter
    {
        void Build(StringBuilder sb, Dapper.DynamicParameters parms);
    }
}
