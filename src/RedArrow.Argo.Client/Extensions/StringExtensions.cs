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

        public static string Pluralize(this string text)
        {
            // Create a dictionary of exceptions that have to be checked first
            // This is very much not an exhaustive list!
            Dictionary<string, string> exceptions = new Dictionary<string, string>() {
                { "man", "men" },
                { "woman", "women" },
                { "child", "children" },
                { "tooth", "teeth" },
                { "foot", "feet" },
                { "mouse", "mice" },
                { "belief", "beliefs" } };

            if (exceptions.ContainsKey(text.ToLowerInvariant()))
            {
                return exceptions[text.ToLowerInvariant()];
            }

            if (text.EndsWith("y", StringComparison.OrdinalIgnoreCase) &&
                !text.EndsWith("ay", StringComparison.OrdinalIgnoreCase) &&
                !text.EndsWith("ey", StringComparison.OrdinalIgnoreCase) &&
                !text.EndsWith("iy", StringComparison.OrdinalIgnoreCase) &&
                !text.EndsWith("oy", StringComparison.OrdinalIgnoreCase) &&
                !text.EndsWith("uy", StringComparison.OrdinalIgnoreCase))
            {
                return text.Substring(0, text.Length - 1) + "ies";
            }

            if (text.EndsWith("us", StringComparison.CurrentCultureIgnoreCase))
            {
                // http://en.wikipedia.org/wiki/Plural_form_of_words_ending_in_-us
                return text + "es";
            }

            if (text.EndsWith("ss", StringComparison.CurrentCultureIgnoreCase))
            {
                return text + "es";
            }

            if (text.EndsWith("s", StringComparison.CurrentCultureIgnoreCase))
            {
                return text;
            }

            if (text.EndsWith("x", StringComparison.CurrentCultureIgnoreCase) ||
                text.EndsWith("ch", StringComparison.CurrentCultureIgnoreCase) ||
                text.EndsWith("sh", StringComparison.CurrentCultureIgnoreCase))
            {
                return text + "es";
            }

            if (text.EndsWith("f", StringComparison.CurrentCultureIgnoreCase) && text.Length > 1)
            {
                return text.Substring(0, text.Length - 1) + "ves";
            }

            if (text.EndsWith("fe", StringComparison.CurrentCultureIgnoreCase) && text.Length > 2)
            {
                return text.Substring(0, text.Length - 2) + "ves";
            }

            return text + "s";
        }
    }
}