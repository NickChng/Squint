using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PropertyChanged;
using Squint.Extensions;
using Squint.ViewModels;

namespace Squint
{
    public interface ITrackedValue
    {
        bool IsChanged { get; }
    }

   [AddINotifyPropertyChangedInterface]
    public class TrackedValue<T> : ObservableObject, ITrackedValue, INotifyPropertyChanged, IComparable<TrackedValue<T>>
    {
        protected T _ReferenceValue;
        protected T _CurrentValue;

        public string DisplayName { get; set; }

        public bool IsChanged { get; private set; }
        public string Display()
        {
            var TE = _CurrentValue as Enum;
            if (TE != null)
                return TE.Display();
            else return _CurrentValue.ToString();
        }

        public bool isDefined
        {
            get
            {
                if (_CurrentValue == null)
                    return false;
                else
                {
                    if (_CurrentValue is double)
                        return !double.IsNaN(Convert.ToDouble(_CurrentValue));
                    if (_CurrentValue is int)
                        return Convert.ToInt32(_CurrentValue) == int.MinValue;
                }
                return true;
            }
        }
        public TrackedValue(T value)
        {
            _ReferenceValue = value;
            _CurrentValue = value;
        }

        public void AcceptChanges()
        {
            IsChanged = false;
            RaisePropertyChangedEvent(nameof(IsChanged));
            _ReferenceValue = _CurrentValue;
        }

        public void RejectChanges()
        {
            _CurrentValue = _ReferenceValue;
        }

        public int CompareTo(TrackedValue<T> other)
        {
            if (_CurrentValue == null)
                return 1;
            else
            {
                if (((IComparable)_CurrentValue).CompareTo(other.Value) < 0)
                    return -1;
                else if (((IComparable)_CurrentValue).CompareTo(other.Value) > 0)
                    return 1;
                return 0;
            }
        }

        public T Value
        {
            get
            {
                return _CurrentValue;
            }
            set
            {
                _CurrentValue = value;
                if (_CurrentValue == null)
                    if (_ReferenceValue == null)
                        IsChanged = false;
                    else
                        IsChanged = true;
                else
                    IsChanged = !_CurrentValue.Equals(_ReferenceValue);

            }
        }
        public T ReferenceValue { get { return _ReferenceValue; } }
    }

}
