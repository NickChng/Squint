using PropertyChanged;
//using System.Windows.Forms;

namespace SquintScript.ViewModels
{

    [AddINotifyPropertyChangedInterface]
    public class SessionView
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        public int ID { get; private set; }
        public string SessionComment { get; private set; }
        public string SessionDisplay { get; private set; }
        public string SessionCreator { get; private set; }
        public string SessionDateTime { get; private set; }
        public string SessionMouseOver { get; private set; }
        public SessionView(Session S)
        {
            ID = S.ID;
            SessionComment = S.SessionComment;
            SessionCreator = S.SessionCreator;
            SessionDateTime = S.SessionDateTime;
            SessionDisplay = S.SessionComment;
            SessionMouseOver = string.Format("{0} {1} ({2})", S.ProtocolName, SessionDateTime, SessionCreator);
        }

    }
}
