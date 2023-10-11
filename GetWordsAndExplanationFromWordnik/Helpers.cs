using System.Text;

namespace GetWordsAndExplanationFromWordnik
{
    public static class Helpers
    {
        /// <summary>
        /// Zasadniczo usuwa tagi html z tekstu (<>) i zamienia je na cudzysłowy - na potrzeby wordnik'a.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ParseStringFromHtml(string str)
        {

            StringBuilder sb = new StringBuilder();

            bool toQuotation = false;

            foreach (char c in str)
            {
                if (c == '<')
                {
                    toQuotation = true;
                    continue;
                }
                else if (c == '>' && toQuotation)
                {
                    sb.Append('"');
                    toQuotation = false;
                    continue;
                }

                if (!toQuotation) sb.Append(c);

            }

            return sb.ToString();
        }
    }
}
