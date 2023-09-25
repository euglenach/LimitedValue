using System;

namespace LimitedValues
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
        protected T value;
        protected T min;
        protected T max;
        public T Value => value;
        public T Min => min;
        public T Max => max;

        public bool InBounded(T value)
        {
            return ComparableUtil.IsUpperMin(value,Min) && ComparableUtil.IsUnderMax(value, Max);
        }

        public virtual void SetValue(T value)
        {
            if(!ComparableUtil.IsUpperMin(value,Min))
                this.value = Min;
            else if(!ComparableUtil.IsUnderMax(value, Max))
                this.value = max;
            else
                this.value = value;
        }
        
        public virtual bool TrySetMax(T max)
        {
            if(ComparableUtil.IsUpperMin(max, Min))
            {
                this.max = max;
                if(value.CompareTo(this.max) > 0) value = max;
                return true;
            }
            return false;
        }
        
        public virtual bool TrySetMin(T min)
        {
            if(ComparableUtil.IsUnderMax(min, Max))
            {
                this.min = min;
                if(value.CompareTo(this.min) < 0) value = min;
                return true;
            }
            return false;
        }
        
        public LimitedValue(IReadOnlyLimitedValue<T> origin)
        {
            (value, min, max) = (origin.Value, origin.Min, origin.Max);
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
    }
}
