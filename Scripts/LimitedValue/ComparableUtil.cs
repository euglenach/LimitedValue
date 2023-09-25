using System;

namespace LimitedValues
{
    internal static class ComparableUtil
    {
        public static bool IsUnderMax<T>(T value, T max) where T: IComparable<T>
        {
            return value.CompareTo(max) <= 0;
        }

        public static bool  IsUpperMin<T>(T value, T min) where T: IComparable<T>
        {
            return value.CompareTo(min) >= 0;
        }

        public static bool InBounded<T>(T value, T min, T max) where T: IComparable<T>
        {
            return IsUpperMin(value,min) && IsUnderMax(value, max);
        }

        public static bool IsValid<T>(T value, T min, T max) where T: IComparable<T>
        {
            return InBounded(value, min, max) && IsUpperMin(max, min) && IsUnderMax(min, max);
        }
    }
}
