using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Extensions over DynamicParamters
    /// </summary>
    public static class DynamicParametersExtensions
    {
        /// <summary>
        /// Adds single parameter to DynamicParameters. <br />
        /// Checks for name clashes, and will rename parameter if necessary. <br />
        /// If parameter is renamed the new name will be returned, else returns null.
        /// </summary>
        public static string MergeParameter(this DynamicParameters target, string parameterName, object parameterValue)
        {
            string newParameterName = parameterName;
            int _autoNamedParametersCount = 0;
            while (target.ParameterNames.Contains(newParameterName))
            {
                newParameterName = "p" + _autoNamedParametersCount.ToString();
                _autoNamedParametersCount++;
            }
            target.Add(newParameterName, parameterValue);
            if (newParameterName != parameterName)
                return newParameterName;
            return null;
        }

        /// <summary>
        /// Merges multiple parameters into this CommandBuilder. <br />
        /// Checks for name clashes, and will rename parameters (in the CommandBuilder only) if necessary. <br />
        /// If some parameter is renamed the returned Sql statement will containg the original sql replaced with new names, else (if nothing changed) returns null. <br />
        /// This method does NOT append Parser SQL to CommandBuilder SQL (you may want to save this SQL statement elsewhere)
        /// </summary>
        public static string MergeParameters(this DynamicParameters target, DynamicParameters parameters, string sql)
        {
            string newSql = sql;
            foreach (var parameterName in parameters.ParameterNames)
            {
                string newParameterName = target.MergeParameter(parameterName, parameters.Get<object>(parameterName));
                if (newParameterName != null)
                    newSql = newSql.Replace("@" + parameterName, "@" + newParameterName);
            }
            if (newSql != sql)
                return newSql;
            return null;
        }

    }
}
