using System;
using System.Linq;
using System.Data;
using System.Xml.Serialization;
using System.IO;

namespace Squint
{
     public partial class ConstraintModel
        {
            public class InterpolatedThreshold : IReferenceThreshold, ITrackedValue
            {
                public class ThresholdInterpolator
                {
                    public class XYPoint
                    {
                        [XmlAttribute]
                        public double X { get; set; }
                        [XmlAttribute]
                        public double Y { get; set; }
                    }
                    [XmlArray]
                    public XYPoint[] MajorDeviation { get; set; }
                    [XmlArray]
                    public XYPoint[] MinorDeviation { get; set; }
                    [XmlArray]
                    public XYPoint[] Stop { get; set; }
                }

                public ReferenceThresholdCalculationTypes ReferenceThresholdCalculationType { get; private set; } = ReferenceThresholdCalculationTypes.Interpolated;
                private double Interpolate(double Xi, double[] X, double[] Y)
                {
                    if (X.Length < 2)
                    {
                        return Y.FirstOrDefault();
                    }
                    else
                    {
                        double X1 = X[0];
                        double X2 = X[1];
                        double Y1 = Y[0];
                        double Y2 = Y[1];
                        int c = 2;
                        while (Xi >= X2 && c < X.Length)
                        {
                            X1 = X2;
                            X2 = X[c];
                            Y1 = Y2;
                            Y2 = Y[c];
                            c++;
                        }
                        double lever = (Xi - X1) / (X2 - X1);
                        return Math.Round(((Y2 - Y1) * lever + Y1) * 100) / 100;
                    }
                }

                public bool IsChanged
                {
                    get { return false; } // cannot change this threshold type
                }
                public double ReferenceValue
                {
                    get
                    {
                        if (MinorViolation != null)
                            return (double)_MinorViolation;
                        if (MajorViolation != null)
                            return (double)_MajorViolation;
                        else
                            return double.NaN;
                    }
                }
                public double? ReferenceMajorViolation { get { return _MajorViolation; } }
                public double? ReferenceMinorViolation { get { return _MinorViolation; } }
                public double? ReferenceStop { get { return _Stop; } }

                private double? _MajorViolation { get; set; }
                private double? _MinorViolation { get; set; }
                private double? _Stop { get; set; }
                public double? MajorViolation
                {
                    get
                    {
                        return _MajorViolation;
                    }
                    set { }
                }

                public double? MinorViolation { get { return _MinorViolation; } set { } }
                public double? Stop { get { return _Stop; } set { } }
                
                public InterpolatedThreshold(string dataPath, double? xi = null)
                {
                    DataPath = dataPath;

                    // Load data
                    XmlSerializer ser = new XmlSerializer(typeof(ThresholdInterpolator));
                    ThresholdInterpolator Data;
                    using (var datastream = new StreamReader(DataPath))
                    {
                        Data = (ThresholdInterpolator)ser.Deserialize(datastream);
                    }
                    if (Data != null)
                    {
                        if (Data.MajorDeviation != null)
                        {
                            XMajor = Data.MajorDeviation.OrderBy(x => x.X).Select(x => x.X).ToArray();
                            YMajor = Data.MajorDeviation.OrderBy(x => x.X).Select(x => x.Y).ToArray();
                        }
                        if (Data.MinorDeviation != null)
                        {
                            XMinor = Data.MinorDeviation.OrderBy(x => x.X).Select(x => x.X).ToArray();
                            YMinor = Data.MinorDeviation.OrderBy(x => x.X).Select(x => x.Y).ToArray();
                        }
                        if (Data.Stop != null)
                        {
                            XStop = Data.Stop.OrderBy(x => x.X).Select(x => x.X).ToArray();
                            YStop = Data.Stop.OrderBy(x => x.X).Select(x => x.Y).ToArray();
                        }
                    }
                    if (xi != null)
                    {
                        Xi = (double)xi;
                    }

                }

                public string DataPath { get; private set; }
                private double _Xi = double.NaN;
                private double[] XMajor;
                private double[] YMajor;
                private double[] XMinor;
                private double[] YMinor;
                private double[] XStop;
                private double[] YStop;
                public double Xi
                {
                    get { return _Xi; }
                    set
                    {
                        if (_Xi != value)
                        {
                            _Xi = value;
                            // recalc
                            if (XMajor != null && YMajor != null)
                                _MajorViolation = Interpolate(_Xi, XMajor, YMajor);
                            if (XMinor != null && YMinor != null)
                                _MinorViolation = Interpolate(_Xi, XMinor, YMinor);
                            if (XStop != null && YStop != null)
                                _Stop = Interpolate(_Xi, XStop, YStop);
                        }
                    }
                }
            }

        }

}

