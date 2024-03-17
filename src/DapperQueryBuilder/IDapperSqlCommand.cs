namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// Dapper Sql Command that can be executed
    /// </summary>
    public interface IDapperSqlCommand : ISqlCommand
    {
        /// <summary>Sql Parameters converted into Dapper format</summary>
        ParametersDictionary DapperParameters { get; }
    }

    /// <summary>
    /// Dapper Sql Command that can be executed
    /// </summary>
    public interface IDapperSqlCommand<T> : ISqlCommand<T>, IDapperSqlCommand
        where T : IDapperSqlCommand<T>, ISqlCommand<T>
    {
    }
}
