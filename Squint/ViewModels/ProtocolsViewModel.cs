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
using Controls = Squint.Views;
using Squint.TestFramework;
using Squint.Interfaces;
using Squint.Extensions;
using System.Windows.Threading;
using Npgsql;
using Prism.Events;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProtocolsViewModel : ObservableObject
    {
        private IEventAggregator ea;
        public ProtocolsViewModel() { }
        public ProtocolsViewModel(MainViewModel parentView, SquintModel model, IEventAggregator ea_in)
        {
            _model = model;
            ea = ea_in;
            Subscribe();

            ParentView = parentView;
            var ProtocolPreviews = new List<ProtocolSelector>();
            foreach (var PP in _model.GetProtocolPreviewList())
            {
                ProtocolPreviews.Add(new ProtocolSelector(PP));
            }
            Protocols = new ObservableCollection<ProtocolSelector>(ProtocolPreviews);
            ChecklistViewModel = new Checklist_ViewModel(this, _model, ea);
            // Subscribe to events
            _model.CurrentStructureSetChanged += UpdateAvailableStructureIds;
            _model.ProtocolListUpdated += UpdateProtocolList;
            _model.ProtocolClosed += Ctr_ProtocolClosed;
            _model.ProtocolOpened += Ctr_ProtocolOpened;
        }
        private void Subscribe()
        {
            //ea.GetEvent<ProtocolOpenedEvent>().Subscribe(OnProtocolOpened);
            //ea.GetEvent<ProtocolClosedEvent>().Subscribe(OnProtocolClosed);
        }

        public bool AdminOptionsToggle
        {
            get
            {
                if (ParentView == null)
                    return false;
                else
                    return ParentView.AdminOptionsToggle;
            }
        }
        private void Ctr_ProtocolOpened(object sender, EventArgs e)
        {
            UpdateProtocolView();
            isProtocolLoaded = true;
            isLoadProtocolPanelVisible = false;
        }

        public ICommand ExpandLoadProtocolCommand
        {
            get { return new DelegateCommand(ExpandLoadProtocol); }
        }

        private void ExpandLoadProtocol(object param = null)
        {
            isLoadProtocolPanelVisible = !isLoadProtocolPanelVisible;
        }

        private void Ctr_ProtocolClosed(object sender, EventArgs e)
        {
            isProtocolLoaded = false;
            UpdateProtocolView();
        }

        // ViewModels
        public MainViewModel ParentView { get; set; }
        private SquintModel _model;
        public Checklist_ViewModel ChecklistViewModel { get; set; }
        private Protocol _P
        {
            get
            {
                if (_model == null)
                    return null;
                else
                    return _model.CurrentProtocol;
            }
        }
        public ProtocolSelector SelectedProtocol { get; set; } = new ProtocolSelector(new ProtocolPreview());

        public bool isProtocolLoaded { get; set; }

        public bool isLoadProtocolPanelVisible { get; set; } = true;

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
                    return _P.TreatmentCentre.Value;
            }
            set
            {
                if (_P != null)
                {
                    _P.TreatmentCentre.Value = value;
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
                    return _P.TreatmentSite.Value;
            }
            set
            {
                if (_P != null)
                {
                    _P.TreatmentSite.Value = value;
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

        private int _SelectedIndex = 0;
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
        private int _SelectedStructureIndex = 0;
        public int SelectedStructureIndex
        {
            get
            {
                return _SelectedStructureIndex;
            }
            set
            {
                _SelectedStructureIndex = value;
            }
        }


        public StructureViewModel SelectedStructure { get; set; }
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
        public ObservableCollection<StructureViewModel> Structures { get; set; } = new ObservableCollection<StructureViewModel>();
        public int NumStructures { get { return Structures.Count; } }
        public ObservableCollection<string> AvailableStructureIds { get; private set; } = new ObservableCollection<string>() { "No structure set" };
        public ObservableCollection<ProtocolSelector> Protocols { get; set; } = new ObservableCollection<ProtocolSelector>();
        public ObservableCollection<ComponentModel> ComponentModels { get; set; } = new ObservableCollection<ComponentModel>();
        public int NumComponents { get { return ComponentModels.Count; } }
        public ObservableCollection<ConstraintViewModel> ConstraintViewModels { get; set; } = new ObservableCollection<ConstraintViewModel>();
        private ConstraintViewModel _SelectedConstraint;
        public ConstraintViewModel SelectedConstraint
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
            AvailableStructureIds = new ObservableCollection<string>(_model.GetCurrentStructures());
        }
        private void UpdateProtocolList(object sender, EventArgs e)
        {
            ObservableCollection<ProtocolSelector> UpdatedList = new ObservableCollection<ProtocolSelector>();
            foreach (ProtocolPreview P in _model.GetProtocolPreviewList())
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

        public void AddConstraint(int protocolStructureId = 0)
        {
            if (_model.ProtocolLoaded)
            {
                ConstraintModel con;
                if (protocolStructureId == 0 )
                    con = _model.AddConstraint(ConstraintTypes.Unset, ComponentModels.FirstOrDefault().Id, Structures.FirstOrDefault().Id);
                else
                    con = _model.AddConstraint(ConstraintTypes.Unset, ComponentModels.FirstOrDefault().Id, protocolStructureId);
                ConstraintViewModels.Add(new ConstraintViewModel(con, Structures.LastOrDefault(), _model));
                con.ConstraintFlaggedForDeletion += Con_ConstraintFlaggedForDeletion;
            }

        }

        private void Con_ConstraintFlaggedForDeletion(object sender, int ConId)
        {
            ConstraintModel Con = sender as ConstraintModel;
            var DeletedConstraint = ConstraintViewModels.FirstOrDefault(x => x.Id == ConId);
            if (DeletedConstraint != null)
                ConstraintViewModels.Remove(DeletedConstraint);
            Con.ConstraintFlaggedForDeletion -= Con_ConstraintFlaggedForDeletion;
        }

        public async void AddStructure()
        {
            if (_model.ProtocolLoaded)
            {
                var E = await _model.AddNewStructure();
                var newStructure = new StructureViewModel(E, _model, ea);
                Structures.Add(newStructure);
                AddConstraint(newStructure.Id);
            }
        }

        public ICommand AddComponentCommand
        {
            get { return new DelegateCommand(AddComponent); }
        }

        private void AddComponent(object param = null)
        {
            if (_model.ProtocolLoaded)
            {
                var newComponent = _model.AddComponent();
                newComponent.DisplayOrder = ComponentModels.Count + 1;
                ComponentModels.Add(newComponent);
                foreach (var conS in ConstraintViewModels.ToList())
                {
                    conS.ComponentModels.Add(newComponent);
                }
                foreach (var AV in ParentView.AssessmentsVM.Assessments)
                {
                    AV.AddACV(new AssessmentComponentViewModel(AV, newComponent, _model.GetAssessmentList().FirstOrDefault(x => x.ID == AV.AssessmentId), _model));
                }
            }
        }
        public ICommand ShowComponentCommand
        {
            get { return new DelegateCommand(ShowComponent); }
        }
        private void ShowComponent(object param = null)
        {
            var CS = (ComponentViewModel)param;
            CS.Pinned = !CS.Pinned;
        }
        public ICommand DeleteComponentCommand
        {
            get { return new DelegateCommand(DeleteComponent); }
        }

        private void DeleteComponent(object param)
        {
            if (_model.ProtocolLoaded)
            {
                if (_model.CurrentProtocol.Components.Count > 1)
                {
                    var csToDelete = param as ComponentModel;
                    var response = MessageBox.Show("Deleting this component will delete all associated constraints, continue?", "Delete component", MessageBoxButton.OKCancel);
                    if (response == MessageBoxResult.OK)
                    {
                        _model.DeleteComponent(csToDelete.Id);
                        ComponentModels.Remove(csToDelete);
                        foreach (var CS in ConstraintViewModels.ToList())
                        {
                            if (CS.Component.Id == csToDelete.Id)
                            {
                                _model.DeleteConstraint(CS.Id);
                                ConstraintViewModels.Remove(CS);
                            }
                            else
                                CS.ComponentModels.Remove(CS.ComponentModels.FirstOrDefault(x => x.Id == csToDelete.Id)); // remove this component as an option from the pull down of remaining constraints
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Protocol must have at least one component.", "Cannot delete");
                }
            }
        }

        public ICommand AddConstraintCommand
        {
            get { return new DelegateCommand(AddConstraint); }
        }
        public ICommand AddStructureCommand
        {
            get { return new DelegateCommand(AddStructure); }
        }

        public ICommand DeleteStructureCommand
        {
            get { return new DelegateCommand(DeleteStructure); }
        }

        private void AddConstraint(object param = null)
        {
            AddConstraint(0);
        }
        private void AddStructure(object param = null)
        {
            AddStructure();
        }

        private void DeleteStructure(object param)
        {
            if (_model.ProtocolLoaded)
            {
                if (_model.CurrentProtocol.Structures.Count > 1)
                {
                    var ssToDelete = param as StructureViewModel;
                    var response = MessageBox.Show("Confirm deletion of this structure? Deleting this structure will delete all of its constraints", "Delete structure", MessageBoxButton.OKCancel);
                    if (response == MessageBoxResult.OK)
                    {
                        foreach (var CS in ConstraintViewModels.ToList())
                        {
                            if (CS.SS.Id == ssToDelete.Id)
                            {
                                _model.DeleteConstraint(CS.Id);
                                ConstraintViewModels.Remove(CS);
                            }
                        }
                        _model.DeleteStructure(ssToDelete.Id);
                        Structures.Remove(ssToDelete);
                    }
                }
            }
        }

        public ICommand DeleteConstraintCommand
        {
            get { return new DelegateCommand(DeleteConstraint); }
        }
        private void DeleteConstraint(object param = null)
        {
            var CS = param as ConstraintViewModel;
            if (CS != null)
            {
                if (CS.Id > 0)
                {
                    var D = MessageBox.Show("Delete this constraint?", "Confirm deletion", MessageBoxButton.OKCancel);
                    if (D == MessageBoxResult.OK)
                    {
                        _model.DeleteConstraint(CS.Id);
                        ConstraintViewModels.Remove(CS);
                    }
                }
                else
                {
                    _model.DeleteConstraint(CS.Id);
                    ConstraintViewModels.Remove(CS);
                }
            }
        }

        public ICommand GetConstraintInformationCommand
        {
            get { return new DelegateCommand(GetConstraintInformation); }
        }
        public void GetConstraintInformation(object param = null)
        {
            var CS = (ConstraintViewModel)param;
            CS.ConstraintInfoVisibility ^= true;
        }
        public ICommand PinConstraintCommand
        {
            get { return new DelegateCommand(PinConstraintDetails); }
        }
        private void PinConstraintDetails(object param = null)
        {
            var CS = (ConstraintViewModel)param;
            CS.Pinned = !CS.Pinned;
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
            if (_model.ProtocolLoaded)
                _model.CloseProtocol();
            var PS = (ProtocolSelector)param;
            if (PS.Id == 0)
                return; // no protocol selected;
            ParentView.LoadingString = "Loading selected protocol...";
            ParentView.isLoading = true;
            if (await Task.Run(() => _model.LoadProtocolFromDb(PS.ProtocolName)))
            {
                //UpdateProtocolView();
                isProtocolLoaded = true;
                if (_model.PatientOpen)
                {
                    ParentView.AssessmentsVM.AddAssessment();
                }
                isLoadProtocolPanelVisible = false;
                ParentView.isLoading = false;

            }
            else
                MessageBox.Show("Problem loading protocol.");
        }
        public void UpdateProtocolView()
        {
            RaisePropertyChangedEvent(nameof(ProtocolName));
            RaisePropertyChangedEvent(nameof(TreatmentCentre));
            RaisePropertyChangedEvent(nameof(TreatmentSite));
            RaisePropertyChangedEvent(nameof(ProtocolType));
            RaisePropertyChangedEvent(nameof(ApprovalLevel));
            RaisePropertyChangedEvent(nameof(Comments));
            foreach (var CS in ConstraintViewModels)
            {
                CS.Unsubscribe();
            }
            foreach (var SS in Structures)
            {
                SS.Unsubscribe();
            }
            List<StructureViewModel> SSList = new List<StructureViewModel>();
            List<ConstraintViewModel> ConList = new List<ConstraintViewModel>();
            List<ComponentModel> CompList = new List<ComponentModel>();
            Structures = new ObservableCollection<StructureViewModel>();
            ComponentModels = new ObservableCollection<ComponentModel>();
            ConstraintViewModels = new ObservableCollection<ConstraintViewModel>();
            if (_model.ProtocolLoaded)
            {
                foreach (var S in _model.CurrentProtocol.Structures.OrderBy(x => x.DisplayOrder))
                {
                    var SS = new StructureViewModel(S, _model, ea);
                    SSList.Add(SS);
                }
                foreach (var C in _model.GetAllConstraints().OrderBy(x => x.DisplayOrder))
                {
                    StructureViewModel SS = SSList.FirstOrDefault(x => x.Id == C.PrimaryStructureId);
                    ConstraintViewModel CS = new ConstraintViewModel(C, SS, _model);
                    ConList.Add(CS);
                }
                foreach (var Comp in _model.CurrentProtocol.Components.OrderBy(x => x.DisplayOrder))
                {
                    CompList.Add(Comp);
                }
                Structures = new ObservableCollection<StructureViewModel>(SSList);
                ComponentModels = new ObservableCollection<ComponentModel>(CompList);
                ConstraintViewModels = new ObservableCollection<ConstraintViewModel>(ConList);
                //ParentView.AssessmentsVM = new AssessmentsView(ParentView);
                ParentView.AssessmentsVM.LoadAssessmentViews();
            }
            else
                isLoadProtocolPanelVisible = true;
        }

        private async void UpdateProtocol(object param = null)
        {
            ParentView.LoadingString = "Validating changes...";
            bool areChangeDescriptionsComplete = true;
            List<string> IncompleteChangeDefinitions = new List<string>() { "Please enter Change Descriptions for the following modified constraints:" };
            foreach (ConstraintViewModel CS in ConstraintViewModels.Where(x => x.isModified == true))
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
            await Task.Run(() => _model.Save_UpdateProtocol());
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
            _model.Save_DuplicateProtocol();
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
                    await Task.Run(() => _model.DeleteProtocol(PS.Id));
            }
            ParentView.isLoading = false;
        }

    }
}
