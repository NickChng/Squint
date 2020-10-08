using PropertyChanged;
using SquintScript.Interfaces;
using System;
using System.Collections.Generic;
using SquintScript.Extensions;

namespace SquintScript.TestFramework
{
    [AddINotifyPropertyChangedInterface]
    public class TestRangeItem<T> : TestListItem<T>, ITestListItem<T>
    {
        public void SetCheckValue(object CheckThis)
        {
            Check = (T)CheckThis;
            RaisePropertyChangedEvent(nameof(CheckValueString));
            RaisePropertyChangedEvent(nameof(Warning));
        }
        public EditTypes EditType { get; private set; } = EditTypes.RangeValues;
        public TestType Test { get; } = TestType.Range;
        public string ReferenceValueString
        {
            get
            {
                if (Reference == null || Reference2 == null)
                    return _EmptyRefValueString;
                else if (Reference.Value is double)
                {
                    var dRef1 = Convert.ToDouble(Reference.Value);
                    var dRef2 = Convert.ToDouble(Reference2.Value);
                    if (double.IsNaN(dRef1) || double.IsNaN(dRef2))
                        return "";
                    else
                        return string.Format("{0:0.###} - {1:0.###}", Reference.Value, Reference2.Value);
                }
                else return string.Format("{0:0.###} - {1:0.###}", Reference.Value, Reference2.Value);

            }
        }

        public bool IsDirty { get { return (Reference.IsChanged || Reference2.IsChanged); } }
        public T SetReference
        {
            set
            {
                if (Reference == null)
                {
                    Reference = new TrackedValue<T>(default(T));
                }
                if (value != null)
                {
                    Reference.Value = (T)value;
                    RaisePropertyChangedEvent(nameof(IsDirty));
                }
            }
            get
            {
                if (Reference != null)
                    return Reference.Value;
                else
                    return default;
            }
        }

        public T SetReference2
        {
            set
            {
                if (Reference2 == null)
                {
                    Reference2 = new TrackedValue<T>(default(T));
                }
                if (value != null)
                {
                    Reference2.Value = (T)value;
                    RaisePropertyChangedEvent(nameof(IsDirty));
                }
            }
            get
            {
                if (Reference2 != null)
                    return Reference2.Value;
                else
                    return default;
            }
        }

        public TrackedValue<T> Reference2 { get; set; }
        public string CheckValueString
        {
            get
            {
                if (Check == null)
                    return _EmptyCheckValueString;
                else
                {
                    if (Check is double || Check is int)
                        return string.Format("{0:0.###}", Check);
                    else if (Check is Enum)
                        return (Check as Enum).Display();
                    else
                        return Check.ToString();
                }
            }
        }

        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;

        public bool? Warning
        {
            get
            {
                if (CheckPass == null)
                    return null;
                else
                    return !(bool)CheckPass;
            }
            set { }
        }
        public bool? CheckPass
        {
            get
            {
                if (Reference == null)
                {
                    if (ParameterOption == ParameterOptions.Required)
                        return false;
                    else
                        return null;
                }
                else if (Check == null)
                {
                    if (ParameterOption == ParameterOptions.Required)
                        return false;
                    else
                        return null;
                }
                else
                {
                    if (Comparer<T>.Default.Compare((T)Check, Reference.Value) >= 0 && Comparer<T>.Default.Compare((T)Check, Reference2.Value) <= 0)
                        return true;
                    else
                        return false;
                }
            }
        }

        public override string WarningString
        {
            get
            {
                if (Reference == null || Reference2 == null)
                    return _EmptyRefValueString;
                if (Check == null)
                    return _EmptyCheckValueString;
                else
                    return _WarningString;
            }
            set
            {
                _WarningString = value;
            }
        }
        public TestRangeItem(CheckTypes CT, T V, TrackedValue<T> min, TrackedValue<T> max, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            CheckType = CT;
            Reference = min;
            Reference2 = max;
            Check = V;
            _WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
        public void RejectChanges()
        {
            if (Reference != null)
            {
                Reference.RejectChanges();
            }
        }
    }

}
//}

