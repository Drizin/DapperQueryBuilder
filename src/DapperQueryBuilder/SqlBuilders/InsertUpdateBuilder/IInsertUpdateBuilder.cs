namespace InterpolatedSql.Dapper.SqlBuilders.InsertUpdateBuilder
{
    public interface IInsertUpdateBuilder<U, RB, R> : InterpolatedSql.SqlBuilders.InsertUpdateBuilder.IInsertUpdateBuilder<U, RB, R>
        where U : IInsertUpdateBuilder<U, RB, R>, IBuildable<R>
        where RB : IDapperSqlBuilder, IBuildable<R>
        where R : class, IInterpolatedSql, IDapperSqlCommand
    {
    }
    public interface IInsertUpdateBuilder : IInsertUpdateBuilder<InsertUpdateBuilder, ISqlBuilder, IDapperSqlCommand>
    {

    }
}
