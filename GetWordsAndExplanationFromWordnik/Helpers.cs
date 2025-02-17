using System.Reflection;
using System.Text;

namespace GetWordsAndExplanationFromWordnik;

public static class Helpers
{

    public static string ParseStringInOddWordnikHtml(string str)
    {

        StringBuilder sb = new();

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


    public static string GetMethodNameAndError(Exception ex)
    {
        string methodNameAndError = string.Empty;
        var declaringType = MethodBase.GetCurrentMethod()?.DeclaringType?.Name ?? "Unknown type";
        methodNameAndError += $"({declaringType}): {ex.Message}";
        return methodNameAndError;
    }


    public static string GetStringsAsOneString(List<string> lst)
    {
        return lst.Aggregate((i, j) => i + " | " + j);
    }

    public static string GetCurrentMethodName()
    {
        return System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name ?? "UnknownType";
    }
}
