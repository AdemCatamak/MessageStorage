using System;

namespace TestUtility
{
    public static class AssertThat
    {
        public static void GreaterThan<T>(T minValue, T actual, string? message = null)
            where T : IComparable
        {
            if (actual.CompareTo(minValue) > 0)
            {
                return;
            }

            string exceptionMessage = message ?? $"Threshold : {minValue} -- Actual : {actual}";
            throw new AssertGreaterThanException(exceptionMessage);
        }

        public static void LessThan<T>(T maxValue, T actual, string? message = null)
            where T : IComparable
        {
            if (actual.CompareTo(maxValue) < 0)
            {
                return;
            }

            string exceptionMessage = message ?? $"Threshold : {maxValue} -- Actual : {actual}";
            throw new AssertLessThanException(exceptionMessage);
        }
    }

    public class AssertGreaterThanException : Exception
    {
        public AssertGreaterThanException(string message) : base(message)
        {
        }
    }

    public class AssertLessThanException : Exception
    {
        public AssertLessThanException(string message) : base(message)
        {
        }
    }
}