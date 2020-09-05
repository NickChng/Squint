﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using PropertyChanged;
using System.Data;
using g3;

namespace SquintScript
{

    [AddINotifyPropertyChangedInterface]
    public class Component : INotifyPropertyChanged
    {
        //Required notification class
        public event PropertyChangedEventHandler PropertyChanged;


        public List<Constraint> Constraints = new List<Constraint>();
        public List<ImagingProtocols> ImagingProtocols { get; set; } = new List<ImagingProtocols>();
        public List<Beam> Beams = new List<Beam>();
        //public List<Assessment> RegisteredAssessments = new List<Assessment>();
        public Component(int CompId, int ProtocolId)
        {
            ID = CompId;
            ProtocolID = ProtocolId;
        }
        public Component(DbComponent DbO)
        {
            ID = DbO.ID;
            MinColOffset = new TrackedValue<int?>(DbO.MinColOffset);
            MinBeams = new TrackedValue<int?>(DbO.MinBeams);
            MaxBeams = new TrackedValue<int?>(DbO.MaxBeams);
            NumIso = new TrackedValue<int?>(DbO.NumIso);
            _ComponentName = new TrackedValue<string>(DbO.ComponentName);
            ComponentType = new TrackedValue<ComponentTypes>((ComponentTypes)DbO.ComponentType);
            _NumFractions = new TrackedValue<int>(DbO.NumFractions);
            _ReferenceDose = new TrackedValue<double>(DbO.ReferenceDose);
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
            _ReferenceDose = new TrackedValue<double>(ReferenceDose_in);
            DisplayOrder = DisplayOrder_in;
            ProtocolID = protocolID;
            PNVMin = new TrackedValue<double?>(null);
            PNVMax = new TrackedValue<double?>(null);
            PrescribedPercentage = new TrackedValue<double?>(null);
        }

        //public event EventHandler<int> ComponentDeleted;
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
        //public ComponentTypes ComponentType
        //{
        //    get { return _ComponentType.Value; }
        //    set { _ComponentType.Value = value; }
        //}
        public TrackedValue<int?> MinBeams { get; private set; } = new TrackedValue<int?>(null);
        //public int MinBeams
        //{
        //    get { return _MinBeams.Value; }
        //    set { if { _MinBeams.Value = value; } }
        //}
        public TrackedValue<int?> MaxBeams { get; private set; } = new TrackedValue<int?>(null);
        //public int MaxBeams
        //{
        //    get { return _MaxBeams.Value; }
        //    set { _MaxBeams.Value = value; }
        //}
        public TrackedValue<int?> NumIso { get; private set; } = new TrackedValue<int?>(null);
        //public int NumIso
        //{
        //    get { return _NumIso.Value; }
        //    set { _NumIso.Value = value; }
        //}
        public TrackedValue<int?> MinColOffset { get; private set; } = new TrackedValue<int?>(null);
        //public int MinColOffset
        //{
        //    get { return _MinColOffset.Value; }
        //    set { _MinColOffset.Value = value; }
        //}
        public bool isTDFmodified { get; private set; }
        private TrackedValue<double> _ReferenceDose;
        public double ReferenceDoseOriginal { get { return _ReferenceDose.ReferenceValue; } }
        public double ReferenceDose
        {
            get
            {
                return _ReferenceDose.Value;
            }
            set
            {
                if (Math.Abs(_ReferenceDose.Value - value) > 1E-5 && value > 0)
                {
                    _ReferenceDose.Value = value;
                    ReferenceDoseChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public TrackedValue<double?> PNVMin;

        public TrackedValue<double?> PNVMax;

        public TrackedValue<double?> PrescribedPercentage;

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
                    _NumFractions.Value = value;
                    ReferenceFractionsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public bool ToRetire { get; private set; } = false;
        public void FlagForDeletion()
        {
            ToRetire = true;
            //ComponentDeleted?.Invoke(this, ID);
        }

        //public void RegisterAssessment(Assessment SA)
        //{
        //    RegisteredAssessments.Add(SA);
        //}
    }

}

