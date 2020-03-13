using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using wpfcolor = System.Windows.Media.Colors;

namespace SquintScript
{
    public class VisibilityStructureEditorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            StructureSelector V = value as StructureSelector;
            if (V != null)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? V = value as Visibility?;
            if (V == Visibility.Hidden)
                return false;
            else
                return true;
        }
    }
    public class ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            bool? V = value as bool?;
            if (V == true)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? V = value as Visibility?;
            if (V == Visibility.Hidden)
                return false;
            else
                return true;
        }
    }
    public class GetConstraintStructureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            StructureSelector SS = value as StructureSelector;
            if (SS != null)
                return SS;
            else
                return null;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            StructureSelector SS = value as StructureSelector;
            if (SS != null)
                return SS;
            else return null;

        }
    }
    public class WidthConverter : IValueConverter
    {
        public object Convert(object o, Type type, object parameter, CultureInfo culture)
        {
            ListView l = o as ListView;
            GridView g = l.View as GridView;
            double total = 0;
            for (int i = 0; i < g.Columns.Count - 1; i++)
            {
                total += g.Columns[i].ActualWidth;
            }
            return (l.ActualWidth - total);
        }

        public object ConvertBack(object o, Type type, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    public class VisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            bool? V = value as bool?;
            if (V == null)
                return Visibility.Collapsed;
            if (V == false)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? V = value as Visibility?;
            if (V == Visibility.Hidden)
                return false;
            else
                return true;
        }
    }
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            bool? V = value as bool?;
            if (V == null)
                return Visibility.Collapsed;
            if (V == true)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? V = value as Visibility?;
            if (V == Visibility.Hidden)
                return false;
            else
                return true;
        }
    }
    public class ColumnHeaderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            var C = (value as System.Windows.Controls.Primitives.DataGridColumnHeader).Column as SquintDataColumn;
            if (C != null)
            {
                if ((string)parameter == "Foreground")
                    return new SolidColorBrush(C.AV.TextColor);
                else
                    return new SolidColorBrush(System.Windows.Media.Colors.SteelBlue);
                //return new SolidColorBrush(C.AV.Color);
            }
            else
                return "White";
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }
    public class RowHeaderBackGroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            ConstraintSelector CS = value as ConstraintSelector;
            if (CS != null)
            {
                if (CS.ReferenceType == ReferenceTypes.Lower)
                    return new SolidColorBrush(Colors.IndianRed);
                else
                    return new SolidColorBrush(Colors.SteelBlue);
            }
            else
                return new SolidColorBrush(Colors.SteelBlue);
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }
    public class ItemToDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return "";
            else
            {
                var PS = value as ProtocolView.ProtocolSelector;
                switch (parameter)
                {
                    case "Centre":
                        return PS.TreatmentCentre.Display();
                    case "Site":
                        return PS.TreatmentSite.Display();
                    case "Type":
                        return PS.ProtocolType.Display();
                    case "Modified":
                        return PS.LastModifiedBy;
                    case "ApprovalLevel":
                        return PS.ApprovalLevel.Display();
                    default:
                        return "missing converter parameter";
                }
            }
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }
    public class FilterComboBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Enum)
                return (value as Enum).Display();
            else
                return "";

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }
    public class RowHeaderConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            ConstraintSelector CS = value[0] as ConstraintSelector;
            if (CS != null)
                return CS.FullConstraintDefinition;
            else
                return "";
        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TypeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
             object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ReferenceTypes)value)
            {
                case ReferenceTypes.Lower:
                    return true;
                case ReferenceTypes.Upper:
                    return false;
                default:
                    return false;
            }
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }
    public class TypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ReferenceTypes)value)
            {
                case ReferenceTypes.Lower:
                    return new SolidColorBrush(Colors.ForestGreen);
                case ReferenceTypes.Upper:
                    return new SolidColorBrush(Colors.Tomato);
                default:
                    return new SolidColorBrush(Colors.WhiteSmoke);
            }
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }
    public class TypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ReferenceTypes)value)
            {
                case ReferenceTypes.Lower:
                    return "Lower";
                case ReferenceTypes.Upper:
                    return "Upper";
                default:
                    return "?";
            }
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }

    public class ColumnHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            var C = (value as System.Windows.Controls.Primitives.DataGridColumnHeader).Column as SquintDataColumn;
            if (C != null)
                return C.Header;
            else
                return "";
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }
    public class ResultConverter2 : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            var AV = (value[0] as AssessmentView);
            var CS = (value[1] as ConstraintSelector);
            if (AV != null && CS != null)
            {
                if (CS.isResultCalculating(AV.AssessmentId))
                    return "Calculating..";
                else
                {
                    var test = CS.GetResult(AV.AssessmentId);
                    return CS.GetResult(AV.AssessmentId);
                }
            }
            else return "";


        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ResultConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            var Col = (value[1] as DataGridCell).Column as SquintDataColumn;
            var CS = (value[0] as ConstraintSelector);
            if (Col != null & CS != null)
            {
                if (CS.isResultCalculating(Col.AV.AssessmentId))
                    return "Calculating..";
                else
                {
                    var test = CS.GetResult(Col.AV.AssessmentId);
                    return CS.GetResult(Col.AV.AssessmentId);
                }
            }
            else
                return "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    //public class NulltoDisabledConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType,
    //          object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        if ((value as ICollection).Count > 0)
    //            return true;
    //        else
    //            return false;
    //    }
    //    public object ConvertBack(object value, Type targetTypes,
    //           object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return "";
    //    }
    //}

    public class DoubleToDisplay : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double)
            {
                double V = (double)value;
                if (double.IsNaN(V))
                    return "-";
                else
                    return V.ToString();
            }
            else
                return "-";

        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            double result;
            bool isDouble = Double.TryParse(value as string, out result);
            if (isDouble)
            {
                return result;
            }
            else
                return double.NaN;
        }
    }

    public class ExpanderConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //double result = 1.0;
            //for (int i = 0; i < values.Length; i++)
            //{
            //    if (values[i] is double)
            //        result *= (double)values[i];
            //}
            if (values.Count() > 1)
            {
                if (values[1] is int)
                    return ((double)values[0] * System.Convert.ToDouble(values[1]) * Double.Parse(parameter as string));
                else
                    return 0;
            }
            else
                return (double)values[0] * Double.Parse(parameter as string);

            //return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }
    }

    public class SelectionConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.Count() < 3)
            {
                if (value[1] is bool && value[0] is bool)
                    if ((bool)value[0] || (bool)value[1])
                        return true;
                    else
                        return false;
                else
                {
                    if (value[0] is bool)
                        return (bool)value[0];
                    else
                        return false;
                }
            }
            else
            {
                if (value[1] is bool && value[0] is bool && value[2] is bool)
                    if (((bool)value[0] || (bool)value[1]) & !(bool)value[2])
                        return true;
                    else
                        return false;
                else
                {
                    if (value[0] is bool)
                        return (bool)value[0];
                    else
                        return false;
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ConstraintSelectedHighlight : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            var selectedCS = (value[0] as ConstraintSelector);
            var rowCS = (value[1] as ConstraintSelector);
            if (selectedCS != null && rowCS != null)
                if (selectedCS.Id == rowCS.Id)
                    return new SolidColorBrush(Colors.Goldenrod);
                else
                    return new SolidColorBrush(Colors.Black);
            else
                return new SolidColorBrush(Colors.Black);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SortStructuresConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            StructureSelector SS = (value[0] as StructureSelector);
            ObservableCollection<Ctr.EclipseStructure> AvailableStructures = value[1] as ObservableCollection<Ctr.EclipseStructure>;
            if (SS == null)
            {
                return AvailableStructures.OrderBy(x => x.Id);
            }
            if (AvailableStructures.Count != 0)
            {
                double[] LD = new double[AvailableStructures.Count];
                for (int i = 0; i < AvailableStructures.Count; i++)
                {
                    LD[i] = double.PositiveInfinity;
                }
                int c = 0;
                foreach (string S in AvailableStructures.Select(x => x.Id))
                {
                    var CurrentId = SS.ES.Id.ToUpper();
                    var stripString = S.Replace(@"B_", @"").Replace(@"_", @"").ToUpper();
                    if (CurrentId == "") // if not assigned, use first alias
                    {
                        foreach (string Alias in SS.GetAliases())
                        {
                            var CompString = Alias.Replace(@"B_", @"").Replace(@"_", @"").ToUpper();
                            double LDist = LevenshteinDistance.Compute(stripString, CompString);
                            if (stripString.ToUpper().Contains(CompString) && stripString != "" && CompString !="")
                                LDist = Math.Min(LDist, 1.5);
                            LD[c] = Math.Min(LD[c], LDist);
                        }
                    }
                    else
                    {
                        var CompString = CurrentId.Replace(@"B_", @"").Replace(@"_", @"").ToUpper();
                        double LDist = LevenshteinDistance.Compute(stripString, CompString);
                        if (stripString.ToUpper().Contains(CompString) && stripString != "" && CompString != "")
                            LDist = Math.Min(LDist, 1.5);
                        LD[c] = LDist;
                    }
                    c++;
                }
                var temp = new ObservableCollection<Ctr.EclipseStructure>(AvailableStructures.Zip(LD, (s, l) => new { key = s, LD = l }).OrderBy(x => x.LD).Select(x => x.key).ToList());
                return temp;
            }
            else
                return AvailableStructures;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ResultFlagColorConverter2 : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            var AV = (value[0] as AssessmentView);
            var CS = (value[1] as ConstraintSelector);
            if (AV != null && CS != null)
            {
                if (CS.isResultCalculating(AV.AssessmentId))
                {
                    return new SolidColorBrush(Colors.LightSteelBlue);
                }
                List<ConstraintResultStatusCodes> StatusCodes = CS.GetStatusCodes(AV.AssessmentId);
                if (StatusCodes != null)
                {
                    bool ValidResult = true;
                    if (StatusCodes.Contains(ConstraintResultStatusCodes.LinkedPlanError))
                        ValidResult = false;
                    if (StatusCodes.Contains(ConstraintResultStatusCodes.ErrorUnspecified))
                        ValidResult = false;
                    if (StatusCodes.Contains(ConstraintResultStatusCodes.ConstraintUndefined))
                        ValidResult = false;
                    if (StatusCodes.Contains(ConstraintResultStatusCodes.NotLinked))
                        ValidResult = false;
                    if (StatusCodes.Contains(ConstraintResultStatusCodes.StructureEmpty))
                        ValidResult = false;
                    if (StatusCodes.Contains(ConstraintResultStatusCodes.StructureNotFound))
                        ValidResult = false;
                    if (StatusCodes.Contains(ConstraintResultStatusCodes.NoDoseDistribution))
                        ValidResult = false;
                    if (StatusCodes.Contains(ConstraintResultStatusCodes.RefDoseInValid))
                        ValidResult = false;
                    if (ValidResult == false)
                        return new SolidColorBrush(Colors.Transparent);
                    else
                        switch (CS.GetViolationStatus(AV.AssessmentId))
                        {
                            case ConstraintThresholdNames.MinorViolation:
                                return new SolidColorBrush(Colors.Orange);
                            case ConstraintThresholdNames.MajorViolation:
                                return new SolidColorBrush(Colors.Red);
                            case ConstraintThresholdNames.Stop:
                                return new SolidColorBrush(Colors.MediumSeaGreen);
                            case ConstraintThresholdNames.None:
                                return new SolidColorBrush(Colors.PaleGreen);
                            default:
                                return new SolidColorBrush(Colors.Transparent);
                        }
                }
                else
                    return new SolidColorBrush(Colors.Transparent);
            }
            else
                return new SolidColorBrush(Colors.Transparent);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ResultFlagColorConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            var Col = (value[1] as DataGridCell).Column as SquintDataColumn;
            var CS = (value[0] as ConstraintSelector);
            if (Col != null & CS != null)
            {
                if (Col.AV == null)
                    return new SolidColorBrush(Colors.Transparent);
                else
                {
                    if (CS.isResultCalculating(Col.AV.AssessmentId))
                    {
                        return new SolidColorBrush(Colors.LightSteelBlue);
                    }
                    List<ConstraintResultStatusCodes> StatusCodes = CS.GetStatusCodes(Col.AV.AssessmentId);
                    if (StatusCodes != null)
                    {
                        bool ValidResult = true;
                        if (StatusCodes.Contains(ConstraintResultStatusCodes.LinkedPlanError))
                            ValidResult = false;
                        if (StatusCodes.Contains(ConstraintResultStatusCodes.ErrorUnspecified))
                            ValidResult = false;
                        if (StatusCodes.Contains(ConstraintResultStatusCodes.ConstraintUndefined))
                            ValidResult = false;
                        if (StatusCodes.Contains(ConstraintResultStatusCodes.NotLinked))
                            ValidResult = false;
                        if (StatusCodes.Contains(ConstraintResultStatusCodes.StructureEmpty))
                            ValidResult = false;
                        if (StatusCodes.Contains(ConstraintResultStatusCodes.StructureNotFound))
                            ValidResult = false;
                        if (StatusCodes.Contains(ConstraintResultStatusCodes.NoDoseDistribution))
                            ValidResult = false;
                        if (StatusCodes.Contains(ConstraintResultStatusCodes.RefDoseInValid))
                            ValidResult = false;
                        if (ValidResult == false)
                            return new SolidColorBrush(Colors.Transparent);
                        else
                            switch (CS.GetViolationStatus(Col.AV.AssessmentId))
                            {
                                case ConstraintThresholdNames.MinorViolation:
                                    return new SolidColorBrush(Colors.Orange);
                                case ConstraintThresholdNames.MajorViolation:
                                    return new SolidColorBrush(Colors.Red);
                                case ConstraintThresholdNames.Stop:
                                    return new SolidColorBrush(Colors.MediumSeaGreen);
                                case ConstraintThresholdNames.None:
                                    return new SolidColorBrush(Colors.PaleGreen);
                                default:
                                    return new SolidColorBrush(Colors.Transparent);
                            }
                    }
                    else
                    {
                        return new SolidColorBrush(Colors.Transparent);
                    }
                }
            }
            else
                return new SolidColorBrush(Colors.Transparent);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StructureDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            StructureSelector V = value as StructureSelector;
            return string.Format("{0} (Label={1})", V.ES.Id, V.ES.LabelName);
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? V = value as Visibility?;
            if (V == Visibility.Collapsed)
                return false;
            else
                return true;
        }
    }
    public class EnumDisplayTypesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            var E = (value as Enum);
            if (E != null)
                //return (E.GetType().GetProperty((string)parameter).GetValue(E, null) as Enum).Display();
                return E.Display();
            else
                return "";

            //return (value as Enum).Display();
            //switch (value.GetType())
            //{
            //    case ConstraintUnits:
            //        re
            //}
            //if (U != null)
            //    return U.Display();
            //else
            //    return "";
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
    public class ShowConstraintEditor : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            bool? ItemSelected = value[0] as bool?;
            bool? MouseOverList = value[1] as bool?;
            if (ItemSelected == null || MouseOverList == null)
                return false;
            if ((bool)ItemSelected && (bool)MouseOverList)
                return true;
            else
                return false;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class CollimatorAngleCheckConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            if ((double)value == 0)
                return new SolidColorBrush(wpfcolor.Black);
            else
                return new SolidColorBrush(wpfcolor.Tomato);
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? V = value as Visibility?;
            if (V == Visibility.Hidden)
                return false;
            else
                return true;
        }
    }
    public class VisBoolToHeight : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            bool? V = value as bool?;
            if (V == true)
                return Double.NaN;
            else
                return 0;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility? V = value as Visibility?;
            if (V == Visibility.Hidden)
                return false;
            else
                return true;
        }
    }

    public class TestConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            var EclipseId = (value as string);
            if (EclipseId == null)
                return "";
            else
                return EclipseId;
        }
    }

    public class ScrollBarVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
                return System.Windows.Controls.ScrollBarVisibility.Auto;
            else
                return System.Windows.Controls.ScrollBarVisibility.Hidden;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            var EclipseId = (value as string);
            if (EclipseId == null)
                return "";
            else
                return EclipseId;
        }
    }

    public class Color2Brush : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                return new SolidColorBrush((Color)value);
            else
                return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RefreshListConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            return (value[0] as ConstraintSelector).GetStructureColors;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (bool v in value)
            {
                if (v)
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
