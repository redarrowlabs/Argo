using System;
using System.Collections.Generic;

namespace RedArrow.Argo.Client.Extensions
{
    public static class StringExtensions
    {
        public static string Camelize(this string source)
        {
            return source.Substring(0, 1).ToLower() + source.Substring(1);
        }
    }
}