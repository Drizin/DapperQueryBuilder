using InterpolatedSql;

namespace InterpolatedSql.Dapper
{
    /// <summary>
    /// Global Options
    /// </summary>
    public class InterpolatedSqlDapperOptions
    {
        /// <summary>
        /// Responsible for parsing SqlParameters (see <see cref="IInterpolatedSql.SqlParameters"/>) 
        /// into a list of SqlParameterInfo that 
        /// </summary>
        public static SqlParameterMapper InterpolatedSqlParameterParser = new SqlParameterMapper();
    }
}
