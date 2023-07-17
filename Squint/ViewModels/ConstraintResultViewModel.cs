using System.Collections.Generic;
//using System.Windows.Forms;

namespace Squint
{
    public class ConstraintResultViewModel
    {
        public ConstraintResultViewModel(ConstraintResult CR)
        {
            AssessmentID = CR.AssessmentID;
            ConstraintID = CR.ConstraintID;
            Result = CR.ResultString;
            ResultValue = CR.ResultValue;
            ThresholdStatus = CR.ThresholdStatus;
            StatusCodes = CR.StatusCodes;
            ReferenceType = CR.ReferenceType;
            LabelName = CR.LinkedLabelName;
            isCalculating = CR.isCalculating;
        }
        public int AssessmentID { get; private set; }
        public int ConstraintID { get; private set; }
        public bool isCalculating { get; private set; }
        public double ResultValue { get; private set; }
        public string LabelName { get; private set; }
        public ReferenceTypes ReferenceType { get; private set; }
        public string Result { get; private set; }
        public List<ConstraintResultStatusCodes> StatusCodes { get; private set; } = new List<ConstraintResultStatusCodes>();
        public ReferenceThresholdTypes ThresholdStatus { get; private set; }
    }
}
