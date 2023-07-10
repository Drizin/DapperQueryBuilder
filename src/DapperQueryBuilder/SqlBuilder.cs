using InterpolatedSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
#if NET6_0_OR_GREATER
using System.Runtime.CompilerServices;
#endif

namespace DapperQueryBuilder
{
    /// <summary>
    /// Exactly like <see cref="InterpolatedSqlBuilder"/> but it requires an underlying IDbConnection, 
    /// provides facades (as extension-methods) to invoke Dapper extensions (see <see cref="ICompleteCommandExtensions"/>),
    /// and maps to Dapper <see cref="Dapper.DynamicParameters"/> type.
    /// </summary>
    public class SqlBuilder : InterpolatedSqlBuilder<SqlBuilder>, ICompleteCommand
    {
        #region Members
        private ParametersDictionary? _cachedDapperParameters = null;

        /// <summary>Sql Parameters converted into Dapper format</summary>
        public ParametersDictionary DapperParameters => _cachedDapperParameters ?? (_cachedDapperParameters = ParametersDictionary.LoadFrom(this));

        [Obsolete("Use DapperParameters")] public ParametersDictionary Parameters => DapperParameters;
        #endregion

        #region ctors
        /// <inheritdoc cref="SqlBuilder" />
        protected internal SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options, StringBuilder? format, List<InterpolatedSqlParameter>? arguments) : base(options, format, arguments) 
        {
            DbConnection = connection;
            Options.CalculateAutoParameterName = (parameter, pos) => DapperQueryBuilderOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }

        /// <inheritdoc cref="SqlBuilder" />
        public SqlBuilder(IDbConnection connection, InterpolatedSqlBuilderOptions? options = null) : base(options)
        {
            DbConnection = connection;
            Options.CalculateAutoParameterName = (parameter, pos) => DapperQueryBuilderOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
        }

        /// <summary>
        /// New CommandBuilder based on an initial command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="cnn">Underlying connection</param>
        /// <param name="command">SQL command</param>
        public SqlBuilder(IDbConnection cnn, FormattableString command, InterpolatedSqlBuilderOptions? options = null) : this(cnn, options)
        {
            Options.CalculateAutoParameterName = (parameter, pos) => DapperQueryBuilderOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
            AppendFormattableString(command);
        }

        /// <summary>
        /// New CommandBuilder based on an initial command. <br />
        /// Parameters embedded using string-interpolation will be automatically converted into Dapper parameters.
        /// </summary>
        /// <param name="cnn">Underlying connection</param>
        /// <param name="command">SQL command</param>
        public SqlBuilder(IDbConnection cnn, InterpolatedSqlBuilder command, InterpolatedSqlBuilderOptions? options = null) : this(cnn, options)
        {
            Options.CalculateAutoParameterName = (parameter, pos) => DapperQueryBuilderOptions.InterpolatedSqlParameterParser.CalculateAutoParameterName(parameter, pos, base.Options);
            Append(command);
        }
        #endregion

        /// <inheritdoc />
        protected override void PurgeParametersCache()
        {
            base.PurgeParametersCache();
            _cachedDapperParameters = null;
        }
    }
}
