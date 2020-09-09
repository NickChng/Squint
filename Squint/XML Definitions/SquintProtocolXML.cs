using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using VMS.TPS.Common.Model.API;
using SquintScript.Extensions;

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
            Constraints = new ConstraintListDefinition();
        }
        public int ProtocolID;
        public string protocolSource;
        public int version = 1;
        public class BaseConstraintDefinition
        {
            [XmlIgnore]
            public int ID;
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
            public string MinBeams { get; set; }
            [XmlAttribute]
            public string MaxBeams { get; set; }
            [XmlAttribute]
            public string NumIso { get; set; }
            [XmlAttribute]
            public string MinColOffset { get; set; }
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
            public double StartAngle { get; set; } = double.NaN;
            [XmlAttribute]
            public double EndAngle { get; set; } = double.NaN;
            [XmlAttribute]
            public double StartAngleTolerance { get; set; } = double.NaN;

            [XmlAttribute]
            public double EndAngleTolerance { get; set; } = double.NaN;
        }
        public class BeamDefinition
        {
            [XmlAttribute]
            public string ProtocolBeamName { get; set; } = "unset";
            [XmlAttribute]
            public string Technique { get; set; } = "unset";

            [XmlAttribute]
            public string ToleranceTable { get; set; } = "unset";

            [XmlAttribute]
            public string MinMUWarning { get; set; }
            [XmlAttribute]
            public string MaxMUWarning { get; set; }
            [XmlAttribute]
            public string MinColRotation { get; set; }
            [XmlAttribute]
            public string MaxColRotation { get; set; }
            [XmlAttribute]
            public string CouchRotation { get; set; }
            [XmlAttribute]
            public string MinX { get; set; }
            [XmlAttribute]
            public string MaxX { get; set; }
            [XmlAttribute]
            public string MinY { get; set; }
            [XmlAttribute]
            public string MaxY { get; set; }

            [XmlAttribute]
            public string JawTracking_Indication { get; set; } = "Optional";

            [XmlElement("ValidEnergies")]
            public EnergiesDefintion Energies { get; set; } = new EnergiesDefintion();

            [XmlElement("EclipseAliases")]
            public EclipseAliases EclipseAliases { get; set; }
            [XmlElement("ValidGeometries")]
            public ValidGeometriesDefintiion ValidGeometries { get; set; }

            private BolusDefinition[] _BolusDefinitions;
            [System.Xml.Serialization.XmlArrayItemAttribute("Bolus", IsNullable = false)]
            public BolusDefinition[] BolusDefinitions { get { return _BolusDefinitions; } set { _BolusDefinitions = value; } }

        }
        public class SupportsDefinition
        {
            [XmlAttribute]
            public string CouchSurface { get; set; }
            [XmlAttribute]
            public string CouchInterior { get; set; }
            [XmlAttribute]
            public string Indication { get; set; } = "Optional";
        }

        public class BolusDefinition
        {

            [XmlAttribute]
            public string HU { get; set; }
            [XmlAttribute]
            public string Thickness { get; set; }
            [XmlAttribute]
            public string ToleranceHU { get; set; }
            [XmlAttribute]
            public string ToleranceThickness { get; set; }
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
            public string HU { get; set; }
            [XmlAttribute]
            public string ToleranceHU { get; set; }
            [XmlAttribute]
            public string ProtocolStructureName { get; set; }
        }
        public class CalculationDefinition
        {
            [XmlAttribute]
            public string Algorithm;
            [XmlAttribute]
            public string VMATOptimizationAlgorithm;
            [XmlAttribute]
            public string IMRTOptimizationAlgorithm;
            [XmlAttribute]
            public string VMATAirCavityCorrection;
            [XmlAttribute]
            public string IMRTAirCavityCorrection;
            [XmlAttribute]
            public string AlgorithmResolution;
            [XmlAttribute]
            public string FieldNormalizationMode;
            [XmlAttribute]
            public string HeterogeneityOn;
        }
        public class SimulationDefinition
        {
            [XmlAttribute]
            public string SliceSpacing;
        }
        public class PrescriptionDefinition
        {
            [XmlAttribute]
            public string NumFractions;
            [XmlAttribute]
            public string ReferenceDose;
            [XmlAttribute]
            public string PNVMin;
            [XmlAttribute]
            public string PNVMax;
            [XmlAttribute]
            public string PrescribedPercentage;
        }
        public class ProtocolChecklistDefinition
        {

            [XmlElement("Supports")]
            public SupportsDefinition Supports { get; set; } = new SupportsDefinition();

            [XmlElement("Artifacts")]
            public ArtifactsDefinition Artifacts { get; set; } = new ArtifactsDefinition();
            [XmlElement("Simulation")]
            public SimulationDefinition Simulation { get; set; } = new SimulationDefinition();
            [XmlElement("Calculation")]
            public CalculationDefinition Calculation { get; set; } = new CalculationDefinition();
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
            public EclipseAliases EclipseAliases { get; set; } = new EclipseAliases();
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
            public string Threshold { get; set; }
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
        
        public class ConstraintListDefinition
        {
            public ConstraintListDefinition()
            {
                ConstraintList = new List<ConstraintDefinition>();
            }
            [XmlElement("Constraint")]
            public List<ConstraintDefinition> ConstraintList { get; set; }
        }
        public class ConstraintDefinition : BaseConstraintDefinition
        {
            [XmlAttribute]
            public string ReferenceType;
            [XmlAttribute]
            public string ConstraintValue;
            [XmlAttribute]
            public string ConstraintUnit;
            [XmlAttribute]
            public string ConstraintType;
            [XmlAttribute]
            public string MajorViolation;
            [XmlAttribute]
            public string MinorViolation;
            [XmlAttribute]
            public string Stop;
            [XmlAttribute]
            public string ReferenceUnit;
            [XmlAttribute]
            public string DataTablePath;
            [XmlAttribute]
            public string ThresholdCalculationType;
            [XmlAttribute]
            public string ReferenceStructureName;
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
                double constraintValue = double.Parse(ConstraintValue);
                string referenceVals = string.Format("[Major] {0}, [Minor] {1}, [Stop] {2}", MajorViolation, MinorViolation, Stop);
                switch (ConstraintType.ToUpper())
                {
                    case "CV":
                        if (ConstraintUnit.ToUpper() == "RELATIVE")
                            DvhUnitString = ConstraintUnits.Percent.Display();
                        else
                            DvhUnitString = ConstraintUnits.cGy.Display();
                        if (ReferenceUnit.ToUpper() == "RELATIVE")
                            ConstraintUnitString = ConstraintUnits.Percent.Display();
                        else
                            ConstraintUnitString = ConstraintUnits.cc.Display();
                        return string.Format("CV{0:0.###} [{1}] {2} {3:0.###} [{4}]", ConstraintValue, DvhUnitString, ReferenceType, referenceVals, ConstraintUnitString);
                    case "V":
                        if (ConstraintUnit.ToUpper() == "RELATIVE")
                            DvhUnitString = ConstraintUnits.Percent.Display();
                        else
                            DvhUnitString = ConstraintUnits.cGy.Display();
                        if (ReferenceUnit.ToUpper() == "RELATIVE")
                            ConstraintUnitString = ConstraintUnits.Percent.Display();
                        else
                            ConstraintUnitString = ConstraintUnits.cc.Display();
                        return string.Format("V{0:0.###} [{1}] {2} {3:0.###} [{4}]", ConstraintValue, DvhUnitString, ReferenceType, referenceVals, ConstraintUnitString);
                    case "D":                        //Exception for min and max dose
                        if (constraintValue < 1E-5)
                        { // max dose
                            if (ConstraintUnit.ToUpper() == "RELATIVE")
                                return string.Format("Max Dose {0} {1:0.###} [{2}]", ReferenceType, referenceVals, ConstraintUnits.Percent.Display());
                            else
                                return string.Format("Max Dose {0} {1:0} [{2}]", ReferenceType, referenceVals, ConstraintUnits.cc.Display());
                        }
                        else if (Math.Abs(constraintValue - 100) < 1E-5 && ConstraintUnit.ToUpper() == "RELATIVE")
                        {
                            if (ConstraintUnit.ToUpper() == "RELATIVE")
                                return string.Format("Min Dose {0} {1:0.###} [{2}]", ReferenceType, referenceVals, ConstraintUnits.Percent.Display());
                            else
                                return string.Format("Min Dose {0} {1:0} [{2}]", ReferenceType, referenceVals, ConstraintUnits.cGy.Display());
                        }
                        else
                        {
                            if (ConstraintUnit.ToUpper() == "RELATIVE")
                                DvhUnitString = ConstraintUnits.Percent.Display();
                            else
                                DvhUnitString = ConstraintUnits.cc.Display();
                            if (ReferenceUnit.ToUpper() == "RELATIVE")
                                ConstraintUnitString = ConstraintUnits.Percent.Display();
                            else
                                ConstraintUnitString = ConstraintUnits.cGy.Display();
                            if (ConstraintUnit.ToUpper() == "RELATIVE")
                                return string.Format("D{0:0.###} [{1}] {2} {3:0.#} [{4}]", ConstraintValue, DvhUnitString, ReferenceType, referenceVals, ConstraintUnitString);
                            else
                                return string.Format("D{0:0.###} [{1}] {2} {3:0} [{4}]", ConstraintValue, DvhUnitString, ReferenceType, referenceVals, ConstraintUnitString);
                        }
                    case "M":
                        return string.Format("Mean Dose {0} {1:0} [{2}]", ReferenceType, ConstraintValue, ConstraintUnits.cGy.Display());
                    case "CI":
                        if (constraintValue.CloseEnough(0)) // Display exception if it's the whole volume for readability
                            return  string.Format("Total volume is {0} {1} % of {2} volume", ReferenceType, referenceVals, ReferenceStructureName);
                        else
                        {
                            if (ConstraintUnit.ToUpper() == "RELATIVE")
                                DvhUnitString = ConstraintUnits.Percent.Display();
                            else
                                DvhUnitString = ConstraintUnits.cc.Display();
                            if (ReferenceUnit.ToUpper() == "RELATIVE")
                                return string.Format("V{0}[{1}] {2} {3}{4} of the {5} volume", ConstraintValue, DvhUnitString, ReferenceType, referenceVals, ConstraintUnits.Percent.Display(), ReferenceStructureName);
                            else 
                                return string.Format("V{0}[{1}] {2} {3} {4} the {5} volume", ConstraintValue, DvhUnitString, ReferenceType, referenceVals, ConstraintUnits.Multiple.Display(), ReferenceStructureName);
                        }
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
            //public event EventHandler<ComponentChangedEventArgs> ComponentChanged;
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
        
            public class ComponentConstraintsDefinition
            {
                public ComponentConstraintsDefinition()
                {
                    Constraint = new List<ConstraintDefinition>();
                }
                [XmlElement("Constraint")]
                public List<ConstraintDefinition> Constraint { get; set; }
            }
            [XmlElement("Prescription")]
            public PrescriptionDefinition Prescription { get; set; } = new PrescriptionDefinition();

            [XmlElement("Constraints")]
            public ComponentConstraintsDefinition Constraints { get; set; }
            
            [XmlElement("ImagingProtocols")]
            public ImagingProtocolsDefinition ImagingProtocols { get; set; }
            [XmlElement("Beams")]
            public BeamsDefinition Beams { get; set; }

        }
        
        //public class ConException
        //{
        //    //Properties
        //    [XmlAttribute]
        //    public string Property { get; set; }
        //    [XmlAttribute]
        //    public double PropertyValue { get; set; }
        //    [XmlAttribute]
        //    public string PropertyString { get; set; }
        //    [XmlAttribute]
        //    public string Description { get; set; }
        //}

        [XmlElement("ProtocolMetaData")]
        public ProtocolMetaDataDefinition ProtocolMetaData { get; set; } = new ProtocolMetaDataDefinition();
        [XmlElement("ImagingProtocols")]
        public ImagingProtocolsDefinition ImagingProtocols { get; set; } = new ImagingProtocolsDefinition();
        [XmlElement("Structures")]
        public StructuresDefinition Structures { get; set; } = new StructuresDefinition();
        [XmlElement("ProtocolChecklist")]
        public ProtocolChecklistDefinition ProtocolChecklist { get; set; } = new ProtocolChecklistDefinition();
        [XmlElement("Components")]
        public ComponentsDefinition Components { get; set; } = new ComponentsDefinition();
        [XmlElement("Constraints")]
        public ConstraintListDefinition Constraints { get; set; } = new ConstraintListDefinition();
        
    }
}
