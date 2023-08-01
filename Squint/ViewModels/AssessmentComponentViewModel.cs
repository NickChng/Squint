using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using PropertyChanged;
using System.Windows;
using Squint.Extensions;
using System.Threading.Tasks;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class AssessmentComponentViewModel : ObservableObject, IDisposable
    {
        public string ComponentName
        {
            get { return Comp.ComponentName; }
            set
            {
                if (value != Comp.ComponentName)
                {
                    Comp.ComponentName = value;
                }
            }
        }
        public bool Warning { get; set; } = false;
        public string WarningString { get; set; } = "";
        public bool EnableCourseSelection { get; set; } = true;
        public bool DisableAutomaticAssociation = false;
        private CourseSelector _SelectedCourse;
        public CourseSelector SelectedCourse
        {
            get { return _SelectedCourse; }
            set
            {
                RaisePropertyChangedEvent("EnableCourseSelection");
                if (_SelectedCourse != value && value != null) // refresh available plans
                {
                    _SelectedCourse = value;
                    EnableCourseSelection = false;
                    SetPlanIds(_SelectedCourse.CourseId);
                }

            }
        }
        private async void SetPlanIds(string CourseId)
        {
            try
            {
                ParentView.ParentView.ParentView.isLoading = true;
                ParentView.ParentView.ParentView.LoadingString = "Loading course plans...";
                List<PlanDescriptor> result = await _model.GetPlanDescriptors(_SelectedCourse.CourseId);
                ObservableCollection<PlanSelector> NewPlans = new ObservableCollection<PlanSelector>();
                foreach (var d in result.Where(x => x.Type == Comp.ComponentType.Value))
                {
                    NewPlans.Add(new PlanSelector(d.PlanId, d.PlanUID, CourseId, d.StructureSetUID, this));
                }
                Plans = NewPlans;
                ParentView.ParentView.ParentView.isLoading = false;
                ParentView.ParentView.ParentView.LoadingString = "";
                EnableCourseSelection = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0} {1} {2}", ex.Message, ex.InnerException, ex.StackTrace));
            }
        }
        private PlanSelector _SelectedPlan;
        public PlanSelector SelectedPlan
        {
            get { return _SelectedPlan; }
            set
            {
                if (value == null)
                {
                    if (!DisableAutomaticAssociation)
                    {
                        _model.ClearPlanAssociation(Comp.Id, A.ID);
                    }
                }
                if (value != _SelectedPlan)
                {
                    _SelectedPlan = value;
                    if (value != null)
                        SetPlanAsync();
                    RaisePropertyChangedEvent("SelectedPlan");
                }
            }
        }
        public bool IsSum
        {
            get
            {
                if (Comp.ComponentType.Value == ComponentTypes.Sum)
                    return true;
                else
                    return false;
            }
        }
        private SquintModel _model;
        private ComponentModel Comp;
        private AssessmentViewModel A;
        private AssessmentView ParentView;
        public ObservableCollection<CourseSelector> Courses { get; set; } = new ObservableCollection<CourseSelector>();
        public ObservableCollection<PlanSelector> Plans { get; set; } = new ObservableCollection<PlanSelector>();
        public AssessmentComponentViewModel(AssessmentView AV, ComponentModel CompIn, AssessmentViewModel Ain, SquintModel model)
        {
            Comp = CompIn;
            ParentView = AV;
            _model = model;
            Comp.PropertyChanged += UpdateStatus;
            _model.CurrentStructureSetChanged += UpdateStatus;
            A = Ain;
            var _P = _model.GetPlanAssociation(Comp.Id, A.ID); // check if plan is associated
            if (_P != null)
                Warning = _model.GetPlanAssociation(Comp.Id, A.ID).LoadWarning; // initialize warning 
            foreach (string CourseName in _model.GetCourseNames())
            {
                Courses.Add(new CourseSelector(CourseName));
            }

        }
        public void Dispose()
        {
            Comp.PropertyChanged -= UpdateStatus;
            _model.CurrentStructureSetChanged -= UpdateStatus;
        }
        private async void SetPlanAsync()
        {
            if (!DisableAutomaticAssociation)
            {
                try
                {
                    ParentView.ParentView.ParentView.isLoading = true;
                    ParentView.ParentView.ParentView.LoadingString = "Loading plan...";
                    var CSC = await _model.AssociatePlanToComponent(A.ID, Comp.Id, _SelectedCourse.CourseId, _SelectedPlan.PlanId, Comp.ComponentType.Value, true);
                    await Task.Run(() => _model.UpdateConstraints(Comp.Id, A.ID));
                    UpdateWarning(CSC);
                    ParentView.ParentView.ParentView.isLoading = false;
                    ParentView.ParentView.ParentView.LoadingString = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2})", ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
            ParentView.UpdateWarning();
        }
        private void UpdateStatus(object sender, EventArgs e)
        {
            if (Comp != null)
            {
                var CSC = A.StatusCodes(Comp);
                UpdateWarning(CSC);
            }
        }
        private void UpdateWarning(List<ComponentStatusCodes> CSC)
        {
            Warning = false;
            WarningString = "";
            if (CSC != null)
            {
                foreach (var status in CSC)
                {
                    if (status == ComponentStatusCodes.Evaluable)
                        continue;
                    else
                    {
                        Warning = true;
                        if (WarningString == "")
                            WarningString = status.Display();
                        else
                            WarningString = WarningString + System.Environment.NewLine + status.Display();
                    }
                }
                ParentView.UpdateWarning();
            }
        }
    }
}
