using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace DapperQueryBuilder.Tests
{
    /// <summary>
    /// This is just a wrapper around IDbConnection, which allows us to inspect how Dapper is preparing our Commands
    /// </summary>
    public class UnitTestsDbConnection : IDbConnection
    {
        private readonly IDbConnection _conn;

        // Since Dapper clears the parameters of IDbCommands after their execution we have to store a copy of the information instead of storing the IDbCommand itself
        public List<ExecutedCommandDetails> PreviousCommands { get; set; } = new List<ExecutedCommandDetails>();

        public UnitTestsDbConnection(IDbConnection connection)
        {
            _conn = connection;
        }

        public string ConnectionString { get => _conn.ConnectionString; set => _conn.ConnectionString = value; }

        public int ConnectionTimeout => _conn.ConnectionTimeout;

        public string Database => _conn.Database;

        public ConnectionState State => _conn.State;

        public IDbTransaction BeginTransaction()
        {
            return _conn.BeginTransaction();
        }

        public IDbTransaction BeginTransaction(IsolationLevel il)
        {
            return _conn.BeginTransaction(il);
        }

        public void ChangeDatabase(string databaseName)
        {
            _conn.ChangeDatabase(databaseName);
        }

        public void Close()
        {
            _conn.Close();
        }

        public IDbCommand CreateCommand()
        {
            return new UnitTestsDbCommand(_conn.CreateCommand(), this);
        }

        public void Dispose()
        {
            _conn.Dispose();
        }

        public void Open()
        {
            _conn.Open();
        }

        [DebuggerDisplay("{CommandText}")]
        public class ExecutedCommandDetails
        {
            public string CommandText { get; set; }
            public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        }
    }

}
