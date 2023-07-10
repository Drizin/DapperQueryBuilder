using Dapper;
using InterpolatedSql;

namespace DapperQueryBuilder
{
    public static class FilterExtensions
    {
        /// <summary>
        /// If you're using Filters in standalone structure (without QueryBuilder), <br />
        /// you can just "build" the filters over a ParameterInfos and get the string for the filters (with leading WHERE)
        /// </summary>
        public static string BuildFilters(this Filters filters, DynamicParameters target)
        {
            ParametersDictionary parameters = new ParametersDictionary();
            foreach (var parameter in parameters.Values)
                SqlParameterMapper.Default.AddToDynamicParameters(target, parameter);

            InterpolatedSqlBuilder command = new InterpolatedSqlBuilder();
            filters.WriteTo(command);
            if (!command.IsEmpty)
                command.InsertLiteral(0, "WHERE ");
            foreach(var parameter in ParametersDictionary.LoadFrom(command))
                SqlParameterMapper.Default.AddToDynamicParameters(target, parameter.Value);
            return command.Sql;
        }
    }
}
