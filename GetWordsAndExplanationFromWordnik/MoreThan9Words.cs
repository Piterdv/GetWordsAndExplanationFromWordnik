using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GetWordsAndExplanationFromWordnik
{
    public class MoreThan9Words
    {
        private readonly IListOfWords _listOfWords;
        private readonly ILogger<MoreThan9Words> _logger;
        private readonly IConfiguration _config;

        public MoreThan9Words(IListOfWords listOfWords, ILogger<MoreThan9Words> logger, IConfiguration config)
        {
            _listOfWords = listOfWords;
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Get more than 9 words from Wordnik, because you should create new account for every new 9 words on 1 minute
        /// </summary>
        /// <param name="moreThan9">True - more, false only one word.</param>
        /// <returns></returns>
        public async Task<List<string>> GetMoreThen9Words(bool moreThan9 = true)
        {
            List<string> words = new List<string>();
            int count = 0;

            int howManyWords = moreThan9 ? _config.GetValue<int>("HowManyWordsGet") : 1; 

            for (int i = 0; i < howManyWords; i++)
            {

                count++;

                var result = await _listOfWords.GetWord(!moreThan9);
                foreach (var item in result)
                {
                    words.Add(item);
                    //Console.WriteLine(item);
                }

                if (count >= _config.GetValue<int>("HowManyWordsGet")) break;

                //Wordnik has limit 9 requests per minute
                if (moreThan9 && i < _config.GetValue<int>("HowManyWordsGet") - 1) Task.Delay(60 * 1000).Wait();

            }

            return words;
        }
    }
}
