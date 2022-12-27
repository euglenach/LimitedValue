using System;

namespace LimitedValue
{
    public interface IReadOnlyLimitedValue<T> where T: IComparable<T>
    {
        T Value {get;}
        T Min {get;}
        T Max {get;}
        bool InBounded(T value);
    }

    public interface ILimitedValue<T> : IReadOnlyLimitedValue<T> where T: IComparable<T>
    {
        void SetValue(T value);
        bool TrySetMax(T max);
        bool TrySetMin(T min);
    }
    
    [Serializable]
    public class LimitedValue<T> : ILimitedValue<T> where T: IComparable<T>
    {
        private T value;
        private T min;
        private T max;
        public T Value => value;
        public T Min => min;
        public T Max => max;

        public bool InBounded(T value)
        {
            return ComparableUtil.IsUpperMin(value,Min) && ComparableUtil.IsUnderMax(value, Max);
        }

        public void SetValue(T value)
        {
            if(!ComparableUtil.IsUpperMin(value,Min))
                this.value = Min;
            else if(!ComparableUtil.IsUnderMax(value, Max))
                this.value = max;
            else
                this.value = value;
        }
        
        public bool TrySetMax(T max)
        {
            if(ComparableUtil.IsUpperMin(max, Min))
            {
                this.max = max;
                return true;
            }
            return false;
        }
        
        public bool TrySetMin(T min)
        {
            if(ComparableUtil.IsUnderMax(min, Max))
            {
                this.min = min;
                return true;
            }
            return false;
        }

        protected LimitedValue(T value, T min, T max)
        {
            this.value = value;
            this.min = min;
            this.max = max;
        }
        
        protected LimitedValue(T min, T max)
        {
            this.value = max;
            this.min = min;
            this.max = max;
        }

        public void Deconstruct(out T value, out T min,  out T max)
        {
            value = Value;
            min = Min;
            max = Max;
        }
        
        public static LimitedValue<T2> Create<T2>(T2 value, T2 min, T2 max) where T2: IComparable<T2>
        {
            if(!ComparableUtil.IsValid(value, min, max)) return null;
            return new LimitedValue<T2>(value, min, max);
        }
        
        public static LimitedValue<T2> Create<T2>(T2 min, T2 max) where T2: IComparable<T2>
        {
            if(!ComparableUtil.IsValid(max, min, max)) return null;
            return new LimitedValue<T2>(max, min, max);
        }

        public override string ToString()
        {
            return $"{nameof(value)}: {value}, {nameof(min)}: {min}, {nameof(max)}: {max}";
        }

        public static implicit operator T(LimitedValue<T> self) => self.value;

        static class ComparableUtil
        {
            public static bool IsUnderMax<T2>(T2 value, T2 max) where T2: IComparable<T2>
            {
                return value.CompareTo(max) <= 0;
            }

            public static bool  IsUpperMin<T2>(T2 value, T2 min) where T2: IComparable<T2>
            {
                return value.CompareTo(min) >= 0;
            }

            public static bool InBounded<T2>(T2 value, T2 min, T2 max) where T2: IComparable<T2>
            {
                return IsUpperMin(value,min) && IsUnderMax(value, max);
            }

            public static bool IsValid<T2>(T2 value, T2 min, T2 max) where T2: IComparable<T2>
            {
                return InBounded(value, min, max) && IsUpperMin(max, min) && IsUnderMax(min, max);
            }
        }
    }
}
