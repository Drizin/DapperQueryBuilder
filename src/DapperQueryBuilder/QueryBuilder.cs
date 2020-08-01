using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Query Builder
    /// </summary>
    public class QueryBuilder : CommandBuilder, IEmptyQueryBuilder, ISelectBuilder, ISelectDistinctBuilder, IFromBuilder, IWhereBuilder, IGroupByBuilder, IGroupByHavingBuilder, IOrderByBuilder, ICompleteQuery
    {
        #region Members
        private readonly List<string> _selectColumns = new List<string>();
        private readonly List<string> _fromTables = new List<string>();
        private readonly Filters _filters = new Filters();
        private readonly List<string> _orderBy = new List<string>();
        private readonly List<string> _groupBy = new List<string>();
        private readonly List<string> _having = new List<string>();
        private int? _rowCount = null;
        private int? _offset = null;
        private bool _isSelectDistinct = false;
        private string _queryTemplate = null;
        #endregion

        #region ctors
        /// <summary>
        /// New empty QueryBuilder. Should be constructed using .Select(), .From(), .Where(), etc.
        /// </summary>
        /// <param name="cnn"></param>
        public QueryBuilder(IDbConnection cnn) : base(cnn)
        {
        }

        /// <summary>
        /// New QueryBuilder based on an initial query. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "filters" (without where) (if any filter is defined).
        /// </param>
        public QueryBuilder(IDbConnection cnn, FormattableString query) : base(cnn)
        {
            var parser = new InterpolatedStatementParser(query);
            string querySql = parser.Sql;
            foreach (var statementParameterName in parser.Parameters.ParameterNames)
                base.AddParameter(statementParameterName, parser.Parameters.Get<object>(statementParameterName), ref querySql);
            _queryTemplate = querySql;
        }
        #endregion

        /// <summary>
        /// Adds one column to the select clauses
        /// </summary>
        public ISelectBuilder Select(string column)
        {
            return Select(column, new string[] { });
        }

        /// <summary>
        /// Adds one or more columns to the select clauses
        /// </summary>
        public ISelectBuilder Select(string column, params string[] moreColumns)
        {
            _selectColumns.Add(column);
            foreach (var col in moreColumns)
                _selectColumns.Add(col);
            return this;
        }

        /// <summary>
        /// Adds one column to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        public ISelectDistinctBuilder SelectDistinct(string select)
        {
            return SelectDistinct(select, new string[] { });
        }

        /// <summary>
        /// Adds one or more columns to the select clauses, and defines that query is a SELECT DISTINCT type
        /// </summary>
        public ISelectDistinctBuilder SelectDistinct(string select, params string[] moreColumns)
        {
            _isSelectDistinct = true;
            _selectColumns.Add(select);
            foreach (var col in moreColumns)
                _selectColumns.Add(col);
            return this;
        }

        /// <summary>
        /// Adds a new table to from clauses. <br />
        /// "FROM" word is optional. <br />
        /// You can add an alias after table name. <br />
        /// You can also add INNER JOIN, LEFT JOIN, etc (with the matching conditions).
        /// </summary>
        public IFromBuilder From(string from)
        {
            if (!_fromTables.Any() && !Regex.IsMatch(from, "\\b FROM \\b", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace))
                from = "FROM " + from;
            _fromTables.Add(from);
            return this;
        }
        //TODO: create options with InnerJoin, LeftJoin, RightJoin, FullJoin, CrossJoin? Create overloads with table alias?



        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public IWhereBuilder Where(Filter filter)
        {
            // Check for name clashes in parameters, we may have to rename parameters
            string filterSql = filter.Sql;
            foreach (var statementParameterName in filter.Parameters.ParameterNames)
                base.AddParameter(statementParameterName, filter.Parameters.Get<object>(statementParameterName), ref filterSql);
            filter.Sql = filterSql;
            _filters.Add(filter);
            return this;
        }

        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        public IWhereBuilder Where(FormattableString filter)
        {
            return Where(new Filter(filter));
        }

        ///// <summary>
        ///// Adds a new condition to where clauses.
        ///// </summary>
        //public IWhereBuilder Where(RawString filter) // If I accept RawStrings (implicitly converted from strings) someone may pass args like $"{param1}" + $"{param1}", and that would be translated into a RawString and would NOT be injection-safe!
        //{
        //    return Where(new Filter(filter));
        //}

        /// <summary>
        /// Adds a new column to orderby clauses.
        /// </summary>
        public IOrderByBuilder OrderBy(string orderBy)
        {
            _orderBy.Add(orderBy);
            return this;
        }

        /// <summary>
        /// Adds a new column to groupby clauses.
        /// </summary>
        public IGroupByBuilder GroupBy(string groupBy)
        {
            _groupBy.Add(groupBy);
            return this;
        }

        /// <summary>
        /// Adds a new condition to having clauses.
        /// </summary>
        public IGroupByHavingBuilder Having(string having)
        {
            _having.Add(having);
            return this;
        }

        /// <summary>
        /// Adds offset and rowcount clauses
        /// </summary>
        public ICompleteQuery Limit(int offset, int rowCount)
        {
            _offset = offset;
            _rowCount = rowCount;
            return this;
        }


        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public override string Sql
        {
            get
            {
                //if (sql != null && sql.Length > 0)
                //    return sql.ToString();

                StringBuilder finalSql = new StringBuilder();

                // If Query Template is provided, we assume it contains both SELECT and FROMs
                if (_queryTemplate != null)
                    finalSql.Append(_queryTemplate);
                else if (_selectColumns.Any())
                    finalSql.AppendLine($"SELECT {(_isSelectDistinct ? "DISTINCT ": "")}{string.Join(", ", _selectColumns)}");
                else 
                    finalSql.AppendLine($"SELECT {(_isSelectDistinct ? "DISTINCT ": "")}*");

                if (_queryTemplate == null && _fromTables.Any())
                    finalSql.AppendLine($"{string.Join(Environment.NewLine, _fromTables)}"); //TODO: inner join and left/outer join shortcuts?

                if (_filters.Any())
                {
                    StringBuilder filtersString = new StringBuilder(); 
                    foreach (var filter in _filters)
                    {
                        if (_filters.IndexOf(filter) > 0)
                            filtersString.Append(" AND ");
                        string filterSql = filter.Sql;
                        filtersString.Append(filterSql);
                    }
                    if (_queryTemplate != null && _queryTemplate.Contains("/**where**/"))
                        finalSql.Replace("/**where**/", "WHERE " + filtersString.ToString());
                    else if (_queryTemplate != null && _queryTemplate.Contains("{where}"))
                        finalSql.Replace("{where}", "WHERE " + filtersString.ToString());
                    else if (_queryTemplate != null && _queryTemplate.Contains("/**filters**/"))
                        finalSql.Replace("/**filters**/", filtersString.ToString());
                    else if (_queryTemplate != null && _queryTemplate.Contains("{filters}"))
                        finalSql.Replace("{filters}", filtersString.ToString());
                    else
                    {
                        //TODO: if Query Template was provided, check if Template ends with "WHERE" or "WHERE 1=1" or "WHERE 0=0", or "WHERE 1=1 AND", etc. remove all that and replace.
                        // else...
                        //TODO: if Query Template was provided, check if Template ends has WHERE with real conditions... set hasWhereConditions=true 
                        // else...
                        finalSql.AppendLine("WHERE " + filtersString.ToString());
                    }

                }
                if (_orderBy.Any())
                    finalSql.AppendLine($"ORDER BY {string.Join(", ", _orderBy)}");
                if (_groupBy.Any())
                    finalSql.AppendLine($"GROUP BY {string.Join(", ", _groupBy)}");
                if (_having.Any())
                    finalSql.AppendLine($"HAVING {string.Join(" AND ", _having)}");
                if (_rowCount != null)
                    finalSql.AppendLine($"OFFSET {_offset ?? 0} ROWS FETCH NEXT {_rowCount} ROWS ONLY"); // TODO: PostgreSQL? "LIMIT row_count OFFSET offset"

                return finalSql.ToString();
            }
        }

    }
}
