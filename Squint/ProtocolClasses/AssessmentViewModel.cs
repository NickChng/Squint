using System;
using System.Collections.Generic;
using System.Linq;
using PropertyChanged;
using System.Data;

namespace Squint
{

    [AddINotifyPropertyChangedInterface]
    public class AssessmentViewModel
    {
        //Events
        //public event PropertyChangedEventHandler PropertyChanged;
        private SquintModel _model;
        public AssessmentViewModel(DbAssessment DbO, SquintModel model)
        {
            _model = model;
            ID = DbO.ID;
            PID = DbO.PID;
            DisplayOrder = DbO.DisplayOrder;
            PatientName = DbO.PatientName;
            Comments = DbO.Comments;
            AssessmentName = DbO.AssessmentName;
            SquintUser = DbO.SquintUser;
        }
        public AssessmentViewModel(int SessionID_in, string AssessmentName_in, int DisplayOrder_in, SquintModel model)
        {
            ID = IDGenerator.GetUniqueId();
            _model = model;
            DisplayOrder = DisplayOrder_in;
            AssessmentName = AssessmentName_in;
            SessionID = SessionID_in;
            PID = _model.PatientID;
            PatientName = string.Format("{0},{1}", _model.PatientLastName, _model.PatientFirstName);
            DateOfAssessment = String.Format("{0:g}", DateTimeOffset.Now);
            SquintUser = _model.SquintUser;
        }
        public int DisplayOrder { get; set; } = 0;
        public int ID { get; private set; }
        public int SessionID { get; private set; }
        public string PID { get; private set; }
        public string PatientName { get; private set; }
        public string SquintUser { get; private set; }
        public string DateOfAssessment { get; private set; }
        public string AssessmentName { get; set; }
        public string Comments { get; set; }
        public List<ComponentStatusCodes> StatusCodes(ComponentModel SC)
        {
            PlanAssociationViewModel PA = _model.GetPlanAssociation(SC.Id, ID);
            if (PA != null)
                return PA.GetErrorCodes();
            else
                return null;
        }
    }


}

