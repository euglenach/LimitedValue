#if LIMITEDVALUE_UNIRX_SUPPORT
using System;
using UniRx;

namespace LimitedValues
{
    public readonly struct ObserveAnyEvent<T> where T : IComparable<T>
    {
        public readonly T Value;
        public readonly T Min;
        public readonly T Max;

        public ObserveAnyEvent(T value, T min, T max)
        {
            Value = value;
            Min = min;
            Max = max;
        }
    }
    
    public interface IReadOnlyLimitedReactiveProperty<T> : IReadOnlyReactiveProperty<T>, IReadOnlyLimitedValue<T> where T: IComparable<T>
    {
        IObservable<T> ObserveMax();
        IObservable<T> ObserveMin();

        IObservable<ObserveAnyEvent<T>> ObserveAny();

        new T Value{get;}
    }
    
    public class LimitedReactiveProperty<T> : LimitedValue<T>, IReadOnlyLimitedReactiveProperty<T>, IDisposable where T: IComparable<T>
    {
        protected bool isDisposed;
        protected Subject<T> valueChanged = new();
        protected Subject<T> minChanged = new();
        protected Subject<T> maxChanged = new();
        protected Subject<ObserveAnyEvent<T>> anyChanged = new();
        public bool HasValue => true;

        public override void SetValue(T value) => SetValue(value, false);

        public override bool TrySetMax(T max) => TrySetMax(max, false);

        public override bool TrySetMin(T min) => TrySetMin(min, false);

        public void SetValue(T value, bool forceNotify)
        {
            var prev = this.value;
            base.SetValue(value);
            if(forceNotify || prev.CompareTo(this.value) != 0)
            {
                valueChanged?.OnNext(this.value);
                anyChanged?.OnNext(new (this.value, this.min, this.max));
            }
        }
        
        public bool TrySetMax(T max, bool forceNotify)
        {
            var prev = this.max;
            var r = base.TrySetMax(max);

            if(r)
            {
                if(forceNotify || prev.CompareTo(this.max) != 0)
                {
                    maxChanged?.OnNext(this.max);
                    anyChanged?.OnNext(new (this.value, this.min, this.max));
                }
            }
            return r;
        }
        
        public bool TrySetMin(T min, bool forceNotify)
        {
            var prev = this.min;
            var r = base.TrySetMin(min);
            if(forceNotify || prev.CompareTo(this.min) != 0)
            {
                minChanged?.OnNext(this.min);
                anyChanged?.OnNext(new (this.value, this.min, this.max));
            }
            return r;
        }
        
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (isDisposed)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }
            
            // raise latest value on subscribe
            valueChanged ??= new();
            observer.OnNext(Value);
            return valueChanged.Subscribe(observer);
        }

        public IObservable<T> ObserveMax()
        {
            if (isDisposed) return Observable.Empty<T>();
            return maxChanged ??= new();
        }
        
        public IObservable<T> ObserveMin()
        {
            if (isDisposed) return Observable.Empty<T>();
            return minChanged ??= new();
        }

        public IObservable<ObserveAnyEvent<T>> ObserveAny()
        {
            if (isDisposed) return Observable.Empty<ObserveAnyEvent<T>>();
            return anyChanged ??= new();
        }

        public new static LimitedReactiveProperty<T2> Create<T2>(T2 value, T2 min, T2 max) where T2: IComparable<T2>
        {
            var instance = LimitedValue<T2>.Create(value, min, max);
            return instance is not null? new(value, min, max) : null;
        }
        
        public new static LimitedReactiveProperty<T2> Create<T2>(T2 min, T2 max) where T2: IComparable<T2>
        {
            var instance = LimitedValue<T2>.Create(min, max);
            return instance is not null? new(instance.Value, min, max) : null;
        }
        
        public static LimitedReactiveProperty<T2> Create<T2>(LimitedValue<T2> origin) where T2: IComparable<T2>
        {
            return new(origin);
        }

        public LimitedReactiveProperty(IReadOnlyLimitedValue<T> origin) : base(origin.Value, origin.Min, origin.Max){}

        protected LimitedReactiveProperty(T value, T min, T max) : base(value, min, max)
        {}

        protected LimitedReactiveProperty(T min, T max) : base(min, max)
        {}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;
            isDisposed = true;
            valueChanged?.Dispose();
            minChanged?.Dispose();
            maxChanged?.Dispose();
        }
    }
}
#endif