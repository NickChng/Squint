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

        public ObservableCollection<string> ReferenceValueOptions { get; set; }
        public int EditMode { get; set; } = 0;

        public string TestName { get; set; }
        public TestType TestType { get; set; } // to implement

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
        public TestListStringValueItem(string TN, string V, string RV, List<string> referenceValueOptions = null, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "", int editMode = 0)
        {
            TestName = TN;
            EditMode = editMode;
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
    }

    [AddINotifyPropertyChangedInterface]
    public class TestListIntValueItem : ITestListItem<int?>
    {

        public ObservableCollection<string> ReferenceValueOptions { get; set; }
        public int EditMode { get; set; } = 0;

        public string TestName { get; set; }
        public TestType TestType { get; set; } // to implement

        public string ReferenceValueString
        {
            get
            {
                if (TestType == TestType.Range)
                {
                    if (_Min != null && _Max != null)
                        return string.Format("{0} - {1}", _Min, _Max);
                    else if (_Min == null && _Max != null)
                        return string.Format(" \u2264 {0}", _Max);
                    else if (_Max == null & _Min != null)
                        return string.Format(" \u2265 {0}", _Min);
                    else
                        return _EmptyRefValueString;
                }
                else if (ReferenceValue == null)
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

        public bool Warning
        {
            get
            {
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
                            case TestType.Range:
                                return !(CheckValue >= _Min && CheckValue <= _Max);
                            case TestType.Equality:
                                return !(ReferenceValue == CheckValue);
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
        private double Eps;
        private int? _Min;
        private int? _Max;
        private string _EmptyCheckValueString = "-";
        private string _EmptyRefValueString = "Not specified";

        public string WarningString { get; set; } = "DefaultWarning";
        public TestListIntValueItem(string TN, int? CV, int? RV, TestType testType = TestType.Equality, List<string> referenceValueOptions = null, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "", double eps = 0, int? min = null, int? max = null, int editMode = 0)
        {
            TestName = TN;
            Eps = eps;
            TestType = testType;
            EditMode = editMode;
            ReferenceValue = RV;
            if (referenceValueOptions != null)
                ReferenceValueOptions = new ObservableCollection<string>(referenceValueOptions);
            CheckValue = CV;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
            _Min = min;
            _Max = max;
        }

    }

    [AddINotifyPropertyChangedInterface]
    public class TestListDoubleValueItem : ITestListItem<double?>
    {

        public ObservableCollection<string> ReferenceValueOptions { get; set; }
        public int EditMode { get; set; } = 0;

        public string TestName { get; set; }
        public TestType TestType { get; set; } // to implement

        public string ReferenceValueString
        {
            get
            {
                if (TestType == TestType.Range)
                {
                    if (_Min != null && _Max != null)
                    {
                        var test = string.Format("{0:0.###} - {1:0.###}", (double)_Min, (double)_Max);
                        return test;
                    }
                    else
                        return _EmptyRefValueString;
                }
                else if (ReferenceValue == null)
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
                if (TestType == TestType.Unset) // if test type is unset never warn
                    return false;
                if (CheckValue == null)
                {
                    if (ParameterOption == ParameterOptions.None || ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
                        return false;
                    else
                        return true;
                }
                else
                    if (ParameterOption == ParameterOptions.None)
                    return true;
                if (ReferenceValue == null)
                {
                    if (TestType != TestType.Range && ParameterOption == ParameterOptions.Required)
                        return true;
                    else
                        return false;
                }
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
        private double Eps;
        private double? _Min;
        private double? _Max;

        private string _EmptyCheckValueString = "-";
        private string _EmptyRefValueString = "Not specified";

        public string CheckNullWarningString = "";
        public string RefNullWarningString = "";
        private string _WarningString;

        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Optional;
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
                    case TestType.Range:
                        if (!(CheckValue >= _Min && CheckValue <= _Max))
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
        public TestListDoubleValueItem(string TN, double? CV, double? RV, TestType testType, List<string> referenceValueOptions = null, string WS = "", string EmptyCheckValueString = null,
            string EmptyRefValueString = null, double eps = 1E-5, double? min = null, double? max = null, int editMode = 0)
        {
            TestName = TN;
            TestType = testType;
            EditMode = editMode;
            if (RV != null)
                if (!double.IsNaN((double)RV))
                    ReferenceValue = RV;
            if (referenceValueOptions != null)
                ReferenceValueOptions = new ObservableCollection<string>(referenceValueOptions);
            if (CV != null)
                if (!double.IsNaN((double)CV))
                    CheckValue = CV;
            Eps = eps;
            _WarningString = WS;
            _Min = min;
            _Max = max;
            if (EmptyCheckValueString != null)
                _EmptyCheckValueString = EmptyCheckValueString;
            if (EmptyRefValueString != null)
                _EmptyRefValueString = EmptyRefValueString;
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class TestListDoubleRangeItem : ITestListItem<double?>
    {
        public ObservableCollection<string> ReferenceValueOptions { get; set; }
        public int EditMode { get; set; } = 0;

        public TestType TestType { get; set; } = TestType.Range;
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
                if (TestType == TestType.Unset) // if test type is unset never warn
                    return false;
                if (CheckValue == null)
                {
                    if (ParameterOption == ParameterOptions.None || ParameterOption == ParameterOptions.Optional) // no warning if checkvalue is null and optional
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (ParameterOption == ParameterOptions.None)
                        return true;
                }
                if (ReferenceValue == null || ReferenceValueMax == null)
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

        public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Optional;
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
            string EmptyRefValueString = null, int editMode = 0)
        {
            TestName = TN;
            EditMode = editMode;
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
    }

    [AddINotifyPropertyChangedInterface]
    public class TestListBoolValueItem : ITestListItem<bool?>
    {

        public ObservableCollection<string> ReferenceValueOptions { get; set; } = new ObservableCollection<string>() { "True", "False" };
        public int EditMode { get; set; } = 0;

        public string TestName { get; set; }
        public TestType TestType { get; set; } = TestType.Equality;

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
        private double Eps;

        private string _EmptyCheckValueString = "";
        private string _EmptyRefValueString = "";

        public string WarningString { get; set; } = "DefaultWarning";
        public TestListBoolValueItem(string TN, bool? CV, bool? RV, TestType testType = TestType.Equality, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "", int editMode = 0)
        {
            TestName = TN;
            TestType = testType;
            EditMode = editMode;
            ReferenceValue = RV;
            CheckValue = CV;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
    }


    [AddINotifyPropertyChangedInterface]
    public class TestListBeamStartStopItem : ITestListItem<Ctr.BeamGeometry>
    {

        public ObservableCollection<string> ReferenceValueOptions { get; set; }
        public int EditMode { get; set; } = 0;

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
            string EmptyRefValueString = "", int editMode = 0)
        {
            TestName = TN;
            EditMode = editMode;
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
                        double InvariantMinStart = InvariantMaxStart - 2*G.StartAngleTolerance;
                        double InvariantMaxEnd = G.GetInvariantAngle(G.EndAngle) + G.EndAngleTolerance;
                        double InvariantMinEnd = InvariantMaxEnd - 2*G.EndAngleTolerance;

                        var FieldStart = G.GetInvariantAngle(CheckValue.StartAngle);
                        var FieldEnd = G.GetInvariantAngle(CheckValue.EndAngle);

                        if (FieldStart >= InvariantMinStart && FieldStart <= InvariantMaxStart && FieldEnd >= InvariantMinEnd && FieldEnd <= InvariantMaxEnd)
                            ReferenceValue = G;
                    }
                }

        }

    }
}
