//using System.Windows.Forms;

namespace Squint.ViewModels
{

    public class AssessmentPreviewViewModel
    {
        public AssessmentPreviewViewModel(int _ID, string _AssessmentName, string _ReferenceProtocol, string _User, string _DateOfAssessment, string _Comments)
        {
            ID = _ID;
            AssessmentName = _AssessmentName;
            ReferenceProtocol = _ReferenceProtocol;
            User = _User;
            DateOfAssessment = _DateOfAssessment;
            Comments = _Comments;
        }
        public int ID { get; private set; }
        public string AssessmentName { get; private set; }
        public string ReferenceProtocol { get; private set; }
        public string User { get; private set; }
        public string DateOfAssessment { get; private set; }
        public string Comments { get; private set; }
    }

}
