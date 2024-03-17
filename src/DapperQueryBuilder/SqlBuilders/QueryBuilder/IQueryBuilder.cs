namespace InterpolatedSql.Dapper.SqlBuilders
{
    public interface IQueryBuilder<U, RB, R> : InterpolatedSql.SqlBuilders.IQueryBuilder<U, RB, R>
        where U : IQueryBuilder<U, RB, R>, IBuildable<R>
        where RB : IDapperSqlBuilder, IBuildable<R>
        where R : class, IInterpolatedSql, IDapperSqlCommand
    {
    }
    public interface IQueryBuilder : IQueryBuilder<QueryBuilder, ISqlBuilder, IDapperSqlCommand>
    {

    }
}
