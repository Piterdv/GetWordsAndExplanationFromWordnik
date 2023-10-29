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

        /// <summary>
        /// Pobieram tylko nazwę metody i błąd.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string GetMethodNameAndError(Exception ex)
        {
            string mn = string.Empty;
            mn += ("({0}): " + ex.Message, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);
            return mn;
        }

        /// <summary>
        /// Zamienia listę stringów na jeden string, gdzie elementy listy są oddzielone znakiem "|".
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        public static string GetListAsString(List<string> lst)
        {
            return lst.Aggregate((i, j) => i + " | " + j);
        }
    }
}
