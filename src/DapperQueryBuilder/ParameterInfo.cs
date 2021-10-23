using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// SQL parameter which is passed to Dapper
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{Name,nq} = {Value,nq}")]
    public class ParameterInfo
    {
        #region Members
        /// <summary>
        /// Auto-generated name of parameter like p0, p1, etc. Does NOT contain database-specific prefixes like @ or :
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

        internal Action<object> OutputCallback { get; set; }
        #endregion

        #region ctors
        /// <summary>
        /// New Parameter
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="dbType">The type of the parameter.</param>
        /// <param name="direction">The in or out direction of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        public ParameterInfo(string name, object value, DbType? dbType, ParameterDirection? direction, int? size) : this(name, value, dbType, direction, size, null, null)
        {
        }

        /// <summary>
        /// New Parameter
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <param name="dbType">The type of the parameter.</param>
        /// <param name="direction">The in or out direction of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        /// <param name="precision">The precision of the parameter.</param>
        /// <param name="scale">The scale of the parameter.</param>
        public ParameterInfo(string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            this.Name = name;
            this.Value = value;
            this.DbType = dbType;
            this.ParameterDirection = direction ?? ParameterDirection.Input;
            this.Size = size;
            this.Precision = precision;
            this.Scale = scale;
        }

        /// <summary>
        /// Creates a new Output Parameter (can be Output, InputOutput, or ReturnValue) <br />
        /// and registers a callback action which (after command invocation) will populate back parameter output value into an instance property.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="target">Target variable where output value will be set.</param>
        /// <param name="expression">Property where output value will be set. If it's InputOutput type this value will be passed.</param>
        /// <param name="dbType">The type of the parameter.</param>
        /// <param name="direction">The type of output of the parameter.</param>
        /// <param name="size">The size of the parameter.</param>
        /// <param name="precision">The precision of the parameter.</param>
        /// <param name="scale">The scale of the parameter.</param>
        public static ParameterInfo CreateOutputParameter<T,TP>(string name, T target, Expression<Func<T, TP>> expression, OutputParameterDirection direction = OutputParameterDirection.Output, DbType? dbType = null, int? size = null, byte? precision = null, byte? scale = null)
        {
            object value = null;

            // For InputOutput we send current value
            if (direction == OutputParameterDirection.InputOutput)
                value = expression.Compile().Invoke(target);

            ParameterInfo parameter = new ParameterInfo(name, value, dbType, (ParameterDirection)direction, size, precision, scale);

            var setter = GetSetter(expression);
            parameter.OutputCallback = new Action<object>(o =>
            {
                TP val;
                if (o is TP)
                    val = (TP)o;
                else
                {
                    try
                    {
                        val = (TP)Convert.ChangeType(o, typeof(TP));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Can't convert {parameter.Name} ({parameter.Value}) to type {typeof(TP).Name}", ex);
                    }
                }
                setter(target, val); // TP (property type) must match the return value
            });

            return parameter;
        }
        #endregion

        #region Enums
        /// <summary>
        /// Type of Output
        /// </summary>
        public enum OutputParameterDirection
        {
            /// <summary>
            /// The parameter is an output parameter.
            /// </summary>
            Output = 2,

            /// <summary>
            /// The parameter is capable of both input and output.
            /// </summary>
            InputOutput = 3,

            /// <summary>
            /// The parameter represents a return value from an operation such as a stored procedure, built-in function, or user-defined function.
            /// </summary>
            ReturnValue = 6
        }
        #endregion

        /// <summary>
        /// Convert a lambda expression for a getter into a setter
        /// </summary>
        private static Action<T, TProperty> GetSetter<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var memberExpression = (MemberExpression)expression.Body;
            var property = (PropertyInfo)memberExpression.Member;
            var setMethod = property.GetSetMethod();

            var parameterT = Expression.Parameter(typeof(T), "x");
            var parameterTProperty = Expression.Parameter(typeof(TProperty), "y");

            var newExpression =
                Expression.Lambda<Action<T, TProperty>>(
                    Expression.Call(parameterT, setMethod, parameterTProperty),
                    parameterT,
                    parameterTProperty
                );

            return newExpression.Compile();
        }

    }
}
