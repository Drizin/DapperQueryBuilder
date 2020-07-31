using System;
using System.Collections.Generic;
using System.Text;

namespace DapperQueryBuilder
{
    /// <summary>
    /// RawString is just a wrapper around string (with implicit conversion to/from string) which allows us to prioritize methods which use IFormattable (interpolated strings) instead of strings <br />
    /// If you use interpolated strings (which allow to use a wide range of action delegates) you'll end up using the methods overloads which accept IFormattable. <br />
    /// If you just pass a regular string it will be converted to RawString. 
    /// Based on https://www.damirscorner.com/blog/posts/20180921-FormattableStringAsMethodParameter.html
    /// </summary>
    public class RawString
    {
        private string Value { get; }

        private RawString(string str)
        {
            Value = str;
        }

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator RawString(string str) => new RawString(str);

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator RawString(FormattableString formattable) => new RawString(formattable.ToString());

        /// <summary>
        /// Implicit conversion
        /// </summary>
        public static implicit operator string(RawString raw) => raw.Value;
    }
}
