namespace Squint
{
    public class FixedThreshold : IReferenceThreshold, ITrackedValue
    {
        public ReferenceThresholdCalculationTypes ReferenceThresholdCalculationType { get; private set; } = ReferenceThresholdCalculationTypes.Fixed;

        public bool IsChanged
        {
            get
            {
                if (_MajorViolation.IsChanged || _MinorViolation.IsChanged || _Stop.IsChanged)
                    return true;
                else
                    return false;
            }
        }
        public double ReferenceValue
        {
            get
            {
                if (MinorViolation != null)
                    return (double)MinorViolation;
                if (MajorViolation != null)
                    return (double)MajorViolation;
                else
                    return double.NaN;
            }
        }

        private TrackedValue<double?> _MajorViolation { get; set; }
        private TrackedValue<double?> _MinorViolation { get; set; }
        private TrackedValue<double?> _Stop { get; set; }

        public double? ReferenceMajorViolation { get { return _MajorViolation.ReferenceValue; } }
        public double? ReferenceMinorViolation { get { return _MinorViolation.ReferenceValue; } }
        public double? ReferenceStop { get { return _Stop.ReferenceValue; } }

        public double? MajorViolation
        {
            get
            {
                return _MajorViolation.Value;
            }
            set
            {
                _MajorViolation.Value = value;
            }
        }
        public double? MinorViolation
        {
            get
            {
                return _MinorViolation.Value;
            }
            set
            {
                _MinorViolation.Value = value;
            }
        }
        public double? Stop
        {
            get
            {
                return _Stop.Value;
            }
            set
            {
                _Stop.Value = value;
            }
        }
        public string DataPath { get; } = "";

        public FixedThreshold(double? major = null, double? minor = null, double? stop = null)
        {
            _MajorViolation = new TrackedValue<double?>(major);
            _MinorViolation = new TrackedValue<double?>(minor);
            _Stop = new TrackedValue<double?>(stop);
        }
    }

}

