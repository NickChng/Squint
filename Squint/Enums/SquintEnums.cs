﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.DataAnnotations;


namespace SquintScript
{
    //Type Definitions

    public enum DatabaseStatus
    {
        [Description("NonExistent")] NonExistent = 0,
        [Description("Exists")] Exists = 1,
        [Description("Creating")] Creating = 2,
        [Description("ConnectionError")] ConnectionError = 3,
    }
    public enum CheckTypes
    {
        [Description("Unset")] Unset,
        [Description("Series Id")] SeriesId,
        [Description("Study Id")] StudyId,
        [Description("Series Comment")] SeriesComment,
        [Description("Image Comment")] ImageComment,
        [Description("Number of slices")] NumSlices,
        [Description("Slice spacing")] SliceSpacing,
        [Description("Plan normalization")] PlanNormalization,
        [Description("Volume Dose Algorithm")] Algorithm,
        [Description("VMAT Optimization Algorithm")] AlgorithmVMATOptimization,
        [Description("IMRT Optimization Algorithm")] AlgorithmIMRTOptimization,
        [Description("Dose grid resolution")] DoseGridResolution,
        [Description("Heterogeneity on")] HeterogeneityOn,
        [Description("Field norm mode")] FieldNormMode,
        [Description("Couch surface HU")] CouchSurfaceHU,
        [Description("Couch interior HU")] CouchInteriorHU,
        [Description("Artifact HU")] ArtifactHU,
        [Description("Prescription dose")] PrescriptionDose,
        [Description("Number of fractions")] NumFractions,
        [Description("Course intent")] CourseIntent,
        [Description("Plan normalization value range")] PNVRange,
        [Description("Prescribed percentage")] PrescribedPercentage,
        [Description("VMAT Air Cavity Correction")] AirCavityCorrectionVMAT,
        [Description("IMRT Air Cavity Correction")] AirCavityCorrectionIMRT,
        [Description("Min. subvolume")] MinSubvolume,
        [Description("Number of isocentres")] NumIsocentres,
        [Description("Number of fields")] NumFields,
        [Description("Min. collimator offset")] MinColOffset,
        [Description("MU range")] MURange,
        [Description("Valid energies")] ValidEnergies,
        [Description("Beam geometry")] BeamGeometry,
        [Description("Couch rotation")] CouchRotation,
        [Description("Jaw tracking")] JawTracking,
        [Description("Minimum MLC offset from axial plane")] MinMLCOffsetFromAxial,
        [Description("Minimum X field size")] MinimumXfieldSize,
        [Description("Maximum X field size")] MaximumXfieldSize,
        [Description("Minimum Y field size")] MinimumYfieldSize,
        [Description("Maximum Y field size")] MaximumYfieldSize,
        [Description("Bolus HU")] BolusHU,
        [Description("Bolus thickness [cm]")] BolusThickness,
        [Description("Tolerance Table")] ToleranceTable,
        [Description("Density Override")] DensityOverride
    }
    public enum Energies
    {
        [Description("Unset")] Unset = 1,
        [Description("6X")] Photons6 = 2,
        [Description("10X")] Photons10 = 3,
        [Description("15X")] Photons15 = 4,
        [Description("10XFFF")] Photons10FFF =5
    }
    public enum Trajectories
    {
        [Description("Unset")] Unset = 1,
        [Description("Static")] Static = 2,
        [Description("Clockwise")] CW = 3,
        [Description("Counterclockwise")] CCW = 4,
    }
    public enum ParameterOptions
    {
        [Description("Unset")] Unset,
        [Description("Optional")] Optional,
        [Description("Required")] Required,
        [Description("None")] None,
    }
    public enum EditTypes
    {
        [Description("Unset")] Unset,
        [Description("SingleSelection")] SingleSelection,
        [Description("SingleValue")] SingleValue,
        [Description("SingleValueWithTolerance")] SingleValueWithTolerance,
        [Description("RangeValues")] RangeValues,
        [Description("AnyOfValues")] AnyOfValues,
    }
    public enum ImagingProtocols
    {
        [Description("None")] Unset,
        [Description("Single-kV")] kV,
        [Description("2D-kV")] kV_2D,
        [Description("Bolus-setup")] Bolus,
        [Description("Pre-Tx CBCT")] PreCBCT,
        [Description("Post-Tx CBCT")] PostCBCT,
    }

    public enum TreatmentIntents
    {
        [Description("")] Unset,
        [Description("Curative")] Curative,
        [Description("Adjuvant")] Adjuvant,
        [Description("Neo-adjuvant")] NeoAdjuvant,
        [Description("Palliative")] Palliative,
    }

    public enum AlgorithmVolumeDoseTypes
    {
        [Description("Unset")] Unset,
        [Description("AAA_11031")] AAA_11031,
        [Description("AAA_13623")] AAA_13623,
        [Description("AAA_15606")] AAA_15606,
    }
    public enum AlgorithmVMATOptimizationTypes
    {
        [Description("Unset")] Unset,
        [Description("PRO_13623")] PRO_13623,
        [Description("PO_13623")] PO_13623,
        [Description("PO_15606")] PO_15606,
    }
    public enum AlgorithmIMRTOptimizationTypes
    {
        [Description("Unset")] Unset,
        [Description("PRO_13623")] PRO_13623,
        [Description("PO_13623")] PO_13623,
        [Description("PO_15606")] PO_15606,
    }

    public enum FieldNormalizationTypes
    {
        [Description("Unset")] Unset,
        [Description("100% to isocenter")] ISO,
        [Description("100% to central axis Dmax")] CAX,
        [Description("100% to field Dmax")] field,
        [Description("No field normalization")] None,
    }

    public enum TestType
    {
        [Description("None")] Unset,
        [Description("Equality")] Equality,
        [Description("GreaterThan")] GreaterThan,
        [Description("LessThan")] LessThan,
        [Description("Range")] Range,
        [Description("Multiple-choice")] MC,
    }

    public enum FieldType
    {
        [Description("Unidentified")] Unset,
        [Description("Bolus Setup")] BolusSetup,
        [Description("Left Lateral kV")] LL_kv,
        [Description("Right Lateral kV")] RL_kv,
        [Description("CBCT")] CBCT,
        [Description("Anterior kV")] Ant_kv,
        [Description("Posterior kV")] Post_kv,
        [Description("Static Tx")] STATIC,
        [Description("ARC Tx")] ARC,
        [Description("Unload")] Unload,
    }

    public enum StructureTypes
    {
        [Description("-")] Unset,
        [Description("GTV")] GTV,
        [Description("CTV")] CTV,
        [Description("PTV")] PTV,
        [Description("Bone")] Bone,
        [Description("Organ")] Organ,
        [Description("PRV")] PRV,
        [Description("Optimization Structure")] Opti,
        [Description("Dose Region")] DoseRegion,
        [Description("Bolus")] Bolus,
        [Description("Patient reference")] PatientReference,
        [Description("Artifact")] Artifact
    }
    //public enum StructureLabelEnum // Note: XML input/output uses the description, not the enum values
    //{
    //    [Description("")] Unset,
    //    [Description("Bladder")] Bladder,
    //    [Description("Brachial Plexus")] BrachialPlexus,
    //    [Description("Brain")] Brain,
    //    [Description("Brainstem")] Brainstem,
    //    [Description("CTVn")] CTVn,
    //    [Description("CTVp")] CTVp,
    //    [Description("Esophagus")] Esophagus,
    //    [Description("External Genitalia")] ExternalGenitalia,
    //    [Description("Eye")] Eye,
    //    [Description("Femur")] Femur,
    //    [Description("GTV")] GTV,
    //    [Description("Heart")] Heart,
    //    [Description("Iliac Crest")] IliacCrest,
    //    [Description("Laryngopharynx")] Laryngopharynx,
    //    [Description("Lens")] Lens,
    //    [Description("Liver")] Liver,
    //    [Description("Mandible")] Mandible,
    //    [Description("Optics")] Optics,
    //    [Description("Oral Cavity")] OralCavity,
    //    [Description("Parotid")] Parotid,
    //    [Description("PTVn")] PTVn,
    //    [Description("PTVp")] PTVp,
    //    [Description("PTVp2")] PTVp2,
    //    [Description("Rectum")] Rectum,
    //    [Description("Small Bowel")] SmallBowel,
    //    [Description("Spinal Cord")] SpinalCord,
    //    [Description("Submandibular")] Submandibular,
    //    [Description("Large Bowel")] LargeBowel,
    //    [Description("Kidney")] Kidney,
    //    [Description("Sacral Canal")] SacralCanal
    //}
    public enum ComponentStatusCodes
    {
        [Description("")] NoError,
        [Description("Evaluable")] Evaluable,
        [Description("Plan not found / Not linked")] NotLinked,
        [Description("Empty Contour")] StructureNotContoured,
        [Description("ID not found")] StructureIDNotFound,
        [Description("Ref Dose Mismatch")] ReferenceDoseMismatch,
        [Description("# Fraction Mismatch")] NumFractionsMismatch,
        [Description("Error in sum constituent")] ConstituentError,
        [Description("Plan linked to constituent is not in sum")] ConstituentNotInSum,
        [Description("Plan sum contains a different number of plans than protocol constituents")] PlansNotEqualToConstituents,
        [Description("Changed since last session")] ChangedSinceLastSession,
        [Description("Plan is not linked to current structure set")] StructureSetDiscrepancy,
        [Description("UNDEFINED ERROR")] NotDefined,
        [Description("No dose")] NoDoseDistribution,
    }

    public enum ConstraintResultStatusCodes
    {
        [Description("")] NoError,
        [Description("Error")] ErrorUnspecified,
        [Description("No Dose Calc")] NoDoseDistribution,
        [Description("Not linked")] NotLinked,
        [Description("-")] ConstraintUndefined,
        [Description("Plan-Protocol Mismatch")] LinkedPlanError,
        [Description("No Structure")] StructureNotFound,
        [Description("No Contour")] StructureEmpty,
        [Description("Label Mismatch")] LabelMismatch,
        [Description("Loaded")] Loaded,
        [Description("Reference dose invalid")] RefDoseInValid,
        [Description("Warning: Relative dose reported for plan sum")] RelativeDoseForSum,
    }

    public enum Permissions
    {

        [Description("can save protocol")] CanSaveProtocol,
        [Description("can overwrite protocol")] CanOverwriteProtocol,
        [Description("can import protocol")] CanImportProtocol,
        [Description("can retire protocol")] CanRetireProtocol,
        [Description("can approve protocol")] CanApproveProtocol,
        [Description("can approve assessment")] CanApproveAssessment,
        [Description("can save assessment")] CanSaveAssessment,
        [Description("can retire assessment")] CanRetireAssessment,
        [Description("can overwrite assessment")] CanOverwriteAssessment

    }

    public enum ChangeStatus
    {
        [Description("Unset")] Unset,
        [Description("Unchanged")] Unchanged,
        [Description("Modified")] Modified,
        [Description("Deleted")] Deleted,
        [Description("New")] New,
    }

    public enum PlanTypes
    {
        Unset,
        Single,
        Sum
    }

    public enum SchemaShortNames
    {
        [Description("SQUINT")] SQUINT,
        [Description("FMA")] FMA,
        [Description("VMS")] VMS,
        [Description("RADLEX")] RADPLEX
    }
    public enum SchemaFullNames
    {
        [Description("SQUINT")] SQUINT,
        [Description("FMA (3.2)")] FMA,
        [Description("99VMS_STRUCTCODE (1.0)")] VMS,
        [Description("RADLEX (3.8)")] RADPLEX
    }

    public enum ComponentTypes
    {
        [Description("")] Unset = 0,
        [Description("Phase")] Phase = 1,
        [Description("Sum")] Sum = 2,
    }
    public enum ConstraintTypeCodes
    {
        [Description("-")] Unset = 0,
        [Description("D")] D = 1,
        [Description("V")] V = 2,
        [Description("M")] M = 3,
        [Description("C")] CV = 4,
        [Description("CI")] CI = 5
    }

    public enum Dvh_Types
    {
        [Description("-")] Unset = 0,
        [Description("CV")] CV = 1,
        [Description("V")] V = 2,
        [Description("D")] D = 3,
        [Description("Mean Dose")] M = 4
    }

    public enum ReferenceThresholdCalculationTypes
    {
        [Description("Unset")] Unset,
        [Description("Fixed")] Fixed,
        [Description("Interpolated")] Interpolated,
    }

    public enum InterpolationParameterTypes
    {
        [Description("Unset")] Unset,
        [Description("StructureVolume")] StructureVolume,
    }

    public enum ReferenceThresholdTypes
    {
        [Description("Unset")] Unset,
        [Description("None")] None,
        [Description("MajorViolation")] MajorViolation,
        [Description("MinorViolation")] MinorViolation,
        [Description("Stop")] Stop,
    }

    public enum ConstraintThresholdTypes
    {
        Unset,
        Violation,
        Goal
    }

    public enum ReferenceTypes
    {
        [Description("-")] Unset = 0,
        [Description("\u2264")] Upper = 1,
        [Description("\u2265")] Lower = 2
    }
    public enum ConstraintUnits
    {
        [Description("-")] Unset = 0,
        [Description("%")] Percent = 1,
        [Description("cGy")] cGy = 2,
        [Description("cc")] cc = 3,
        [Description("\u00D7")] Multiple = 4
    }

    public static class DvhVolumeUnits
    {
        public readonly static BindingList<ConstraintUnits> VolUnits = new BindingList<ConstraintUnits>()
        {
            ConstraintUnits.Unset,
            ConstraintUnits.cc,
            ConstraintUnits.Percent
        };
    }

    public static class DvhDoseUnits
    {
        public readonly static BindingList<ConstraintUnits> DoseUnits = new BindingList<ConstraintUnits>()
        {
            ConstraintUnits.Unset,
            ConstraintUnits.cGy,
            ConstraintUnits.Percent
        };
    }


    public enum UnitScale
    {
        Unset,
        Relative,
        Absolute
    }

    public enum TreatmentCentres
    {
        [Description("")] Unset,
        [Description("Show all")] All,
        [Description("Provincial")] Provincial,
        [Description("CN")] CN,
        [Description("VCC")] VCC,
        [Description("VIC")] VIC,
        [Description("AC")] AC,
        [Description("FVCC")] FVCC,
        [Description("CSI")] CSI
    }
    public enum ApprovalLevels
    {
        [Description("")] Unset,
        [Description("Show all")] All,
        [Description("Unapproved")] Unapproved,
        [Description("Reviewed")] Reviewed,
        [Description("Approved")] Approved,
        [Description("Retired")] Retired,
    }
    public enum ProtocolTypes
    {
        [Description("")] Unset,
        [Description("Show all")] All,
        [Description("Clinical")] Clinical,
        [Description("Trial")] Trial,
        [Description("Research")] Research,
        [Description("Development")] Development,
    }
    public enum TreatmentSites
    {
        [Description("")] Unset,
        [Description("Show all")] All,
        [Description("GU")] GU,
        [Description("GI")] GI,
        [Description("Lung")] Lung,
        [Description(@"H&N")] HN,
        [Description("CNS")] CNS,
        [Description("Sarcoma")] Sarcoma,
        [Description("Breast")] Breast,
        [Description("Lymphoma")] Lymphoma,
        [Description("Palliative")] Palliative,
    }
    public enum TreatmentTechniques
    {
        [Description("")] Unset,
        [Description("Conformal")] Conformal,
        [Description("IMRT")] IMRT,
        [Description("VMAT")] VMAT,
        [Description("SABRstatic")] SABRstatic,
        [Description("SABRVMAT")] SABRVMAT,
        [Description("POP")] POP,
        [Description("Single direct")] SingleDirect
    }
}