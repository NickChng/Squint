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

namespace SquintScript.ViewModelClasses
{
    [AddINotifyPropertyChangedInterface]
    public class TestListStringValueItem : ITestListItem<string>
    {
        public string TestName { get; set; }
        public TestType TestType { get; set; } = TestType.Equality;

        public EditTypes EditType { get; } = EditTypes.SingleValue;
        public string ReferenceValueString
        {
            get
            {
                if (ReferenceValue == "")
                    return _EmptyRefValueString;
                else
                    return ReferenceValue;
            }
            set
            {
                ReferenceValue = value;
            }
        }
        public string ReferenceValue { get; set; }

        public string CheckValue { get; set; }

        public string CheckValueString
        {
            get
            {
                if (CheckValue == "")
                    return _EmptyCheckValueString;
                else
                    return CheckValue;
            }
        }
        public Visibility TestVisibility
        {
            get
            {
                if (ReferenceValue == "")
                    return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        private string _EmptyCheckValueString = "";
        private string _EmptyRefValueString = "";

        public ParameterOptions ParameterOption = ParameterOptions.Required;
        public bool Warning
        {
            get
            {
                if (ReferenceValue == null)
                    return false;
                else if (CheckValue == null)
                {
                    if (ParameterOption == ParameterOptions.Required)
                        return true;
                    else return false;
                }
                else
                {
                    if (ReferenceValue == CheckValue)
                        return false;
                    else return true;
                }
            }
        }
        public string WarningString { get; set; } = "DefaultWarning";
        public TestListStringValueItem(string TN, string V, string RV, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            TestName = TN;
            ReferenceValue = RV;
            CheckValue = V;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
        public void CommitChanges() { }
    }

    [AddINotifyPropertyChangedInterface]
    public class TestListStringChoiceItem : ITestListItem<string>
    {

        public ObservableCollection<string> ReferenceValueOptions { get; set; }

        public string TestName { get; set; }
        public TestType TestType { get; set; } // to implement

        public EditTypes EditType { get; } = EditTypes.SingleSelection;
        public string ReferenceValueString
        {
            get
            {
                if (TestType == TestType.MC)
                {
                    if (ReferenceValueOptions.Contains(CheckValue))
                        return CheckValueString;
                    else return "Invalid option";
                }
                if (ReferenceValue == "")
                    return _EmptyRefValueString;
                else
                    return ReferenceValue;
            }
            set
            {
                ReferenceValue = value;
            }
        }
        public string ReferenceValue { get; set; }

        public string CheckValue { get; set; }

        public string CheckValueString
        {
            get
            {
                if (CheckValue == "")
                    return _EmptyCheckValueString;
                else
                    return CheckValue;
            }
        }
        public Visibility TestVisibility
        {
            get
            {
                if (ReferenceValue == "")
                    return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        private string _EmptyCheckValueString = "";
        private string _EmptyRefValueString = "";

        public ParameterOptions ParameterOption = ParameterOptions.Required;
        public bool Warning
        {
            get
            {
                if (ReferenceValue == null && ReferenceValueOptions == null)
                    return false;
                else if (TestType == TestType.MC && ReferenceValueOptions != null)
                {
                    if (ReferenceValueOptions.Contains(CheckValue))
                        return false;
                    else return true;
                }
                else
                {
                    if (ReferenceValue == CheckValue)
                        return false;
                    else return true;
                }
            }
        }
        public string WarningString { get; set; } = "DefaultWarning";
        public TestListStringChoiceItem(string TN, string V, string RV, List<string> referenceValueOptions = null, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            TestName = TN;
            ReferenceValue = RV;
            if (referenceValueOptions != null)
            {
                ReferenceValueOptions = new ObservableCollection<string>(referenceValueOptions);
                TestType = TestType.MC;
            }
            CheckValue = V;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
        
        public void CommitChanges() { }
    }

    [AddINotifyPropertyChangedInterface]
    public class TestListStringAnyItem : ITestListItem<string>
    {

        public ObservableCollection<string> ReferenceValueOptions { get; set; }

        public string TestName { get; set; }
        public TestType TestType { get; set; } // to implement

        public EditTypes EditType { get; } = EditTypes.AnyOfValues;

        public string ReferenceValueString
        {
            get
            {
                if (TestType == TestType.MC)
                {
                    if (ReferenceValueOptions.Contains(CheckValue))
                        return CheckValueString;
                    else return "Invalid option";
                }
                if (ReferenceValue == "")
                    return _EmptyRefValueString;
                else
                    return ReferenceValue;
            }
            set
            {
                ReferenceValue = value;
            }
        }
        public string ReferenceValue { get; set; }

        public string CheckValue { get; set; }

        public string CheckValueString
        {
            get
            {
                if (CheckValue == "")
                    return _EmptyCheckValueString;
                else
                    return CheckValue;
            }
        }
        public Visibility TestVisibility
        {
            get
            {
                if (ReferenceValue == "")
                    return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        private string _EmptyCheckValueString = "";
        private string _EmptyRefValueString = "";

        public ParameterOptions ParameterOption = ParameterOptions.Required;
        public bool Warning
        {
            get
            {
                if (ReferenceValueOptions == null)
                    return false;
                else if (CheckValue == null)
                {
                    if (ParameterOption == ParameterOptions.Required)
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (ReferenceValueOptions.Contains(CheckValue))
                        return false;
                    else return true;
                }
            }
        }
        public string WarningString { get; set; } = "DefaultWarning";
        public TestListStringAnyItem(string TN, string V, List<string> referenceValueOptions = null, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            TestName = TN;
            if (referenceValueOptions != null)
            {
                ReferenceValueOptions = new ObservableCollection<string>(referenceValueOptions);
                TestType = TestType.MC;
            }
            CheckValue = V;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }

        public void CommitChanges() { }
    }

    [AddINotifyPropertyChangedInterface]
    public class TestListIntValueItem : ITestListItem<int?>
    {

        public string TestName { get; set; }
        public TestType TestType { get; set; } = TestType.Equality;// to implement 
        public EditTypes EditType { get; } = EditTypes.SingleValue;
        public string ReferenceValueString
        {
            get
            {
                if (ReferenceValue == null)
                    return _EmptyRefValueString;
                else
                {
                    return ReferenceValue.ToString();
                }
            }
            set
            {
                if (int.TryParse(value, out int result))
                    ReferenceValue = result;
            }
        }
        public int? ReferenceValue { get; set; }

        public int? CheckValue { get; set; }

        public string CheckValueString
        {
            get
            {
                if (CheckValue == null)
                    return _EmptyCheckValueString;
                else return CheckValue.ToString();
            }
        }
        public Visibility TestVisibility
        {
            get
            {
                if (ReferenceValue == null)
                    return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }
        public ParameterOptions ParameterOption = ParameterOptions.Required;
        public bool Warning
        {
            get
            {
                if (ReferenceValue == null)
                    return false;
                else
                {
                    if (CheckValue == null)
                    {
                        if (ParameterOption == ParameterOptions.Required)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        switch (TestType)
                        {
                            case TestType.Equality:
                                return !(Math.Abs((int)ReferenceValue - (int)CheckValue) <= Eps);
                            case TestType.GreaterThan:
                                return !(CheckValue >= ReferenceValue);
                            case TestType.LessThan:
                                return !(CheckValue <= ReferenceValue);
                            default:
                                return true;
                        }
                    }
                }
            }
        }
        private string _EmptyCheckValueString = "-";
        private string _EmptyRefValueString = "Not specified";
        public int Eps { get; set; } = 0;

        public string WarningString { get; set; } = "DefaultWarning";
        public TestListIntValueItem(string TN, int? CV, int? RV, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "", int eps = 0)
        {
            TestName = TN;
            Eps = eps;
            ReferenceValue = RV;
            CheckValue = CV;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }

        public void CommitChanges() { }
    }

    [AddINotifyPropertyChangedInterface]
    public class TestListDoubleValueItem : ITestListItem<double?>
    {

        public string TestName { get; set; }
        public CheckTypes CheckType { get; set; }
        public TestType TestType { get; set; } = TestType.Equality;  // to implement 
        public EditTypes EditType { get; } = EditTypes.SingleValue;
        public string ReferenceValueString
        {
            get
            {
                if (ReferenceValue == null)
                    return _EmptyRefValueString;
                else
                {
                    switch (TestType)
                    {
                        case TestType.Equality:
                            {
                                if (Eps > 1E-2)
                                    return string.Format("{0:0.###} \u00b1 {1:0.#}", ReferenceValue, Eps);
                                else
                                    return string.Format("{0:0.###}", ReferenceValue);
                            }
                        case TestType.GreaterThan:
                            return string.Format("\u2265 {0:0.###}", ReferenceValue);
                        case TestType.LessThan:
                            return string.Format("\u2264 {0:0.###}", ReferenceValue);
                        default:
                            return string.Format("{0:0.###}", ReferenceValue);
                    }

                }

            }
            set
            {
                if (double.TryParse(value, out double result))
                    ReferenceValue = result;
            }
        }
        public double? ReferenceValue { get; set; }

        public double? CheckValue { get; set; }

        public string CheckValueString
        {
            get
            {
                if (CheckValue != null)
                    return string.Format("{0:0.###}", CheckValue);
                else
                    return _EmptyCheckValueString;
            }
        }
        public Visibility TestVisibility
        {
            get
            {
                if (ReferenceValue == null)
                    return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }
        public bool Warning
        {
            get
            {
                if (ReferenceValue == null)
                    return false;
                else if (CheckValue == null)
                {
                    if (ParameterOption == ParameterOptions.Required) // no warning if checkvalue is null and optional
                        return true;
                    else
                        return false;
                }
                else
                {
                    switch (TestType)
                    {
                        case TestType.Equality:
                            return !((double)ReferenceValue).CloseEnough((double)CheckValue, Eps);
                        case TestType.GreaterThan:
                            return !(CheckValue >= ReferenceValue);
                        case TestType.LessThan:
                            return !(CheckValue <= ReferenceValue);
                        default:
                            return true;
                    }
                }
            }
        }
        public double Eps { get; set; }

        private string _EmptyCheckValueString = "-";
        private string _EmptyRefValueString = "Not specified";

        public string CheckNullWarningString = "";
        public string RefNullWarningString = "";
        private string _WarningString;

        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;
        public string WarningString
        {
            get
            {
                if (TestType == TestType.Unset) // if test type is unset never warn
                    return "";
                if (CheckValue == null)
                {
                    if (ParameterOption == ParameterOptions.None)
                        return "";
                    else if (ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
                        return "Optional";
                    else
                        return _EmptyCheckValueString;
                }
                else
                    if (ParameterOption == ParameterOptions.None)
                    return "Not indicated";
                if (ReferenceValue == null)
                {
                    if (TestType != TestType.Range)
                        if (ParameterOption == ParameterOptions.Required)
                            return _EmptyRefValueString;
                        else
                            return "";
                }
                switch (TestType)
                {
                    case TestType.Equality:
                        if (!((double)ReferenceValue).CloseEnough((double)CheckValue, Eps))
                            return _WarningString;
                        break;
                    case TestType.GreaterThan:
                        if (!(CheckValue >= ReferenceValue))
                            return _WarningString;
                        break;
                    case TestType.LessThan:
                        if (!(CheckValue <= ReferenceValue))
                            return _WarningString;
                        break;
                    default:
                        return _WarningString;
                }
                return _WarningString;
            }
            set { }
        }
        public TestListDoubleValueItem(string TN, double? CV, double? RV, string WS = "", string EmptyCheckValueString = null,
            string EmptyRefValueString = null, double eps = 1E-5)
        {
            TestName = TN;
            if (RV != null)
                if (!double.IsNaN((double)RV))
                    ReferenceValue = RV;
            if (CV != null)
                if (!double.IsNaN((double)CV))
                    CheckValue = CV;
            Eps = eps;
            _WarningString = WS;
            if (EmptyCheckValueString != null)
                _EmptyCheckValueString = EmptyCheckValueString;
            if (EmptyRefValueString != null)
                _EmptyRefValueString = EmptyRefValueString;
        }
        public void CommitChanges()
        {
            if (ReferenceValue != null)
                Ctr.UpdateChecklistReferenceValue(CheckType, (double)ReferenceValue);
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class TestListDoubleRangeItem : ITestListItem<double?>
    {

        public TestType TestType { get; set; } = TestType.Range;
        public EditTypes EditType { get; } = EditTypes.RangeValues;
        public string TestName { get; set; }
        public string ReferenceValueString
        {
            get
            {
                if (ReferenceValue != null && ReferenceValueMax != null)
                {
                    var test = string.Format("{0:0.###} - {1:0.###}", (double)ReferenceValue, (double)ReferenceValueMax);
                    return test;
                }
                else
                    return _EmptyRefValueString;
            }
            set
            {
                if (double.TryParse(value, out double result))
                    ReferenceValue = result;
            }
        }
        public double? ReferenceValue { get; set; }
        public double? ReferenceValueMax { get; set; }

        public double? CheckValue { get; set; }

        public string CheckValueString
        {
            get
            {
                if (CheckValue != null)
                    return string.Format("{0:0.###}", CheckValue);
                else
                    return _EmptyCheckValueString;
            }
        }
        public Visibility TestVisibility
        {
            get
            {
                if (ReferenceValue == null)
                    return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        public bool Warning
        {
            get
            {
                if (CheckValue == null)
                {
                    if (ParameterOption == ParameterOptions.None || ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
                        return false;
                    else
                        return true;
                }
                else if (ReferenceValue == null || ReferenceValueMax == null)
                {
                    if (ParameterOption == ParameterOptions.Required)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return !(CheckValue >= ReferenceValue && CheckValue <= ReferenceValueMax);
                }
            }
        }

        private string _EmptyCheckValueString = "-";
        private string _EmptyRefValueString = "Not specified";

        public string CheckNullWarningString = "";
        public string RefNullWarningString = "";
        private string _WarningString;

        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;
        public string WarningString
        {
            get
            {
                if (TestType == TestType.Unset) // if test type is unset never warn
                    return "";
                if (CheckValue == null)
                {
                    if (ParameterOption == ParameterOptions.None)
                        return "";
                    else if (ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
                        return "Optional";
                    else
                        return _EmptyCheckValueString;
                }
                else
                    if (ParameterOption == ParameterOptions.None)
                    return "Not indicated";
                if (ReferenceValue == null || ReferenceValueMax == null)
                {
                    if (ParameterOption == ParameterOptions.Required)
                        return _EmptyRefValueString;
                    else
                        return "";
                }
                if (!(CheckValue >= ReferenceValue && CheckValue <= ReferenceValueMax))
                    return _WarningString;
                else
                    return "";
            }
            set { }
        }
        public TestListDoubleRangeItem(string TN, double? CV, double? min = null, double? max = null, string WS = "", string EmptyCheckValueString = null,
            string EmptyRefValueString = null)
        {
            TestName = TN;
            if (min != null)
                if (!double.IsNaN((double)min))
                    ReferenceValue = min;
            if (max != null)
                if (!double.IsNaN((double)max))
                    ReferenceValueMax = max;
            if (CV != null)
                if (!double.IsNaN((double)CV))
                    CheckValue = CV;
            _WarningString = WS;
            if (EmptyCheckValueString != null)
                _EmptyCheckValueString = EmptyCheckValueString;
            if (EmptyRefValueString != null)
                _EmptyRefValueString = EmptyRefValueString;
        }

        public void CommitChanges()
        { }

    }

    [AddINotifyPropertyChangedInterface]
    public class TestListIntRangeItem : ITestListItem<int?>
    {

        public TestType TestType { get; set; } = TestType.Range;
        public EditTypes EditType { get; } = EditTypes.RangeValues;
        public string TestName { get; set; }
        public string ReferenceValueString
        {
            get
            {
                if (ReferenceValue != null && ReferenceValueMax != null)
                {
                    var test = string.Format("{0:0.###} - {1:0.###}", (double)ReferenceValue, (double)ReferenceValueMax);
                    return test;
                }
                else
                    return _EmptyRefValueString;
            }
            set
            {
                if (int.TryParse(value, out int result))
                    ReferenceValue = result;
            }
        }
        public int? ReferenceValue { get; set; }
        public int? ReferenceValueMax { get; set; }

        public int? CheckValue { get; set; }

        public string CheckValueString
        {
            get
            {
                if (CheckValue != null)
                    return string.Format("{0:0.###}", CheckValue);
                else
                    return _EmptyCheckValueString;
            }
        }
        public Visibility TestVisibility
        {
            get
            {
                if (ReferenceValue == null)
                    return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        public bool Warning
        {
            get
            {
                if (CheckValue == null)
                {
                    if (ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
                        return false;
                    else
                        return true;
                }
                else if (ReferenceValue == null || ReferenceValueMax == null)
                {
                    if (ParameterOption == ParameterOptions.Required)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return !(CheckValue >= ReferenceValue && CheckValue <= ReferenceValueMax);
                }
            }
        }

        private string _EmptyCheckValueString = "-";
        private string _EmptyRefValueString = "Not specified";

        public string CheckNullWarningString = "";
        public string RefNullWarningString = "";
        private string _WarningString;

        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;
        public string WarningString
        {
            get
            {
                if (TestType == TestType.Unset) // if test type is unset never warn
                    return "";
                if (CheckValue == null)
                {
                    if (ParameterOption == ParameterOptions.None)
                        return "";
                    else if (ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
                        return "Optional";
                    else
                        return _EmptyCheckValueString;
                }
                else
                    if (ParameterOption == ParameterOptions.None)
                    return "Not indicated";
                if (ReferenceValue == null || ReferenceValueMax == null)
                {
                    if (ParameterOption == ParameterOptions.Required)
                        return _EmptyRefValueString;
                    else
                        return "";
                }
                if (!(CheckValue >= ReferenceValue && CheckValue <= ReferenceValueMax))
                    return _WarningString;
                else
                    return "";
            }
            set { }
        }
        public TestListIntRangeItem(string TN, int? CV, int? min = null, int? max = null, string WS = "", string EmptyCheckValueString = null,
            string EmptyRefValueString = null)
        {
            TestName = TN;
            if (min != null)
                if (!double.IsNaN((double)min))
                    ReferenceValue = min;
            if (max != null)
                if (!double.IsNaN((double)max))
                    ReferenceValueMax = max;
            if (CV != null)
                if (!double.IsNaN((double)CV))
                    CheckValue = CV;
            _WarningString = WS;
            if (EmptyCheckValueString != null)
                _EmptyCheckValueString = EmptyCheckValueString;
            if (EmptyRefValueString != null)
                _EmptyRefValueString = EmptyRefValueString;
        }
        public void CommitChanges()
        {
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class TestListBoolValueItem : ITestListItem<bool?>
    {
        public string TestName { get; set; }
        public TestType TestType { get; set; } = TestType.Equality;

        public EditTypes EditType { get; } = EditTypes.SingleSelection;

        public ObservableCollection<string> ReferenceValueOptions { get; set; } = new ObservableCollection<string>() { true.ToString(), false.ToString() };

        public string ReferenceValueString
        {
            get
            {
                if (ReferenceValue != null)
                    return ReferenceValue.ToString();
                else
                    return _EmptyRefValueString;
            }
            set
            {
                if (bool.TryParse(value, out bool result))
                    ReferenceValue = result;
            }
        }
        public bool? ReferenceValue { get; set; }

        public bool? CheckValue { get; set; }

        public string CheckValueString
        {
            get
            {
                if (CheckValue != null)
                    return CheckValue.ToString();
                else
                    return _EmptyCheckValueString;
            }
        }
        public Visibility TestVisibility
        {
            get
            {
                if (ReferenceValue == null)
                    return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        public bool Warning
        {
            get
            {
                if (TestType == TestType.Unset)
                    return false;
                if (ReferenceValue == null)
                    return false;
                else
                {
                    if (CheckValue == null)
                        return true;
                    else
                    {
                        switch (TestType)
                        {
                            case TestType.Equality:
                                return ReferenceValue != CheckValue;
                            default:
                                return true;
                        }
                    }
                }
            }
        }

        private string _EmptyCheckValueString = "";
        private string _EmptyRefValueString = "";

        public string WarningString { get; set; } = "DefaultWarning";
        public TestListBoolValueItem(string TN, bool? CV, bool? RV, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            TestName = TN;
            ReferenceValue = RV;
            CheckValue = CV;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
        public void CommitChanges()
        {
        }

    }
    [AddINotifyPropertyChangedInterface]
    public class TestListBeamStartStopItem : ITestListItem<Ctr.BeamGeometry>
    {
        public string TestName { get; set; }
        public TestType TestType { get; set; } // to implement

        public string ReferenceValueString
        {
            get
            {
                if (ReferenceValue != null)
                    return ReferenceValue.GeometryName;
                else
                    return _EmptyRefValueString;
            }
            set
            { }
        }
        public Ctr.BeamGeometry ReferenceValue { get; set; }

        public Ctr.BeamGeometry CheckValue { get; set; }
        public string CheckValueString
        {
            get
            {
                if (CheckValue != null)
                {
                    if (CheckValue.Trajectory == Trajectories.Static)
                        return string.Format("{0:0.###}", CheckValue.StartAngle);
                    else // arc
                        return string.Format("Start: {0:0.###}  Stop: {1:0.###}", CheckValue.StartAngle, CheckValue.EndAngle);
                }
                else
                    return _EmptyCheckValueString;
            }
        }
        public Visibility TestVisibility
        {
            get
            {
                if (ReferenceValue == null)
                    return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        public bool Warning
        {
            get
            {
                if ((CheckValue != null && ReferenceValue != null) && ParameterOption == ParameterOptions.Required)
                    return false;
                if (CheckValue == null && ParameterOption == ParameterOptions.None)
                    return false;
                if (CheckValue != null && ReferenceValue != null)
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
            CheckValue = CV as Ctr.BeamGeometry;
            WarningString = WS;
            _ReferenceGeometryOptions = referenceRange;
            if (EmptyCheckValueString != null)
                _EmptyCheckValueString = EmptyCheckValueString;
            if (EmptyRefValueString != null)
                _EmptyRefValueString = EmptyRefValueString;

            if (CheckValue != null && _ReferenceGeometryOptions != null)
                foreach (var G in _ReferenceGeometryOptions)
                {
                    if (CV.Trajectory == Trajectories.Static)
                    {
                        if (CV.StartAngle.CloseEnough(G.StartAngle, G.StartAngleTolerance))
                            ReferenceValue = G;
                    }
                    else // some kind of arc
                    {
                        double InvariantMaxStart = G.GetInvariantAngle(G.StartAngle) + G.StartAngleTolerance;
                        double InvariantMinStart = InvariantMaxStart - 2 * G.StartAngleTolerance;
                        double InvariantMaxEnd = G.GetInvariantAngle(G.EndAngle) + G.EndAngleTolerance;
                        double InvariantMinEnd = InvariantMaxEnd - 2 * G.EndAngleTolerance;

                        var FieldStart = G.GetInvariantAngle(CheckValue.StartAngle);
                        var FieldEnd = G.GetInvariantAngle(CheckValue.EndAngle);

                        if (FieldStart >= InvariantMinStart && FieldStart <= InvariantMaxStart && FieldEnd >= InvariantMinEnd && FieldEnd <= InvariantMaxEnd)
                            ReferenceValue = G;
                    }
                }

        }
        public void CommitChanges()
        {
        }
    }

}

