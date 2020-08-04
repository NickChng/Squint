using System;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;
using PropertyChanged;
using System.Windows.Controls;
using System.Windows;
using wpfcolors = System.Windows.Media.Colors;
using wpfcolor = System.Windows.Media.Color;
using wpfbrush = System.Windows.Media.SolidColorBrush;
using Controls = SquintScript.Controls;
using SquintScript.ViewModelClasses;
using SquintScript.Interfaces;
using SquintScript.Extensions;
using System.Windows.Threading;
using Npgsql;

namespace SquintScript.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProtocolView : ObservableObject
    {
        public ProtocolView(Presenter parentView)
        {
            ParentView = parentView;
            AssessmentPresenter = new AssessmentsView(this);
            var ProtocolPreviews = Ctr.GetProtocolPreviewList();
            ChecklistViewModel = new Checklist_ViewModel(this);
            foreach (var PP in ProtocolPreviews)
            {
                Protocols.Add(new ProtocolSelector(PP));
            }
            // Subscribe to events
            Ctr.CurrentStructureSetChanged += UpdateAvailableStructureIds;
            Ctr.ProtocolListUpdated += UpdateProtocolList;
            Ctr.ProtocolConstraintOrderChanged += UpdateConstraintOrder;
        }
        // ViewModels
        public Presenter ParentView { get; set; }
        public ViewModels.Checklist_ViewModel ChecklistViewModel { get; set; }

        public void Unsubscribe()
        {
            Ctr.CurrentStructureSetChanged -= UpdateAvailableStructureIds;
            Ctr.ProtocolListUpdated -= UpdateProtocolList;
            Ctr.ProtocolConstraintOrderChanged -= UpdateConstraintOrder;
            ChecklistViewModel.Unsubscribe();

        }
        private Ctr.Protocol _P
        {
            get { return Ctr.GetActiveProtocol(); }
        }
        public ProtocolSelector SelectedProtocol { get; set; } = new ProtocolSelector(new Ctr.ProtocolPreview());

        public bool isProtocolLoaded { get; set; }

        public string Comments 
        { 
            get 
            {
                if (_P != null)
                    return _P.Comments;
                else
                    return "";
            } 
            set 
            {
                if (_P != null)
                    _P.Comments = value; 
            } 
        }

        public string ProtocolName
        {
            get
            {
                if (_P == null)
                    return "No protocol loaded";
                else
                    return _P.ProtocolName;
            }
            set
            {
                if (_P != null && value != null)
                {
                    _P.ProtocolName = value;
                }
            }
        }
        public ProtocolTypes ProtocolType
        {
            get
            {
                if (_P == null)
                    return ProtocolTypes.Unset;
                else
                    return _P.ProtocolType;
            }
            set
            {
                if (_P != null)
                {
                    _P.ProtocolType = value;
                }
            }
        }
        public TreatmentCentres TreatmentCentre
        {
            get
            {
                if (_P == null)
                    return TreatmentCentres.Unset;
                else
                    return _P.TreatmentCentre;
            }
            set
            {
                if (_P != null)
                {
                    _P.TreatmentCentre = value;
                }
            }
        }
        public TreatmentSites TreatmentSite
        {
            get
            {
                if (_P == null)
                    return TreatmentSites.Unset;
                else
                    return _P.TreatmentSite;
            }
            set
            {
                if (_P != null)
                {
                    _P.TreatmentSite = value;
                }
            }
        }
        public ApprovalLevels ApprovalLevel
        {
            get
            {
                if (_P == null)
                    return ApprovalLevels.Unset;
                else
                    return _P.ApprovalLevel;
            }
            set
            {
                if (_P != null)
                {
                    _P.ApprovalLevel = value;
                }
            }
        }


        public ObservableCollection<TreatmentCentres> CentreList { get; set; } = new ObservableCollection<TreatmentCentres>(Enum.GetValues(typeof(TreatmentCentres)).Cast<TreatmentCentres>());
        public ObservableCollection<TreatmentSites> SiteList { get; set; } = new ObservableCollection<TreatmentSites>(Enum.GetValues(typeof(TreatmentSites)).Cast<TreatmentSites>());
        public ObservableCollection<ApprovalLevels> ApprovalList { get; set; } = new ObservableCollection<ApprovalLevels>(Enum.GetValues(typeof(ApprovalLevels)).Cast<ApprovalLevels>());
        public ObservableCollection<ProtocolTypes> ProtocolTypeList { get; set; } = new ObservableCollection<ProtocolTypes>(Enum.GetValues(typeof(ProtocolTypes)).Cast<ProtocolTypes>());
        public string LastModifiedBy
        {
            get
            {
                if (_P == null)
                    return "";
                else
                    return _P.LastModifiedBy;
            }
        }

        public bool RefreshFlag { get; private set; } = false;
        public bool DragSelected { get; set; } = false;
        private int _SelectedIndex = -2;
        public int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                _SelectedIndex = value;
            }
        }

        public static readonly DependencyProperty IsDraggingProperty = DependencyProperty.RegisterAttached("IsDragging", typeof(bool), typeof(ProtocolView), new UIPropertyMetadata(false));

        public int DropIndex { get; set; } = -1;
        public StructureSelector SelectedStructure { get; set; }
        public int _ProgressPercentage;
        public int ProgressPercentage
        {
            get { return _ProgressPercentage; }
            set
            {
                _ProgressPercentage = value;
                //RaisePropertyChangedEvent();
            }
        }
        public Progress<int> Progress { get; set; }
        public ObservableCollection<StructureSelector> Structures { get; set; } = new ObservableCollection<StructureSelector>();
        public ObservableCollection<string> AvailableStructureIds { get; private set; } = new ObservableCollection<string>() { "No structure set" };
        public ObservableCollection<ProtocolSelector> Protocols { get; set; } = new ObservableCollection<ProtocolSelector>();
        public ObservableCollection<ComponentSelector> Components { get; set; } = new ObservableCollection<ComponentSelector>();
        public ObservableCollection<ConstraintSelector> Constraints { get; set; } = new ObservableCollection<ConstraintSelector>();
        public AssessmentsView AssessmentPresenter { get; set; } = new AssessmentsView(null);
        private ConstraintSelector _SelectedConstraint;
        public ConstraintSelector SelectedConstraint
        {
            get { return _SelectedConstraint; }
            set
            {
                if (value != _SelectedConstraint)
                {
                    _SelectedConstraint = value;
                }
            }
        }
        private void UpdateAvailableStructureIds(object sender, EventArgs e)
        {
            AvailableStructureIds = new ObservableCollection<string>(Ctr.GetCurrentStructures());
        }
        private void UpdateProtocolList(object sender, EventArgs e)
        {
            ObservableCollection<ProtocolSelector> UpdatedList = new ObservableCollection<ProtocolSelector>();
            foreach (Ctr.ProtocolPreview P in Ctr.GetProtocolPreviewList())
            {
                UpdatedList.Add(new ProtocolSelector(P));
            }
            Protocols = UpdatedList;
        }
        private void UpdateConstraintOrder(object sender, EventArgs e)
        {
            //ObservableCollection<ConstraintSelector> CSReorder = new ObservableCollection<ConstraintSelector>();
            //foreach (ConstraintSelector CS in Constraints.OrderBy(x => x.DisplayOrder))
            //    CSReorder.Add(CS);
            //for (int c = 0; c < CSReorder.Count; c++) // only re-bind collection if necessary
            //{
            //    if (CSReorder[c] != Constraints[c])
            //    {
            //        Constraints = CSReorder;
            //        break;
            //    }
            //}
        }
     
        public void AddConstraint()
        {
            if (Ctr.ProtocolLoaded)
            {
                var Con = Ctr.AddConstraint(ConstraintTypeCodes.Unset, Components.FirstOrDefault().Id, Structures.FirstOrDefault().Id);
                Constraints.Add(new ConstraintSelector(Con, Structures.FirstOrDefault()));
            }

        }

        public void AddStructure()
        {
            if (Ctr.ProtocolLoaded)
            {
                var E = Ctr.AddNewStructure();
                Structures.Add(new StructureSelector(E));
            }
        }

        public ICommand AddComponentCommand
        {
            get { return new DelegateCommand(AddComponent); }
        }

        private void AddComponent(object param = null)
        {
            if (Ctr.ProtocolLoaded)
            {
                var newComponent = Ctr.AddComponent();
                newComponent.DisplayOrder = Components.Count + 1;
                var CS = new ComponentSelector(newComponent);
                Components.Add(CS);
                foreach (var conS in Constraints.ToList())
                {
                   conS.Components.Add(CS);
                }
            }
        }

        public ICommand DeleteComponentCommand
        {
            get { return new DelegateCommand(DeleteComponent); }
        }

        private void DeleteComponent(object param)
        {
            if (Ctr.ProtocolLoaded)
            {
                if (Ctr.GetComponentList().Count > 1)
                {
                    var csToDelete = param as ComponentSelector;
                    var response = MessageBox.Show("Deleting this component will delete all associated constraints, continue?", "Delete component", MessageBoxButton.OKCancel);
                    if (response == MessageBoxResult.OK)
                    {
                        Ctr.DeleteComponent(csToDelete.Id);
                        Components.Remove(csToDelete);
                        foreach (var CS in Constraints.ToList())
                        {
                            if (CS.Component.Id == csToDelete.Id)
                                Constraints.Remove(CS);
                            else
                                CS.Components.Remove(CS.Components.FirstOrDefault(x => x.Id == csToDelete.Id));
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Protocol must have at least one component.", "Cannot delete");
                }
            }
        }

        public ICommand DeleteConstraintCommand
        {
            get { return new DelegateCommand(DeleteConstraint); }
        }
        private void DeleteConstraint(object param = null)
        {
            var CS = param as ConstraintSelector;
            if (CS != null)
            {
                if (CS.Id > 0)
                {
                    var D = MessageBox.Show("Delete this constraint?", "Confirm deletion", MessageBoxButton.OKCancel);
                    if (D == MessageBoxResult.OK)
                        Ctr.DeleteConstraint(CS.Id);
                }
                else
                    Ctr.DeleteConstraint(CS.Id);
                Constraints.Remove(CS);
            }
        }

        public void ViewLoadedProtocol()
        {
            UpdateProtocolView();
            isProtocolLoaded = true;
        }
        public ICommand LoadSelectedProtocolCommand
        {
            get { return new DelegateCommand(LoadSelectedProtocol); }
        }
        private async void LoadSelectedProtocol(object param)
        {
            if (ParentView.isLoading == true)
                return;
            var PS = (ProtocolSelector)param;
            if (PS.Id == 0)
                return; // no protocol selected;
            ParentView.LoadingString = "Loading selected protocol...";
            ParentView.isLoading = true;
            AssessmentPresenter = new AssessmentsView(this);
            ChecklistViewModel.Unsubscribe();
            ChecklistViewModel = new Checklist_ViewModel(this);
            await Task.Run(() => Ctr.LoadProtocolFromDb(PS.ProtocolName));
            UpdateProtocolView();
            isProtocolLoaded = true;
            ParentView.isLoading = false;
        }
        public void UpdateProtocolView()
        {
            RaisePropertyChangedEvent(nameof(ProtocolName));
            RaisePropertyChangedEvent(nameof(TreatmentCentre));
            RaisePropertyChangedEvent(nameof(TreatmentSite));
            RaisePropertyChangedEvent(nameof(ProtocolType));
            RaisePropertyChangedEvent(nameof(ApprovalLevel));
            RaisePropertyChangedEvent(nameof(Comments));
            List<StructureSelector> SSList = new List<StructureSelector>();
            List<ConstraintSelector> ConList = new List<ConstraintSelector>();
            List<ComponentSelector> CompList = new List<ComponentSelector>();
            foreach (var S in Ctr.GetStructureList().OrderBy(x => x.DisplayOrder))
            {
                var SS = new StructureSelector(S);
                SSList.Add(SS);
            }
            foreach (var C in Ctr.GetConstraints().OrderBy(x => x.DisplayOrder))
            {
                StructureSelector SS = SSList.FirstOrDefault(x => x.Id == C.PrimaryStructureID);
                ConstraintSelector CS = new ConstraintSelector(C, SS);
                ConList.Add(CS);
            }
            foreach (var Comp in Ctr.GetComponentList().OrderBy(x => x.DisplayOrder))
            {
                CompList.Add(new ComponentSelector(Comp));
            }
            Structures = new ObservableCollection<StructureSelector>(SSList);
            Components = new ObservableCollection<ComponentSelector>(CompList);
            Constraints = new ObservableCollection<ConstraintSelector>(ConList);
            AssessmentPresenter = new AssessmentsView(this);
            AssessmentPresenter.LoadAssessmentViews();
        }

        private async void UpdateProtocol(object param = null)
        {
            ParentView.LoadingString = "Validating changes...";
            bool areChangeDescriptionsComplete = true;
            List<string> IncompleteChangeDefinitions = new List<string>() { "Please enter Change Descriptions for the following modified constraints:" };
            foreach (ConstraintSelector CS in Constraints.Where(x => x.isModified == true))
            {
                if (CS.ChangeDescription == "")
                {
                    areChangeDescriptionsComplete = false;
                    IncompleteChangeDefinitions.Add(CS.ShortConstraintDefinition);
                }
            }
            if (!areChangeDescriptionsComplete)
            {
                MessageBox.Show(string.Join(Environment.NewLine, IncompleteChangeDefinitions));
                return;
            }

            ParentView.LoadingString = "Updating Protocol...";
            ParentView.isLoading = true;
            await Task.Run(() => Ctr.Save_UpdateProtocol());
            ParentView.isLoading = false;
            LoadSelectedProtocol(ProtocolName);
        }
        public ICommand UpdateProtocolCommand
        {
            get { return new DelegateCommand(UpdateProtocol); }
        }

        public ICommand DuplicateProtocolCommand
        {
            get { return new DelegateCommand(DuplicateProtocol); }
        }
        private void DuplicateProtocol(object param = null)
        {
            Ctr.Save_DuplicateProtocol();
            LoadSelectedProtocol(ProtocolName);
        }
        public ICommand DeleteSelectedProtocolCommand
        {
            get { return new DelegateCommand(DeleteSelectedProtocol); }
        }
        private async void DeleteSelectedProtocol(object param = null)
        {
            var PS = (param as ProtocolSelector);
            ParentView.LoadingString = "Deleting selected protocol...";
            ParentView.isLoading = true;
            if (PS != null)
            {
                var Result = MessageBox.Show(string.Format("Are you sure you want to delete this protocol ({0})", PS.ProtocolName), "Confirm deletion", MessageBoxButton.OKCancel);
                if (Result == MessageBoxResult.OK)
                    await Task.Run(() => Ctr.DeleteProtocol(PS.Id));
            }
            ParentView.isLoading = false;
        }

    }
}
