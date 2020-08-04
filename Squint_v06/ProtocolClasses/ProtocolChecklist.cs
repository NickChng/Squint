﻿using System.Collections.Generic;
using PropertyChanged;

namespace SquintScript
{
    public static partial class Ctr
    {
        [AddINotifyPropertyChangedInterface]
        public class ProtocolChecklist
        {
            public ProtocolChecklist(DbProtocolChecklist DbO)
            {
                ID = DbO.ID;
                ProtocolId = DbO.ProtocolID;
                //TreatmentTechniqueType = (TreatmentTechniques)DbO.TreatmentTechniqueType;
                //MinFields = DbO.MinFields;
                //MaxFields = DbO.MaxFields;
                //VMAT_MinFieldColSeparation = DbO.VMAT_MinFieldColSeparation;
                //NumIso = DbO.NumIso;
                //MinXJaw = DbO.MinXJaw;
                //MaxXJaw = DbO.MaxXJaw;
                //MinYJaw = DbO.MinYJaw;
                //MaxYJaw = DbO.MaxYJaw;
                //VMAT_JawTracking = (ParameterOptions)DbO.VMAT_JawTracking;
                Algorithm = new TrackedValue<AlgorithmTypes>((AlgorithmTypes)DbO.Algorithm);
                FieldNormalizationMode = new TrackedValue<FieldNormalizationTypes>((FieldNormalizationTypes)DbO.FieldNormalizationMode);
                AlgorithmResolution = new TrackedValue<double?>(DbO.AlgorithmResolution);
                SliceSpacing = new TrackedValue<double?>(DbO.SliceSpacing);
                HeterogeneityOn = new TrackedValue<bool?>(DbO.HeterogeneityOn);
                //Couch
                SupportIndication = new TrackedValue<ParameterOptions>((ParameterOptions)DbO.SupportIndication);
                CouchSurface = new TrackedValue<double?>(DbO.CouchSurface);
                CouchInterior = new TrackedValue<double?>(DbO.CouchInterior);
                //Artifact
                foreach (DbArtifact DbA in DbO.Artifacts)
                {
                    Artifact A = new Artifact(DbA);
                    Artifacts.Add(A);
                }
                foreach (DbBolus DbB in DbO.Boluses)
                {
                    BolusDefinition B = new BolusDefinition(DbB);
                    Boluses.Add(B);
                }

            }
            public ProtocolChecklist(int protocolId)
            {
                ID = IDGenerator();
                ProtocolId = protocolId;
            }
            public int ID { get; set; }
            public int ProtocolId { get; set; }
            //public TreatmentTechniques TreatmentTechniqueType { get; set; }
            //public double MinFields { get; set; }
            //public double MaxFields { get; set; }
            //public double MinMU { get; set; }
            //public double MaxMU { get; set; }
            //public double VMAT_MinColAngle { get; set; }
            //public double VMAT_MinFieldColSeparation { get; set; }
            //public int NumIso { get; set; }
            //public bool VMAT_SameStartStop { get; set; }
            //public double MinXJaw { get; set; }
            //public double MaxXJaw { get; set; }
            //public double MinYJaw { get; set; }
            //public double MaxYJaw { get; set; }
            public TrackedValue<ParameterOptions> VMAT_JawTracking { get; set; } = new TrackedValue<ParameterOptions>(ParameterOptions.Unset);
            public TrackedValue<AlgorithmTypes> Algorithm { get; set; } = new TrackedValue<AlgorithmTypes>(AlgorithmTypes.Unset);
            public TrackedValue<FieldNormalizationTypes> FieldNormalizationMode { get; set; } = new TrackedValue<FieldNormalizationTypes>(FieldNormalizationTypes.Unset);
            public TrackedValue<double?> AlgorithmResolution { get; set; } = new TrackedValue<double?>(null);
            public TrackedValue<double?> SliceSpacing { get; set; } = new TrackedValue<double?>(null);
            public TrackedValue<bool?> HeterogeneityOn { get; set; } = new TrackedValue<bool?>(null);
            public TrackedValue<ParameterOptions> SupportIndication { get; set; } = new TrackedValue<ParameterOptions>(ParameterOptions.Unset);
            public TrackedValue<double?> CouchSurface { get; set; } = new TrackedValue<double?>(null);
            public TrackedValue<double?> CouchInterior { get; set; } = new TrackedValue<double?>(null);
            public List<Artifact> Artifacts { get; set; } = new List<Artifact>();
            public List<BolusDefinition> Boluses { get; set; } = new List<BolusDefinition>();
        }

    }
}

