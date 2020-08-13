using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperQueryBuilder.Tests
{
    /// <summary>
    /// This is just a wrapper around IDbCommand, which allows us to inspect how Dapper is preparing our Commands
    /// </summary>
    public class UnitTestsDbCommand : IDbCommand
    {
        private readonly IDbCommand _cmd;
        public UnitTestsDbCommand(IDbCommand command)
        {
            _cmd = command;
        }

        public string CommandText { get => _cmd.CommandText; set => _cmd.CommandText = value; }
        public int CommandTimeout { get => _cmd.CommandTimeout; set => _cmd.CommandTimeout = value; }
        public CommandType CommandType { get => _cmd.CommandType; set => _cmd.CommandType = value; }
        public IDbConnection Connection { get => _cmd.Connection; set => _cmd.Connection = value; }

        public IDataParameterCollection Parameters => _cmd.Parameters;

        public IDbTransaction Transaction { get => _cmd.Transaction; set => _cmd.Transaction = value; }
        public UpdateRowSource UpdatedRowSource { get => _cmd.UpdatedRowSource; set => _cmd.UpdatedRowSource = value; }

        public void Cancel()
        {
            _cmd.Cancel();
        }

        public IDbDataParameter CreateParameter()
        {
            return _cmd.CreateParameter();
        }

        public void Dispose()
        {
            _cmd.Dispose();
        }

        public int ExecuteNonQuery()
        {
            return _cmd.ExecuteNonQuery();
        }

        public IDataReader ExecuteReader()
        {
            return _cmd.ExecuteReader();
        }

        public IDataReader ExecuteReader(CommandBehavior behavior)
        {
            return _cmd.ExecuteReader(behavior);
        }

        public object ExecuteScalar()
        {
            return _cmd.ExecuteScalar();
        }

        public void Prepare()
        {
            _cmd.Prepare();
        }

    }

}
