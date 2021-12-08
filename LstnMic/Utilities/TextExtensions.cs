using System.Collections.Generic;
using System.Linq;

namespace BlueMaria.Utilities
{
    public static class TextExtensions
    {
        public static string SafeAggregate(this IEnumerable<string> list)
        {
            if (list == null) return "";
            return list.SafeAggregate(", ");
        }

        public static string SafeAggregate(this IEnumerable<string> list, string delimiter)
        {
            return list
                .Aggregate("", (output, next) => output + ((output.Length > 0) ? delimiter.ToString() : "") + (next ?? ""));
        }

    }

}
