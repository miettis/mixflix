using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MixFlix.Crawler
{
    public static class Helpers
    {
        public static string Base64Decode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            try
            {
                byte[] data = Convert.FromBase64String(input);
                return Encoding.UTF8.GetString(data);
            }
            catch (FormatException)
            {
                // Handle invalid Base64 string
                return input; // or throw an exception, or return an empty string, etc.
            }
        }

        // Checks if text contains only local (Latin + Scandinavian, German, French, Spanish) letters, punctuation, or whitespace
        public static bool IsLocalText(string text)
        {
            if (string.IsNullOrEmpty(text)) return true;
            // Allow: Basic Latin, punctuation, whitespace, and extended European letters
            // Scandinavian: åäöÅÄÖøØæÆ
            // German: ßüÜöÖäÄ (ä, Ä, ö, Ö, ü, Ü already included), ß
            // French: çÇéÉèÈêÊëËàÀâÂîÎïÏôÔûÛùÙüÜœŒæÆ
            // Spanish: ñÑáÁéÉíÍóÓúÚüÜ
            // Regex: any character NOT in allowed set
            return !Regex.IsMatch(text, @"[^\p{IsBasicLatin}\p{P}\p{Zs}åäöÅÄÖøØæÆßüÜçÇéÉèÈêÊëËàÀâÂîÎïÏôÔûÛùÙœŒñÑáÁíÍóÓúÚ]");
        }
    }
}
