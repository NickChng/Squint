using System.ComponentModel;

namespace SquintScript
{
    public class StructureLabel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public StructureLabel(DbStructureLabel DbO)
        {
            ID = DbO.ID;
            StructureType = (StructureTypes)DbO.StructureType;
            Code = DbO.Code;
            LabelName = DbO.StructureLabel;
            AlphaBetaRatio = DbO.AlphaBetaRatio;
            Description = DbO.Description;
            Designator = DbO.Designator;
        }
        public int ID { get; private set; }
        public StructureTypes StructureType { get; private set; }
        public string Code { get; private set; }
        public string LabelName { get; private set; }
        public double AlphaBetaRatio { get; private set; }
        public string Description { get; private set; }
        public string Designator { get; private set; }

    }

}

