namespace Helper
{
    public static class StringExtensions
    {
        /// <summary>
        /// check link is http or https
        /// </summary>
        /// <param name="input"></param>
        /// <returns>true if input is http link</returns>
        public static bool IsHttpLink(this string input)
        {
            return input.StartsWith("http://") || input.StartsWith("https://");
        }
    }
}