using PropertyChanged;
using Squint.Interfaces;
using System;
using System.Collections.ObjectModel;
using Squint.Extensions;

namespace Squint.TestFramework
{
    [AddINotifyPropertyChangedInterface]
    public class TestValueItem<T> : TestListItem<T>, ITestListItem<T>
    {
        public void SetCheckValue(object CheckThis)
        {
            Check = (T)CheckThis;
            RaisePropertyChangedEvent(nameof(CheckValueString));
            RaisePropertyChangedEvent(nameof(Warning));
        }
        public EditTypes EditType { get; private set; } = EditTypes.SingleValue;
        public TestType Test { get; set; } = TestType.Equality;
        public ObservableCollection<T> ReferenceCollection { get; set; } = new ObservableCollection<T>();
        public string ReferenceValueString
        {
            get
            {
                if (Reference == null)
                    return _EmptyRefValueString;
                else
                {
                    if (Reference.Value == null)
                        return _EmptyRefValueString;
                    if (Reference.Value is double || Reference.Value is int)
                    {
                        var dCheck = Convert.ToDouble(Reference.Value);
                        if (double.IsNaN(dCheck))
                            return "";
                        switch (Test)
                        {
                            case TestType.Equality:
                                if (Tolerance != null)
                                    if ((dynamic)Tolerance.Value > 1E-2)
                                        return string.Format("{0:0.###} \u00b1 {1:0.#}", Reference.Value, Tolerance.Value);
                                    else
                                        return string.Format("{0:0.###}", Reference.Value);
                                else
                                    return string.Format("{0:0.###}", Reference.Value);
                            case TestType.GreaterThan:
                                return string.Format("\u2265 {0:0.###}", Reference.Value);
                            case TestType.LessThan:
                                return string.Format("\u2264 {0:0.###}", Reference.Value);
                            default:
                                return string.Format("{0:0.###}", Reference.Value);
                        }
                    }
                    else if (Check is Enum)
                        return string.Format("{0}", (Reference.Value as Enum).Display());
                    else
                        return Reference.Value.ToString();
                }
            }
        }
        public bool EditEnabled { get; set; } = true;
        public bool IsDirty { get { return (Reference.IsChanged || Tolerance.IsChanged); } }
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
                    //RaisePropertyChangedEvent(nameof(IsDirty));
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

        public T SetTolerance
        {
            set
            {
                if (Tolerance == null)
                {
                    Tolerance = new TrackedValue<T>(default(T));
                }
                if (value != null)
                {
                    Tolerance.Value = (T)value;
                    //RaisePropertyChangedEvent(nameof(ReferenceValueString));
                }
            }
            get
            {
                if (Tolerance != null)
                    return Tolerance.Value;
                else
                    return default;
            }
        }

        public string CheckValueString
        {
            get
            {
                if (Check == null)
                    return _EmptyCheckValueString;
                if (Check is double)
                {
                    var dCheck = Convert.ToDouble(Check);
                    if (double.IsNaN(dCheck))
                        return "";
                    else
                        return string.Format("{0:0.###}", Check);
                }
                if (Check is int)
                {
                    return string.Format("{0:0.###}", Check);
                }
                if (Check is Enum)
                    return (Check as Enum).Display();
                else
                    return Check.ToString();

                //if (Check is double)
                //{
                //    if (double.IsNaN((double?)Check))
                //        return "";
                //    else
                //        return string.Format("{0:0.###}", Check);
                //}
                //if (Check is int)
                //{
                //    return string.Format("{0:0.###}", Check);
                //}
                //else if (Check is Enum)
                //    return (Check as Enum).Display();
                //else
                //    return Check.ToString();
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
                    if (Reference.Value == null || Check == null)  // this is hacky; need to decide whether to support Reference.Value is null or Reference = null but not both.
                        if (ParameterOption == ParameterOptions.Required)
                            return false;
                        else
                            return null;
                    switch (Test)
                    {
                        case TestType.Equality:
                            if (Tolerance == null)
                            {
                                if ((dynamic)Reference.Value == (dynamic)Check)
                                    return true;
                                else
                                    return false;
                            }
                            else
                            {
                                if (Math.Abs((dynamic)Reference.Value - (dynamic)Check) <= (dynamic)Tolerance.Value+1E-5)
                                    return true;
                                else
                                    return false;
                            }
                        case TestType.GreaterThan:
                            if ((dynamic)Check >= (dynamic)Reference.Value)
                                return true;
                            else
                                return false;
                        case TestType.LessThan:
                            if ((dynamic)Check <= (dynamic)Reference.Value)
                                return true;
                            else
                                return false;
                        default:
                            return false;
                    }
                }
            }
        }
        public TestValueItem(CheckTypes CT, T V, TrackedValue<T> RV, TrackedValue<T> Tol = null, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            CheckType = CT;
            Reference = RV;
            Tolerance = Tol;
            Check = V;
            if (V is Enum)
            {
                EditType = EditTypes.SingleSelection;
                foreach (T E in Enum.GetValues(typeof(T)))
                {
                    ReferenceCollection.Add(E);
                }
                if (Reference != null)
                    SetReference = Reference.Value;
                else
                    SetReference = default(T);

            }
            
            if (Tol != null)
                EditType = EditTypes.SingleValueWithTolerance;
            _WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
    }
}
//}

