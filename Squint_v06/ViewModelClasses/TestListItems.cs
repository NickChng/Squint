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
        public CheckTypes CheckType { get; set; }
        
        public string OptionalNameSuffix { get; set; } = "";
        public string CheckTypeString { get { return string.Format("{0} {1}", CheckType.Display(), OptionalNameSuffix); } }
        public TrackedValue<T> Reference { get; set; }
        public TrackedValue<T> Tolerance { get; set; }
        
        public T Check { get; set; }

        protected string _EmptyCheckValueString = "Check value undefined";
        protected string _EmptyRefValueString = "No reference";
        protected string _WarningString = "Warning";

        public virtual string WarningString
        {
            get
            {
                if (Reference == null)
                    return _EmptyRefValueString;
                if (!Reference.isDefined)
                    return _EmptyRefValueString;
                if (Check == null)
                    return _EmptyCheckValueString;
                if (Check is double)
                {
                    if (double.IsNaN(Convert.ToDouble(Check)))
                        return _EmptyCheckValueString;
                    else
                        return _WarningString;
                }
                else
                    return _WarningString;
            }
            set
            {
                _WarningString = value;
            }
        }
    }
    public abstract class TestListClassItem<T> : ObservableObject where T : class
    {
        public CheckTypes CheckType { get; set; }

        public string OptionalNameSuffix { get; set; } = "";
        public string CheckTypeString { get { return string.Format("{0} {1}", CheckType.Display(), OptionalNameSuffix); } }
        public TrackedValue<T> Reference { get; set; }
        public TrackedValue<T> Tolerance { get; set; }

        public T Check { get; set; }

        protected string _EmptyCheckValueString = "Check value undefined";
        protected string _EmptyRefValueString = "No reference";
        protected string _WarningString = "Warning";

        public virtual string WarningString
        {
            get
            {
                if (Reference == null)
                    return _EmptyRefValueString;
                if (!Reference.isDefined)
                    return _EmptyRefValueString;
                if (Check == null)
                    return _EmptyCheckValueString;
                if (Check is double)
                {
                    if (double.IsNaN(Convert.ToDouble(Check)))
                        return _EmptyCheckValueString;
                    else
                        return _WarningString;
                }
                else
                    return _WarningString;
            }
            set
            {
                _WarningString = value;
            }
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class InvalidCheckItem : ITestListItem
    {
        public void SetCheckValue(object value) { }
        public CheckTypes CheckType { get; set; }
        public void CommitChanges() { }
        public void RejectChanges() { }

        public string WarningString 
        {
            get { return "Invalid"; }
        }
        public bool Warning
        {
            get { return true; }
        }
        public string ReferenceValueString
        {
            get { return " - "; }
        }
        public string CheckValueString
        {
            get { return " - "; }
        }

    }

    [AddINotifyPropertyChangedInterface]
    public class CheckValueItem<T> : TestListItem<T>, ITestListItem<T> 
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
                    Reference = new TrackedValue<T>(default(T));
                }
                if (value != null)
                {
                    Reference.Value = (T)value;
                    //RaisePropertyChangedEvent(nameof(ReferenceValueString));
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
        public CheckValueItem(CheckTypes CT, T V, TrackedValue<T> RV, TrackedValue<T> Tol = null, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            CheckType = CT;
            Reference = RV;
            Check = V;
            Tolerance = Tol;
            if (V is Enum)
            {
                EditType = EditTypes.SingleSelection;
                foreach (T E in Enum.GetValues(typeof(T)))
                {
                    ReferenceCollection.Add(E);
                }
            }
            if (Tol != null)
                EditType = EditTypes.SingleValueWithTolerance;
            _WarningString = WS;
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
                Reference.RejectChanges();
            if (Tolerance != null)
                Tolerance.RejectChanges();
        }
    }

    //[AddINotifyPropertyChangedInterface]
    //public class CheckClassItem<T> : TestListClassItem<T>, ITestListClassItem<T> where T : class
    //{
    //    public void SetCheckValue(object CheckThis)
    //    {
    //        Check = (T?)CheckThis;
    //        RaisePropertyChangedEvent(nameof(CheckValueString));
    //        RaisePropertyChangedEvent(nameof(Warning));
    //    }
    //    public EditTypes EditType { get; private set; } = EditTypes.SingleValue;
    //    public TestType Test { get; set; } = TestType.Equality;
    //    public ObservableCollection<T> ReferenceCollection { get; set; } = new ObservableCollection<T>();
    //    public string ReferenceValueString
    //    {
    //        get
    //        {
    //            if (Reference == null)
    //                return _EmptyRefValueString;
    //            else
    //            {
    //                if (Check is double || Check is int)
    //                {
    //                    switch (Test)
    //                    {
    //                        case TestType.Equality:
    //                            if (Tolerance != null)
    //                                if ((dynamic)Tolerance.Value > 1E-2)
    //                                    return string.Format("{0:0.###} \u00b1 {1:0.#}", Reference.Value, Tolerance.Value);
    //                                else
    //                                    return string.Format("{0:0.###}", Reference.Value);
    //                            else
    //                                return string.Format("{0:0.###}", Reference.Value);
    //                        case TestType.GreaterThan:
    //                            return string.Format("\u2265 {0:0.###}", Reference.Value);
    //                        case TestType.LessThan:
    //                            return string.Format("\u2264 {0:0.###}", Reference.Value);
    //                        default:
    //                            return string.Format("{0:0.###}", Reference.Value);
    //                    }
    //                }
    //                else if (Check is Enum)
    //                    return string.Format("{0}", (Reference.Value as Enum).Display());
    //                else
    //                    return Reference.Value.ToString();
    //            }
    //        }
    //    }

    //    public T SetReference
    //    {
    //        set
    //        {
    //            if (Reference == null)
    //            {
    //                Reference = new TrackedValue<T>(default(T));
    //            }
    //            if (value != null)
    //            {
    //                Reference.Value = (T)value;
    //                //RaisePropertyChangedEvent(nameof(ReferenceValueString));
    //            }
    //        }
    //        get
    //        {
    //            if (Reference != null)
    //                return Reference.Value;
    //            else
    //                return default;
    //        }
    //    }

    //    public T SetTolerance
    //    {
    //        set
    //        {
    //            if (Tolerance == null)
    //            {
    //                Tolerance = new TrackedValue<T>(default(T));
    //            }
    //            if (value != null)
    //            {
    //                Tolerance.Value = (T)value;
    //                //RaisePropertyChangedEvent(nameof(ReferenceValueString));
    //            }
    //        }
    //        get
    //        {
    //            if (Tolerance != null)
    //                return Tolerance.Value;
    //            else
    //                return default;
    //        }
    //    }

    //    public string CheckValueString
    //    {
    //        get
    //        {
    //            if (Check == null)
    //                return _EmptyCheckValueString;
    //            else
    //            {
    //                if (Check is double || Check is int)
    //                    return string.Format("{0:0.###}", Check);
    //                else if (Check is Enum)
    //                    return (Check as Enum).Display();
    //                else
    //                    return Check.ToString();
    //            }
    //        }
    //    }

    //    public ParameterOptions ParameterOption = ParameterOptions.Required;

    //    public bool Warning
    //    {
    //        get
    //        {
    //            if (CheckPass == null)
    //                return false;
    //            else
    //                return !(bool)CheckPass;
    //        }
    //        set { }
    //    }
    //    public bool? CheckPass
    //    {
    //        get
    //        {
    //            if (Reference == null)
    //            {
    //                if (ParameterOption == ParameterOptions.Required)
    //                    return false;
    //                else
    //                    return null;
    //            }
    //            else if (Check == null)
    //            {
    //                if (ParameterOption == ParameterOptions.Required)
    //                    return false;
    //                else
    //                    return null;
    //            }
    //            else
    //            {
    //                switch (Test)
    //                {
    //                    case TestType.Equality:
    //                        if (Tolerance == null)
    //                        {
    //                            if ((dynamic)Reference.Value == (dynamic)Check)
    //                                return true;
    //                            else
    //                                return false;
    //                        }
    //                        else
    //                        {
    //                            if (Math.Abs((dynamic)Reference.Value - (dynamic)Check) < (dynamic)Tolerance.Value)
    //                                return true;
    //                            else
    //                                return false;
    //                        }
    //                    case TestType.GreaterThan:
    //                        if ((dynamic)Check >= (dynamic)Reference.Value)
    //                            return true;
    //                        else
    //                            return false;
    //                    case TestType.LessThan:
    //                        if ((dynamic)Check <= (dynamic)Reference.Value)
    //                            return true;
    //                        else
    //                            return false;
    //                    default:
    //                        return false;
    //                }
    //            }
    //        }
    //    }
    //    public CheckClassItem(CheckTypes CT, T V, TrackedValue<T> RV, TrackedValue<T> Tol = null, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
    //    {
    //        CheckType = CT;
    //        Reference = RV;
    //        Check = V;
    //        Tolerance = Tol;
    //        if (V is Enum)
    //        {
    //            EditType = EditTypes.SingleSelection;
    //            foreach (T E in Enum.GetValues(typeof(T)))
    //            {
    //                ReferenceCollection.Add(E);
    //            }
    //        }
    //        if (Tol != null)
    //            EditType = EditTypes.SingleValueWithTolerance;
    //        _WarningString = WS;
    //        _EmptyCheckValueString = EmptyCheckValueString;
    //        _EmptyRefValueString = EmptyRefValueString;
    //    }
    //    public void CommitChanges()
    //    {
    //        if (Reference != null)
    //            Ctr.UpdateChecklistReferenceValue(CheckType, Reference.Value);
    //    }
    //    public void RejectChanges()
    //    {
    //        if (Reference != null)
    //            Reference.RejectChanges();
    //        if (Tolerance != null)
    //            Tolerance.RejectChanges();
    //    }
    //}

    [AddINotifyPropertyChangedInterface]
    public class CheckRangeItem<T> : TestListItem<T>, ITestListItem<T>
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
        public CheckRangeItem(CheckTypes CT, T V, TrackedValue<T> min, TrackedValue<T> max, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            CheckType = CT;
            Reference = min;
            Reference2 = max;
            Check = V;
            _WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
        public void CommitChanges() { }
        
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
        public void SetCheckValue(object CheckThis)
        {
            Check = (T)CheckThis;
            RaisePropertyChangedEvent(nameof(CheckValueString));
            RaisePropertyChangedEvent(nameof(Warning));
        }
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
                    if (ParameterOption == ParameterOptions.Required)
                        return false;
                    else
                        return null;
                else if (Check == null)
                {
                    if (ParameterOption == ParameterOptions.Required)
                        return false;
                    else
                        return null;
                }
                else
                {
                    if (ReferenceCollection.Select(x => x.Value).Contains((T)Check))
                        return true;
                    else
                        return false;
                }
            }
        }
        public CheckContainsItem(CheckTypes CT, T V, List<TrackedValue<T>> referenceCollection, string WS = "", string EmptyCheckValueString = "", string EmptyRefValueString = "")
        {
            CheckType = CT;
            ReferenceCollection = new ObservableCollection<TrackedValue<T>>(referenceCollection);
            Check = V;
            WarningString = WS;
            _EmptyCheckValueString = EmptyCheckValueString;
            _EmptyRefValueString = EmptyRefValueString;
        }
        public void CommitChanges() { }
        public void RejectChanges()
        {
            if (Reference != null)
            {
                Reference.RejectChanges();
            }
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class TestListBeamStartStopItem : TestListClassItem<Ctr.BeamGeometry>, ITestListClassItem<Ctr.BeamGeometry> 
    {
        public void SetCheckValue(object CheckThis)
        {
            Check = (Ctr.BeamGeometry)CheckThis;
            RaisePropertyChangedEvent(nameof(CheckValueString));
            RaisePropertyChangedEvent(nameof(Warning));
        }
        public TestType TestType { get; set; } // to implement

        public string ReferenceValueString
        {
            get
            {
                if (Reference != null)
                    if (Reference.isDefined)
                        return Reference.Value.GeometryName;
                    else
                        return _EmptyRefValueString;
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

        private List<Ctr.BeamGeometry> _ReferenceGeometryOptions;

        public TestListBeamStartStopItem(CheckTypes CT, Ctr.BeamGeometry CV, List<Ctr.BeamGeometry> referenceRange, string WS = null, string EmptyCheckValueString = null,
            string EmptyRefValueString = "")
        {
            CheckType = CT;
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

