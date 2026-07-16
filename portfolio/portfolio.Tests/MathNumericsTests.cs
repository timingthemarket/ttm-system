using System;
using System.Linq;
using MathNet.Numerics;
using Xunit;

namespace portfolio.Tests;

public class MathNumericsTests
{
    [Fact]
    public void TestMathOperations()
    {
        // Example test for basic math operations
        var xVals = Enumerable.Range(0, 15).Select(x => (double)x).ToArray();
        double[] array = new double[15];
        Random rand = new Random(42);

        for (int i = 0; i < array.Length; i++)
        {
            double x = i / 14.0; // Normalize to [0, 1]
    
            // Strong positive trend that slows down (negative acceleration)
            double ba = 1.0 + 4.0 * x - 2.0 * x * x * x;
            double noise = (rand.NextDouble() - 0.5) * 0.15;
    
            array[i] = Math.Max(0.1, ba + noise); // Keep positive
        }
        
        double[] p = Fit.Polynomial(xVals, array, 2); 
    }
}