using System;
using System.Text.RegularExpressions;

namespace Guanima.Redis.Utils
{
    public class StringUtils
    {
        const string IPRegexPattern = @"
            ^(                      # Anchor to the beginning of the line
                                    # Invalidations
              (?!25[6-9])           # If 256, 257...259 are found STOP
              (?!2[6-9]\d)          # If 260-299 stop
              (?![3-9]\d\d)         # if 300-999 stop
              (?!000)               # No zeros
              (\d{1,3})             # Between 1-3 numbers
              (?:[.-]?)             # Match but don't capture a . or -
             ){4,6}                 # IPV4 IPV6
            (?<Url>[A-Za-z.\d]*?)   # Subdomain is mostly text
            (?::)
            (?<Port>\d+)";

        private static readonly Regex IPRegex = new Regex(IPRegexPattern, 
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
 
        public static bool IsValidIpAddress(string candidate,out string host, out int port)
        {
            if (!String.IsNullOrEmpty(candidate))
            {
                Match m = IPRegex.Match(candidate);
                if (m.Success)
                {
                    host = m.Groups["Url"].Value;
                    port = int.Parse(m.Groups["Port"].Value);
                    return true;
                }
            }
            host = null;
            port = 6379;
            return false;
        }

        public static bool IsNumericIpAddress(string candidate)
        {
            string host = "";
            int port = 0;
            var good = IsValidIpAddress(candidate, out host, out port)
                        && String.IsNullOrEmpty(host);
            return good;
        }
    }
}
