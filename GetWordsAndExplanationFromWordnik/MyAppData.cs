namespace GetWordsAndExplanationFromWordnik
{
    internal static class MyAppData
    {
        internal const string ApiKey = "xtkqlekin2bswp3h16r5vf5yzv7e21k7q2d90untdtwvd9ntc";
        internal const string BaseAddressOfWordnikExplanationApi = @"http://api.wordnik.com/v4/word.json/";
        internal const string ApiPathForAskingOfWordFromWordnik = @$"/randomWord?api_key={ApiKey}";
        internal const string BaseAddressOfWordnikWordsApi = @"http://api.wordnik.com/v4/words.json/";
        internal const string ApiPathForAskingOfExplanationFromWordnik = @$"/definitions?limit=1&api_key={ApiKey}";
        internal const int HowManyMaxWordsFromWordnikOnOneRequest = 9;
        internal const int HowManyMultiple9 = 1;
        internal const int HowManyWordsSelected = 100; //9;
        internal const int MaxWordLength = 30;
        internal const int MaxWordExplanationLenght = 100;
    }
}
