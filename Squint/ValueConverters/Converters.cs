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
using Squint.ViewModels;
using Squint.Extensions;
using Squint.Views;

namespace Squint.Converters
{
    public class VisibilityStructureEditorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            StructureViewModel V = value as StructureViewModel;
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
    public class VisibilityOptionalIconConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                bool? pass = (bool?)value[0];
                ParameterOptions status = (ParameterOptions)value[1];
                if (status == ParameterOptions.Optional && pass == null)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
            catch
            {
                MessageBox.Show(@"Error casting inputs to VisibilityOptionalIconConverter");
                return Visibility.Hidden;
                //throw new Exception(@"Error casting inputs to VisibilityOptionalIconConverter");
            }

        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double input))
            {
                if (int.TryParse(parameter.ToString(), out int precision))
                {
                    switch (precision)
                    {
                        case 0:
                            return string.Format("{0}", input);
                        case 1:
                            return string.Format("{0:0.#}", input);
                        case 2:
                            return string.Format("{0:0.##}", input);
                        case 3:
                            return string.Format("{0:0.###}", input);
                        default:
                            return input.ToString();
                    }
                }
                else
                    return string.Format("{0.###}", input);
            }
            else
                return "-";
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
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
            StructureViewModel SS = value as StructureViewModel;
            if (SS != null)
                return SS;
            else
                return null;
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            StructureViewModel SS = value as StructureViewModel;
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

    public class VisibilitySelectionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            if (value is Enum)
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

    public class VisibilityTextBoxModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return Visibility.Collapsed;
            if (value is Enum)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
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
            ConstraintViewModel CS = value as ConstraintViewModel;
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

    public class BusyToCursorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is bool))
                return System.Windows.Input.Cursors.Arrow;

            var isBusy = (bool)value;

            if (isBusy)
                return System.Windows.Input.Cursors.Wait;
            else
                return System.Windows.Input.Cursors.Arrow;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReferenceEditConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            bool? isChanged = value as bool?;
            if (isChanged != null)
            {
                if ((bool)isChanged)
                    return new SolidColorBrush(Colors.DarkOrange);
                else
                    return new SolidColorBrush(Colors.Black);
            }
            else
                return new SolidColorBrush(Colors.Black);
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }

    public class AdminColorModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
             object parameter, System.Globalization.CultureInfo culture)
        {
            bool? AdminMode = value as bool?;
            var paramInput = parameter as string;
            Color defaultColour = Colors.CornflowerBlue;
            if (!string.IsNullOrEmpty(paramInput))
                defaultColour = (Color)ColorConverter.ConvertFromString(parameter as string);
            if (AdminMode is null)
            {
                return new SolidColorBrush(defaultColour);
            }
            else
                if ((bool)AdminMode)
                return new SolidColorBrush(Colors.BurlyWood);
            else
                return new SolidColorBrush(defaultColour);
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }

    public class AdminColorModeTestListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
             object parameter, System.Globalization.CultureInfo culture)
        {
            bool? AdminMode = value as bool?;
            var paramInput = parameter as string;
            Color defaultColour = Colors.AliceBlue;
            if (!string.IsNullOrEmpty(paramInput))
                defaultColour = (Color)ColorConverter.ConvertFromString(parameter as string);
            if (AdminMode is null)
            {
                return new SolidColorBrush(defaultColour);
            }
            else
                if ((bool)AdminMode)
                return new SolidColorBrush(Colors.LightGoldenrodYellow);
            else
                return new SolidColorBrush(defaultColour);
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }

    public class AdminColorModeHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
             object parameter, System.Globalization.CultureInfo culture)
        {
            bool? AdminMode = value as bool?;
            var paramInput = parameter as string;
            Color defaultColour = Colors.AliceBlue;
            if (!string.IsNullOrEmpty(paramInput))
                defaultColour = (Color)ColorConverter.ConvertFromString(parameter as string);
            if (AdminMode is null)
            {
                return new SolidColorBrush(defaultColour);
            }
            else
                if ((bool)AdminMode)
                return new SolidColorBrush(Colors.DarkGoldenrod);
            else
                return new SolidColorBrush(defaultColour);
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return "";
        }
    }

    public class UnsetComboColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            var CS = value as ComponentViewModel;
            if (CS != null)
            {
                return new SolidColorBrush(Colors.AliceBlue);
            }
            else
                return new SolidColorBrush(Colors.DarkOrange);
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
                if (value is Enum)
                    return (value as Enum).Display();
                else
                    return "";
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
            ConstraintViewModel CS = value[0] as ConstraintViewModel;
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

    public class VisibilityEditModeConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.Length == 3)
                if (value[2] is bool)
                    if ((bool)value[2] == false)
                        return Visibility.Collapsed; // if editing is explicitly disabled, collapse control
            if (!(value[0] is EditTypes) || value[1] == null)
                return Visibility.Collapsed;
            EditTypes V = (EditTypes)value[0]; // determine the test edittype
            string controlName = (string)value[1];
            switch (V) // determine the control type.  if it matches the edittype, display it
            {
                case EditTypes.SingleSelection:
                    if (controlName == "SingleSelection") // not obvious how to avoid magic strings here with xaml
                        return Visibility.Visible;
                    else return Visibility.Collapsed;
                case EditTypes.SingleValue:
                    if (controlName == "SingleValue")
                        return Visibility.Visible;
                    else return Visibility.Collapsed;
                case EditTypes.SingleValueWithTolerance:
                    if (controlName == "SingleValueWithTolerance")
                        return Visibility.Visible;
                    else return Visibility.Collapsed;
                case EditTypes.RangeValues:
                    if (controlName == "RangeValues")
                        return Visibility.Visible;
                    else return Visibility.Collapsed;
                case EditTypes.AnyOfValues:
                    if (controlName == "AnyOfValues")
                        return Visibility.Visible;
                    else return Visibility.Collapsed;
                default:
                    return Visibility.Collapsed;
            }

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

    public class EditModeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
                if ((bool)value)
                    return new SolidColorBrush(Colors.PapayaWhip);
                else
                    return new SolidColorBrush(Colors.LightSteelBlue);
            else
                return new SolidColorBrush(Colors.LightSteelBlue);
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
            var CS = (value[1] as ConstraintViewModel);
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
            var CS = (value[0] as ConstraintViewModel);
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
                    return string.Format("{0:0.##}", V);
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
            double MinHeight = double.Parse(parameter as string);
            double AnimationVar = (double)values[0];
            switch (values.Count())
            {
                case 0:
                    return MinHeight;
                case 1:
                    return AnimationVar * MinHeight;
                case 2:
                    return AnimationVar * MinHeight;
                case 3:
                    double? HeightPerElement = double.Parse(values[1] as string);
                    int? NumElements = values[2] as int?;
                    if (HeightPerElement != null && NumElements != null)
                        return AnimationVar * Math.Max((double)NumElements * (double)HeightPerElement, MinHeight);
                    else return MinHeight;
                default:
                    return MinHeight;
            }
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
            var selectedCS = (value[0] as ConstraintViewModel);
            var rowCS = (value[1] as ConstraintViewModel);
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
            StructureViewModel SS = (value[0] as StructureViewModel);
            ObservableCollection<string> AvailableStructures = value[1] as ObservableCollection<string>;
            if (SS == null)
            {
                return AvailableStructures;
            }
            if (AvailableStructures.Count != 0)
            {
                double[] LD = new double[AvailableStructures.Count];
                for (int i = 0; i < AvailableStructures.Count; i++)
                {
                    LD[i] = double.PositiveInfinity;
                }
                int c = 0;
                foreach (string S in AvailableStructures)
                {
                    var CurrentId = SS.AssignedStructureId.ToUpper();
                    var stripString = S.Replace(@"B_", @"").Replace(@"_", @"").ToUpper();
                    if (CurrentId == "") // if not assigned, use first alias
                    {
                        foreach (string Alias in SS.Aliases)
                        {
                            var CompString = Alias.Replace(@"B_", @"").Replace(@"_", @"").ToUpper();
                            double LDist = LevenshteinDistance.Compute(stripString, CompString);
                            if (stripString.ToUpper().Contains(CompString) && stripString != "" && CompString != "")
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
                var temp = new ObservableCollection<string>(AvailableStructures.Zip(LD, (s, l) => new { key = s, LD = l }).OrderBy(x => x.LD).Select(x => x.key).ToList());
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
            var CS = (value[1] as ConstraintViewModel);
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
                            case ReferenceThresholdTypes.MinorViolation:
                                return new SolidColorBrush(Colors.Orange);
                            case ReferenceThresholdTypes.MajorViolation:
                                return new SolidColorBrush(Colors.Red);
                            case ReferenceThresholdTypes.Stop:
                                return new SolidColorBrush(Colors.MediumSeaGreen);
                            case ReferenceThresholdTypes.None:
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
            var CS = (value[0] as ConstraintViewModel);
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
                                case ReferenceThresholdTypes.MinorViolation:
                                    return new SolidColorBrush(Colors.Orange);
                                case ReferenceThresholdTypes.MajorViolation:
                                    return new SolidColorBrush(Colors.Red);
                                case ReferenceThresholdTypes.Stop:
                                    return new SolidColorBrush(Colors.MediumSeaGreen);
                                case ReferenceThresholdTypes.None:
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

    public class ConstraintStatusColourConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                switch ((string)value)
                {
                    case @"Modified":
                        return new SolidColorBrush(Colors.DarkOrange);
                    case @"New":
                        return new SolidColorBrush(Colors.ForestGreen);
                    default:
                        return new SolidColorBrush(Colors.DarkOrange);
                }
            }
            else return new SolidColorBrush(Colors.Transparent);
        }
        public object ConvertBack(object value, Type targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class StructureDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
               object parameter, System.Globalization.CultureInfo culture)
        {
            StructureViewModel V = value as StructureViewModel;
            return string.Format("{0} (Label={1})", V.AssignedStructureId, V.LabelName);
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
            if (value == null)
                return "";
            var E = value as Enum;
            if (E != null)
                return E.Display();
            var L = value as IEnumerable;
            if (L != null)
            {
                var newList = new List<string>();
                foreach (var v in L)
                {
                    E = v as Enum;
                    if (E != null)
                        newList.Add(E.Display());
                    else
                        newList.Add(v as string);
                }
                return newList;
            }
            else
                return value as string;

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

    //public class RefreshListConverter : IMultiValueConverter
    //{
    //    public object Convert(object[] value, Type targetType,
    //          object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return (value[0] as ConstraintSelector).GetStructureColors;
    //    }

    //    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class VisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType,
              object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                foreach (bool? v in value)
                {
                    if (v == true)
                        return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                string debugme = "hi";
                return Visibility.Collapsed;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
