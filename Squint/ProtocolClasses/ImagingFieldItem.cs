﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SquintScript.Extensions;
//using System.Windows.Forms;

namespace SquintScript
{
    public class ImagingFieldItem
    {
        private List<string> MVmodes = new List<string>() { "X" };
        public ImagingFieldItem(VMS.TPS.Common.Model.API.Beam ImagingField = null, VMS.TPS.Common.Model.Types.PatientOrientation O = VMS.TPS.Common.Model.Types.PatientOrientation.NoOrientation)
        {
            if (ImagingField != null)
            {
                Id = ImagingField.Id;
                GantryAngle = ImagingField.ControlPoints.FirstOrDefault().GantryAngle;
                CollimatorAngle = ImagingField.ControlPoints.FirstOrDefault().CollimatorAngle;
                Orientation = O;
                Type = IdentifyFieldType();
            }
            if (Type == FieldType.Unset)
            {
                Warning = true;
                WarningMessage = "Name / angle mismatch";
            }
            if (Type == FieldType.BolusSetup && ImagingField.MLC == null)
            {
                Warning = true;
                WarningMessage = "Confirm no MLC needed";
            }

        }
        private FieldType IdentifyFieldType()
        {
            if (Bolus.Match(Id.ToUpper()).Success)
            {
                return FieldType.BolusSetup;
            }
            switch (GantryAngle)
            {
                case 0:
                    if (CBCT.Match(Id.ToUpper()).Success)
                    {
                        return FieldType.CBCT;
                    }
                    else
                    {
                        if (Unload.Match(Id.ToUpper()).Success)
                            return FieldType.Unload;
                        else
                        {
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstSupine && AntField.Match(Id.ToUpper()).Success)
                                return FieldType.Ant_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstSupine && AntField.Match(Id.ToUpper()).Success)
                                return FieldType.Ant_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstProne && PostField.Match(Id.ToUpper()).Success)
                                return FieldType.Post_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstProne && PostField.Match(Id.ToUpper()).Success)
                                return FieldType.Post_kv;
                        }
                    }
                    return FieldType.Unset;
                case 90:
                    {
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstSupine && LeftField.Match(Id.ToUpper()).Success)
                            return FieldType.LL_kv;
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstSupine && RightField.Match(Id.ToUpper()).Success)
                            return FieldType.RL_kv;
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstProne && RightField.Match(Id.ToUpper()).Success)
                            return FieldType.RL_kv;
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstProne && LeftField.Match(Id.ToUpper()).Success)
                            return FieldType.LL_kv;
                        return FieldType.Unset;
                    }
                case 270:
                    {
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstSupine && RightField.Match(Id.ToUpper()).Success)
                            return FieldType.RL_kv;
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstSupine && LeftField.Match(Id.ToUpper()).Success)
                            return FieldType.LL_kv;
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstProne && LeftField.Match(Id.ToUpper()).Success)
                            return FieldType.LL_kv;
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstProne && RightField.Match(Id.ToUpper()).Success)
                            return FieldType.RL_kv;
                        return FieldType.Unset;
                    }
                case 180:
                    if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstSupine && PostField.Match(Id.ToUpper()).Success)
                        return FieldType.Post_kv;
                    if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstSupine && PostField.Match(Id.ToUpper()).Success)
                        return FieldType.Post_kv;
                    if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstProne && AntField.Match(Id.ToUpper()).Success)
                        return FieldType.Ant_kv;
                    if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstProne && AntField.Match(Id.ToUpper()).Success)
                        return FieldType.Ant_kv;
                    return FieldType.Unset;
                default:
                    return FieldType.Unset;
            }
        }
        private VMS.TPS.Common.Model.Types.PatientOrientation Orientation = VMS.TPS.Common.Model.Types.PatientOrientation.NoOrientation;
        private Regex AntField = new Regex("ANT");
        private Regex PostField = new Regex("POST");
        private Regex LeftField = new Regex("LLAT");
        private Regex RightField = new Regex("RLAT");
        private Regex Bolus = new Regex("BOLUS");
        private Regex CBCT = new Regex("CBCT");
        private Regex Unload = new Regex("UNLOAD");
        public FieldType Type { get; private set; } = FieldType.Unset;
        public string TypeString
        {
            get { return Type.Display(); }
        }
        public string Id { get; set; } = "Default Field";
        public bool isMV { get; private set; } = false;
        public double GantryAngle { get; set; } = 0;
        public double CollimatorAngle { get; set; } = 0;
        public bool Warning { get; set; } = false;
        public string WarningMessage { get; set; } = "";
    }

}
