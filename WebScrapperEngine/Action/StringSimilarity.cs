using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapperEngine.Action
{
    public class StringSimilarity
    {
        public static double compareStrings(String str1, String str2) 
        {
            List<String> pairs1 = wordLetterPairs(str1.ToUpper());
            List<String> pairs2 = wordLetterPairs(str2.ToUpper());

            int intersection = 0;
            int union = pairs1.Count + pairs2.Count;

            for (int i = 0; i < pairs1.Count; i++)
            {
                var pair1 = pairs1.ElementAt(i);

                for (int j = 0; j < pairs2.Count; j++)
                {
                    var pair2 = pairs2.ElementAt(j);

                    if (pair1.Equals(pair2))
                    {
                        intersection++;
                        pairs2.RemoveAt(j);
                        break;
                    }
                }
            }

            return Math.Round(2.0 * intersection / union, 5);
        }
        private static List<String> wordLetterPairs(String str) 
        {
            List<String> allPairs = new List<String>();

            // Tokenize the string and put the tokens/words into an array

            String[] words = str.Split(' ');

            // For each word

            for (int w = 0; w < words.Length; w++)
            {

                // Find the pairs of characters

                String[] pairsInWord = letterPairs(words[w]);

                for (int p = 0; p < pairsInWord.Length; p++)
                {

                    allPairs.Add(pairsInWord[p]);

                }

            }

            return allPairs;
        }
        private static String[] letterPairs(String str)
        {
            int numPairs = str.Length - 1;

            String[] pairs = new String[numPairs];

            for (int i = 0; i < numPairs; i++)
            {
                pairs[i] = str.Substring(i, 2);
            }

            return pairs;
        }
    }
}
