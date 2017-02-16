﻿using System.Collections;
using System.Linq;

namespace RedArrow.Argo.Client.Flurl.Shared
{
    /// <summary>
	/// Represents an individual name/value pair within a URL query.
	/// </summary>
	public class QueryParameter
    {
        private object _value;
        private string _encodedValue;

        /// <summary>
        /// Creates a new instance of a query parameter.
        /// </summary>
        public QueryParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Creates a new instance of a query parameter. Allows specifying whether string value provided has
        /// already been URL-encoded.
        /// </summary>
        public QueryParameter(string name, string value, bool isEncoded)
        {
            Name = name;
            if (isEncoded)
            {
                _encodedValue = value as string;
                _value = Url.DecodeQueryParamValue(_encodedValue);
            }
            else
            {
                Value = value;
            }
        }

        /// <summary>
        /// The name (left side) of the query parameter.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value (right side) of the query parameter.
        /// </summary>
        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                _encodedValue = null;
            }
        }

        /// <summary>
        /// Returns the string ("name=value") representation of the query parameter.
        /// </summary>
        /// <param name="encodeSpaceAsPlus">Indicates whether to encode space characters with "+" instead of "%20".</param>
        /// <returns></returns>
        public string ToString(bool encodeSpaceAsPlus)
        {
            if (Value is IEnumerable && !(Value is string))
            {
                return string.Join("&",
                    from v in (Value as IEnumerable).Cast<object>()
                    where v != null
                    let encoded = Url.EncodeQueryParamValue(v, encodeSpaceAsPlus)
                    select $"{Name}={encoded}");
            }
            else
            {
                var encoded = _encodedValue ?? Url.EncodeQueryParamValue(_value, encodeSpaceAsPlus);
                return $"{Name}={encoded}";
            }
        }
    }
}
