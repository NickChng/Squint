using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using PropertyChanged;
using wpfcolors = System.Windows.Media.Colors;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class PatientViewModel : ObservableObject
    {
        public MainViewModel ParentView { get; private set; } = null;

        private SquintModel _model;
        public string PatientId { get; set; } = "";
        public bool AutomaticStructureAliasingEnabled = true;
        public string FullPatientName { get; private set; }
        public System.Windows.Media.SolidColorBrush TextBox_Background_Color { get; set; } = new System.Windows.Media.SolidColorBrush(wpfcolors.AliceBlue);
        public ObservableCollection<CourseSelector> Courses { get; set; } = new ObservableCollection<CourseSelector>() { new CourseSelector() };
        public ObservableCollection<StructureSetSelector> StructureSets { get; set; } = new ObservableCollection<StructureSetSelector>() { new StructureSetSelector(null) };
        private StructureSetSelector _CurrentStructureSet;

        public bool CalculateOnUpdate = true;

        // Visibility toggles
        public bool isPIDVisible { get; set; } = true;
        public StructureSetSelector CurrentStructureSet
        {
            get
            {
                return _CurrentStructureSet;
            }
            set
            {
                if (_CurrentStructureSet != value)
                {
                    _CurrentStructureSet = value;
                    // Apply structure aliasing
                    if (value != null)
                    {
                        SetCurrentStructureSet(_CurrentStructureSet.StructureSetUID, AutomaticStructureAliasingEnabled, CalculateOnUpdate);

                    }
                }
            }
        }
        private async void SetCurrentStructureSet(string structureSetId, bool ApplyAliasing = true, bool CalculatOnStructureSetUpdate = true)
        {
            if (ParentView != null)
            {
                ParentView.isLoading = true;
                ParentView.LoadingString = "Applying structure aliases...";
                _model.SetCurrentStructureSet(_CurrentStructureSet.StructureSetUID);
                if (ApplyAliasing)
                    await Task.Run(() => _model.MatchStructuresByAlias());
                if (CalculatOnStructureSetUpdate)
                    _model.UpdateConstraints();
                ParentView.isLoading = false;
            }
        }

        public PatientViewModel(MainViewModel parentView, SquintModel model)
        {
            ParentView = parentView;
            _model = model;
            _model.PatientOpened += OnPatientOpened;
            _model.PatientClosed += OnPatientClosed;
            _model.CurrentStructureSetChanged += OnCurrentStructureSetChanged;
            _model.AvailableStructureSetsChanged += OnAvailableStructureSetsChanged;
        }

        private void OnPatientOpened(object sender, EventArgs e)
        {
            PatientId = _model.PatientID;
            FullPatientName = string.Format("{0}, {1}", _model.PatientLastName, _model.PatientFirstName);
            StructureSets.Clear();
            foreach (StructureSetHeader StS in _model.GetAvailableStructureSets())
            {
                StructureSets.Add(new StructureSetSelector(StS));
            }
            foreach (string CourseId in _model.GetCourseNames())
            {
                Courses.Add(new CourseSelector(CourseId));
            }
            TextBox_Background_Color = new System.Windows.Media.SolidColorBrush(wpfcolors.AliceBlue);
        }

        private void OnPatientClosed(object sender, EventArgs e)
        {
            FullPatientName = "";
            StructureSets.Clear();
        }

        private async void OnAvailableStructureSetsChanged(object sender, EventArgs e)
        {
            var A = _model.GetAvailableStructureSets();
            StructureSetSelector NewSSS = null;
            foreach (var SS in A) // Make new linked structure set available
            {
                if (!StructureSets.Select(x => x.StructureSetUID).Contains(SS.StructureSetUID))
                {
                    NewSSS = new StructureSetSelector(SS);
                    await Squint.App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        StructureSets.Add(NewSSS);
                    }));

                }
            }
            var ExistingStructureSets = new List<StructureSetSelector>(StructureSets);
            foreach (var SSS in ExistingStructureSets) // remove structure sets that are no longer linked
            {
                if (!A.Select(x => x.StructureSetUID).Contains(SSS.StructureSetUID))
                {
                    await Squint.App.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        StructureSets.Remove(SSS);
                    }));
                }
            }
        }
        private void OnCurrentStructureSetChanged(object sender, EventArgs e)
        {
            if (_model.CurrentStructureSet != null)
                _CurrentStructureSet = StructureSets.FirstOrDefault(x => x.StructureSetUID == _model.CurrentStructureSet.UID);
            else
                _CurrentStructureSet = null;
            RaisePropertyChangedEvent(nameof(CurrentStructureSet));
        }
    }
}
