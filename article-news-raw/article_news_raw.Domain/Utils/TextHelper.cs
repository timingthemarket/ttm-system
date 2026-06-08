
using FuzzySharp;
using MathNet.Numerics.LinearAlgebra;

namespace article_news_raw.Domain.Utils;

public class TextHelper
{
    public static double GetStringSimilarity(string str1, string str2, bool ignoreCase = true, bool fuzzyMatch = true)
    {
        var str1Copy = $"{str1}";
        var str2Copy = $"{str2}";

        if (ignoreCase)
        {
            str1Copy = str1Copy.ToLower();
            str2Copy = str2Copy.ToLower();
        }

        if (fuzzyMatch)
        {
            var ratio = Fuzz.Ratio(str1Copy, str2Copy);
            return ratio / 100.0;
        }

        return CalculateSimpleCosineSimilarity(str1Copy, str2Copy);
    }

    private static double CalculateSimpleCosineSimilarity(string str1, string str2)
    {
        var str1Chars = str1.ToCharArray();
        var str2Chars = str2.ToCharArray();

        var distinctCharArray = str1Chars
            .Concat(str2Chars)
            .Distinct()
            .ToArray();

        var characterValueDictionary = Enumerable.Range(0, distinctCharArray.Length)
            .Select(i => new { Value = i + 1, Character = distinctCharArray[i] })
            .ToDictionary(x => x.Character, x => x.Value);

        var str1CharsValue = str1Chars.Select(c => (double)characterValueDictionary[c]).ToList();
        var str2CharsValue = str2Chars.Select(c => (double)characterValueDictionary[c]).ToList();

        if (str1CharsValue.Count > str2CharsValue.Count)
        {
            var len = str1CharsValue.Count - str2CharsValue.Count;
            var arry = MathUtils.GetArrayWithValues(len, 0, 1.1);
            str2CharsValue.AddRange(arry);
        } else if (str1CharsValue.Count < str2CharsValue.Count)
        {
            var len = str2CharsValue.Count - str1CharsValue.Count;
            var arry = MathUtils.GetArrayWithValues(len, 0, 1.1);
            str1CharsValue.AddRange(arry);
        }

        var normArry1 = MathUtils.NormalizeVector(str1CharsValue.ToArray());
        var normArry2 = MathUtils.NormalizeVector(str2CharsValue.ToArray());

        var similarity = MathUtils.GetCosineSimilarity(normArry1, normArry2);

        if (double.IsNaN(similarity))
            return 0;
        
        return similarity;
    }
}