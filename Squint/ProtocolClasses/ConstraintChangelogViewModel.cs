using System;
using PropertyChanged;

namespace Squint
{

    [AddINotifyPropertyChangedInterface]
    public class ConstraintChangelogViewModel
    {
        public string DateString { get; private set; } = DateTime.Now.ToShortDateString();
        public string ChangeDescription { get; private set; } = "Long multi-line history for testing formatting of control. Long multi-line history for testing formatting of control. Long multi-line history for testing formatting of control.";
        public string ChangeAuthor { get; private set; } = "NoAuthor";
        public int ConstraintID { get; private set; }
        public string ConstraintString { get; private set; } = "Null Constraint String";

        private SquintModel _model;
        public ConstraintChangelogViewModel(SquintModel model, DbConstraintChangelog DbCC = null)
        {
            _model = model;
            if (DbCC != null)
            {
                DateString = string.Join(" ", DateTime.FromBinary(DbCC.Date).ToShortDateString(), DateTime.FromBinary(DbCC.Date).ToShortTimeString());
                ChangeDescription = DbCC.ChangeDescription;
                ChangeAuthor = DbCC.ChangeAuthor;
                ConstraintID = DbCC.ConstraintID;
                ConstraintString = DbCC.ConstraintString;
            }
        }
        public ConstraintChangelogViewModel(SquintModel model, ConstraintModel C)
        {
            _model = model;
            DateString = string.Join(" ", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
            ChangeDescription = @"Ad-hoc constraint";
            ChangeAuthor = _model.SquintUser;
            ConstraintID = C.ID;
            ConstraintString = "";
        }
    }

}

