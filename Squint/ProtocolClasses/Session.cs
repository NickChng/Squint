using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squint
{
    public class Session
    {
      
        public Protocol SessionProtocol;
            
        public List<Assessment> Assessments = new List<Assessment>();

        public List<PlanAssociation> PlanAssociations = new List<PlanAssociation>();
        public int ID { get; private set; }
        public int PatientId { get; set; } = 0;
        public string ProtocolName
        {
            get
            {
                if (SessionProtocol != null)
                    return SessionProtocol.ProtocolName;
                else
                    return "";
            }
        }
         public string SessionComment { get; set; }
        public string SessionCreator { get; set; }
        public string SessionDateTime { get; set; }
        public Session()
        {
            ID = IDGenerator.GetUniqueId();
        }
        public Session(DbSession DbS)
        {
            ID = DbS.ID;
            SessionProtocol = new Protocol(DbS.DbSessionProtocol);
            SessionComment = DbS.SessionComment;
            SessionCreator = DbS.SessionCreator;
            SessionDateTime = DbS.SessionDateTime;
        }
        
    }
}

