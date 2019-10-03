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
            public string Description = "(No description in initial protocol)";
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
        public class BeamsDefinition
        {
            public BeamsDefinition()
            {
                Beam = new List<BeamDefinition>();
            }
            [XmlElement("Beam")]
            public List<BeamDefinition> Beam { get; set; }
            
            [XmlAttribute]
            public int MinBeams { get; set; } = -1;
            [XmlAttribute]
            public int MaxBeams { get; set; } = -1;
            [XmlAttribute]
            public int NumIso { get; set; } = 1;
            [XmlAttribute]
            public int MinColOffset { get; set; } = 20;
        }
        public class EnergiesDefintion
        {
            public EnergiesDefintion()
            {
                Energy = new List<EnergyDefinition>();
            }
            [XmlElement("Energy")]
            public List<EnergyDefinition> Energy { get; set; }
        }
        public class EnergyDefinition
        {
            [XmlAttribute] 
            public string Mode { get; set; }
        }
        
        public class ValidGeometriesDefintiion
        {
            [XmlElement("Geometry")]
            public List<GeometryDefinition> Geometry { get; set; } = new List<GeometryDefinition>();
        }
        public class GeometryDefinition
        {
            [XmlAttribute]
            public string GeometryName { get; set; } = "unset";
            [XmlAttribute]
            public string Trajectory { get; set; } = "unset";
            [XmlAttribute]
            public double MinStartAngle { get; set; } = -1;
            [XmlAttribute]
            public double MinEndAngle { get; set; } = -1;
            [XmlAttribute] 
            public double MaxStartAngle { get; set; } = -1;

            [XmlAttribute] 
            public double MaxEndAngle { get; set; } = -1;
        }
        public class BeamDefinition
        {
            [XmlAttribute]
            public string ProtocolBeamName { get; set; } = "unset";
            [XmlAttribute]
            public string Technique { get; set; } = "unset";
            [XmlAttribute]
            public string Energy { get; set; } = "6X";
            [XmlAttribute]
            public string ToleranceTable { get; set; } = "unset";
          
            [XmlAttribute]
            public double MinMUWarning { get; set; } = -1;
            [XmlAttribute]
            public double MaxMUWarning { get; set; } = -1;
            [XmlAttribute]
            public double MinColRotation { get; set; } = -1;
            [XmlAttribute]
            public double MaxColRotation { get; set; } = -1;
            [XmlAttribute]
            public double CouchRotation { get; set; } = -1;
            [XmlAttribute]
            public double MinX { get; set; } = 3;
            [XmlAttribute]
            public double MaxX { get; set; } = 3;
            [XmlAttribute]
            public double MinY { get; set; } = -1;
            [XmlAttribute]
            public double MaxY { get; set; } = -1;
            [XmlAttribute]
            public string BolusIndication { get; set; }
            [XmlAttribute]
            public double RefBolusHU { get; set; }
            [XmlAttribute]
            public double BolusClinicalMinThickness { get; set; }
            [XmlAttribute]
            public double BolusClinicalMaxThickness { get; set; }
            [XmlAttribute]
            public string VMAT_JawTracking { get; set; }
            [XmlElement("ValidEnergies")]
            public EnergiesDefintion Energies { get; set; } = new EnergiesDefintion();
            [XmlElement("Bolus")]
            public BolusDefinition Bolus { get; set; } = new BolusDefinition();
            
            [XmlElement("EclipseAliases")]
            public EclipseAliases EclipseAliases { get; set; }
            [XmlElement("ValidGeometries")]
            public ValidGeometriesDefintiion ValidGeometries { get; set; }


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
            public double MinThickness { get; set; } = double.NaN;
            [XmlAttribute]
            public double MaxThickness { get; set; } = double.NaN;
            [XmlAttribute]
            public string Indication { get; set; } = "Optional";
        }
        public class ArtifactsDefinition
        {
            [XmlElement("Artifact")]
            public List<ArtifactDefinition> Artifact { get; set; } = new List<ArtifactDefinition>();
        }
        public class ArtifactDefinition
        {
            [XmlAttribute]
            public double HU { get; set; }
            [XmlAttribute]
            public double ToleranceHU { get; set; }
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
            public string GetConstraintString()
            {
                return string.Join(": ", PrimaryStructureName, GetConstraintStringOnly());
            }
            public string GetConstraintStringOnly()
            {
                string ReferenceType;
                if (ConstraintType.ToUpper() == "LOWER")
                    ReferenceType = ReferenceTypes.Lower.Display();
                else
                    ReferenceType = ReferenceTypes.Upper.Display();
                string DoseUnitString;
                if (DoseUnit.ToUpper() == "RELATIVE")
                    DoseUnitString = ConstraintUnits.Percent.Display();
                else
                    DoseUnitString = ConstraintUnits.cGy.Display();
                if (ConstraintVal == 0) // Display exception if it's the whole volume for readability
                    return string.Format("Total volume is {0} {1} % of {2} volume", ReferenceType, ConstraintVal, ReferenceStructureName);
                else
                {
                    if (ConstraintUnit.ToUpper() == "PERCENT")
                        return string.Format("V{0}[{1}] {2} {3}{4} of the {5} volume", DoseVal, DoseUnitString, ReferenceType, ConstraintVal, ConstraintUnits.Percent.Display(), ReferenceStructureName);
                    else if (ConstraintUnit.ToUpper() == "MULTIPLE")
                        return string.Format("V{0}[{1}] {2} {3} {4} the {5} volume", DoseVal, DoseUnitString, ReferenceType, ConstraintVal, ConstraintUnits.Multiple.Display(), ReferenceStructureName);
                    else
                        throw new Exception("Error in formatting constraint string");
                }
            }
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
            //[XmlAttribute]
            //public string EclipseID;
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
            public string GetConstraintString()
            {
                return string.Join(": ", ProtocolStructureName, GetConstraintStringOnly());
            }
            public string GetConstraintStringOnly()
            {
                string ReferenceType;
                if (ConstraintType.ToUpper() == "LOWER")
                    ReferenceType = ReferenceTypes.Lower.Display();
                else
                    ReferenceType = ReferenceTypes.Upper.Display();
                string DvhUnitString;
                string ConstraintUnitString;
                switch (DvhType.ToUpper())
                {
                    case "CV":
                        if (DvhUnit.ToUpper() == "RELATIVE")
                            DvhUnitString = ConstraintUnits.Percent.Display();
                        else
                            DvhUnitString = ConstraintUnits.cGy.Display();
                        if (ConstraintUnit.ToUpper() == "RELATIVE")
                            ConstraintUnitString = ConstraintUnits.Percent.Display();
                        else
                            ConstraintUnitString = ConstraintUnits.cc.Display();
                        return string.Format("CV{0:0.###} [{1}] {2} {3:0.###} [{4}]", DvhVal, DvhUnitString, ReferenceType, ConstraintVal, ConstraintUnitString);
                    case "V":
                        if (DvhUnit.ToUpper() == "RELATIVE")
                            DvhUnitString = ConstraintUnits.Percent.Display();
                        else
                            DvhUnitString = ConstraintUnits.cGy.Display();
                        if (ConstraintUnit.ToUpper() == "RELATIVE")
                            ConstraintUnitString = ConstraintUnits.Percent.Display();
                        else
                            ConstraintUnitString = ConstraintUnits.cc.Display();
                        return string.Format("V{0:0.###} [{1}] {2} {3:0.###} [{4}]", DvhVal, DvhUnitString, ReferenceType, ConstraintVal, ConstraintUnitString);
                    case "D":                        //Exception for min and max dose
                        if (DvhVal < 1E-5)
                        { // max dose
                            if (ConstraintUnit.ToUpper() == "RELATIVE")
                                return string.Format("Max Dose {0} {1:0.###} [{2}]", ReferenceType, ConstraintVal, ConstraintUnits.Percent.Display());
                            else
                                return string.Format("Max Dose {0} {1:0} [{2}]", ReferenceType, ConstraintVal, ConstraintUnits.cc.Display());
                        }
                        else if (Math.Abs(DvhVal - 100) < 1E-5 && ConstraintUnit.ToUpper() == "RELATIVE")
                        {
                            if (ConstraintUnit.ToUpper() == "RELATIVE")
                                return string.Format("Min Dose {0} {1:0.###} [{2}]", ReferenceType, ConstraintVal, ConstraintUnits.Percent.Display());
                            else
                                return string.Format("Min Dose {0} {1:0} [{2}]", ReferenceType, ConstraintVal, ConstraintUnits.cGy.Display());
                        }
                        else
                        {
                            if (DvhUnit.ToUpper() == "RELATIVE")
                                DvhUnitString = ConstraintUnits.Percent.Display();
                            else
                                DvhUnitString = ConstraintUnits.cc.Display();
                            if (ConstraintUnit.ToUpper() == "RELATIVE")
                                ConstraintUnitString = ConstraintUnits.Percent.Display();
                            else
                                ConstraintUnitString = ConstraintUnits.cGy.Display();
                            if (ConstraintUnit.ToUpper() == "RELATIVE")
                                return string.Format("D{0:0.###} [{1}] {2} {3:0.#} [{4}]", DvhVal, DvhUnitString, ReferenceType, ConstraintVal, ConstraintUnitString);
                            else
                                return string.Format("D{0:0.###} [{1}] {2} {3:0} [{4}]", DvhVal, DvhUnitString, ReferenceType, ConstraintVal, ConstraintUnitString);
                        }
                    case "M":
                        return string.Format("Mean Dose {0} {1:0} [{2}]", ReferenceType, ConstraintVal, ConstraintUnits.cGy.Display());
                    default:
                        throw new Exception("Error in formatting constraint string");
                }
            }
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
                Beams = new BeamsDefinition();
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
            [XmlElement("Beams")]
            public BeamsDefinition Beams { get; set; }

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
