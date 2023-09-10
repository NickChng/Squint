using Squint;
using Squint.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Squint.Helpers
{
    public static class TypeDisplay
    {
        public static string Display(ReferenceTypes t)
        {
            switch (t)
            {
                case ReferenceTypes.Unset:
                    return "Unset";
                case ReferenceTypes.Upper:
                    return "\u2264";
                case ReferenceTypes.Lower:
                    return "\u2265";
                default:
                    return "Error";
            }
        }
        public static string Display(ConstraintTypes t)
        {
            switch (t)
            {
                case ConstraintTypes.Unset:
                    return "-";
                case ConstraintTypes.D:
                    return "D";
                case ConstraintTypes.V:
                    return "V";
                case ConstraintTypes.M:
                    return "M";
                case ConstraintTypes.CV:
                    return "C";
                case ConstraintTypes.CI:
                    return "CI";
                default:
                    return "Error";
            }
        }
        //public static string Display(DvhTypes t)
        //{
        //    switch (t)
        //    {
        //        case DvhTypes.Unset:
        //            return "-";
        //        case DvhTypes.CV:
        //            return "CV";
        //        case DvhTypes.V:
        //            return "V";
        //        case DvhTypes.D:
        //            return "D";
        //        case DvhTypes.M:
        //            return "Mean Dose";
        //        default:
        //            return "Error";
        //    }
        //}

        public static string Display(Permissions t)
        {
            switch (t)
            {
                case Permissions.CanSaveProtocol:
                    return "Can save protocol";
                case Permissions.CanOverwriteProtocol:
                    return "Can overwrite protocol";
                case Permissions.CanImportProtocol:
                    return "Can import protocol";
                case Permissions.CanRetireProtocol:
                    return "Can retire protocol";
                case Permissions.CanApproveProtocol:
                    return "Can approve protocol";
                case Permissions.CanApproveAssessment:
                    return "Can approve assessment";
                case Permissions.CanSaveAssessment:
                    return "Can save assessment";
                case Permissions.CanRetireAssessment:
                    return "Can retire assessment";
                case Permissions.CanOverwriteAssessment:
                    return "Can overwrite assessment";
                default:
                    return "Error";

            }
        }

        public static string Display(ApprovalLevels t)
        {
            switch (t)
            {
                case ApprovalLevels.Unset:
                    return "";
                case ApprovalLevels.All:
                    return "Show all";
                case ApprovalLevels.Unapproved:
                    return "Unapproved";
                case ApprovalLevels.Reviewed:
                    return "Reviewed";
                case ApprovalLevels.Approved:
                    return "Approved";
                case ApprovalLevels.Retired:
                    return "Retired";

                default:
                    return "Error";
            }
        }

        public static string Display(TreatmentSites t)
        {
            switch (t)
            {
                case TreatmentSites.Unset:
                    return "";
                case TreatmentSites.All:
                    return "Show all";
                case TreatmentSites.GU:
                    return "GU";
                case TreatmentSites.GI:
                    return "GI";
                case TreatmentSites.Lung:
                    return "Lung";
                case TreatmentSites.HN:
                    return @"H&N";
                case TreatmentSites.CNS:
                    return "CNS";
                case TreatmentSites.Sarcoma:
                    return "Sarcoma";
                case TreatmentSites.Breast:
                    return "Breast";
                case TreatmentSites.Lymphoma:
                    return "Lymphoma";
                case TreatmentSites.Palliative:
                    return "Palliative";
                default:
                    return "Error";
            }
        }

        public static string Display(TreatmentIntents t)
        {
            switch (t)
            {
                case TreatmentIntents.Unset:
                    return "";
                case TreatmentIntents.Curative:
                    return "Curative";
                case TreatmentIntents.Adjuvant:
                    return "Adjuvant";
                case TreatmentIntents.NeoAdjuvant:
                    return "Neo-Adjuvant";
                case TreatmentIntents.Palliative:
                    return "Palliative";
                default:
                    return "Error";
            }
        }

        public static string Display(ProtocolTypes t)
        {
            switch (t)
            {
                case ProtocolTypes.Unset:
                    return "";
                case ProtocolTypes.All:
                    return "Show all";
                case ProtocolTypes.Clinical:
                    return "Clinical";
                case ProtocolTypes.Trial:
                    return "Trial";
                case ProtocolTypes.Research:
                    return "Research";
                case ProtocolTypes.Development:
                    return "Development";
                default:
                    return "Error";
            }
        }

        public static string Display(TreatmentCentres t)
        {
            switch (t)
            {
                case TreatmentCentres.Unset:
                    return "";
                case TreatmentCentres.All:
                    return "Show all";
                case TreatmentCentres.Provincial:
                    return "Provincial";
                case TreatmentCentres.CN:
                    return "CN";
                case TreatmentCentres.VCC:
                    return "VCC";
                case TreatmentCentres.VIC:
                    return "VIC";
                case TreatmentCentres.AC:
                    return "AC";
                case TreatmentCentres.FVCC:
                    return "FVCC";
                case TreatmentCentres.CSI:
                    return "CSI";
                default:
                    return "Error";

            }
        }

        public static string Display(FieldNormalizationTypes t)
        {
            switch (t)
            {
                case FieldNormalizationTypes.Unset:
                    return "";
                case FieldNormalizationTypes.ISO:
                    return "100% to isocenter";
                case FieldNormalizationTypes.CAX:
                    return "100% to central axis Dmax";
                case FieldNormalizationTypes.field:
                    return "100% to field Dmax";
                case FieldNormalizationTypes.None:
                    return "No field normalization";
                default:
                    return "Error";

            }
        }

        public static string Display(ParameterOptions t)
        {
            switch (t)
            {
                case ParameterOptions.Unset:
                    return "";
                case ParameterOptions.Optional:
                    return "Optional";
                case ParameterOptions.Required:
                    return "Required";
                case ParameterOptions.None:
                    return "None";
                default:
                    return "Error";

            }
        }

        public static string Display(AlgorithmVMATOptimizationTypes t)
        {
            switch (t)
            {
                case AlgorithmVMATOptimizationTypes.Unset:
                    return "";
                case AlgorithmVMATOptimizationTypes.PRO_13623:
                    return "PRO_13623";
                case AlgorithmVMATOptimizationTypes.PO_13623:
                    return "PO_13623";
                case AlgorithmVMATOptimizationTypes.PO_15606:
                    return "PO_15606";
                default:
                    return "Error";
            }
        }

        public static string Display(AlgorithmIMRTOptimizationTypes t)
        {
            switch (t)
            {
                case AlgorithmIMRTOptimizationTypes.Unset:
                    return "";
                case AlgorithmIMRTOptimizationTypes.PRO_13623:
                    return "PRO_13623";
                case AlgorithmIMRTOptimizationTypes.PO_13623:
                    return "PO_13623";
                case AlgorithmIMRTOptimizationTypes.PO_15606:
                    return "PO_15606";
                default:
                    return "Error";
            }
        }

        public static string Display(Energies t)
        {
            switch (t)
            {
                case Energies.Unset:
                    return "";
                case Energies.Item6X:
                    return "6X";
                case Energies.Item10X:
                    return "10X";
                case Energies.Item15X:
                    return "15X";
                case Energies.Item10XFFF:
                    return "10X-FFF";
                case Energies.Item6XFFF:
                    return "6X-FFF";
                default:
                    return "Error";
            }
        }


        public static string Display(ImagingProtocolTypes t)
        {
            switch (t)
            {
                case ImagingProtocolTypes.Unset:
                    return "";
                case ImagingProtocolTypes.kV_2D:
                    return "2D kV";
                case ImagingProtocolTypes.PostCBCT:
                    return "Post-Tx CBCT";
                case ImagingProtocolTypes.PreCBCT:
                    return "Pre-Tx CBCT";
                case ImagingProtocolTypes.Bolus:
                    return "Bolus setup";
                case ImagingProtocolTypes.Home:
                    return "Home field";
                default:
                    return "Error";
            }
        }

        public static string Display(ConstraintUnits t)
        {
            switch (t)
            {
                case ConstraintUnits.Unset:
                    return "-";
                case ConstraintUnits.Percent:
                    return "%";
                case ConstraintUnits.cGy:
                    return "cGy";
                case ConstraintUnits.cc:
                    return "cc";
                case ConstraintUnits.Multiple:
                    return "\u00D7";
                default:
                    return "Error";
            }
        }
        

        //public static string Display(ParameterOptions t)
        //{
        //    switch (t)
        //    {
        //    }
        //}


    }
}
