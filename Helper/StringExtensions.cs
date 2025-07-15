using System.Text.RegularExpressions;

namespace Helper
{
    public static class StringExtensions
    {
        /// <summary>
        /// check link is http or https
        /// </summary>
        /// <param name="input"></param>
        /// <returns>true if input is http link</returns>
        public static bool IsHttpLink(this string? input)
        {
            if (input == null)
                return false;
            return input.StartsWith("http://") || input.StartsWith("https://");
        }

        public static bool IsValidUrl(this string? input)
        {
            if (input == null)
                return false;

            string[] patterns =
            [
                @"^(https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?$",
                @"^(https?:\/\/)?(\d{1,3}\.){3}\d{1,3}:\d{1,5}$",
                @"^(https?:\/\/)?localhost:\d{1,5}$"
            ];

            foreach (string pattern in patterns)
            {
                if (Regex.IsMatch(input, pattern))
                    return true;
            }

            return false;
        }

        public static string? ToUnixPath(this string? input)
        {
            return input?.Replace("\\", "/");
        }
    }
}