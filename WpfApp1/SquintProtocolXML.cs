using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace SquintScript
{
    [XmlRoot("SquintProtocol")]
    public class SquintProtocolXML
    {
        public SquintProtocolXML()
        {
            ProtocolMetaData = new ProtocolMetaDataDefinition();
            Structures = new StructuresDefinition();
            Components = new ComponentsDefinition();
            DVHConstraints = new DVHConstraintListDefinition();
            ConformityIndexConstraints = new ConformityIndexConstraintListDefinition();
        }
        public int ProtocolID;
        public string protocolSource;
        public int version = 1;
        public class ConstraintDefinition
        {
            [XmlIgnore]
            public int ID;
            [XmlAttribute]
            public int DisplayOrder = -1;
            [XmlIgnore]
            public int ComponentID;
            [XmlAttribute]
            public string ComponentName;
            [XmlAttribute]
            public string ProtocolStructureName;
            [XmlAttribute]
            public string Description;
        }
        //XML interface classes
        public class ProtocolMetaDataDefinition
        {
            [XmlIgnore]
            public int ID;
            [XmlAttribute]
            public string ProtocolName;
            [XmlAttribute]
            public int NumComponents;
            [XmlAttribute]
            public string ProtocolDate;
            [XmlAttribute]
            public string Author;
            [XmlAttribute]
            public string ApprovalStatus;
            [XmlAttribute]
            public string DiseaseSite;
            [XmlAttribute]
            public string TreatmentCentre;
            [XmlAttribute]
            public string ProtocolType;
            [XmlAttribute]
            public string Intent;
        }
        public class ImagingProtocolDefinition
        {
            [XmlAttribute]
            public string Id { get; set; }
        }
        public class ImagingProtocolsDefinition
        {
            public ImagingProtocolsDefinition()
            {
                ImagingProtocol = new List<ImagingProtocolDefinition>();
            }
            [XmlElement("ImagingProtocol")]
            public List<ImagingProtocolDefinition> ImagingProtocol { get; set; }
        }
        public class SupportsDefinition
        {
            [XmlAttribute]
            public double CouchSurface { get; set; } = double.NaN;
            [XmlAttribute]
            public double CouchInterior { get; set; } = double.NaN;
            [XmlAttribute]
            public string Indication { get; set; } = "Optional";
        }
        public class BolusDefinition
        {
            [XmlAttribute]
            public double HU { get; set; } = double.NaN;
            [XmlAttribute]
            public double Thickness { get; set; } = double.NaN;
            [XmlAttribute]
            public string Indication { get; set; } = "Optional";
        }
        public class ArtifactsDefinition
        {
            [XmlElement("Artifact")]
            public List<ArtifactDefintiion> Artifact { get; set; } = new List<ArtifactDefintiion>();
        }
        public class ArtifactDefintiion
        {
            [XmlAttribute]
            public double HU { get; set; }
            [XmlAttribute]
            public string ProtocolStructureName { get; set; }
        }
        public class CalculationDefinition
        {
            [XmlAttribute]
            public string Algorithm;
            [XmlAttribute]
            public double AlgorithmResolution;
            [XmlAttribute]
            public string FieldNormalizationMode;
            [XmlAttribute]
            public bool HeterogeneityOn;
        }
        public class SimulationDefinition
        {
            [XmlAttribute]
            public double SliceSpacing;
        }
        public class PrescriptionDefinition
        {
            [XmlAttribute]
            public double PNVMin;
            [XmlAttribute]
            public double PNVMax;
            [XmlAttribute]
            public double PrescribedPercentage;
        }
        public class ComponentDefaultsDefinition
        {
            [XmlElement("Supports")]
            public SupportsDefinition Supports { get; set; } = new SupportsDefinition();
            [XmlElement("BolusClinical")]
            public BolusDefinition Bolus { get; set; } = new BolusDefinition();
            [XmlElement("Artifacts")]
            public ArtifactsDefinition Artifacts { get; set; } = new ArtifactsDefinition();
            [XmlElement("Simulation")]
            public SimulationDefinition Simulation { get; set; } = new SimulationDefinition();
            [XmlElement("Calculation")]
            public CalculationDefinition Calculation { get; set; } = new CalculationDefinition();
            [XmlElement("Prescription")]
            public PrescriptionDefinition Prescription { get; set; } = new PrescriptionDefinition();

        }
        public class StructureDefinition
        {
            [XmlIgnore]
            public int ID;
            [XmlAttribute]
            public string Label;
            [XmlAttribute]
            public string ProtocolStructureName;
            [XmlElement("EclipseAliases")]
            public EclipseAliases EclipseAliases { get; set; }
            [XmlElement("StructureChecklist")]
            public StructureChecklistDefinition StructureChecklist { get; set; }
        }
        public class StructureChecklistDefinition
        {
            [XmlElement("PointContourCheck")]
            public PointContourCheckDefinition PointContourCheck { get; set; }
        }
        public class PointContourCheckDefinition
        {
            [XmlAttribute]
            public double Threshold { get; set; } = Double.NaN;
        }
        public class EclipseAliases
        {
            public EclipseAliases()
            {
                EclipseId = new List<EclipseAlias>();
            }
            [XmlElement("EclipseId")]
            public List<EclipseAlias> EclipseId { get; set; }
        }

        public class EclipseAlias
        {
            [XmlAttribute]
            public string Id { get; set; }
        }
        public class StructuresDefinition
        {
            public StructuresDefinition()
            {
                Structure = new List<StructureDefinition>();
            }
            [XmlElement("Structure")]
            public List<StructureDefinition> Structure { get; set; }
        }
        public class ConformityIndexConstraintDefinition : ConstraintDefinition
        {
            //Private backers
            [XmlAttribute]
            public string ConstraintName;
            [XmlAttribute]
            public string PrimaryStructureName;
            [XmlAttribute]
            public string DoseUnit;
            [XmlAttribute]
            public string ReferenceStructureName;
            [XmlAttribute]
            public string ConstraintType;
            [XmlAttribute]
            public string ConstraintUnit;
            [XmlAttribute]
            public double DoseVal;
            [XmlAttribute]
            public string MajorViolation;
            [XmlAttribute]
            public string MinorViolation;
            [XmlAttribute]
            public string Stop;
            [XmlAttribute]
            public double ConstraintVal;
        }
        public class DVHConstraintListDefinition
        {
            public DVHConstraintListDefinition()
            {
                DVHConstraintList = new List<DVHConstraintDefinition>();
            }
            [XmlElement("DVHConstraint")]
            public List<DVHConstraintDefinition> DVHConstraintList { get; set; }
        }
        public class ConformityIndexConstraintListDefinition
        {
            public ConformityIndexConstraintListDefinition()
            {
                ConformityIndexConstraintList = new List<ConformityIndexConstraintDefinition>();
            }
            [XmlElement("ConformityIndexConstraint")]
            public List<ConformityIndexConstraintDefinition> ConformityIndexConstraintList { get; set; }
        }
        public class DVHConstraintDefinition : ConstraintDefinition
        {
            [XmlAttribute]
            public string EclipseID;
            [XmlAttribute]
            public string DvhType;
            [XmlAttribute]
            public double DvhVal;
            [XmlAttribute]
            public string DvhUnit;
            [XmlAttribute]
            public string ConstraintType;
            [XmlAttribute]
            public string MajorViolation;
            [XmlAttribute]
            public string MinorViolation;
            [XmlAttribute]
            public string Stop;
            [XmlAttribute]
            public double ConstraintVal;
            [XmlAttribute]
            public string ConstraintUnit;
        }

        public class ComponentsDefinition
        {
            public ComponentsDefinition()
            {
                Component = new List<ComponentDefinition>();
            }
            [XmlElement("Component")]
            public List<ComponentDefinition> Component { get; set; }
        }
        public class ComponentDefinition
        {
            //public event EventHandler ComponentBeingDisposed;
            public event EventHandler<ComponentChangedEventArgs> ComponentChanged;
            public class ComponentChangedEventArgs : EventArgs
            {
                public int ComponentID;
                public string PropertyName;
            }
            public ComponentDefinition()
            {
                ImagingProtocols = new ImagingProtocolsDefinition();
            }


            [XmlIgnore]
            public int ID;
            [XmlAttribute]
            public string ComponentName;
            [XmlAttribute]
            public string Type;
            [XmlAttribute]
            public int NumFractions;
            [XmlAttribute]
            public int ReferenceDose;

            public class ComponentDVHConstraintsDefinition
            {
                public ComponentDVHConstraintsDefinition()
                {
                    DVHConstraint = new List<DVHConstraintDefinition>();
                }
                [XmlElement("DVHConstraint")]
                public List<DVHConstraintDefinition> DVHConstraint { get; set; }
            }
            public class ComponentConformityIndexConstraintsDefinition
            {
                public ComponentConformityIndexConstraintsDefinition()
                {
                    ConformityIndexConstraint = new List<ConformityIndexConstraintDefinition>();
                }
                [XmlElement("ConformityIndexConstraint")]
                public List<ConformityIndexConstraintDefinition> ConformityIndexConstraint;
            }
            [XmlElement("DVHConstraints")]
            public ComponentDVHConstraintsDefinition DVHConstraints { get; set; }
            [XmlElement("ConformityIndexConstraints")]
            public ComponentConformityIndexConstraintsDefinition ConformityIndexConstraints { get; set; }
            [XmlElement("ImagingProtocols")]
            public ImagingProtocolsDefinition ImagingProtocols { get; set; }

        }
        public class ConException
        {
            //Properties
            [XmlAttribute]
            public string Property { get; set; }
            [XmlAttribute]
            public double PropertyValue { get; set; }
            [XmlAttribute]
            public string PropertyString { get; set; }
            [XmlAttribute]
            public string Description { get; set; }
        }

        [XmlElement("ProtocolMetaData")]
        public ProtocolMetaDataDefinition ProtocolMetaData { get; set; }
        [XmlElement("ImagingProtocols")]
        public ImagingProtocolsDefinition ImagingProtocols { get; set; }
        [XmlElement("Structures")]
        public StructuresDefinition Structures { get; set; }
        [XmlElement("ComponentDefaults")]
        public ComponentDefaultsDefinition ComponentDefaults { get; set; }
        [XmlElement("Components")]
        public ComponentsDefinition Components { get; set; }
        [XmlElement("DVHConstraints")]
        public DVHConstraintListDefinition DVHConstraints { get; set; }
        [XmlElement("ConformityIndexConstraints")]
        public ConformityIndexConstraintListDefinition ConformityIndexConstraints { get; set; }
    }
}
