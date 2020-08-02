using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// Can be both individual filter or a list of filters.
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Writes the SQL Statement of the filter
        /// </summary>
        void WriteFilter(StringBuilder sb);

        /// <summary>
        /// Merges parameters from this filter into a CommandBuilder. <br />
        /// Checks for name clashes, and will rename parameters (in CommandBuilder) if necessary. <br />
        /// If some parameter is renamed the underlying Sql statement will have the new parameter names replaced by their new names.<br />
        /// This method does NOT append Parser SQL to CommandBuilder SQL (you may want to save this SQL statement elsewhere)
        /// </summary>
        void MergeParameters(DynamicParameters target);
    }
}
