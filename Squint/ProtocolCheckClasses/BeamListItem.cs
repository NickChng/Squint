using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using PropertyChanged;
using Squint.TestFramework;
using Squint.ViewModels;
using Squint.Extensions;
using Squint.Interfaces;

namespace Squint
{
    [AddINotifyPropertyChangedInterface]
    public class BeamListItem :ObservableObject
    {
        public event EventHandler FieldChanged;

        private TxFieldItem _Field;
        public TxFieldItem Field
        {
            get { return _Field; }
            set
            {
                _Field = value;
                if (_Field != null & RefBeam != null)
                {
                    RefreshTests();
                    NoFieldAssigned = false;
                    FieldChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public List<TxFieldItem> Fields { get; set; }
        private BeamViewModel RefBeam;

        public void RetireCheck()
        {
            RefBeam.ToRetire = true;
        }
        public ObservableCollection<string> Aliases { get; set; } = new ObservableCollection<string> { @"Field1" };
        public string FieldDescription { get; set; }
        public string BeamId { get { return Field.Id; } }
        public string ProtocolBeamName
        {
            get { return RefBeam.ProtocolBeamName; }
            set { RefBeam.ProtocolBeamName = value; }
        }
        public string Technique
        {
            get { return Field.TypeString; }
        }
        public string Energy
        {
            get { return Field.Energy.Display(); }
        }
        public string ToleranceTable
        {
            get { return Field.ToleranceTable; }
        }
        public double Xmin { get { return Field.Xmin; } }
        public double Ymin { get { return Field.Ymin; } }
        public double Xmax { get { return Field.Xmax; } }
        public double Ymax { get { return Field.Ymax; } }
        public double MU
        {
            get { return Field.MU; }
        }
        public double ColRotation { get { return Field.CollimatorAngle; } }
        public double MinColRotation { get; set; }
        public double MaxColRotation { get; set; }
        public double CouchRotation { get { return Field.CouchRotation; } }
        public double StartAngle { get { return Field.GantryStart; } }
        public double EndAngle { get { return Field.GantryEnd; } }
        public bool NoFieldAssigned { get; set; } = true;
        public string SelectedAlias { get; set; }
        public string NewAlias { get; set; }

        private SquintModel _model;
        public BeamListItem(BeamViewModel RefBeam_in, List<TxFieldItem> TxFields, SquintModel model)
        {
            _model = model;
            RefBeam = RefBeam_in;
            Aliases = RefBeam_in.EclipseAliases;
            Fields = TxFields;
            Field = null;
            
        }
        public void InitializeTests()
        {
            BeamTests.Tests.Clear();
            BeamGeometryInstance beamGeometry = new BeamGeometryInstance();
            if (Fields != null)
                foreach (string alias in RefBeam.EclipseAliases)
                    foreach (TxFieldItem F in Fields)
                        if (F.Id == alias)
                        {
                            Field = F;
                            beamGeometry = new BeamGeometryInstance(Field.GantryStart, Field.GantryEnd, Field.Trajectory, _model);
                            NoFieldAssigned = false;
                        }
            FieldDescription = string.Format(@"Protocol field ""{0}"" assigned to plan field:", RefBeam.ProtocolBeamName);

            // Populate Tests
            BeamTests.Tests.Add(new TestRangeItem<double?>(CheckTypes.MURange, null, RefBeam.MinMUWarning, RefBeam.MaxMUWarning, "MU outside normal range") { ParameterOption = ParameterOptions.Optional });
            BeamTests.Tests.Add(new TestContainsItem<Energies>(CheckTypes.ValidEnergies, Energies.Unset, RefBeam.ValidEnergies, "Not a valid energy") { ParameterOption = ParameterOptions.Required});
            BeamTests.Tests.Add(new TestContainsItem<BeamGeometryInstance>(CheckTypes.BeamGeometry, beamGeometry, RefBeam.ValidGeometries, "No valid geometry found") { ParameterOption = ParameterOptions.Required });
            BeamTests.Tests.Add(new TestValueItem<double?>(CheckTypes.CouchRotation, null, RefBeam.CouchRotation, new TrackedValue<double?>(1E-2), "Non-standard couch rotation"));
            switch (RefBeam.JawTracking_Indication.Value)
            {
                case ParameterOptions.Required:
                    BeamTests.Tests.Add(new TestValueItem<bool?>(CheckTypes.JawTracking, null, new TrackedValue<bool?>(true), null, "No tracking detected"));
                    break;
                case ParameterOptions.Optional:
                    BeamTests.Tests.Add(new TestValueItem<bool?>(CheckTypes.JawTracking, null, null, null, "No tracking detected"));
                    break;
                case ParameterOptions.None:
                    BeamTests.Tests.Add(new TestValueItem<bool?>(CheckTypes.JawTracking, null, new TrackedValue<bool?>(false), null, "No tracking detected"));
                    break;
            }
            BeamTests.Tests.Add(new TestValueItem<double?>(CheckTypes.MinMLCOffsetFromAxial, null, RefBeam.MinColRotation, new TrackedValue<double?>(1E-2), "Collimator less than minimum offset") { ParameterOption = ParameterOptions.Optional, Test = TestType.GreaterThan });
            BeamTests.Tests.Add(new TestValueItem<double?>(CheckTypes.MinimumXfieldSize, null, RefBeam.MinX, null, "X field too small") { Test = TestType.GreaterThan });
            BeamTests.Tests.Add(new TestValueItem<double?>(CheckTypes.MaximumXfieldSize, null, RefBeam.MaxX, null, "X field too large") { Test = TestType.LessThan });
            BeamTests.Tests.Add(new TestValueItem<double?>(CheckTypes.MinimumYfieldSize, null, RefBeam.MinY, null, "Y field too small") { Test = TestType.GreaterThan });
            BeamTests.Tests.Add(new TestValueItem<double?>(CheckTypes.MaximumYfieldSize, null, RefBeam.MaxY, null, "Y field too large") { Test = TestType.LessThan });
            BeamTests.Tests.Add(new TestValueItem<string>(CheckTypes.ToleranceTable, "", RefBeam.ToleranceTable, null, "Incorrect Tol Table"));
            if (Field != null)
                RefreshTests();
            //RaisePropertyChangedEvent(nameof(BeamTests));
        }
      
        private async void RefreshTests()
        {
            if (Field == null)
                return;
            // Add default checks:
            foreach (var Test in BeamTests.Tests)
            {
                switch (Test.CheckType)
                {
                    case CheckTypes.MURange:
                        Test.SetCheckValue(MU);
                        break;
                    case CheckTypes.ValidEnergies:
                        Test.SetCheckValue(Field.Energy);
                        break;
                    case CheckTypes.BeamGeometry:
                        var CheckBeam = new BeamGeometryInstance(Field.GantryStart, Field.GantryEnd, Field.Trajectory, _model);
                        Test.SetCheckValue(CheckBeam);
                        break;
                    case CheckTypes.CouchRotation:
                        Test.SetCheckValue(CouchRotation);
                        break;
                    case CheckTypes.JawTracking:
                        Test.SetCheckValue(Field.isJawTracking);
                        break;
                    case CheckTypes.MinMLCOffsetFromAxial:
                        double Offset = double.NaN;
                        if (ColRotation < 90)
                            Offset = ColRotation;
                        else if (ColRotation < 180)
                            Offset = (180 - ColRotation);
                        else if (ColRotation < 270)
                            Offset = (ColRotation - 180);
                        else
                            Offset = 360 - ColRotation;
                        Test.SetCheckValue(Offset);
                        break;
                    case CheckTypes.MinimumXfieldSize:
                        Test.SetCheckValue(Xmin);
                        break;
                    case CheckTypes.MinimumYfieldSize:
                        Test.SetCheckValue(Ymin);
                        break;
                    case CheckTypes.MaximumXfieldSize:
                        Test.SetCheckValue(Xmax);
                        break;
                    case CheckTypes.MaximumYfieldSize:
                        Test.SetCheckValue(Ymax);
                        break;
                    case CheckTypes.ToleranceTable:
                        Test.SetCheckValue(ToleranceTable);
                        break;
                }
            }

            await AddBolusCheck();

        }
        public async Task<bool> AddBolusCheck()
        {

            TestValueItem<double> BolusTest = null;
            foreach (var bolustest in BeamTests.Tests.Where(x => x.CheckType == CheckTypes.BolusHU).ToList())
                BeamTests.Tests.Remove(bolustest);
            foreach (var bolustest in BeamTests.Tests.Where(x => x.CheckType == CheckTypes.BolusThickness).ToList())
                BeamTests.Tests.Remove(bolustest);
            foreach (BolusDefinition B in RefBeam.Boluses)
            {
                int numBoluses = 0;
                foreach (TxFieldItem.BolusInfo BI in Field.BolusInfos)
                {
                    numBoluses++;
                    BolusTest = new TestValueItem<double>(CheckTypes.BolusHU, BI.HU, B.HU, B.ToleranceHU, @"HU deviation", "Bolus required", "No bolus in protocol");
                    BolusTest.OptionalNameSuffix = string.Format(@"(""{0}"")", BI.Id);
                    BolusTest.ParameterOption = B.Indication.Value;
                    BeamTests.Tests.Add(BolusTest);
                    //ThickCheck
                    var Thick = await _model.GetBolusThickness(Field.CourseId, Field.PlanId, BI.Id);
                    var ThickCheck = new TestValueItem<double>(CheckTypes.BolusThickness, Thick, B.Thickness, B.ToleranceThickness, null, null, null);
                    ThickCheck.OptionalNameSuffix = string.Format(@"(""{0}"") [cm]", BI.Id);
                    ThickCheck.ParameterOption = B.Indication.Value;
                    BeamTests.Tests.Add(ThickCheck);
                }
                if (numBoluses == 0 && B.Indication.Value == ParameterOptions.Required)
                {
                    var ThickCheck = new TestRangeItem<double?>(CheckTypes.BolusThickness, null, null, null, "Bolus Required");
                    BeamTests.Tests.Add(ThickCheck);
                }
            }
            return true;
        }
        public void AddToleranceTableCheck(string ProtocolTolTable)
        {
            //var Warning = false;
            //if (Field.ToleranceTable.ToUpper() != ProtocolTolTable.ToUpper())
            //    Warning = true;
            //var TolTable = new TestListItem(string.Format(@"Tolerance Table (""{0}"")", Field.Id), string.Format("{0:0} HU", Field.ToleranceTable), string.Format("{0:0} HU", ProtocolTolTable), Warning, "");
            //BeamTests.Tests.Add(TolTable);
        }
        public ICommand RemoveAliasCommand
        {
            get { return new DelegateCommand(RemoveAlias); }
        }
        public void RemoveAlias(object param = null)
        {
            string remAlias = param as string;
            if (remAlias != null)
                if (Aliases.Contains(remAlias))
                    Aliases.Remove(remAlias);
        }
        public ICommand AddAliasCommand
        {
            get { return new DelegateCommand(AddAlias); }
        }
        public void AddAlias(object param = null)
        {
            if (NewAlias != null)
                if (!Aliases.Contains(NewAlias))
                    Aliases.Add(NewAlias);
        }
        public TestList_ViewModel BeamTests { get; set; } = new TestList_ViewModel();
    }




}
