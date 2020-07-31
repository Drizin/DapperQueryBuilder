using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder
    /// </summary>
    public class QueryBuilder : IEmptyQueryBuilder, ISelectBuilder, ISelectDistinctBuilder, IFromBuilder, IWhereBuilder, IGroupByBuilder, IGroupByHavingBuilder, IOrderByBuilder, ICompleteQuery
    {

        #region Members
        private readonly IDbConnection _cnn;
        private readonly List<string> _selectColumns = new List<string>();
        private readonly List<string> _fromTables = new List<string>();
        private readonly Filters _filters = new Filters();
        private readonly List<string> _orderBy = new List<string>();
        private readonly List<string> _groupBy = new List<string>();
        private readonly List<string> _having = new List<string>();
        private int? _rowCount = null;
        private int? _offset = null;
        private bool _isSelectDistinct = false;
        private StringBuilder _sql = null;
        public Dapper.DynamicParameters Parameters { get; set; }
        private int _autoNamedParametersCount = 0;

        /// <summary>
        /// 
        /// </summary>
        public string RawSql = null;
        #endregion

        #region ctors
        /// <summary>
        /// New empty QueryBuilder. Should be constructed using .Select(), .From(), .Where(), etc.
        /// </summary>
        /// <param name="cnn"></param>
        public QueryBuilder(IDbConnection cnn)
        {
            _cnn = cnn;
        }

        /// <summary>
        /// New QueryBuilder based on an initial query. 
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="rawSql">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "filters" (without where) (if any filter is defined).
        /// </param>
        public QueryBuilder(IDbConnection cnn, string rawSql)
        {
            _cnn = cnn;
            RawSql = rawSql;
        }
        #endregion

        #region Query<T> - Invoke Dapper

        /// <summary>
        /// Executes the query (using Dapper), returning the data typed as T.
        /// </summary>
        public IEnumerable<T> Query<T>(IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return _cnn.Query<T>(Sql, param: Parameters, transaction: transaction, buffered: buffered, commandTimeout: commandTimeout, commandType: commandType);
        }

        //TODO: IEnumerable<object> Query, IEnumerable<dynamic> Query
        //TODO: Task<IEnumerable<dynamic>> QueryAsync, Task<IEnumerable<T>> QueryAsync<T>, Task<IEnumerable<object>> QueryAsync
        //TODO: QueryFirst, QueryFirstAsync, QueryFirstOrDefault, QueryFirstOrDefaultAsync, QuerySingle, QuerySingleAsync

        #endregion

        public ISelectBuilder Select(string select)
        {
            _selectColumns.Add(select);
            return this;
        }

        public ISelectDistinctBuilder SelectDistinct(string select)
        {
            _selectColumns.Add(select);
            _isSelectDistinct = true;
            return this;
        }

        public IFromBuilder From(string from)
        {
            if (!_fromTables.Any() && !Regex.IsMatch(from, "\\b FROM \\b", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
                from = "FROM " + from;
            _fromTables.Add(from);
            return this;
        }

        public IWhereBuilder Where(FormattableString filter)
        {
            _filters.Add(new Filter(filter));
            _autoNamedParametersCount++;
            return this;
        }
        public IWhereBuilder Where(RawString filter)
        {
            _filters.Add(new Filter(filter));
            _autoNamedParametersCount++;
            return this;
        }

        public IOrderByBuilder OrderBy(string orderBy)
        {
            _orderBy.Add(orderBy);
            return this;
        }

        public IGroupByBuilder GroupBy(string groupBy)
        {
            _groupBy.Add(groupBy);
            return this;
        }

        public IGroupByHavingBuilder Having(string having)
        {
            _having.Add(having);
            return this;
        }

        public ICompleteQuery Limit(int offset, int rowCount)
        {
            _offset = offset;
            _rowCount = rowCount;
            return this;
        }
        public void AddDynamicParams(object param)
        {
            throw new NotImplementedException();
        }


        public string Sql
        {
            get
            {
                if (_sql != null && _sql.Length > 0)
                    return _sql.ToString();

                if (_sql == null)
                {
                    _sql = new StringBuilder();
                    Parameters = new DynamicParameters();
                    _autoNamedParametersCount = 0;
                }

                // If RawSql is provided, we assume it contains both SELECT and FROMs
                if (!string.IsNullOrEmpty(RawSql))
                    _sql.Append(RawSql);

                if (string.IsNullOrEmpty(RawSql) && _selectColumns.Any())
                    _sql.AppendLine($"SELECT {(_isSelectDistinct ? "DISTINCT ": "")}{string.Join(", ", _selectColumns)}");
                else if (string.IsNullOrEmpty(RawSql))
                    _sql.AppendLine($"SELECT {(_isSelectDistinct ? "DISTINCT ": "")}*");

                if (string.IsNullOrEmpty(RawSql) && _fromTables.Any())
                    _sql.AppendLine($"{string.Join(Environment.NewLine, _fromTables)}"); //TODO: inner join and left/outer join shortcuts?

                if (_filters.Any())
                {
                    StringBuilder filtersString = new StringBuilder(); 
                    foreach (var filter in _filters)
                    {
                        if (_filters.IndexOf(filter) > 0)
                            filtersString.Append(" AND ");
                        string filterSql = filter.Sql;
                        foreach(var parameterName in filter.Parameters.ParameterNames)
                        {
                            string newParameterName = parameterName;
                            if (Parameters.ParameterNames.Contains(parameterName) || Regex.IsMatch(parameterName, "p(\\d)*"))
                            {
                                newParameterName = "p" + _autoNamedParametersCount.ToString();
                                filterSql = filterSql.Replace("@" + parameterName, "@" + newParameterName);
                                _autoNamedParametersCount++;
                            }
                            filtersString.Append(filterSql);
                            Parameters.Add(newParameterName, filter.Parameters.Get<object>(parameterName));
                        }

                    }
                    if (!string.IsNullOrEmpty(RawSql) && RawSql.Contains("/**where**/"))
                        _sql.Replace("/**where**/", "WHERE " + filtersString.ToString());
                    else if (!string.IsNullOrEmpty(RawSql) && RawSql.Contains("{where}"))
                        _sql.Replace("{where}", "WHERE " + filtersString.ToString());
                    else if (!string.IsNullOrEmpty(RawSql) && RawSql.Contains("/**filters**/"))
                        _sql.Replace("/**filters**/", filtersString.ToString());
                    else if (!string.IsNullOrEmpty(RawSql) && RawSql.Contains("{filters}"))
                        _sql.Replace("{filters}", filtersString.ToString());
                    else
                    {
                        //TODO: if RawSql was provided, check if RawSql ends with "WHERE" or "WHERE 1=1" or "WHERE 0=0", or "WHERE 1=1 AND", etc. remove all that and replace.
                        // else...
                        //TODO: if RawSql was provided, check if RawSql ends has WHERE with real conditions... set hasWhereConditions=true 
                        // else...
                        _sql.AppendLine("WHERE " + filtersString.ToString());
                    }

                }
                if (_orderBy.Any())
                    _sql.AppendLine($"ORDER BY {string.Join(", ", _orderBy)}");
                if (_groupBy.Any())
                    _sql.AppendLine($"GROUP BY {string.Join(", ", _groupBy)}");
                if (_having.Any())
                    _sql.AppendLine($"HAVING {string.Join(" AND ", _having)}");
                if (_rowCount != null)
                    _sql.AppendLine($"OFFSET {_offset ?? 0} ROWS FETCH NEXT {_rowCount} ROWS ONLY"); // TODO: PostgreSQL? "LIMIT row_count OFFSET offset"

                return _sql.ToString();
            }
        }

    }
}
