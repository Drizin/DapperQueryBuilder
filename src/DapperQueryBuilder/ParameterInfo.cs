using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// SQL parameter which is passed to Dapper
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// Name of parameter. 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Value of parameter
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Parameters added through string interpolation are usually input parameters (passed from C# to SQL), <br />
        /// but you may explicitly describe parameters as Output, InputOutput, or ReturnValues.
        /// </summary>
        public ParameterDirection ParameterDirection { get; set; }

        /// <summary>
        /// Parameters added through string interpolation usually do not need to define their DbType, and Dapper will automatically detect the correct type, <br />
        /// but it's possible to explicitly define the DbType (which Dapper will map to corresponding type in your database)
        /// </summary>
        public DbType? DbType { get; set; }

        /// <summary>
        /// Parameters added through string interpolation usually do not need to define their Size, and Dapper will automatically detect the correct size, <br />
        /// but it's possible to explicitly define the size (usually for strings, where in some specific scenarios you can get better performance by passing the exact data type)
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// Parameters added through string interpolation usually do not need to define this, as Dapper will automatically calculate the correct value
        /// </summary>
        public byte? Precision { get; set; }

        /// <summary>
        /// Parameters added through string interpolation usually do not need to define this, as Dapper will automatically calculate the correct value
        /// </summary>
        public byte? Scale { get; set; }
    }
}
