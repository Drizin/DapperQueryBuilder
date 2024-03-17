using System.Data;

namespace InterpolatedSql.Dapper.SqlBuilders.InsertUpdateBuilder
{
    /// <summary>
    /// Extends IDbConnection to easily build InsertUpdateBuilder
    /// </summary>
    public static partial class IDbConnectionExtensions
    {
        /// <summary>
        /// Creates a new empty InsertUpdateBuilder over current connection
        /// </summary>
        public static InsertUpdateBuilder InsertUpdateBuilder(this IDbConnection cnn, string tableName)
        {
            return new InsertUpdateBuilder(tableName, cnn);
        }
    }
}
