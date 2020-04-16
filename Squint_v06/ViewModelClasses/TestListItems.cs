using PropertyChanged;
using SquintScript.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SquintScript;
using System.Windows.Markup;
using SquintScript.Extensions;
using g3;
using System.ComponentModel;
using SquintScript.ViewModels;

namespace SquintScript.ViewModelClasses
{
    public abstract class TestListItem<T> : ObservableObject
    {
        public string TestName { get; set; }
        public TrackedValue<T> Reference { get; set; }
        public TrackedValue<T> Tolerance { get; set; }
        public T Check { get; set; }
        public CheckTypes CheckType { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    public class CheckValueItem<T> : TestListItem<T>, ITestListItem<T>
    {
        public EditTypes EditType { get; private set; } = EditTypes.SingleValue;
        public TestType Test { get; set; } = TestType.Equality;
        public string ReferenceValueString
        {
            get
            {
                if (Reference == null)
                    return _EmptyRefValueString;
                else
                {
                    if (Check is double || Check is int)
                    {
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

        public T SetReference
        {
            set
            {
                if (Reference == null)
                {
                    Reference = new TrackedValue<T>(Check);
                }
                Reference.Value = value;
                RaisePropertyChangedEvent(nameof(ReferenceValueString));
            }
        }

        public T SetTolerance
        {
            set
            {
                if (Tolerance == null)
                {
                    Tolerance = new TrackedValue<T>(Check);
                }
                Tolerance.Value = value;
                RaisePropertyChangedEvent(nameof(ReferenceValueString));

            }
        }

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
        private string _EmptyCheckValueString = "";
        private string _EmptyRefValueString = "";

        public ParameterOptions ParameterOption = ParameterOptions.Required;

        public bool Warning
        {
            get
            {
                if (CheckPass == null)
                    return false;
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
                    return null;
                else if (Check == null)
                {
                    return null;
                }
                else
                {
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
                                if (Math.Abs((dynamic)Reference.Value - (dynamic)Check) < (dynamic)Tolerance.Value)
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
        public string WarningString { get; set; } = "DefaultWarning";
        public CheckValueItem(string TN, T V, TrackedValue<T> RV, TrackedValue<T> Tol = null, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            TestName = TN;
            Reference = RV;
            Check = V;
            Tolerance = Tol;
            if (Tol != null)
                EditType = EditTypes.SingleValueWithTolerance;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
        public void CommitChanges()
        {
            if (Reference != null)
                Ctr.UpdateChecklistReferenceValue(CheckType, Reference.Value);
        }
        public void RejectChanges()
        {
            if (Reference != null)
            {
                Reference.RejectChanges();
            }
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class CheckRangeItem<T> : TestListItem<T>, ITestListItem<T>
    {
        public EditTypes EditType { get; private set; } = EditTypes.SingleValue;
        public TestType Test { get; } = TestType.Range;
        public string ReferenceValueString
        {
            get
            {
                if (Reference == null)
                    return _EmptyRefValueString;
                else
                    return
                         string.Format("{0:0.###} - {1:0.###}", Reference.Value, Reference2.Value);
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
        private string _EmptyCheckValueString = "";
        private string _EmptyRefValueString = "";

        public ParameterOptions ParameterOption = ParameterOptions.Required;

        public bool Warning
        {
            get
            {
                if (CheckPass == null)
                    return false;
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
                    return null;
                else if (Check == null)
                {
                    return null;
                }
                else
                {
                    if (Comparer<T>.Default.Compare(Check, Reference.Value) >= 0 && Comparer<T>.Default.Compare(Check, Reference2.Value) <= 0)
                        return true;
                    else
                        return false;
                }
            }
        }
        public string WarningString { get; set; } = "DefaultWarning";
        public CheckRangeItem(string TN, T V, TrackedValue<T> min, TrackedValue<T> max, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            TestName = TN;
            Reference = min;
            Reference2 = max;
            Check = V;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
        public void CommitChanges()
        {
            if (Reference != null)
                Ctr.UpdateChecklistReferenceValue(CheckType, Reference.Value);
        }
        public void RejectChanges()
        {
            if (Reference != null)
            {
                Reference.RejectChanges();
            }
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class CheckContainsItem<T> : TestListItem<T>, ITestListItem<T>
    {
        public EditTypes EditType { get; private set; } = EditTypes.SingleValue;

        public ObservableCollection<TrackedValue<T>> ReferenceCollection { get; set; }
        public string ReferenceValueString
        {
            get
            {
                if (ReferenceCollection == null)
                    return _EmptyRefValueString;
                else
                {
                    if (Check is Enum)
                        return string.Format("Any of {0}", string.Join(", ", ReferenceCollection.Select(x => (x.Value as Enum).Display())));
                    else
                        return string.Format("Any of {0}", string.Join(", ", ReferenceCollection.Select(x => x.Value)));
                }
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
        private string _EmptyCheckValueString = "";
        private string _EmptyRefValueString = "";

        public ParameterOptions ParameterOption = ParameterOptions.Required;
        public bool Warning
        {
            get
            {
                if (CheckPass == null)
                    return false;
                else
                    return !(bool)CheckPass;
            }
            set { }
        }
        public bool? CheckPass
        {
            get
            {
                if (ReferenceCollection == null)
                    return null;
                else if (Check == null)
                {
                    return null;
                }
                else
                {
                    if (ReferenceCollection.Select(x => x.Value).Contains(Check))
                        return true;
                    else
                        return false;
                }
            }
        }
        public string WarningString { get; set; } = "DefaultWarning";
        public CheckContainsItem(string TN, T V, List<TrackedValue<T>> referenceCollection, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            TestName = TN;
            ReferenceCollection = new ObservableCollection<TrackedValue<T>>(referenceCollection);
            Check = V;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
        public void CommitChanges()
        {
            if (Reference != null)
                Ctr.UpdateChecklistReferenceValue(CheckType, Reference.Value);
        }
        public void RejectChanges()
        {
            if (Reference != null)
            {
                Reference.RejectChanges();
            }
        }
    }


    //    [AddINotifyPropertyChangedInterface]
    //    public class TestListEnumValueItem<T> : TestListItem<CheckValue<T>, T>, ITestListItem<CheckValue<T>, T> where T : Enum
    //    {
    //        public EditTypes EditType { get; } = EditTypes.SingleValue;
    //        public string ReferenceValueString
    //        {
    //            get
    //            {
    //                if (ReferenceValue == null)
    //                    return _EmptyRefValueString;
    //                else
    //                    return ReferenceValue.ReferenceValue.Value.Display();
    //            }
    //        }

    //        public string CheckValueString
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                    return _EmptyCheckValueString;
    //                else
    //                    return CheckValue.Display();
    //            }
    //        }

    //        private string _EmptyCheckValueString = "";
    //        private string _EmptyRefValueString = "";

    //        public ParameterOptions ParameterOption = ParameterOptions.Required;
    //        public bool Warning
    //        {
    //            get
    //            {
    //                if (ReferenceValue == null)
    //                    return false;
    //                else if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                }
    //                else
    //                {
    //                    if (ReferenceValue.Check(CheckValue))
    //                        return false;
    //                    else return true;
    //                }
    //            }
    //        }
    //        public string WarningString { get; set; } = "DefaultWarning";
    //        public TestListEnumValueItem(string TN, T V, CheckValue<T> RV, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
    //        {
    //            TestName = TN;
    //            ReferenceValue = RV;
    //            CheckValue = V;
    //            WarningString = WS;
    //            _EmptyCheckValueString = EmptyCheckValueString;
    //            _EmptyRefValueString = EmptyRefValueString;
    //        }
    //        public void CommitChanges()
    //        {
    //            if (ReferenceValue != null)
    //                Ctr.UpdateChecklistReferenceValue(CheckType, ReferenceValue.ReferenceValue.Value);
    //        }
    //        public void RejectChanges()
    //        {
    //            if (ReferenceValue != null)
    //            {
    //                ReferenceValue.ReferenceValue.RejectChanges();
    //            }
    //        }
    //    }

    //    [AddINotifyPropertyChangedInterface]
    //    public class TestListStringChoiceItem : TestListItem<CheckValue<string>, string>, ITestListItem<CheckValue<string>, string>
    //    {

    //        public ObservableCollection<string> ReferenceValueOptions { get; set; }

    //        public EditTypes EditType { get; } = EditTypes.SingleSelection;
    //        public string ReferenceValueString
    //        {
    //            get
    //            {
    //                if (ReferenceValue == null)
    //                    return _EmptyRefValueString;
    //                else
    //                    return ReferenceValue.ReferenceValue.Value;
    //            }
    //            set { }
    //        }
    //        public string CheckValueString
    //        {
    //            get
    //            {
    //                if (CheckValue == "")
    //                    return _EmptyCheckValueString;
    //                else
    //                    return CheckValue;
    //            }
    //        }

    //        private string _EmptyCheckValueString = "";
    //        private string _EmptyRefValueString = "";

    //        public ParameterOptions ParameterOption = ParameterOptions.Required;
    //        public bool Warning
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                }
    //                else if (ReferenceValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                }
    //                else
    //                {
    //                    return !(ReferenceValue.Check(CheckValue));
    //                }
    //            }
    //        }
    //        public string WarningString { get; set; } = "DefaultWarning";
    //        public TestListStringChoiceItem(string TN, string V, CheckValue<string> RV, List<string> referenceValueOptions = null, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
    //        {
    //            TestName = TN;
    //            ReferenceValue = RV;
    //            CheckValue = V;
    //            WarningString = WS;
    //            _EmptyCheckValueString = EmptyCheckValueString;
    //            _EmptyRefValueString = EmptyRefValueString;
    //        }

    //        public void CommitChanges()
    //        {
    //            if (ReferenceValue != null)
    //                Ctr.UpdateChecklistReferenceValue(CheckType, ReferenceValue);
    //        }
    //        public void RejectChanges()
    //        {
    //            if (ReferenceValue != null)
    //            {
    //                ReferenceValue.ReferenceValue.RejectChanges();
    //            }
    //        }
    //    }

    //    [AddINotifyPropertyChangedInterface]
    //    public class TestListStringAnyItem : TestListItem<CheckAny<string>, string>, ITestListItem<CheckAny<string>, string>
    //    {

    //        public TestType TestType { get; set; } // to implement

    //        public EditTypes EditType { get; } = EditTypes.AnyOfValues;

    //        public string ReferenceValueString
    //        {
    //            get
    //            {
    //                if (ReferenceValue == null)
    //                    return _EmptyRefValueString;
    //                else
    //                    return string.Join(", ", ReferenceValue.ReferenceCollection.Select(x => x.Value));
    //            }
    //        }

    //        public string CheckValueString
    //        {
    //            get
    //            {
    //                if (CheckValue == "")
    //                    return _EmptyCheckValueString;
    //                else
    //                    return CheckValue;
    //            }
    //        }

    //        private string _EmptyCheckValueString = "";
    //        private string _EmptyRefValueString = "";

    //        public ParameterOptions ParameterOption = ParameterOptions.Required;
    //        public bool Warning
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                }
    //                else if (ReferenceValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                }
    //                else
    //                {
    //                    return !(ReferenceValue.Check(CheckValue));
    //                }
    //            }
    //        }
    //        public string WarningString { get; set; } = "DefaultWarning";
    //        public TestListStringAnyItem(string TN, string V, CheckAny<string> RV, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
    //        {
    //            TestName = TN;
    //            ReferenceValue = RV;
    //            CheckValue = V;
    //            WarningString = WS;
    //            _EmptyCheckValueString = EmptyCheckValueString;
    //            _EmptyRefValueString = EmptyRefValueString;
    //        }

    //        public void CommitChanges()
    //        {
    //            if (ReferenceValue != null)
    //                Ctr.UpdateChecklistReferenceValue(CheckType, ReferenceValue);
    //        }
    //        public void RejectChanges()
    //        {
    //            if (ReferenceValue != null)
    //                foreach (var TV in ReferenceValue.ReferenceCollection)
    //                {
    //                    TV.RejectChanges();
    //                }
    //        }
    //    }

    //    [AddINotifyPropertyChangedInterface]
    //    public class TestListEnumAnyItem<T> : TestListItem<CheckAny<T>, T>, ITestListItem<CheckAny<T>, T> where T : Enum
    //    {

    //        public TestType TestType { get; set; } // to implement

    //        public EditTypes EditType { get; } = EditTypes.AnyOfValues;

    //        public string ReferenceValueString
    //        {
    //            get
    //            {
    //                if (ReferenceValue == null)
    //                    return _EmptyRefValueString;
    //                else
    //                    return @"Any of: " + string.Join(", ", ReferenceValue.ReferenceCollection.Select(x => x.Value.Display()));
    //            }
    //        }

    //        public string CheckValueString
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                    return _EmptyCheckValueString;
    //                else
    //                    return CheckValue.Display();
    //            }
    //        }

    //        private string _EmptyCheckValueString = "";
    //        private string _EmptyRefValueString = "";

    //        public ParameterOptions ParameterOption = ParameterOptions.Required;
    //        public bool Warning
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                }
    //                else if (ReferenceValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                }
    //                else
    //                {
    //                    return !(ReferenceValue.Check(CheckValue));
    //                }
    //            }
    //        }
    //        public string WarningString { get; set; } = "DefaultWarning";
    //        public TestListEnumAnyItem(string TN, T V, CheckAny<T> RV, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
    //        {
    //            TestName = TN;
    //            ReferenceValue = RV;
    //            CheckValue = V;
    //            WarningString = WS;
    //            _EmptyCheckValueString = EmptyCheckValueString;
    //            _EmptyRefValueString = EmptyRefValueString;
    //        }

    //        public void CommitChanges()
    //        {
    //            if (ReferenceValue != null)
    //                Ctr.UpdateChecklistReferenceValue(CheckType, ReferenceValue);
    //        }
    //        public void RejectChanges()
    //        {
    //            if (ReferenceValue != null)
    //                foreach (var TV in ReferenceValue.ReferenceCollection)
    //                {
    //                    TV.RejectChanges();
    //                }
    //        }
    //    }

    //    [AddINotifyPropertyChangedInterface]
    //    public class TestListIntValueItem : TestListItem<CheckValue<int>, int?>, ITestListItem<CheckValue<int>, int?>
    //    {

    //        public EditTypes EditType { get; private set; } = EditTypes.SingleValue;
    //        public string ReferenceValueString
    //        {
    //            get
    //            {
    //                if (ReferenceValue == null)
    //                    return _EmptyRefValueString;
    //                else
    //                {
    //                    return ReferenceValue.ReferenceValue.Value.ToString();
    //                }
    //            }
    //        }

    //        public string CheckValueString
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                    return _EmptyCheckValueString;
    //                else return CheckValue.ToString();
    //            }
    //        }

    //        public ParameterOptions ParameterOption = ParameterOptions.Required;
    //        public bool Warning
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                }
    //                else if (ReferenceValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                }
    //                else
    //                {
    //                    return !(ReferenceValue.Check((int)CheckValue));
    //                }
    //            }
    //        }
    //        private string _EmptyCheckValueString = "-";
    //        private string _EmptyRefValueString = "Not specified";
    //        public string WarningString { get; set; } = "DefaultWarning";
    //        public TestListIntValueItem(string TN, int? CV, CheckValue<int> RV, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "", int eps = 0)
    //        {
    //            TestName = TN;
    //            ReferenceValue = RV;
    //            if (RV != null)
    //                if (RV.Tolerance != null)
    //                    EditType = EditTypes.SingleValueWithTolerance;
    //            CheckValue = CV;
    //            WarningString = WS;
    //            _EmptyCheckValueString = EmptyCheckValueString;
    //            _EmptyRefValueString = EmptyRefValueString;
    //        }

    //        public void CommitChanges() { }
    //        public void RejectChanges() { }
    //    }
    //    [AddINotifyPropertyChangedInterface]
    //    public class TestListDoubleValueItem : TestListItem<CheckValue<double>, double?>, ITestListItem<CheckValue<double>, double?>
    //    {
    //        public EditTypes EditType { get; } = EditTypes.SingleValue;
    //        public string ReferenceValueString
    //        {
    //            get
    //            {
    //                if (ReferenceValue == null)
    //                    return _EmptyRefValueString;
    //                else
    //                {
    //                    switch (ReferenceValue.Test)
    //                    {
    //                        case TestType.Equality:
    //                            {
    //                                if (ReferenceValue.Tolerance.Value > 1E-2)
    //                                    return string.Format("{0:0.###} \u00b1 {1:0.#}", ReferenceValue.ReferenceValue.Value, ReferenceValue.Tolerance.Value);
    //                                else
    //                                    return string.Format("{0:0.###}", ReferenceValue.ReferenceValue.Value);
    //                            }
    //                        case TestType.GreaterThan:
    //                            return string.Format("\u2265 {0:0.###}", ReferenceValue.ReferenceValue.Value);
    //                        case TestType.LessThan:
    //                            return string.Format("\u2264 {0:0.###}", ReferenceValue.ReferenceValue.Value);
    //                        default:
    //                            return string.Format("{0:0.###}", ReferenceValue.ReferenceValue.Value);
    //                    }

    //                }
    //            }
    //            set { }

    //        }

    //        public double SetReference
    //        {
    //            get { return double.NaN; }
    //            set
    //            {
    //                ReferenceValue.ReferenceValue.Value = value;
    //                RaisePropertyChangedEvent(nameof(ReferenceValueString));
    //            }
    //        }
    //        public string CheckValueString
    //        {
    //            get
    //            {
    //                if (CheckValue != null)
    //                    return string.Format("{0:0.###}", CheckValue);
    //                else
    //                    return _EmptyCheckValueString;
    //            }
    //        }
    //        public bool Warning
    //        {
    //            get
    //            {
    //                if (ReferenceValue == null)
    //                    return false;
    //                else if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required) // no warning if checkvalue is null and optional
    //                        return true;
    //                    else
    //                        return false;
    //                }
    //                else
    //                {
    //                    return !ReferenceValue.Check((double)CheckValue);
    //                }
    //            }
    //        }

    //        private string _EmptyCheckValueString = "-";
    //        private string _EmptyRefValueString = "Not specified";

    //        public string CheckNullWarningString = "";
    //        public string RefNullWarningString = "";
    //        private string _WarningString;

    //        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;
    //        public string WarningString
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.None)
    //                        return "";
    //                    else if (ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
    //                        return "Optional";
    //                    else
    //                        return _EmptyCheckValueString;
    //                }
    //                else
    //                    if (ParameterOption == ParameterOptions.None)
    //                    return "Not indicated";
    //                if (ReferenceValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return _EmptyRefValueString;
    //                    else
    //                        return "";
    //                }
    //                if (!(ReferenceValue.Check((double)CheckValue)))
    //                    return _WarningString;
    //                else
    //                    return "";
    //            }
    //        }
    //        public TestListDoubleValueItem(string TN, double? CV, CheckValue<double> RV, string WS = "", string EmptyCheckValueString = null,
    //            string EmptyRefValueString = null)
    //        {
    //            TestName = TN;
    //            if (RV != null)
    //            {
    //                ReferenceValue = RV;
    //                if (RV.Tolerance != null)
    //                    EditType = EditTypes.SingleValueWithTolerance;
    //            }
    //            if (CV != null)
    //                if (!double.IsNaN((double)CV))
    //                    CheckValue = CV;
    //            _WarningString = WS;
    //            if (EmptyCheckValueString != null)
    //                _EmptyCheckValueString = EmptyCheckValueString;
    //            if (EmptyRefValueString != null)
    //                _EmptyRefValueString = EmptyRefValueString;
    //        }



    //        public void CommitChanges()
    //        {
    //            if (ReferenceValue != null)
    //                Ctr.UpdateChecklistReferenceValue(CheckType, (double)ReferenceValue.ReferenceValue.Value);
    //        }
    //        public void RejectChanges()
    //        {
    //            if (ReferenceValue != null)
    //            {
    //                ReferenceValue.ReferenceValue.RejectChanges();
    //                ReferenceValue.Tolerance.RejectChanges();
    //            }
    //        }
    //    }

    //    [AddINotifyPropertyChangedInterface]
    //    public class TestListDoubleRangeItem : TestListItem<CheckRange<double>, double?>, ITestListItem<CheckRange<double>, double?>
    //    {

    //        public EditTypes EditType { get; } = EditTypes.RangeValues;
    //        public string ReferenceValueString
    //        {
    //            get
    //            {
    //                if (ReferenceValue != null)
    //                {
    //                    var test = string.Format("{0:0.###} - {1:0.###}", ReferenceValue.Min.Value, ReferenceValue.Max.Value);
    //                    return test;
    //                }
    //                else
    //                    return _EmptyRefValueString;
    //            }
    //        }
    //        public string CheckValueString
    //        {
    //            get
    //            {
    //                if (CheckValue != null)
    //                    return string.Format("{0:0.###}", CheckValue);
    //                else
    //                    return _EmptyCheckValueString;
    //            }
    //        }

    //        public bool Warning
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.None || ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
    //                        return false;
    //                    else
    //                        return true;
    //                }
    //                else if (ReferenceValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else
    //                        return false;
    //                }
    //                else
    //                {
    //                    return !(ReferenceValue.Check((double)CheckValue));
    //                }
    //            }
    //        }

    //        private string _EmptyCheckValueString = "-";
    //        private string _EmptyRefValueString = "Not specified";

    //        public string CheckNullWarningString = "";
    //        public string RefNullWarningString = "";
    //        private string _WarningString;

    //        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;
    //        public string WarningString
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.None)
    //                        return "";
    //                    else if (ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
    //                        return "Optional";
    //                    else
    //                        return _EmptyCheckValueString;
    //                }
    //                else
    //                    if (ParameterOption == ParameterOptions.None)
    //                    return "Not indicated";
    //                if (ReferenceValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return _EmptyRefValueString;
    //                    else
    //                        return "";
    //                }
    //                if (!ReferenceValue.Check((double)CheckValue))
    //                    return _WarningString;
    //                else
    //                    return "";
    //            }
    //            set { }
    //        }
    //        public TestListDoubleRangeItem(string TN, double? CV, CheckRange<double> RV, string WS = "", string EmptyCheckValueString = null,
    //            string EmptyRefValueString = null)
    //        {
    //            TestName = TN;
    //            if (RV != null)
    //                ReferenceValue = RV;
    //            if (CV != null)
    //                if (!double.IsNaN((double)CV))
    //                    CheckValue = CV;
    //            _WarningString = WS;
    //            if (EmptyCheckValueString != null)
    //                _EmptyCheckValueString = EmptyCheckValueString;
    //            if (EmptyRefValueString != null)
    //                _EmptyRefValueString = EmptyRefValueString;
    //        }

    //        public void CommitChanges()
    //        {
    //            if (ReferenceValue != null)
    //                Ctr.UpdateChecklistReferenceValue(CheckType, ReferenceValue);
    //        }
    //        public void RejectChanges()
    //        {
    //            if (ReferenceValue != null)
    //            {
    //                ReferenceValue.Min.RejectChanges();
    //                ReferenceValue.Max.RejectChanges();
    //            }
    //        }

    //    }

    //    [AddINotifyPropertyChangedInterface]
    //    public class TestListIntRangeItem : TestListItem<CheckRange<int>, int?>, ITestListItem<CheckRange<int>, int?>
    //    {

    //        public TestType TestType { get; set; } = TestType.Range;
    //        public EditTypes EditType { get; } = EditTypes.RangeValues;
    //        public string ReferenceValueString
    //        {
    //            get
    //            {
    //                if (ReferenceValue != null && ReferenceValueMax != null)
    //                {
    //                    var test = string.Format("{0} - {1}", (int)ReferenceValue.Min.Value, (int)ReferenceValue.Max.Value);
    //                    return test;
    //                }
    //                else
    //                    return _EmptyRefValueString;
    //            }
    //        }
    //        public int? ReferenceValueMax { get; set; }

    //        public string CheckValueString
    //        {
    //            get
    //            {
    //                if (CheckValue != null)
    //                    return string.Format("{0:0.###}", CheckValue);
    //                else
    //                    return _EmptyCheckValueString;
    //            }
    //        }
    //        public bool Warning
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
    //                        return false;
    //                    else
    //                        return true;
    //                }
    //                else if (ReferenceValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else
    //                        return false;
    //                }
    //                else
    //                {
    //                    return !(ReferenceValue.Check((int)CheckValue));
    //                }
    //            }
    //        }

    //        private string _EmptyCheckValueString = "-";
    //        private string _EmptyRefValueString = "Not specified";

    //        public string CheckNullWarningString = "";
    //        public string RefNullWarningString = "";
    //        private string _WarningString;

    //        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;
    //        public string WarningString
    //        {
    //            get
    //            {
    //                if (TestType == TestType.Unset) // if test type is unset never warn
    //                    return "";
    //                if (CheckValue == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.None)
    //                        return "";
    //                    else if (ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
    //                        return "Optional";
    //                    else
    //                        return _EmptyCheckValueString;
    //                }
    //                else
    //                    if (ParameterOption == ParameterOptions.None)
    //                    return "Not indicated";
    //                if (ReferenceValue == null || ReferenceValueMax == null)
    //                {
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return _EmptyRefValueString;
    //                    else
    //                        return "";
    //                }
    //                if (!ReferenceValue.Check((int)CheckValue))
    //                    return _WarningString;
    //                else
    //                    return "";
    //            }
    //            set { }
    //        }
    //        public TestListIntRangeItem(string TN, int? CV, CheckRange<int> RV, string WS = "", string EmptyCheckValueString = null,
    //            string EmptyRefValueString = null)
    //        {
    //            TestName = TN;
    //            if (RV != null)
    //                ReferenceValue = RV;
    //            if (CV != null)
    //                if (!double.IsNaN((double)CV))
    //                    CheckValue = CV;
    //            _WarningString = WS;
    //            if (EmptyCheckValueString != null)
    //                _EmptyCheckValueString = EmptyCheckValueString;
    //            if (EmptyRefValueString != null)
    //                _EmptyRefValueString = EmptyRefValueString;
    //        }
    //        public void CommitChanges()
    //        {
    //            if (ReferenceValue != null)
    //                Ctr.UpdateChecklistReferenceValue(CheckType, ReferenceValue);
    //        }
    //        public void RejectChanges()
    //        {
    //            if (ReferenceValue != null)
    //            {
    //                ReferenceValue.Min.RejectChanges();
    //                ReferenceValue.Max.RejectChanges();
    //            }
    //        }
    //    }
    //    [AddINotifyPropertyChangedInterface]
    //    public class TestListBoolValueItem : TestListItem<CheckValue<bool>, bool?>, ITestListItem<CheckValue<bool>, bool?>
    //    {

    //        public EditTypes EditType { get; } = EditTypes.SingleSelection;

    //        public ObservableCollection<string> ReferenceValueOptions { get; set; } = new ObservableCollection<string>() { true.ToString(), false.ToString() };

    //        public string ReferenceValueString
    //        {
    //            get
    //            {
    //                if (ReferenceValue != null)
    //                    return ReferenceValue.ReferenceValue.Value.ToString();
    //                else
    //                    return _EmptyRefValueString;
    //            }
    //        }

    //        public string CheckValueString
    //        {
    //            get
    //            {
    //                if (CheckValue != null)
    //                    return CheckValue.ToString();
    //                else
    //                    return _EmptyCheckValueString;
    //            }
    //        }
    //        public bool Warning
    //        {
    //            get
    //            {
    //                if (CheckValue == null)
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                else if (ReferenceValue == null)
    //                    if (ParameterOption == ParameterOptions.Required)
    //                        return true;
    //                    else return false;
    //                else
    //                {
    //                    if (!ReferenceValue.Check((bool)CheckValue))
    //                        return true;
    //                    else
    //                        return false;
    //                }
    //            }
    //        }

    //        private string _EmptyCheckValueString = "";
    //        private string _EmptyRefValueString = "";

    //        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;

    //        public string WarningString
    //        {
    //            get
    //            {
    //                if (Warning)
    //                    return _WarningString;
    //                else return "";
    //            }
    //        }
    //        private string _WarningString;


    //        public TestListBoolValueItem(string TN, bool? CV, CheckValue<bool> RV, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
    //        {
    //            TestName = TN;
    //            ReferenceValue = RV;
    //            CheckValue = CV;
    //            _WarningString = WS;
    //            _EmptyCheckValueString = EmptyCheckValueString;
    //            _EmptyRefValueString = EmptyRefValueString;
    //        }
    //        public void CommitChanges()
    //        {
    //            if (ReferenceValue != null)
    //                Ctr.UpdateChecklistReferenceValue(CheckType, ReferenceValue);
    //        }
    //        public void RejectChanges()
    //        {
    //            if (ReferenceValue != null)
    //            {
    //                ReferenceValue.ReferenceValue.RejectChanges();
    //            }
    //        }

    //    }
    [AddINotifyPropertyChangedInterface]
    public class TestListBeamStartStopItem : TestListItem<Ctr.BeamGeometry>, ITestListItem<Ctr.BeamGeometry>
    {
        public TestType TestType { get; set; } // to implement

        public string ReferenceValueString
        {
            get
            {
                if (Reference != null)
                    return Reference.Value.GeometryName;
                else
                    return _EmptyRefValueString;
            }
            set
            { }
        }

        public string CheckValueString
        {
            get
            {
                if (Check != null)
                {
                    if (Check.Trajectory == Trajectories.Static)
                        return string.Format("{0:0.###}", Check.StartAngle);
                    else // arc
                        return string.Format("Start: {0:0.###}  Stop: {1:0.###}", Check.StartAngle, Check.EndAngle);
                }
                else
                    return _EmptyCheckValueString;
            }
        }
        public bool Warning
        {
            get
            {
                if ((Check != null && Reference != null) && ParameterOption == ParameterOptions.Required)
                    return false;
                if (Check == null && ParameterOption == ParameterOptions.None)
                    return false;
                if (Check != null && Reference != null)
                    return false;
                return true;
            }
        }
        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;
        private string _EmptyCheckValueString = "-";
        private string _EmptyRefValueString = "Not specified";

        private List<Ctr.BeamGeometry> _ReferenceGeometryOptions;

        public string WarningString { get; set; } = "DefaultWarning";
        public TestListBeamStartStopItem(string TN, Ctr.BeamGeometry CV, List<Ctr.BeamGeometry> referenceRange, string WS = null, string EmptyCheckValueString = null,
            string EmptyRefValueString = "")
        {
            TestName = TN;
            Check = CV as Ctr.BeamGeometry;
            Reference = new TrackedValue<Ctr.BeamGeometry>(null);
            WarningString = WS;
            _ReferenceGeometryOptions = referenceRange;
            if (EmptyCheckValueString != null)
                _EmptyCheckValueString = EmptyCheckValueString;
            if (EmptyRefValueString != null)
                _EmptyRefValueString = EmptyRefValueString;

            if (Check != null && _ReferenceGeometryOptions != null)
                foreach (var G in _ReferenceGeometryOptions)
                {
                    if (CV.Trajectory == Trajectories.Static)
                    {
                        if (CV.StartAngle.CloseEnough(G.StartAngle, G.StartAngleTolerance))
                            Reference.Value = G;
                    }
                    else // some kind of arc
                    {
                        double InvariantMaxStart = G.GetInvariantAngle(G.StartAngle) + G.StartAngleTolerance;
                        double InvariantMinStart = InvariantMaxStart - 2 * G.StartAngleTolerance;
                        double InvariantMaxEnd = G.GetInvariantAngle(G.EndAngle) + G.EndAngleTolerance;
                        double InvariantMinEnd = InvariantMaxEnd - 2 * G.EndAngleTolerance;

                        var FieldStart = G.GetInvariantAngle(Check.StartAngle);
                        var FieldEnd = G.GetInvariantAngle(Check.EndAngle);

                        if (FieldStart >= InvariantMinStart && FieldStart <= InvariantMaxStart && FieldEnd >= InvariantMinEnd && FieldEnd <= InvariantMaxEnd)
                            Reference.Value = G;
                    }
                }

        }
        public void CommitChanges()
        {
            // not implemented for this class
        }
        public void RejectChanges()
        {
            // not implemented for this class
        }
    }

}
//}

