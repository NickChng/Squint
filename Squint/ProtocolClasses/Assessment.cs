using System;
using System.Collections.Generic;
using System.Linq;
using PropertyChanged;
using System.Data;

namespace SquintScript
{
 
        [AddINotifyPropertyChangedInterface]
        public class Assessment
        {
            //Events
            //public event PropertyChangedEventHandler PropertyChanged;
            public Assessment(DbAssessment DbO)
            {
                ID = DbO.ID;
                PID = DbO.PID;
                DisplayOrder = DbO.DisplayOrder;
                PatientName = DbO.PatientName;
                Comments = DbO.Comments;
                AssessmentName = DbO.AssessmentName;
                SquintUser = DbO.SquintUser;
            }
            public Assessment(int SessionID_in, string AssessmentName_in, int DisplayOrder_in)
            {
                ID = IDGenerator.GetUniqueId();
                DisplayOrder = DisplayOrder_in;
                AssessmentName = AssessmentName_in;
                SessionID = SessionID_in;
                PID = Ctr.PatientID;
                PatientName = string.Format("{0},{1}", Ctr.PatientLastName, Ctr.PatientFirstName);
                DateOfAssessment = String.Format("{0:g}", DateTimeOffset.Now);
                SquintUser = Ctr.SquintUser;
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
            public List<ComponentStatusCodes> StatusCodes(Component SC)
            {
                PlanAssociation PA = Ctr.GetPlanAssociation(SC.ID, ID);
                if (PA != null)
                    return PA.GetErrorCodes();
                else
                    return null;
            }
        }

 
}

