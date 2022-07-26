using System;
using System.Collections.Generic;
using System.Text;

namespace kate.shared.Extensions
{
    public static class StringExtension
    {
        public static string[] Split(this string value, string seperator)
        {
            return value.Split(value.ToCharArray());
        }
    }
}
