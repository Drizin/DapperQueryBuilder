using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperQueryBuilder.Tests
{
    /// <summary>
    /// This is just a wrapper around IDbConnection, which allows us to inspect how Dapper is preparing our Commands
    /// </summary>
    public class UnitTestsDbConnection : IDbConnection
    {
        private readonly IDbConnection _conn;
        public IDbCommand LastCommand { get; set; }

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
            LastCommand = new UnitTestsDbCommand(_conn.CreateCommand());
            return LastCommand;
        }

        public void Dispose()
        {
            _conn.Dispose();
        }

        public void Open()
        {
            _conn.Open();
        }
    }

}
