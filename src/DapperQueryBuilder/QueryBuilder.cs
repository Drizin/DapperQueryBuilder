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
    /// Query Builder wraps an underlying SQL statement and the associated parameters. <br />
    /// Allows to easily add new clauses to underlying statement and also add new parameters. <br />
    /// On top of that it also loads a "Filters" property which can track a list of filters <br />
    /// which are later combined (by default with AND) and will replace the keyword /**where**/
    /// </summary>
    public class QueryBuilder : ICompleteCommand
    {
        #region Members
        private readonly Filters _filters = new Filters();
        private readonly List<string> _froms = new List<string>();
        private readonly CommandBuilder _commandBuilder;
        #endregion

        #region Properties
        /// <summary>
        /// How a list of Filters are combined (AND operator or OR operator)
        /// </summary>
        public Filters.FiltersType FiltersType
        {
            get { return _filters.Type; }
            set { _filters.Type = value; }
        }
        #endregion

        #region ctors
        /// <summary>
        /// New empty QueryBuilder. <br />
        /// Query should be built using .Append(), .AppendLine(), or .Where(). <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// Where filters will later replace /**where**/ keyword
        /// </summary>
        public QueryBuilder(IDbConnection cnn)
        {
            _commandBuilder = new CommandBuilder(cnn);
        }

        /// <summary>
        /// New QueryBuilder based on an initial query. <br />
        /// Query can be modified using .Append(), .AppendLine(), .Where(). <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// Where filters will later replace /**where**/ keyword
        /// </summary>
        /// <param name="cnn"></param>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "AND filters" (without where) (if any filter is defined).
        /// </param>
        public QueryBuilder(IDbConnection cnn, FormattableString query)
        {
            _commandBuilder = new CommandBuilder(cnn, query);
        }
        #endregion

        #region Filters/Where
        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public virtual QueryBuilder Where(Filter filter)
        {
            filter.MergeParameters(_commandBuilder.Parameters);
            _filters.Add(filter);
            return this;
        }

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public virtual QueryBuilder Where(Filters filters)
        {
            filters.MergeParameters(_commandBuilder.Parameters);
            _filters.Add(filters);
            return this;
        }
        

        /// <summary>
        /// Adds a new condition to where clauses. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        public virtual QueryBuilder Where(FormattableString filter)
        {
            return Where(new Filter(filter));
        }

        /// <summary>
        /// Writes the SQL Statement of all filter(s) (going recursively if there are nested filters) <br />
        /// Does NOT add leading "WHERE" keyword. <br />
        /// Returns null if no filter was defined.
        /// </summary>
        public string GetFilters()
        {
            if (_filters == null || !_filters.Any())
                return null;

            StringBuilder filtersString = new StringBuilder();
            _filters.WriteFilter(filtersString); // this writes all filters, going recursively if there are nested filters
            return filtersString.ToString();
        }
        #endregion

        #region ICompleteCommand

        #region Sql
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public string Sql
        {
            get
            {
                StringBuilder finalSql = new StringBuilder();

                // If Query Template is provided, we assume it contains both SELECT and FROMs
                if (_commandBuilder.Sql != null)
                    finalSql.Append(_commandBuilder.Sql);

                string filters = GetFilters();
                if (filters != null)
                {
                    if (finalSql.Length > 0 && finalSql.ToString().Contains("/**where**/"))
                        finalSql.Replace("/**where**/", "WHERE " + filters);
                    else if (finalSql.Length > 0 && finalSql.ToString().Contains("{where}"))
                        finalSql.Replace("{where}", "WHERE " + filters);
                    else if (finalSql.Length > 0 && finalSql.ToString().Contains("/**filters**/"))
                        finalSql.Replace("/**filters**/", "AND " + filters);
                    else if (finalSql.Length > 0 && finalSql.ToString().Contains("{filters}"))
                        finalSql.Replace("{filters}", "AND " + filters);
                    else
                    {
                        //TODO: if Query Template was provided, check if Template ends with "WHERE" or "WHERE 1=1" or "WHERE 0=0", or "WHERE 1=1 AND", etc. remove all that and replace.
                        // else...
                        //TODO: if Query Template was provided, check if Template ends has WHERE with real conditions... set hasWhereConditions=true 
                        // else...
                        finalSql.AppendLine("WHERE " + filters);
                    }
                }

                if (_froms.Any())
                {
                    string froms = string.Join(Environment.NewLine, _froms);

                    if (finalSql.Length > 0 && finalSql.ToString().Contains("/**from**/"))
                        finalSql.Replace("/**from**/", "FROM " + froms);
                    else if (finalSql.Length > 0 && finalSql.ToString().Contains("{from}"))
                        finalSql.Replace("{from}", "FROM " + froms);
                    else if (finalSql.Length > 0 && finalSql.ToString().Contains("/**joins**/"))
                        finalSql.Replace("/**joins**/", froms);
                    else if (finalSql.Length > 0 && finalSql.ToString().Contains("{joins}"))
                        finalSql.Replace("{joins}", froms);
                }

                return finalSql.ToString();
            }
        }
        #endregion

        /// <summary>
        /// Parameters of Query
        /// </summary>
        public ParameterInfos Parameters => _commandBuilder.Parameters;

        /// <summary>
        /// Underlying connection
        /// </summary>
        public IDbConnection Connection => _commandBuilder.Connection;
        #endregion

        /// <summary>
        /// Appends a statement to the current command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="statement">SQL command</param>
        public QueryBuilder Append(FormattableString statement)
        {
            _commandBuilder.Append(statement); 
            return this; 
        }

        /// <summary>
        /// Appends a statement to the current command, but before statement adds a linebreak. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="statement">SQL command</param>
        public QueryBuilder AppendLine(FormattableString statement)
        {
            _commandBuilder.AppendLine(statement);
            return this;
        }

        /// <summary>
        /// Adds a new join to the FROM clause.
        /// </summary>
        public virtual QueryBuilder From(FormattableString fromString)
        {
            var parsedStatement = new InterpolatedStatementParser(fromString);
            string sql = parsedStatement.Sql;
            if (parsedStatement.Parameters.Any())
            {
                sql = (Parameters.MergeParameters(parsedStatement.Parameters, parsedStatement.Sql) ?? sql);
            }
            _froms.Add(sql);
            return this;
        }
    }
}
