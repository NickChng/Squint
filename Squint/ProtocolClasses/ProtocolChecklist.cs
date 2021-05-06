using System.Collections.Generic;
using System.Collections.ObjectModel;
using PropertyChanged;

namespace SquintScript
{
    [AddINotifyPropertyChangedInterface]
    public class ProtocolChecklist
    {
        public ProtocolChecklist(DbProtocolChecklist DbO)
        {
            ID = DbO.ID;
            ProtocolId = DbO.ProtocolID;
            Algorithm = new TrackedValue<AlgorithmVolumeDoseTypes>((AlgorithmVolumeDoseTypes)DbO.AlgorithmVolumeDose);
            AlgorithmVMATOptimization = new TrackedValue<AlgorithmVMATOptimizationTypes>((AlgorithmVMATOptimizationTypes)DbO.AlgorithmVMATOptimization);
            AlgorithmIMRTOptimization = new TrackedValue<AlgorithmIMRTOptimizationTypes>((AlgorithmIMRTOptimizationTypes)DbO.AlgorithmIMRTOptimization);
            AirCavityCorrectionVMAT = new TrackedValue<string>(DbO.AirCavityCorrectionVMAT);
            AirCavityCorrectionIMRT = new TrackedValue<string>(DbO.AirCavityCorrectionIMRT);
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
            //CTDevices
            foreach (DbCTDeviceId CT in DbO.CTDeviceIds)
            {
                CTDeviceIds.Add(CT.CTDeviceId);
            }

        }
        public ProtocolChecklist(int protocolId)
        {
            ID = IDGenerator.GetUniqueId();
            ProtocolId = protocolId;
        }
        public int ID { get; set; }
        public int ProtocolId { get; set; }
      
        public TrackedValue<ParameterOptions> VMAT_JawTracking { get; set; } = new TrackedValue<ParameterOptions>(ParameterOptions.Unset);
        public TrackedValue<AlgorithmVolumeDoseTypes> Algorithm { get; set; } = new TrackedValue<AlgorithmVolumeDoseTypes>(AlgorithmVolumeDoseTypes.Unset);
        public TrackedValue<AlgorithmVMATOptimizationTypes> AlgorithmVMATOptimization { get; set; } = new TrackedValue<AlgorithmVMATOptimizationTypes>(AlgorithmVMATOptimizationTypes.Unset);
        public TrackedValue<AlgorithmIMRTOptimizationTypes> AlgorithmIMRTOptimization { get; set; } = new TrackedValue<AlgorithmIMRTOptimizationTypes>(AlgorithmIMRTOptimizationTypes.Unset);
        public TrackedValue<FieldNormalizationTypes> FieldNormalizationMode { get; set; } = new TrackedValue<FieldNormalizationTypes>(FieldNormalizationTypes.Unset);
        public TrackedValue<string> AirCavityCorrectionVMAT { get; set; } = new TrackedValue<string>(null);
        public TrackedValue<string> AirCavityCorrectionIMRT { get; set; } = new TrackedValue<string>(null);
        public TrackedValue<double?> AlgorithmResolution { get; set; } = new TrackedValue<double?>(null);
        public TrackedValue<double?> SliceSpacing { get; set; } = new TrackedValue<double?>(null);
        public TrackedValue<bool?> HeterogeneityOn { get; set; } = new TrackedValue<bool?>(null);
        public TrackedValue<ParameterOptions> SupportIndication { get; set; } = new TrackedValue<ParameterOptions>(ParameterOptions.Unset);
        public TrackedValue<double?> CouchSurface { get; set; } = new TrackedValue<double?>(null);
        public TrackedValue<double?> CouchInterior { get; set; } = new TrackedValue<double?>(null);
        public List<Artifact> Artifacts { get; set; } = new List<Artifact>();

        public ObservableCollection<string> CTDeviceIds { get; set; } = new ObservableCollection<string>();
        public List<BolusDefinition> Boluses { get; set; } = new List<BolusDefinition>();
    }

}

