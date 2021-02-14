using Dapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace DapperQueryBuilder
{
    /// <summary>
    /// A List of Parameters that are passed to Dapper methods
    /// </summary>
    public class ParameterInfos : Dictionary<string, ParameterInfo>, SqlMapper.IDynamicParameters, SqlMapper.IParameterCallbacks
    {
        #region members
        private DynamicParameters _dapperParameters = null;
        #endregion

        #region ctors
        /// <summary>
        /// List of SQL parameters which are passed to Dapper
        /// </summary>
        public ParameterInfos() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
        #endregion

        #region DapperParameters
        /// <summary>
        /// Convert the current parameters into Dapper Parameters, since Dapper will automagically set DbTypes, Sizes, etc, and map to our databases
        /// </summary>
        public virtual DynamicParameters DapperParameters
        {
            get
            {
                if (_dapperParameters == null)
                {
                    _dapperParameters = new DynamicParameters();
                    foreach (var parameter in this.Values)
                    {
                        _dapperParameters.Add(parameter.Name, parameter.Value, parameter.DbType, parameter.ParameterDirection, parameter.Size);
                    }
                }
                return _dapperParameters;
            }
        }
        #endregion

        /// <summary>
        /// Add a parameter to this dynamic parameter list.
        /// </summary>
        public void Add(ParameterInfo parameter)
        {
            this[parameter.Name] = parameter;
        }

        /// <summary>
        /// Get parameter value
        /// </summary>
        public T Get<T>(string key) => (T)this[key].Value;

        /// <summary>
        /// Parameter Names
        /// </summary>
        public HashSet<string> ParameterNames => new HashSet<string>(this.Keys);

        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            // we just rely on Dapper.DynamicParameters which implements IDynamicParameters like a charm
            ((SqlMapper.IDynamicParameters)DapperParameters).AddParameters(command, identity);
        }

        /// <summary>
        /// After Dapper command is executed, we should get output/return parameters back
        /// </summary>
        void SqlMapper.IParameterCallbacks.OnCompleted()
        {
            var dapperParameters = DapperParameters;

            // Update output and return parameters back
            foreach (var oparm in this.Values.Where(p => p.ParameterDirection != ParameterDirection.Input))
            {
                oparm.Value = dapperParameters.Get<object>(oparm.Name);
                oparm.OutputCallback?.Invoke(oparm.Value);
            }
        }


        #region Add Existing Parameter
        /// <summary>
        /// Merges single parameter into this list. <br />
        /// Checks for name clashes, and will rename parameter if necessary. <br />
        /// If parameter is renamed the new name will be returned, else returns null.
        /// </summary>
        public string MergeParameter(ParameterInfo parameter)
        {
            string newParameterName = parameter.Name;
            int _autoNamedParametersCount = 0;
            while (ParameterNames.Contains(newParameterName))
            {
                newParameterName = "p" + _autoNamedParametersCount.ToString();
                _autoNamedParametersCount++;
            }

            // Create a copy, it's safer
            ParameterInfo newParameter = new ParameterInfo(
                name: newParameterName, 
                value: parameter.Value,
                dbType: parameter.DbType, 
                direction: parameter.ParameterDirection,
                size: parameter.Size, 
                precision: parameter.Precision,
                scale: parameter.Scale
            );
            newParameter.OutputCallback = parameter.OutputCallback;

            Add(newParameter);
            if (newParameterName != parameter.Name)
                return newParameterName;
            return null;
        }

        /// <summary>
        /// Merges multiple parameters into this list. <br />
        /// Checks for name clashes, and will rename parameters if necessary. <br />
        /// If some parameter is renamed the returned Sql statement will containg the original sql replaced with new names, else (if nothing changed) returns null. <br />
        /// </summary>
        public string MergeParameters(ParameterInfos parameters, string sql)
        {
            Dictionary<string, string> renamedParameters = new Dictionary<string, string>();
            foreach (var parameter in parameters.Values)
            {
                string newParameterName = MergeParameter(parameter);
                if (newParameterName != null)
                    renamedParameters.Add("@" + parameter.Name, "@" + newParameterName);
            }
            if (renamedParameters.Any())
            {
                Regex matchParametersRegex = new Regex("(?:\\s|\\b) (" + string.Join("|", renamedParameters.Select(p=>p.Key)) + ") (?:\\b|\\s*)",
                    RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
                string newSql = matchParametersRegex.Replace(sql, match => {
                    Group group = match.Groups[1];
                    string replace = renamedParameters[group.Value];
                    return String.Format("{0}{1}{2}", match.Value.Substring(0, group.Index - match.Index), replace, match.Value.Substring(group.Index - match.Index + group.Length));
                });
                return newSql;
            }
            return null;
        }

        #endregion


    }


}
