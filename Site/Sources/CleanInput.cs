using System;
using System.Text.RegularExpressions;

namespace Site.Sources
{
    public static class Cleaner
    {
        public static string СlearSpaces(string value)
        {
            try
            {
                return Regex.Replace(value, @"[\s]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            catch (RegexMatchTimeoutException)
            {
                return "";
            }
        }

        public static string СlearWord(string value)
        {
            try
            {
                return Regex.Replace(value, @"[^\w]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            catch (RegexMatchTimeoutException)
            {
                return "";
            }
        }
    }
}