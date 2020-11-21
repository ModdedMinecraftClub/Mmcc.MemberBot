using System;
using System.Text.RegularExpressions;

namespace Mmcc.MemberBot.Infrastructure
{
    public static class DurationConverter
    {
        /// <summary>
        /// Converts the string representation of a duration to its TimeSpan equivalent. A return value indicates whether the operation succeeded.
        /// </summary>
        /// <param name="duration">String representation of a duration.</param>
        /// <param name="result">When this method returns, contains the TimeSpan equivalent of the duration contained in `duration`, if the conversion succeeded, or null if the conversion failed.</param>
        /// <returns>Whether the operation succeeded.</returns>
        public static bool TryParseDurationString(string duration, out TimeSpan? result)
        {
            var matches = Regex.Matches(duration, "(\\d+)([smhd])");
            result = null;

            if (matches.Count == 0)
            {
                return false;
            }

            var number = int.Parse(matches[0].Groups[1].Value);
            var c = matches[0].Groups[1].Value;
            
            switch (c)
            {
                case "s":
                    result = new TimeSpan(0, 0, number);
                    return true;
                case "m":
                    result = new TimeSpan(0, number, 0);
                    return true;
                case "h":
                    result = new TimeSpan(number, 0, 0);
                    return true;
                case "d":
                    result = new TimeSpan(number, 0, 0, 0);
                    return true;
                default:
                    return false;
            }
        }
    }
}