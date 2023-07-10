using System;
using System.Data;

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
        ParametersDictionary DapperParameters { get; }

        [Obsolete("Use DapperParameters")] ParametersDictionary Parameters { get; }

        /// <summary>
        /// Underlying connection
        /// </summary>
        IDbConnection DbConnection { get; }
    }
}
