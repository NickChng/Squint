using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using PropertyChanged;
using System.Data;
using g3;

namespace Squint
{

    [AddINotifyPropertyChangedInterface]
    public class Component : INotifyPropertyChanged
    {
        //Required notification class
        public event PropertyChangedEventHandler PropertyChanged;
        public List<Constraint> Constraints = new List<Constraint>();
        public List<ImagingProtocolTypes> ImagingProtocols { get; set; } = new List<ImagingProtocolTypes>();
        public List<Beam> Beams = new List<Beam>();
        public Component(int CompId, int ProtocolId)
        {
            ID = CompId;
            ProtocolID = ProtocolId;
        }
        public Component(DbComponent DbO)
        {
            ID = DbO.ID;
            MinColOffset = new TrackedValue<double?>(DbO.MinColOffset);
            MinBeams = new TrackedValue<int?>(DbO.MinBeams);
            MaxBeams = new TrackedValue<int?>(DbO.MaxBeams);
            NumIso = new TrackedValue<int?>(DbO.NumIso);
            _ComponentName = new TrackedValue<string>(DbO.ComponentName);
            ComponentType = new TrackedValue<ComponentTypes>((ComponentTypes)DbO.ComponentType);
            _NumFractions = new TrackedValue<int>(DbO.NumFractions);
            _TotalDose = new TrackedValue<double>(DbO.ReferenceDose);
            PNVMin = new TrackedValue<double?>(DbO.PNVMin);
            PNVMax = new TrackedValue<double?>(DbO.PNVMax);
            PrescribedPercentage = new TrackedValue<double?>(DbO.PrescribedPercentage);
            DisplayOrder = DbO.DisplayOrder;
            ProtocolID = DbO.ProtocolID;
        }
        public Component(int protocolID, string ComponentName_in, int DisplayOrder_in, int ReferenceFractions_in = 0, double ReferenceDose_in = 0, ComponentTypes ComponentType_in = ComponentTypes.Phase)
        {
            ID = IDGenerator.GetUniqueId();
            _ComponentName = new TrackedValue<string>(ComponentName_in);
            ComponentType = new TrackedValue<ComponentTypes>(ComponentType_in);
            _NumFractions = new TrackedValue<int>(ReferenceFractions_in);
            _TotalDose = new TrackedValue<double>(ReferenceDose_in);
            DisplayOrder = DisplayOrder_in;
            ProtocolID = protocolID;
            PNVMin = new TrackedValue<double?>(null);
            PNVMax = new TrackedValue<double?>(null);
            PrescribedPercentage = new TrackedValue<double?>(null);
        }

        public event EventHandler ReferenceDoseChanged;
        public event EventHandler ReferenceFractionsChanged;
        public class ComponentArgs : EventArgs
        {
            public object Value;
        }
        public int ID { get; private set; }
        public int DisplayOrder { get; set; }
        public int ProtocolID { get; private set; }

        private TrackedValue<string> _ComponentName;
        public string ComponentName
        {
            get { return _ComponentName.Value; }
            set { if (value != null) { _ComponentName.Value = value; } }
        }
        public TrackedValue<ComponentTypes> ComponentType { get; private set; }

        private TrackedValue<double> _TotalDose;
        public double TotalDoseReference { get { return _TotalDose.ReferenceValue; } }
        public double TotalDose
        {
            get
            {
                return _TotalDose.Value;
            }
            set
            {
                if (Math.Abs(_TotalDose.Value - value) > 1E-5 && value > 0)
                {
                    double oldDose = _TotalDose.Value;
                    _TotalDose.Value = value;
                    ReferenceDoseChanged?.Invoke(oldDose, EventArgs.Empty);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalDose))); // unclear why this seems to be needed for this property in order to fire.  this fires automatically for updates to NumFractions
                }
            }
        }
        private TrackedValue<int> _NumFractions;
        public int NumFractions
        {
            get
            {
                return _NumFractions.Value;
            }
            set
            {
                if (value != NumFractions && value > 0)
                {
                    int prevFractions = _NumFractions.Value;
                    _NumFractions.Value = value;
                    ReferenceFractionsChanged?.Invoke(prevFractions, EventArgs.Empty);
                }
            }
        }
        public bool ToRetire { get; private set; } = false;

        // Checklist properties
        public TrackedValue<int?> MinBeams { get; private set; } = new TrackedValue<int?>(null);
        public TrackedValue<int?> MaxBeams { get; private set; } = new TrackedValue<int?>(null);
        public TrackedValue<int?> NumIso { get; private set; } = new TrackedValue<int?>(null);
        public TrackedValue<double?> MinColOffset { get; private set; } = new TrackedValue<double?>(null);
        public TrackedValue<double?> PNVMin;
        public TrackedValue<double?> PNVMax;
        public TrackedValue<double?> PrescribedPercentage;


        public void FlagForDeletion()
        {
            ToRetire = true;
        }

    }

}

