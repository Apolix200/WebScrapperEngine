using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapperEngine.Action
{
    public class StringSimilarity
    {
        public static double CompareStrings(string str1, string str2)
        {
            if (string.IsNullOrWhiteSpace(str1) || string.IsNullOrWhiteSpace(str2))
                return 0.0;

            var pairs1 = WordLetterPairs(str1.ToUpper());
            var pairs2 = WordLetterPairs(str2.ToUpper());

            if (pairs1.Count == 0 || pairs2.Count == 0)
                return 0.0;

            // Build frequency map for pairs2
            var freq2 = new Dictionary<string, int>();
            foreach (var p in pairs2)
            {
                if (freq2.ContainsKey(p))
                    freq2[p]++;
                else
                    freq2[p] = 1;
            }

            int intersection = 0;

            // Count intersections using the dictionary
            foreach (var p1 in pairs1)
            {
                if (freq2.TryGetValue(p1, out int count) && count > 0)
                {
                    intersection++;
                    freq2[p1] = count - 1;
                }
            }

            int union = pairs1.Count + pairs2.Count;
            return Math.Round(2.0 * intersection / union, 5);
        }

        private static List<string> WordLetterPairs(string str)
        {
            var pairs = new List<string>();
            var words = str.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                var lp = LetterPairs(word);
                pairs.AddRange(lp);
            }

            return pairs;
        }

        private static IEnumerable<string> LetterPairs(string str)
        {
            if (str.Length < 2)
                yield break;

            for (int i = 0; i < str.Length - 1; i++)
                yield return str.Substring(i, 2);
        }
    }
}
