using System;
using System.Collections.Generic;
using System.Linq;
using PropertyChanged;
using System.Data;

namespace SquintScript
{
    public static partial class Ctr
    {
        [AddINotifyPropertyChangedInterface]
        public class Assessment
        {
            //Events
            //public event PropertyChangedEventHandler PropertyChanged;
            //public event EventHandler<int> AssessmentDeleted;
            //public event EventHandler<int> ComponentUnlinked;
            //public event EventHandler AssessmentOverwriting;
            //public event EventHandler<int> PlanMappingChanged;
            //public event EventHandler NewAssessmentCommitted;
            //public event EventHandler<int> ComponentAssociationChange;
            //public event EventHandler AssessmentNameChanged;
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
            public Assessment(string AssessmentName_in, int DisplayOrder_in)
            {
                ID = Ctr.IDGenerator();
                DisplayOrder = DisplayOrder_in;
                AssessmentName = AssessmentName_in;
                ProtocolID = DataCache.CurrentProtocol.ID;
                PID = DataCache.Patient.Id;
                PatientName = string.Format("{0},{1}", DataCache.Patient.LastName, DataCache.Patient.FirstName);
                DateOfAssessment = String.Format("{0:g}", DateTimeOffset.Now);
                SquintUser = Ctr.SquintUser;
            }
            public int DisplayOrder { get; set; } = 0;
            public int ID { get; private set; }
            public int ProtocolID { get; private set; }
            public string PID { get; private set; }
            public string PatientName { get; private set; }
            public string SquintUser { get; private set; }
            public string DateOfAssessment { get; private set; }
            public string AssessmentName { get; set; }
            public string Comments { get; set; }
            public List<ComponentStatusCodes> StatusCodes(int ComponentID)
            {
                PlanAssociation ECP = GetPlanAssociation(ComponentID, ID);
                if (ECP != null)
                    return ECP.GetErrorCodes();
                else
                    return null;
            }
        }

    }
}

