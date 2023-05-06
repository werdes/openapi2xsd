using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAPI2XSD.Extensions
{
    public static class IEnumerableExtensions
    {
        public static string Join<T>(this IEnumerable<T> values, string separator)
        {
            string returnValue = string.Empty;
            for(int i = 0; i < values.Count(); i++)
            {
                if (i > 0) returnValue += separator;
                returnValue += values.ElementAt(i)?.ToString();
            }
            return returnValue; 
        }
    }
}
