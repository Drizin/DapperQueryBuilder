namespace DapperQueryBuilder
{
    /// <summary>
    /// Class for storing the values of the limit and offset that will be added to the sql statement
    /// </summary>
    internal class Limit
    {
        public Limit(int numRows) : this(numRows, null) { }

        public Limit(int numRows, int? offset)
        {
            Offset = offset;
            NumRows = numRows;
        }

        /// <summary>
        /// How many rows to offset before taking the limit. If null, then doesn't include an offset
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        /// Number of rows to return in the sql query
        /// </summary>
        public int NumRows { get; set; }

        /// <summary>
        /// Generates the SQL statement to add the limit and offset to the SQL script
        /// </summary>
        /// <returns></returns>
        public string CreateSql()
        {
            if (Offset is null)
            {
                return $"LIMIT {NumRows}";
            }
            return $"LIMIT {NumRows} OFFSET {Offset.Value}";
        }
    }
}
