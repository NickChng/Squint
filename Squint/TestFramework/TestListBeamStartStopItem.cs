using PropertyChanged;
using Squint.Interfaces;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Squint;
using System.Windows.Markup;
using Squint.Extensions;
using g3;
using System.ComponentModel;
using System.Windows.Media.Converters;

namespace Squint.TestFramework
{

    [AddINotifyPropertyChangedInterface]
    public class TestListBeamStartStopItem : TestListClassItem<BeamGeometryDefinition>, ITestListClassItem<BeamGeometryDefinition> 
    {
        public void SetCheckValue(object CheckThis)
        {
            Check = (BeamGeometryDefinition)CheckThis;
            RaisePropertyChangedEvent(nameof(CheckValueString));
            RaisePropertyChangedEvent(nameof(Warning));
        }
        public EditTypes EditType { get; private set; } = EditTypes.AnyOfValues;

        public bool IsDirty { get { return false; } } // at present no way to edit beamgeometry
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
                    if (Check.Trajectory == TrajectoryTypes.Static)
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

        private List<BeamGeometryDefinition> _ReferenceGeometryOptions;

        public TestListBeamStartStopItem(CheckTypes CT, BeamGeometryDefinition CV, List<BeamGeometryDefinition> referenceRange, string WS = null, string EmptyCheckValueString = null,
            string EmptyRefValueString = "")
        {
            CheckType = CT;
            Check = CV as BeamGeometryDefinition;
            Reference = new TrackedValue<BeamGeometryDefinition>(null);
            WarningString = WS;
            _ReferenceGeometryOptions = referenceRange;
            if (EmptyCheckValueString != null)
                _EmptyCheckValueString = EmptyCheckValueString;
            if (EmptyRefValueString != null)
                _EmptyRefValueString = EmptyRefValueString;

            if (Check != null && _ReferenceGeometryOptions != null)
                foreach (var G in _ReferenceGeometryOptions)
                {
                    if (CV.Trajectory == TrajectoryTypes.Static)
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

