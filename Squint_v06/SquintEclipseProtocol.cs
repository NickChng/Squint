using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using VMS.TPS.Common.Model.API;
//using VMS.TPS.Common.Model.Types;
using System.Data.Entity;
using System.Data;

namespace SquintScript
{
    public class SquintEclipseProtocol
    {
        [Serializable, XmlRoot("Protocol")]
        public class Protocol
        {
            public Protocol()
            {
                Version = 1.9;
                StructureTemplate = new StructureTemplateClass();
                FieldAlignmentRules = new FieldAlignmentRulesClass();
                Phases = new PhasesClass();
                Review = new Review();
            }
            [XmlAttribute]
            public double Version { get; set; }
            public PreviewClass Preview { get; set; }
            [XmlElement]
            public StructureTemplateClass StructureTemplate { get; set; }
            [XmlElement]
            public FieldAlignmentRulesClass FieldAlignmentRules { get; set; }
            public PhasesClass Phases { get; set; }
            public Review Review { get; set; }
        }
        public class PreviewClass
        {
            [XmlAttribute]
            public string ID { get; set; }
            [XmlAttribute]
            public string Type { get; set; }
            [XmlAttribute]
            public string ApprovalStatus { get; set; }
            [XmlAttribute]
            public string Diagnosis { get; set; }
            [XmlAttribute]
            public string TreatmentSite { get; set; }
            [XmlAttribute]
            public string AssignedUsers { get; set; }
            [XmlAttribute]
            public string Description { get; set; }
            [XmlAttribute]
            public string LastModified { get; set; }
            [XmlAttribute]
            public string ApprovalHistory { get; set; }
        }
        public class StructureClass
        {
            [XmlAttribute]
            public string ID { get; set; }
            [XmlElement]
            public Identification Identification { get; set; }
            [XmlElement]
            public int TypeIndex { get; set; }
            [XmlElement]
            public EmptyClass ColorAndStyle { get; set; }
            [XmlElement]
            public int? SearchCTLow { get; set; }
            [XmlElement]
            public int? SearchCTHigh { get; set; }
            [XmlElement]
            public int DVHLineStyle { get; set; }
            [XmlElement]
            public int DVHLineColor { get; set; }
            [XmlElement]
            public int DVHLineWidth { get; set; }
            [XmlElement]
            public int? EUDAlpha { get; set; }
            [XmlElement]
            public int? TCPAlpha { get; set; }
            [XmlElement]
            public int? TCPBeta { get; set; }
            [XmlElement]
            public int? TCPGamma { get; set; }
        }
        public class StructuresClass
        {
            public StructuresClass()
            {
                Structure = new List<StructureClass>();
            }
            [XmlElement]
            public List<StructureClass> Structure;
        }
        public class StructureTemplateClass
        {
            public StructureTemplateClass()
            {
                Structures = new StructuresClass();
            }
            public StructuresClass Structures { get; set; }
        }
        public class FieldAlignmentRulesClass
        {

        }
        public class StructureCode
        {
            public StructureCode()
            {
                Code = new EmptyClass();
                CodeScheme = new EmptyClass();
                CodeSchemeVersion = new EmptyClass();
            }
            [XmlElement]
            public EmptyClass Code { get; set; }
            [XmlElement]
            public EmptyClass CodeScheme { get; set; }
            [XmlElement]
            public EmptyClass CodeSchemeVersion { get; set; }
        }
        public class Identification
        {
            public Identification()
            {
                VolumeID = new EmptyClass();
                VolumeCode = new EmptyClass();
                VolumeType = new EmptyClass();
                VolumeCodeTable = new EmptyClass();
                StructureCode = new StructureCode();
            }
            [XmlElement]
            public EmptyClass VolumeID { get; set; }
            [XmlElement]
            public EmptyClass VolumeCode { get; set; }
            [XmlElement]
            public EmptyClass VolumeType { get; set; }
            [XmlElement]
            public EmptyClass VolumeCodeTable { get; set; }
            [XmlElement]
            public StructureCode StructureCode { get; set; }
        }
        public class PhasesClass
        {
            public PhasesClass()
            {
                Phase = new List<PhaseClass>();
            }
            [XmlElement]
            public List<PhaseClass> Phase { get; set; }
        }
        public class PhaseClass
        {
            public PhaseClass()
            {
                Prescription = new PrescriptionClass();
            }
            [XmlAttribute]
            public string ID { get; set; }
            [XmlElement]
            public string Mode { get; set; }
            [XmlElement]
            public int? DefaultEnergyKV { get; set; }
            [XmlElement]
            public int FractionCount { get; set; }
            [XmlElement]
            public int? FractionsPerWeek { get; set; }
            [XmlElement]
            public int? FractionsPerDay { get; set; }
            [XmlElement]
            public string TreatmentUnit { get; set; }
            [XmlElement]
            public string TreatmentStyle { get; set; }
            [XmlElement]
            public string ImmobilizationDevice { get; set; }
            [XmlElement]
            public string LocalizationTechnique { get; set; }
            [XmlElement]
            public PrescriptionClass Prescription { get; set; }
            [XmlElement]
            public PlanTemplate PlanTemplate { get; set; }
            [XmlElement]
            public ObjectiveTemplate ObjectiveTemplate { get; set; }
        }
        public class PrescriptionClass
        {
            [XmlElement("Item")]
            public List<Item> Items = new List<Item>();
            [XmlElement("MeasureItem")]
            public List<MeasureItem> MeasureItems = new List<MeasureItem>();
        }
        public class Item 
        {
            [XmlAttribute]
            public string ID { get; set; }
            [XmlElement]
            public int Type { get; set; }
            [XmlElement]
            public int Modifier { get; set; }
            [XmlElement]
            public double Parameter { get; set; }
            [XmlElement]
            public double? Dose { get; set; }
            [XmlElement]
            public double TotalDose { get; set; }
        }
        public class MeasureItem 
        {
            [XmlAttribute]
            public string ID { get; set; }
            [XmlElement]
            public int? Type { get; set; }
            [XmlElement]
            public int? Modifier { get; set; }
            [XmlElement]
            public double? Value { get; set; }
            [XmlElement]
            public double? TypeSpecifier { get; set; }
            [XmlElement]
            public bool ReportDQPValueInAbsoluteUnits { get; set; }
        }
        public class PlanTemplate
        {
            public PlanTemplate()
            {
                PrescribedPercentage = 1;
                FieldAlignmentRules = new FieldAlignmentRulesClass();
                PrescriptionSite = new PrescriptionSiteClass();
                Boluses = new EmptyClass();
                Fields = new EmptyClass();
            }
            [XmlElement]
            public double PrescribedPercentage { get; set; }
            [XmlElement]
            public double DosePerFraction { get; set; }
            [XmlElement]
            public int FractionCount { get; set; }
            [XmlElement]
            public FieldAlignmentRulesClass FieldAlignmentRules { get; set; }
            [XmlElement]
            public PrescriptionSiteClass PrescriptionSite { get; set; }
            [XmlElement]
            public EmptyClass Boluses { get; set; }
            [XmlElement]
            public EmptyClass Fields { get; set; }
        }
        public class ObjectiveTemplate
        {
            public ObjectiveTemplate()
            {
                Type = "Helios";
                Helios = new Helios();
                ObjectivesAllStructures = new EmptyClass();
            }
            [XmlElement]
            public string Type { get; set; }
            [XmlElement]
            public Helios Helios { get; set; }
            [XmlElement]
            public EmptyClass ObjectivesAllStructures { get; set; }
        }
        public class PrescriptionSiteClass
        {
            public PrescriptionSiteClass()
            {
                VolumeID = "";
                VolumeCode = new EmptyClass();
                VolumeType = new EmptyClass();
                VolumeCodeTable = new EmptyClass();
            }
            [XmlElement]
            public string VolumeID { get; set; }
            [XmlElement]
            public EmptyClass VolumeCode { get; set; }
            [XmlElement]
            public EmptyClass VolumeType { get; set; }
            [XmlElement]
            public EmptyClass VolumeCodeTable { get; set; }
        }
        public class EmptyClass
        {
            string value { get; set; }
        }
        public class Helios
        {
            public Helios()
            {
                DefaultFixedJaws = false;
                Interpolate = false;
                UseColors = false;
                DefaultSmoothingX = 40;
                DefaultSmoothingY = 40;
                DefaultMinimizeDose = 0;
                DefaultOptimizationType = @"Beamlet";
                MaxIterations = 1000;
                MaxTime = 100;
            }
            [XmlAttribute]
            public bool DefaultFixedJaws { get; set; }
            [XmlAttribute]
            public bool Interpolate { get; set; }
            [XmlAttribute]
            public bool UseColors { get; set; }
            [XmlElement]
            public int DefaultSmoothingX { get; set; }
            [XmlElement]
            public int DefaultSmoothingY { get; set; }
            [XmlElement]
            public int DefaultMinimizeDose { get; set; }
            [XmlElement]
            public string DefaultOptimizationType { get; set; }
            [XmlElement]
            public int MaxIterations { get; set; }
            [XmlElement]
            public int MaxTime { get; set; }
        }
        public class Review
        {
            public Review()
            {
                ShowMin = true;
                ShowMax = true;
                ShowMean = true;
                ShowModal = false;
                ShowMedian = false;
                ShowStdDev = false;
                ShowEUD = true;
                ShowTCP = false;
                ShowNTCP = false;
                ShowNDR = false;
                ShowEquivalentSphereDiameter = false;
                ShowConformityIndex = false;
                ShowGradientMeasure = false;
            }
            [XmlAttribute]
            public bool ShowMin { get; set; }
            [XmlAttribute]
            public bool ShowMax { get; set; }
            [XmlAttribute]
            public bool ShowMean { get; set; }
            [XmlAttribute]
            public bool ShowModal { get; set; }
            [XmlAttribute]
            public bool ShowMedian { get; set; }
            [XmlAttribute]
            public bool ShowStdDev { get; set; }
            [XmlAttribute]
            public bool ShowEUD { get; set; }
            [XmlAttribute]
            public bool ShowTCP { get; set; }
            [XmlAttribute]
            public bool ShowNTCP { get; set; }
            [XmlAttribute]
            public bool ShowNDR { get; set; }
            [XmlAttribute]
            public bool ShowEquivalentSphereDiameter { get; set; }
            [XmlAttribute]
            public bool ShowConformityIndex { get; set; }
            [XmlAttribute]
            public bool ShowGradientMeasure { get; set; }
        }

    }

}
