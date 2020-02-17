using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquintScript
{
    public class TrackedValue<T> : IRevertibleChangeTracking, IComparable<TrackedValue<T>> where T : IComparable
    {
        public bool IsChanged { get; private set; } = false;
        protected T _ReferenceValue;
        protected T _CurrentValue;

        public string Display()
        {
            var TE = _CurrentValue as Enum;
            if (TE != null)
                return TE.Display();
            else return _CurrentValue.ToString();
        }

        public TrackedValue(T value)
        {
            _ReferenceValue = value;
            _CurrentValue = value;
        }

        public void AcceptChanges()
        {
            IsChanged = false;
            _ReferenceValue = _CurrentValue;
        }

        public void RejectChanges()
        {
            _CurrentValue = _ReferenceValue;
        }

        public int CompareTo(TrackedValue<T> other)
        {
            if (_CurrentValue.CompareTo(other.Value) < 0) return -1;
            else if (_CurrentValue.CompareTo(other.Value) > 0) return 1;
            return 0;
        }

        public T Value
        {
            get
            {
                return _CurrentValue;
            }
            set
            {
                _CurrentValue = Value;
                IsChanged = true;
            }
        }
        public T ReferenceValue { get { return _ReferenceValue; } }
    }

    public class TrackedValueWithReferences<T> : TrackedValue<T> where T : IComparable
    {
        public TrackedValue<T> majorViolation { get; private set; }
        public TrackedValue<T> minorViolation { get; private set; }
        public TrackedValue<T> stop { get; private set; }

        public TrackedValueWithReferences(T value) : base(value)
        {
            _ReferenceValue = value;
        }
        public void SetMajorViolation(T value)
        {
            majorViolation = new TrackedValue<T>(value);
        }
        public void SetMinorViolation(T value)
        {
            minorViolation = new TrackedValue<T>(value);
        }
        public void SetStop(T value)
        {
            stop = new TrackedValue<T>(value);
        }

    }
}
