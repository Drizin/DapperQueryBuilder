using InterpolatedSql;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Any command (Contains Connection, SQL, and Parameters) which is complete for execution.
    /// </summary>
    public interface ICompleteCommand : ICommand, IInterpolatedSql
    {
    }
}
