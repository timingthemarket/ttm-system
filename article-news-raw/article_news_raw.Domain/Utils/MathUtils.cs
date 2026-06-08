using MathNet.Numerics.LinearAlgebra;

namespace article_news_raw.Domain.Utils;

public class MathUtils
{
    public static double GetCosineSimilarity(double[] v1, double[] v2)
    {
        var vector1 = Vector<double>.Build.DenseOfArray(v1);
        var vector2 = Vector<double>.Build.DenseOfArray(v2);

        var matrix1 = Matrix<double>.Build.DenseOfColumnVectors(vector1).Transpose();

        var norm1 = vector1.L2Norm();
        var norm2 = vector2.L2Norm();

        var dotProduct = matrix1.Multiply(vector2);

        return dotProduct.First() / (norm1 * norm2);
    }

    /// <summary>
    ///     https://developers.google.com/machine-learning/data-prep/transform/normalization
    /// </summary>
    /// <param name="arry"></param>
    /// <param name="logNorm"></param>
    /// <returns></returns>
    public static double[] NormalizeVector(double[] arry, bool logNorm = false)
    {
        var arryCopy = arry.ToList();

        var min = arryCopy.Min();
        var max = arryCopy.Max();

        var maxMin = max - min;

        if (Math.Abs(maxMin) < 0.0001)
            maxMin = 0.001;

        if (logNorm)
            arryCopy = arryCopy.Select(Math.Log10).ToList();

        return arryCopy.Select(x => (x - min) / maxMin).ToArray();
    }

    public static double[] GetArrayWithValues(int length, double value, double? incVal = null) =>
        Enumerable.Range(0, length).Select(i =>
        {
            if (incVal.HasValue) 
                return value * (i + 1) * incVal.Value;

            return value;
        }).ToArray();
}