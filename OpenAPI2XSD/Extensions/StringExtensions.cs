using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenAPI2XSD.Extensions
{
    public static class StringExtensions
    {
        public static string ReduceToSimple(this string str)
        {
            return Regex.Replace(str, "[^0-9a-zA-Z]+", "");
        }
    }
}
