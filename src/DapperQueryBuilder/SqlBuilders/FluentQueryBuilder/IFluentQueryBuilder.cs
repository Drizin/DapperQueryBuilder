using System.Data;

namespace InterpolatedSql.Dapper.SqlBuilders.FluentQueryBuilder
{
    public interface IFluentQueryBuilder 
        : InterpolatedSql.SqlBuilders.FluentQueryBuilder.IFluentQueryBuilder<IFluentQueryBuilder, SqlBuilder, IDapperSqlCommand>, 
        IQueryBuilder<IFluentQueryBuilder, SqlBuilder, IDapperSqlCommand>,
        InterpolatedSql.SqlBuilders.ISqlBuilder<IFluentQueryBuilder, IDapperSqlCommand>, 
        IBuildable<IDapperSqlCommand>
    {
        //ParametersDictionary DapperParameters { get; }
        IDbConnection DbConnection { get; set; }

        IDapperSqlCommand Build();
    }
}