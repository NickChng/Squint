namespace Squint
{
    public interface IReferenceThreshold : ITrackedValue
    {
        ReferenceThresholdCalculationTypes ReferenceThresholdCalculationType { get; }
        double ReferenceValue { get; }
        double? Stop { get; set; }
        double? MajorViolation { get; set; }
        double? MinorViolation { get; set; }
        double? ReferenceStop { get; }
        double? ReferenceMajorViolation { get; }
        double? ReferenceMinorViolation { get; }

        string DataPath { get; }
    }

}

