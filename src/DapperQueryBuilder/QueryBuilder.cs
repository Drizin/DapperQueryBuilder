using InterpolatedSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Exactly like <see cref="InterpolatedSqlBuilder"/> but also wraps an underlying IDbConnection
    /// and has a "Filters" property which can track a list of filters which are later combined (by default with AND) and will replace the keyword /**where**/
    /// </summary>
    public class QueryBuilder : InterpolatedSqlBuilder<QueryBuilder>, ICompleteCommand
    {
        #region Members
        private ParametersDictionary? _cachedDapperParameters = null;

        /// <summary>Sql Parameters converted into Dapper format</summary>
        public ParametersDictionary DapperParameters => _cachedDapperParameters ?? (_cachedDapperParameters = ParametersDictionary.LoadFrom(this));

        [Obsolete("Use DapperParameters")] public ParametersDictionary Parameters => DapperParameters;

        protected readonly Filters _filters = new Filters();
        protected readonly InterpolatedSqlBuilder _froms = new InterpolatedSqlBuilder();
        protected readonly InterpolatedSqlBuilder _selects = new InterpolatedSqlBuilder();
        protected InterpolatedSqlBuilder? _cachedCombinedQuery = null;

        // HACK: QueryBuilder inherits from InterpolatedStringBuilder to offer the Fluent API, but its final command should combine multiple blocks
        // since base class methods use Format, in some moments (including in the constructor) we should NOT return the combined command
        private bool _shouldBuildCombinedQuery = false;

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
        public QueryBuilder(IDbConnection connection) : base()
        {
            DbConnection = connection;
            Options.CalculateAutoParameterName = (parameter, pos) => DapperQueryBuilderOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
            _shouldBuildCombinedQuery = true;
        }

        /// <summary>
        /// New QueryBuilder based on an initial query. <br />
        /// Query can be modified using .Append(), .AppendLine(), .Where(). <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// Where filters will later replace /**where**/ keyword
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="query">You can use "{where}" or "/**where**/" in your query, and it will be replaced by "WHERE + filters" (if any filter is defined). <br />
        /// You can use "{filters}" or "/**filters**/" in your query, and it will be replaced by "AND filters" (without where) (if any filter is defined).
        /// </param>
        public QueryBuilder(IDbConnection connection, FormattableString query) : base(query)
        {
            DbConnection = connection;
            Options.CalculateAutoParameterName = (parameter, pos) => DapperQueryBuilderOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
            _shouldBuildCombinedQuery = true;
        }
        #endregion

        #region Filters/Where
        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public virtual QueryBuilder Where(Filter filter)
        {
            _filters.Add(filter);
            PurgeLiteralCache();
            PurgeParametersCache();
            return this;
        }

        /// <summary>
        /// Adds a new condition to where clauses.
        /// </summary>
        public virtual QueryBuilder Where(Filters filters)
        {
            _filters.Add(filters);
            PurgeLiteralCache();
            PurgeParametersCache();
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
        public InterpolatedSqlBuilder? GetFilters()
        {
            if (_filters == null || !_filters.Any())
                return null;

            InterpolatedSqlBuilder filters = new InterpolatedSqlBuilder();
            _filters.WriteTo(filters); // this writes all filters, going recursively if there are nested filters
            return filters;
        }
        #endregion

        #region ICompleteCommand

        #region Sql
       
        /// <summary>
        /// Gets the combined command
        /// </summary>
        public virtual InterpolatedSqlBuilder CombinedQuery
        {
            get
            {
                if (_cachedCombinedQuery != null)
                    return _cachedCombinedQuery;

                InterpolatedSqlBuilder combinedQuery;

                if (_format.Length > 0)
                {
                    _shouldBuildCombinedQuery = false;
                    combinedQuery = new InterpolatedSqlBuilder(Options, new StringBuilder(_format.Length).Append(_format), _sqlParameters.ToList());
                    _shouldBuildCombinedQuery = true;
                }
                else
                    combinedQuery = new InterpolatedSqlBuilder(Options);

                if (_filters.Any())
                {
                    var filters = GetFilters()!;

                    string matchKeyword;
                    int matchPos;
                    if (((matchKeyword = "/**where**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                        ((matchKeyword = "{where}")     != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                    {
                        // Template has a Placeholder for Filters
                        filters.InsertLiteral(0, "WHERE ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, filters);
                    }
                    else if (((matchKeyword = "/**filters**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                             ((matchKeyword = "{filters}")     != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                    {
                        // Template has a Placeholder for Filters
                        filters.InsertLiteral(0, "AND ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, filters);
                    }
                    else
                    {
                        //TODO: if Query Template was provided, check if Template ends with "WHERE" or "WHERE 1=1" or "WHERE 0=0", or "WHERE 1=1 AND", etc. remove all that and replace.
                        // else...
                        //TODO: if Query Template was provided, check if Template ends has WHERE with real conditions... set hasWhereConditions=true 
                        // else...
                        combinedQuery.Append(filters);
                    }
                }

                if (_froms.Sql?.Length > 0)
                {
                    _froms.TrimEnd();
                    string matchKeyword;
                    int matchPos;
                    if (((matchKeyword = "/**from**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                        ((matchKeyword = "{from}")     != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                    {
                        // Template has a Placeholder for FROMs
                        _froms.InsertLiteral(0, "FROM ");
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, _froms);
                    }
                    else if (((matchKeyword = "/**joins**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                            ((matchKeyword = "{joins}")      != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                    {
                        // Template has a placeholder for JOINS (yeah - JOINS and FROMS are currently using same variable)
                        combinedQuery.Remove(matchPos, matchKeyword.Length);
                        combinedQuery.Insert(matchPos, _froms);
                    }
                }

                if (_selects.Sql?.Length > 0)
                {
                    _selects.TrimEnd();

                    if (_selects.Sql?.Length > 0)
                    {
                        string matchKeyword;
                        int matchPos;
                        if (((matchKeyword = "/**select**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                            ((matchKeyword = "{select}")     != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                        {
                            // Template has a Placeholder for SELECT
                            _selects.InsertLiteral(0, "SELECT ");
                            combinedQuery.Remove(matchPos, matchKeyword.Length);
                            combinedQuery.Insert(matchPos, _selects);
                        }
                        else if (((matchKeyword = "/**selects**/") != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1) ||
                                ((matchKeyword = "{selects}")      != null && (matchPos = combinedQuery.IndexOf(matchKeyword)) != -1))
                        {
                            // Template has a placeholder for SELECTS - which means that
                            // SELECT should be already in template and user just wants to add more columns using "selects" placeholder
                            _selects.InsertLiteral(0, ", ");
                            combinedQuery.Remove(matchPos, matchKeyword.Length);
                            combinedQuery.Insert(matchPos, _selects);
                        }
                    }
                }

                _cachedCombinedQuery = combinedQuery;
                return _cachedCombinedQuery;
            }
        }
        #endregion

        /// <inheritdoc />
        public override IReadOnlyList<InterpolatedSqlParameter> SqlParameters => _shouldBuildCombinedQuery ? CombinedQuery.SqlParameters : base.SqlParameters;

        /// <inheritdoc />
        public override string Format => _shouldBuildCombinedQuery ? CombinedQuery.Format : base.Format;

        /// <inheritdoc />
        protected override void PurgeLiteralCache()
        {
            _cachedCombinedQuery = null;
            base.PurgeLiteralCache();
        }

        #endregion

#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new join to the FROM clause.
        /// </summary>
        public virtual QueryBuilder From(ref InterpolatedSqlHandler value)
        {
            _froms.Append(value.InterpolatedSqlBuilder);
            _froms.AppendLiteral(NewLine); //TODO: bool AutoLineBreaks
            PurgeLiteralCache();
            PurgeParametersCache();
            return this;
        }
#endif

        /// <summary>
        /// Adds a new join to the FROM clause.
        /// </summary>
        public virtual QueryBuilder From(LegacyFormattableString fromString)
        {
            _froms.Append(fromString);
            _froms.AppendLiteral(NewLine); //TODO: bool AutoLineBreaks
            PurgeLiteralCache();
            PurgeParametersCache();
            return this;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Adds a new column to the SELECT clause.
        /// </summary>
        public virtual QueryBuilder Select(ref InterpolatedSqlHandler value)
        {
            if (!_selects.IsEmpty)
                _selects.AppendLiteral(", ");
            if (value.InterpolatedSqlBuilder.SqlParameters.Count == 0) // if it's just a plain string, then it's code (not user input) - so it's safe // TODO: review this! should we always get strings here? what about SELECT [expression] ? allow both?
                _selects.AppendLiteral(value.InterpolatedSqlBuilder.Format);
            else
                _selects.Append(value.InterpolatedSqlBuilder);
            PurgeLiteralCache();
            PurgeParametersCache();
            return this;
        }
#endif

        /// <summary>
        /// Adds a new column to the SELECT clause.
        /// </summary>
        public virtual QueryBuilder Select(LegacyFormattableString selectString)
        {
            if (!_selects.IsEmpty)
                _selects.AppendLiteral(", ");
            if (((FormattableString)selectString).ArgumentCount == 0) // if it's just a plain string, then it's code (not user input) - so it's safe // TODO: review this! should we always get strings here? what about SELECT [expression] ? allow both?
                _selects.AppendLiteral(((FormattableString)selectString).Format);
            else
                _selects.Append(selectString);
            PurgeLiteralCache();
            PurgeParametersCache();
            return this;
        }

    }
}
