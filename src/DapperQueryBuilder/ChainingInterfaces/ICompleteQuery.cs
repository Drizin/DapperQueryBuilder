using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperQueryBuilder
{
    public interface ICompleteQuery
    {
        IEnumerable<T> Query<T>(IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null);
        void AddDynamicParams(object param);
        string Sql { get; }
        Dapper.DynamicParameters Parameters { get; }
    }
}
