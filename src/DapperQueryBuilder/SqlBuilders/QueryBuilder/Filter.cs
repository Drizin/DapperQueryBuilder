using System;

namespace DapperQueryBuilder
{
    public class Filter : InterpolatedSql.SqlBuilders.Filter
    {
        /// <summary>
        /// New Filter statement. <br />
        /// Example: $"[CategoryId] = {categoryId}" <br />
        /// Example: $"[Name] LIKE {productName}"
        /// </summary>
        public Filter(FormattableString filter) : base(filter)
        {
        }
    }
}
