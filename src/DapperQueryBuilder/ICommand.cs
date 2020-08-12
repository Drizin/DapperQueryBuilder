using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Any command (Contains Connection, SQL, and Parameters)
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// SQL of Command
        /// </summary>
        string Sql { get; }

        /// <summary>
        /// Parameters of Command
        /// </summary>
        ParameterInfos Parameters { get; }

        /// <summary>
        /// Underlying connection
        /// </summary>
        IDbConnection Connection { get; }
    }
}
