using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DapperQueryBuilder
{
	/// <summary>
	/// An adapter class that allows a normal SQL string and parameters object
	/// to be used by the DapperQueryBuilder.QueryBuilder constructor instead
	/// of a plain formattable string. This allows the initial SQL template to be
	/// constructed using string interpolation.
	/// </summary>
	public class InterpolatedStatementAdapter : FormattableString
	{
		readonly string _originalSql;
		readonly string _sql;
		readonly object[] _arguments;
		static readonly Regex _regex = new Regex(@"(@[a-z0-9_]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Instantiate an formattable string by replacing parameters
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		public InterpolatedStatementAdapter(string sql, object parameters)
		{
			_originalSql = sql;

			//Convert parameters object to indexed object array
			var paramDict = ToDictionary(parameters);
			_arguments = new object[paramDict.Keys.Count];
			var map = new Dictionary<string, int>();
			int idx = 0;
			foreach (string key in paramDict.Keys)
			{
				_arguments[idx] = paramDict[key];
				map[key] = idx;
				idx++;
			}

			//Convert SQL to formattable string
			_sql = _regex.Replace(sql, m =>
			{
				string paramName = m.Value.Substring(1);

				if (!map.ContainsKey(paramName))
					throw new Exception($"Parameter {paramName} not supplied");

				return "{" + map[paramName] + "}";
			});
		}

        #region FormattableString implementation
        /// <summary>
        /// Gets the number of arguments to be formatted.
        /// </summary>
        public override int ArgumentCount => _arguments.Length;
		/// <summary>
		/// Gets the original SQL statement
		/// </summary>
		public override string Format => _sql;
		/// <summary>
		/// Returns the argument at the specified index position.
		/// </summary>
		/// <param name="index">The index of the argument. Its value can range from zero to one less than the value of System.FormattableString.ArgumentCount.</param>
		/// <returns>The argument.</returns>
		public override object GetArgument(int index) => _arguments[index];
		/// <summary>
		/// Returns an object array that contains one or more objects to format.
		/// </summary>
		/// <returns>n object array that contains one or more objects to format.</returns>
		public override object[] GetArguments() => _arguments;
		/// <summary>
		/// Returns the original SQL statement
		/// </summary>
		/// <param name="formatProvider"></param>
		/// <returns>The original SQL statement</returns>
		public override string ToString(IFormatProvider formatProvider) => _originalSql;
		/// <summary>
		/// Returns the original SQL statement
		/// </summary>
		/// <returns>The original SQL statement</returns>
		public override string ToString() => _originalSql;
        #endregion

        static Dictionary<string, object> ToDictionary(object o)
		{
			var dictionary = new Dictionary<string, object>();

			if (o != null)
			{
				foreach (var propertyInfo in o.GetType().GetProperties())
				{
					if (propertyInfo.GetIndexParameters().Length == 0)
					{
						dictionary.Add(propertyInfo.Name, propertyInfo.GetValue(o, null));
					}
				}
			}

			return dictionary;
		}
	}
}
