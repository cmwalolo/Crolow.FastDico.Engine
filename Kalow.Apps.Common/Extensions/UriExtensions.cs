using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Kalow.Apps.Common.Extensions
{
    public static class UriExtensions
    {
        public static Dictionary<string, string> ParseQueryParameters(this Uri uri)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);

            foreach (string key in query)
            {
                if (key != null)
                    result[key] = query[key]!;
            }

            return result;
        }
    }
}
