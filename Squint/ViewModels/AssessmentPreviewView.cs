//using System.Windows.Forms;

namespace SquintScript
{

    public class AssessmentPreviewView
    {
        public AssessmentPreviewView(int _ID, string _AssessmentName, string _ReferenceProtocol, string _User, string _DateOfAssessment, string _Comments)
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
