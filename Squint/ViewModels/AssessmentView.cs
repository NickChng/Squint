using System.Linq;
using System.Collections.ObjectModel;
using PropertyChanged;
using System.Windows;
using wpfcolor = System.Windows.Media.Color;
using Squint.Extensions;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class AssessmentView : ObservableObject
    {
        private SquintModel _model;
        public string ContentControlDataField { get; set; }
        public wpfcolor Color { get; set; }
        public wpfcolor TextColor { get; set; }
        public string TextDataField { get; set; }
        public string AssessmentName { get; set; }
        public bool Pinned { get; set; } = false;
        public bool Warning
        {
            get
            {
                foreach (var ACV in ACVs)
                {
                    if (ACV.Warning)
                        return true;
                }
                return false;
            }
        }
        public void UpdateWarning()
        {
            RaisePropertyChangedEvent("Warning"); // this to update the warning symbol in the parent view;
        }

        public int AssessmentId
        {
            get { return A.ID; }
        }
        public AssessmentsViewModel ParentView;
        private AssessmentViewModel A;
        public int ComponentCount
        {
            get { return ACVs.Count; }
        }
        public ObservableCollection<AssessmentComponentViewModel> ACVs { get; set; } = new ObservableCollection<AssessmentComponentViewModel>(); // only add to this from outside classes through AddACV, so ComponentCount is notified
        
        //public AssessmentView(string assessmentName)
        //{
        //    AssessmentName = assessmentName;
        //}
        public AssessmentView(wpfcolor Color_in, wpfcolor TextColor_in, AssessmentsViewModel ParentView_in, SquintModel model)
        {
            ParentView = ParentView_in;
            _model = model;
            Color = Color_in;
            TextColor = TextColor_in;
            if (!_model.PatientOpen)
            {
                MessageBox.Show("Please load patient first...");
                return;
            }
            A = _model.NewAssessment();
            AssessmentName = A.AssessmentName;
            foreach (Component Comp in _model.CurrentProtocol.Components.OrderBy(x=>x.DisplayOrder))
            {
                ACVs.Add(new AssessmentComponentViewModel(this, Comp, A, _model));
            }
        }
        public AssessmentView(AssessmentViewModel Ain, wpfcolor Color_in, wpfcolor TextColor_in, AssessmentsViewModel ParentView_in, SquintModel model)
        {
            A = Ain;
            _model = model;
            Color = Color_in;
            TextColor = TextColor_in;
            AssessmentName = Ain.AssessmentName;
            ParentView = ParentView_in;
            foreach (Component Comp in _model.CurrentProtocol.Components)
            {
                var ACV = new AssessmentComponentViewModel(this, Comp, A, _model);
                ACV.DisableAutomaticAssociation = true;
                var _P = _model.GetPlanAssociation(Comp.ID, A.ID);
                if (_P != null)
                {
                    ACV.WarningString = _P.LoadWarningString;
                    if (!_P.LoadWarning) // if there's a load warning (e.g can't find the file, don't set the combo boxes
                    {
                        ACV.SelectedCourse = ACV.Courses.FirstOrDefault(x => x.CourseId == _P.CourseId);
                        ACV.SelectedPlan = ACV.Plans.FirstOrDefault(x => x.PlanId == _P.PlanId);
                        foreach (ComponentStatusCodes code in _P.GetErrorCodes())
                        {
                            if (code == ComponentStatusCodes.Evaluable)
                                continue;
                            else
                            {
                                ACV.Warning = true;
                                ACV.WarningString = string.Join(System.Environment.NewLine, code.Display());
                            }
                        }
                    }
                    else
                        ACV.Warning = true;
                }
                ACV.DisableAutomaticAssociation = false;
                ACVs.Add(ACV);
            }

        }

        public void AddACV(AssessmentComponentViewModel ACV)
        {
            ACVs.Add(ACV);
            RaisePropertyChangedEvent(nameof(ComponentCount));
        }
        public void Delete()
        {
            foreach (var ACV in ACVs)
                ACV.Dispose();
            _model.RemoveAssessment(A.ID);
        }
    }
}
