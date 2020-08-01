using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    public interface ICommandBuilder
    {
        //void AddDynamicParams(object param);

        /// <summary>
        /// SQL of Command
        /// </summary>
        string Sql { get; }

        /// <summary>
        /// Parameters of Command
        /// </summary>
        Dapper.DynamicParameters Parameters { get; }

    }
}
