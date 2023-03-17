// ReSharper disable once CheckNamespace
namespace System;

public static class MathExtensions
{
    public static double Pmi(double conditionalProbability, double probability)
    {
        return Math.Log2(conditionalProbability / probability);
    }
}