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
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="dbType">The type of the parameter.</param>
        /// <param name="direction">The in or out direction of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        public void Add(string name, object value, DbType? dbType, ParameterDirection? direction, int? size)
        {
            this[name] = new ParameterInfo()
            {
                Name = name,
                Value = value,
                ParameterDirection = direction ?? ParameterDirection.Input,
                DbType = dbType,
                Size = size
            };
        }

        /// <summary>
        /// Add a parameter to this dynamic parameter list.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="dbType">The type of the parameter.</param>
        /// <param name="direction">The in or out direction of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        /// <param name="precision">The precision of the parameter.</param>
        /// <param name="scale">The scale of the parameter.</param>
        public void Add(string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            this[name] = new ParameterInfo()
            {
                Name = name,
                Value = value,
                ParameterDirection = direction ?? ParameterDirection.Input,
                DbType = dbType,
                Size = size,
                Precision = precision,
                Scale = scale
            };
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

            foreach (var oparm in this.Values.Where(p => p.ParameterDirection != ParameterDirection.Input))
                this[oparm.Name].Value = dapperParameters.Get<object>(oparm.Name);
        }

    }


}
