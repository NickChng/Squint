using Prism.Events;
using PropertyChanged;
using Squint.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using wpfcolor = System.Windows.Media.Color;
using wpfcolors = System.Windows.Media.Colors;

namespace Squint.ViewModels
{

    [AddINotifyPropertyChangedInterface]
    public class ProtocolSelector : ObservableObject
    {
        private ProtocolPreview _pp;
        public ProtocolSelector(ProtocolPreview pp)
        {
            _pp = pp;
            //Ctr.ProtocolListUpdated += OnProtocolListUpdated;
        }

        public int Id { get { return _pp.ID; } }
        public string ProtocolName { get { return _pp.ProtocolName; } }

        public TreatmentCentres TreatmentCentre { get { return _pp.TreatmentCentre; } }
        public TreatmentSites TreatmentSite { get { return _pp.TreatmentSite; } }

        public string LastModifiedBy { get { return _pp.LastModifiedBy; } }
        public ProtocolTypes ProtocolType { get { return _pp.ProtocolType; } }
        public ApprovalLevels ApprovalLevel { get { return _pp.Approval; } }

    }
    [AddINotifyPropertyChangedInterface]
    public class StructureViewModel : ObservableObject
    {
        private IEventAggregator ea;
        public bool DragSelected { get; set; } = false;
        private SquintModel _model;
        private StructureModel E;
        public int Id
        {
            get { return E.ID; }
        }
        public bool isUserAdded
        {
            get
            {
                if (E.ID < 0)
                    return true;
                else
                    return false;
            }
        }
        public string ProtocolStructureName
        {
            get { return E.ProtocolStructureName; }
            set
            {
                if (value.Length > 1)
                {
                    E.ProtocolStructureName = value;
                    RaisePropertyChangedEvent("StructureId");
                }
                else MessageBox.Show("Structure name must be greater than 1 character");
            }
        }
        public bool Pinned { get; set; } = false;
        public string AssignedStructureId
        {
            get { return E.AssignedStructureId; }
            set
            {
                if (value != null)
                {
                    E.AssignedStructureId = value;
                    if (isUserAdded)
                        ProtocolStructureName = value;
                    RaisePropertyChangedEvent(nameof(this.StructureColor));
                    RaisePropertyChangedEvent(nameof(this.LabelName));
                    _model.UpdateConstraints(E);
                    _model.UpdateConstraintThresholds(E);
                    ea.GetEvent<StructureEclipseIdAssigmentChanged>().Publish(new StructureEclipseIdAssignmentChangedArgs { EclipseStructureId = value, ProtocolStructureId = ProtocolStructureName });
                }
            }
        }
        public ObservableCollection<string> Aliases { get; set; }
        public string LabelName
        {
            get
            {
                if (_model.CurrentStructureSet != null)
                    return E.EclipseStructureLabel(_model.CurrentStructureSet.UID);
                else
                    return "";
            }
        }

        public int DisplayOrder
        {
            get { return E.DisplayOrder; }
            set { E.DisplayOrder = value; }
        }
        public StructureLabel StructureLabel
        {
            get { return E.StructureLabel; }
            set
            {
                if (value != null)
                {
                    E.StructureLabel = value;
                    RaisePropertyChangedEvent(nameof(AlphaBetaRatio));
                }
            }
        }
        public string AlphaBetaRatio
        {
            get
            {
                if (E.AlphaBetaRatioOverride != null)
                    return string.Format(@"({0}\{1} = {2:0})", '\u03B1', '\u03B2', E.AlphaBetaRatioOverride);
                else
                {
                    if (E.StructureLabel.AlphaBetaRatio > 0.1)
                        return string.Format(@"({0}\{1} = {2:0})", '\u03B1', '\u03B2', E.StructureLabel.AlphaBetaRatio);
                    else
                        return string.Format(@"(No BED adjustment)");
                }
            }
        }

        public double AlphaBetaRatioOverride
        {
            get
            {
                if (E.AlphaBetaRatioOverride == null)
                    return E.StructureLabel.AlphaBetaRatio;
                else
                    return (double)E.AlphaBetaRatioOverride;
            }
            set
            {
                E.AlphaBetaRatioOverride = value;
                RaisePropertyChangedEvent(nameof(AlphaBetaRatio));
            }
        }
        public bool LabelIsConsistent
        {
            get
            {
                if (_model.CurrentStructureSet == null)
                    return false;
                else
                    if (E.EclipseStructureLabel(_model.CurrentStructureSet.UID).ToUpper() == E.StructureLabel.LabelName.ToUpper() || E.AssignedStructureId == "")
                {
                    return false;
                }
                else
                    return true;
            }
        }
        public string LabelMismatchTooltip
        {
            get
            {
                if (_model.CurrentStructureSet == null)
                    return "";
                else
                    return string.Format("Label mismatch: Assigned structure label is {0}", E.EclipseStructureLabel(_model.CurrentStructureSet.UID));
            }
        }
        public StructureViewModel(StructureModel Ein, SquintModel model, IEventAggregator ea_in)
        {
            //Must unsubscribe when cleared!
            _model = model;
            ea = ea_in;
            E = Ein;
            Aliases = E.DefaultEclipseAliases;
            E.PropertyChanged += OnProtocolStructureChanged;
            _LabelFilterText = "";
            _source = new ObservableCollection<StructureLabel>(_model.GetStructureLabels());
        }
        public void Unsubscribe()
        {
            E.PropertyChanged -= OnProtocolStructureChanged;
        }
        public string NewAlias { get; set; }
        public int SelectedAliasIndex { get; set; }

        private string _LabelFilterText;
        public string LabelFilterText
        {
            get { return _LabelFilterText; }
            set
            {
                _LabelFilterText = value;
                RaisePropertyChangedEvent(nameof(LimitedSource));
            }
        }
        private ObservableCollection<StructureLabel> _source;
        public IEnumerable<StructureLabel> LimitedSource
        {
            get
            {
                return _source.Where(x => x.LabelName.ToLower().Contains(_LabelFilterText.ToLower())).Take(10);
            }
        }

        public ICommand AddStructureAliasCommand
        {
            get { return new DelegateCommand(AddStructureAlias); }
        }
        private void AddStructureAlias(object param = null)
        {
            _model.AddStructureAlias(Id, NewAlias);
            RaisePropertyChangedEvent(nameof(Aliases));
        }
        public ICommand RemoveStructureAliasCommand
        {
            get { return new DelegateCommand(RemoveStructureAlias); }
        }
        private void RemoveStructureAlias(object param = null)
        {
            _model.RemoveStructureAlias(Id, param as string);
            RaisePropertyChangedEvent(nameof(Aliases));
        }
        public ICommand PinOptionsCommand
        {
            get { return new DelegateCommand(PinOptions); }
        }
        private void PinOptions(object param = null)
        {
            Pinned ^= true;
        }

        public ICommand EditStructureCommand
        {
            get { return new DelegateCommand(EditStructure); }
        }

        public bool EditStructureVisibility { get; set; }
        private void EditStructure(object param = null)
        {
            EditStructureVisibility ^= true;
        }
        public wpfcolor? StructureColor
        {
            get
            {
                if (_model.CurrentStructureSet != null)
                {
                    return E.GetStructureColor(_model.CurrentStructureSet.UID);
                }
                else return null;
            }

        }
        private void OnProtocolStructureChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChangedEvent(e.PropertyName);
            if (e.PropertyName == nameof(StructureModel.AssignedStructureId))
            {
                RaisePropertyChangedEvent(nameof(StructureColor)); // this is for when the Assigned Eclipse structure itself is changed
                RaisePropertyChangedEvent(nameof(LabelIsConsistent)); // this is for when the Assigned Eclipse structure itself is changed
                RaisePropertyChangedEvent(nameof(LabelMismatchTooltip));
                RaisePropertyChangedEvent(nameof(LabelName));
            }
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class ComponentViewModel : ObservableObject
    {

        private SquintModel _model;
        private bool isSelected { get; set; }

        public bool isCreated { get { return Id < 0; } }
        public bool Pinned { get; set; } = false;
        public int DisplayHeight { get; } = 100;
        private ComponentModel _comp;
        public int Id
        {
            get { return _comp.Id; }
        }
        public string ComponentName
        {
            get { return _comp.ComponentName; }
            set { _comp.ComponentName = value; }
        }
        public ComponentTypes ComponentType
        {
            get { return _comp.ComponentType.Value; }
            set { _comp.ComponentType.Value = value; }
        }
        public double ReferenceDose
        {
            get { return _comp.TotalDose; }
            set
            {
                SetReferenceDose(value);
                RaisePropertyChangedEvent("ComponentTDFDescription"); // Fody will update the primary property, this is to update the description
            }
        }

        private int _numFractions;
        public int NumFractions
        {
            get { return _numFractions; }
            set
            {
                _numFractions = value; // necessary because the await in SetFractions may result in the UI retrieving Comp.NumFractions before it has been updated
                SetFractions(value);
                RaisePropertyChangedEvent("ComponentTDFDescription"); // Fody will update the primary property, this is to update the description
            }
        }
        public string ComponentTDFDescription
        {
            get { return string.Format("({0:0.#} Gy in {1} fractions)", ReferenceDose / 100, NumFractions); }
        }
        public ComponentViewModel(ComponentModel Compin, SquintModel model)
        {
            _model = model;
            _comp = Compin;
            _numFractions = _comp.NumFractions;
            AvailableComponentTypes.Clear();
            foreach (ComponentTypes T in Enum.GetValues(typeof(ComponentTypes)))
            {
                AvailableComponentTypes.Add(T);
            }
        }
        private async void SetFractions(int NumFractions)
        {
            _comp.NumFractions = NumFractions;
            await Task.Run(() => _model.UpdateConstraints(_comp.Id, null));
        }
        private async void SetReferenceDose(double dose)
        {
            _comp.TotalDose = dose;
            await Task.Run(() => _model.UpdateConstraints(_comp.Id, null));
        }
        public ObservableCollection<ComponentTypes> AvailableComponentTypes { get; set; } = new ObservableCollection<ComponentTypes>() { ComponentTypes.Phase };
    }
    [AddINotifyPropertyChangedInterface]
    public class AssignmentSelector
    {
        public int ComponentId { get; set; }
        public int PlanId { get; set; }
    }

    [AddINotifyPropertyChangedInterface]
    public class PlanSelector
    {
        public string PlanId { get; private set; } = "";
        public string PlanUID { get; private set; } = "";
        public string StructureSetUID { get; private set; } = "";
        public string CourseId { get; private set; } = "";
        public PlanTypes PlanType { get; set; } = PlanTypes.Unset;

        public PlanSelector(string planId = "", string planUID = "", string courseId = "", string structureSetUID = "", AssessmentComponentViewModel ACVinit = null)
        {
            PlanId = planId;
            StructureSetUID = structureSetUID;
            PlanUID = planUID;
            CourseId = courseId;
            ACV = ACVinit;
        }
        public AssessmentComponentViewModel ACV = null;
        public OptimizationCheckViewModel VM = new OptimizationCheckViewModel();
    }
    [AddINotifyPropertyChangedInterface]
    public class CourseSelector
    {
        public string CourseId { get; set; }
        public CourseSelector(string courseId = "")
        {
            CourseId = courseId;
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class StructureSetSelector
    {
        private StructureSetHeader _StS = null;
        public string StructureSetId
        {
            get
            {
                if (_StS == null)
                    return "";
                else
                    return _StS.StructureSetId;
            }
        }
        public string StructureSetUID
        {
            get
            {
                if (_StS == null)
                    return "";
                else
                    return _StS.StructureSetUID;
            }
        }

        public string ComboBoxDisplay
        {
            get
            {
                return string.Format("{0} ({1})", StructureSetId, LinkedPlanId);
            }
        }

        public string LinkedPlanId
        {
            get
            {
                return _StS.LinkedPlanId;
            }
        }
        public StructureSetSelector(StructureSetHeader StS = null)
        {
            _StS = StS;
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {

        private IEventAggregator ea;

        public MainViewModel()
        {
            ea = new EventAggregator();
            _model = new SquintModel();
        }


        public async void InitializeModel()
        {
            _model.DatabaseCreating += Model_DatabaseCreating;
            _model.ESAPIInitializing += Model_ESAPIInitializing;
            _model.DatabaseInitializing += Model_DatabaseInitializing;
            await Task.Run(() => _model.InitializeModel());
            ModelInitialized();
        }
        public void Dispose()
        {
            _model.Dispose();
        }
        private async void ModelInitialized()
        {
            PatientVM = new PatientViewModel(this, _model);
            StartupVM.InitializingMessages = "Loading protocol list...";
            await Task.Run(() =>
            {
                ProtocolsVM = new ProtocolsViewModel(this, _model, ea);
                SessionsVM = new SessionsViewModel(this, _model);
                AssessmentsVM = new AssessmentsViewModel(this, _model);
                SquintIsInitializing = false;
            });
        }

        private void Model_DatabaseCreating(object sender, EventArgs e)
        {
            StartupVM.InitializingMessages = "Preparing Squint database for first use...";
        }

        private void Model_DatabaseInitializing(object sender, EventArgs e)
        {
            StartupVM.InitializingMessages = "Connecting to Squint database...";
        }

        private void Model_ESAPIInitializing(object sender, EventArgs e)
        {
            StartupVM.InitializingMessages = "Starting ESAPI...";
        }

        private SquintModel _model = new SquintModel();
        public LoadingViewModel Loading_ViewModel { get; set; } = new LoadingViewModel();
        public ProtocolsViewModel ProtocolsVM { get; set; }
        public PatientViewModel PatientVM { get; set; }
        public SessionsViewModel SessionsVM { get; set; }
        public AssessmentsViewModel AssessmentsVM { get; set; }
        public EclipseProtocolPopupViewModel EclipseProtocolPopupVM { get; set; }

        public StartupViewModel StartupVM { get; set; } = new StartupViewModel();
        public bool AdminOptionsToggle { get; set; } = false;
        public bool SquintIsInitializing { get; set; } = false;
        public string InitializingMessages { get; set; } = "Squint is initializing, please wait...";
        public bool SquintIsBusy { get; set; } = false;
        public int NumAdminButtons { get; private set; } = 8;
        public bool PlanCheckVisible { get; set; } = false; // normallly false
        public string PlanCheckLoadingMessage { get; set; } = "Checking plan, please wait...";
        public bool isPlanCheckCalculating { get; set; } = false;
        public bool ProtocolCheckVisible { get; set; } = true; // normally true;
        public bool SessionSelectVisibility { get; set; } = false;
        public bool SessionSaveVisibility { get; set; } = false;
        public bool ConstraintInfoVisibility { get; set; } = false;
        public bool isLoading { get; set; } = false;
        public string LoadingString { get; set; } = "";
        public bool isUserPanelVisible { get; set; } = false;
        public bool isConfigVisible { get; set; } = false;

        public bool isStructurePanelVisible { get; set; } = true;
        public ObservableCollection<string> Protocols { get; } = new ObservableCollection<string> { "Protocol1", "Protocol2" };
        //UI Commands
        public ICommand ChangeVisibilityCommand
        {
            get { return new DelegateCommand(ChangeVisibility); }
        }
        public ICommand SyncrhonizePatientCommand
        {
            get { return new DelegateCommand(SynchronizePatient); }
        }

        public ICommand ExpandStructuresCommand
        {
            get { return new DelegateCommand(ExpandStructures); }
        }
        public ICommand EnterKeyCommand_PID
        {
            get { return new DelegateCommand(LoadPatient); }
        }


        public ICommand ShowConfigCommand
        {
            get { return new DelegateCommand(ShowConfig); }
        }
        public ICommand ShiftConstraintUpCommand
        {
            get { return new DelegateCommand(ShiftConstraintUp); }
        }
        public ICommand ShiftConstraintDownCommand
        {
            get { return new DelegateCommand(ShiftConstraintDown); }
        }
        public ICommand AddConstraintBelowCommand
        {
            get { return new DelegateCommand(AddConstraintBelow); }
        }
        public ICommand ExpandUserPanelCommand
        {
            get { return new DelegateCommand(ExpandUserPanel); }
        }
        public ICommand LaunchAdminViewCommand
        {
            get { return new DelegateCommand(LaunchAdminView); }
        }
        public ICommand SaveWorkspaceCommand
        {
            get
            {
                return new DelegateCommand(SaveSessionDialog);
            }
        }

        public ICommand ViewAvailableSessionsCommand
        {
            get
            {
                return new DelegateCommand(ViewAvailableSessions);
            }
        }
        private void ViewAvailableSessions(object param = null)
        {
            if (_model.PatientOpen)
                SessionSelectVisibility ^= true;
        }


        public ICommand CloseCheckListCommand
        {
            get { return new DelegateCommand(CloseCheckList); }
        }

        public ICommand ImportProtocolDirectoryCommand
        {
            get { return new DelegateCommand(ImportProtocolDirectory); }
        }
        private async void ImportProtocolDirectory(object param = null)
        {
            System.Windows.Forms.FolderBrowserDialog f = new System.Windows.Forms.FolderBrowserDialog();
            f.Description = "Please select import folder...";
            var ConfigPath = _model.Config.SquintProtocols.FirstOrDefault(x => x.Site == _model.Config.Site.CurrentSite);
            if (ConfigPath != null)
                f.SelectedPath = ConfigPath.Path;
            else
                f.SelectedPath = @"\\srvnetapp02\bcca\docs\physics\cn\software\squint\xml protocol library\";
            SquintIsBusy = true;
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string[] filenames = System.IO.Directory.GetFiles(f.SelectedPath);
                    List<Task> importTasks = new List<Task>();
                    Task finaltask = new Task(new Action(() => { }));
                    int count = 0;
                    double total = filenames.Count();
                    int filecount = 0;
                    foreach (string file in filenames)
                    {
                        filecount++;
                        string ext = System.IO.Path.GetExtension(file);
                        if (ext == ".xml")
                            try
                            {
                                if (filecount == filenames.Count())
                                    finaltask = new Task(() => _model.ImportProtocolFromXML(file, true));
                                else
                                {
                                    importTasks.Add(Task.Run(() =>
                                    {
                                        _model.ImportProtocolFromXML(file, false);
                                    }
                                    ));
                                    count++;
                                }
                            }
                            catch
                            {
                                MessageBox.Show(string.Format("Error importing {0}", file));
                            }
                    }
                    await Task.WhenAll(importTasks);
                    finaltask.RunSynchronously();
                }
                catch (Exception ex)
                {
                    Helpers.Logger.AddLog(string.Format("Error in batch import \r\n {0} \r\n {1} \r\n {2}",
                        ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
            SquintIsBusy = false;
            MessageBox.Show("Complete!");
        }

        public ICommand ImportProtocolCommand
        {
            get { return new DelegateCommand(ImportProtocol); }
        }

        private async void ImportProtocol(object param = null)
        {
            if (_model.PatientOpen || _model.NumAssessments > 0)
            {
                System.Windows.Forms.DialogResult DR = System.Windows.Forms.MessageBox.Show("This will close the current protocol and any assessments. Any unsaved changes will be lost. Continue?", "Import from XML", System.Windows.Forms.MessageBoxButtons.YesNo);
                if (DR == System.Windows.Forms.DialogResult.No)
                    return;
            }
            _model.StartNewSession();
            SquintIsBusy = true;
            System.Windows.Forms.OpenFileDialog d = new System.Windows.Forms.OpenFileDialog();
            d.Title = "Open Ctr.GetProtocolView() File";
            d.Filter = "XML files|*.xml";
            var ConfigPath = _model.Config.SquintProtocols.FirstOrDefault(x => x.Site == _model.Config.Site.CurrentSite);
            if (ConfigPath != null)
                d.InitialDirectory = ConfigPath.Path;
            else
                d.InitialDirectory = @"\\srvnetapp02\bcca\docs\Physics\CN\Software\Squint\XML Protocol Library\";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MessageBox.Show(d.FileName.ToString());
                bool ImportSuccessful = await Task.Run(() => _model.ImportProtocolFromXML(d.FileName, true));
                if (!ImportSuccessful)
                {
                    MessageBox.Show("Error in importing protocol, please review XML file");
                }
                else
                    MessageBox.Show("Protocol successfully imported.");
            }
            SquintIsBusy = false;

        }
        public ICommand ExportProtocolCommand
        {
            get { return new DelegateCommand(ExportProtocol); }
        }
        private async void ExportProtocol(object param = null)
        {
            if (_model.ProtocolLoaded)
            {
                System.Windows.Forms.SaveFileDialog d = new System.Windows.Forms.SaveFileDialog();
                d.Title = "Export current protocol";
                d.Filter = "XML files|*.xml";
                var ConfigPath = _model.Config.SquintProtocols.FirstOrDefault(x => x.Site == _model.Config.Site.CurrentSite);
                if (ConfigPath != null)
                    d.InitialDirectory = ConfigPath.ExportPath;
                else
                    d.InitialDirectory = @"\\srvnetapp02\bcca\docs\Physics\CN\Software\Squint\XML Protocol Library\Export";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    MessageBox.Show(d.FileName.ToString());
                    LoadingString = "Exporting current protocol";
                    isLoading = true;
                    await Task.Run(() => _model.ExportProtocolAsXML(d.FileName));
                    isLoading = false;
                    LoadingString = "";
                }
            }
        }

        public ICommand ImportEclipseCommand
        {
            get { return new DelegateCommand(ImportEclipseProtocol); }
        }

        public bool ImportEclipseProtocolVisibility { get; set; } = false;
        public void ImportEclipseProtocol(object param = null)
        {
            if (_model.PatientOpen || _model.NumAssessments > 0)
            {
                System.Windows.Forms.DialogResult DR = System.Windows.Forms.MessageBox.Show("Please close patient before importing protocols...");
                return;
            }
            EclipseProtocolPopupVM = new EclipseProtocolPopupViewModel(this, _model);
            ImportEclipseProtocolVisibility ^= true;
            //        SquintIsBusy = true;
        }

        private void CloseCheckList(object param = null)
        {
            ProtocolCheckVisible = true;
            PlanCheckVisible = false;
        }

        public ICommand ToggleChecklistViewCommand
        {
            get { return new DelegateCommand(ToggleCheckListView); }
        }
        private void ToggleCheckListView(object param = null)
        {
            ProtocolCheckVisible ^= false;
            PlanCheckVisible ^= true;
            ea.GetEvent<PlanChecklistClosing>().Publish();
        }
        public ICommand DuplicateProtocolCommand
        {
            get { return new DelegateCommand(DuplicateProtocol); }
        }
        private async void DuplicateProtocol(object param = null)
        {

            LoadingString = "Creating copy of current protocol";
            isLoading = true;
            await Task.Run(() => _model.Save_DuplicateProtocol());
            isLoading = false;
            LoadingString = "";
        }

        private double findMinDiff(double[] arr)
        {
            // Sort array in  
            // non-decreasing order 
            Array.Sort(arr);
            var n = arr.Length;
            // Initialize difference 
            // as infinite 
            double diff = double.MaxValue;

            // Find the min diff by  
            // comparing adjacent pairs 
            // in sorted array 
            for (int i = 0; i < n - 1; i++)
                if (arr[i + 1] - arr[i] > 180)
                {
                    var val = ((360 - arr[i + 1]) + arr[i]);
                    if (val > 90)
                        val = 180 - val;
                    if (val < diff)
                    {
                        diff = val;
                    }
                }
                else
                    if (arr[i + 1] - arr[i] < diff)
                    diff = arr[i + 1] - arr[i];

            // Return min diff 
            return diff;
        }
        private void SaveSessionDialog(object param = null)
        {
            SessionSaveVisibility ^= true;
        }


        private void LaunchAdminView(object param = null)
        {
            if (_model.PatientOpen)
                MessageBox.Show("Please close the current patient before starting protocol administration.");
            else
                AdminOptionsToggle ^= true;
        }
        private void AddConstraintBelow(object param = null)
        {
            ConstraintViewModel CS = param as ConstraintViewModel;
            if (CS != null)
            {
                _model.DuplicateConstraint(CS.Id);
            }
        }


        private void ShowConfig(object param = null)
        {
            if (_model.SquintUser == "nchng")
                isConfigVisible = !isConfigVisible;
        }

        private void ShiftConstraintUp(object param = null)
        {
            ConstraintViewModel CS = param as ConstraintViewModel;
            if (CS != null)
            {
                _model.ShiftConstraintUp(CS.Id);
            }
        }
        private void ShiftConstraintDown(object param = null)
        {
            ConstraintViewModel CS = param as ConstraintViewModel;
            if (CS != null)
            {
                _model.ShiftConstraintDown(CS.Id);
            }
        }



        public ICommand UpdateProtocolCommand
        {
            get { return new DelegateCommand(UpdateProtocol); }
        }
        private async void UpdateProtocol(object param = null)
        {
            if (_model.ProtocolLoaded)
            {
                LoadingString = "Updating Protocol";
                isLoading = true;
                await Task.Run(() => _model.Save_UpdateProtocol());
                isLoading = false;
                LoadingString = "";
            }
        }

        public ICommand DeleteSelectedProtocolCommand
        {
            get { return new DelegateCommand(DeleteProtocol); }
        }
        private async void DeleteProtocol(object param = null)
        {
            if (_model.CurrentProtocol != null)
            {
                var result = System.Windows.Forms.MessageBox.Show("This will delete the current protocol. Are you sure?", "Confirm protocol deletion", System.Windows.Forms.MessageBoxButtons.OKCancel);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    LoadingString = "Deleting Protocol";
                    isLoading = true;
                    await Task.Run(() => _model.DeleteProtocol(_model.CurrentProtocol.ID));
                    _model.StartNewSession();
                    isLoading = false;
                    LoadingString = "";
                }
            }
            else if (ProtocolsVM.SelectedProtocol != null)
            {
                var result = System.Windows.Forms.MessageBox.Show("This will delete the SELECTED protocol. Are you sure?", "Confirm protocol deletion", System.Windows.Forms.MessageBoxButtons.OKCancel);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    LoadingString = "Deleting Protocol";
                    isLoading = true;
                    await Task.Run(() => _model.DeleteProtocol(ProtocolsVM.SelectedProtocol.Id));
                    isLoading = false;
                    LoadingString = "";
                }
            }
        }

        private async void LoadPatient(object param = null)
        {
            if (AdminOptionsToggle)
            {
                MessageBox.Show("Please disable administration mode before loading a patient.", "Admin mode active");
                return;
            }
            //            PatientVM = new PatientViewModel(this);
            isLoading = true;
            LoadingString = "Loading patient";
            bool OpenNewPatient = PatientVM.PatientId != _model.PatientID;
            string NextPatient = PatientVM.PatientId;
            if (_model.PatientOpen)
            {
                var Result = MessageBox.Show("Close current patient and all assessments?", "Close Patient?", MessageBoxButton.OKCancel);
                if (Result == MessageBoxResult.Cancel)
                {
                    isLoading = false;
                    LoadingString = "";
                    return;
                }
                CloseCheckList();
                _model.ClosePatient();
                _model.CloseProtocol();
                _model.StartNewSession();
            }
            AdminOptionsToggle = false;
            if (OpenNewPatient)
            {
                bool Success = await _model.OpenPatient(NextPatient);
                if (!Success)
                    PatientVM.TextBox_Background_Color = new System.Windows.Media.SolidColorBrush(wpfcolors.DarkOrange);
            }
            isLoading = false;
            LoadingString = "";

        }
        private static EventHandler SynchronizeHandler;
        private void SynchronizePatient(object param = null)
        {
            if (_model.PatientOpen && _model.ProtocolLoaded && _model.GetAssessmentList().Count > 0)
            {
                var W = new WaitWindow();
                W.Show();
                SynchronizeHandler = new EventHandler((sender, e) => OnSynchronizationComplete(sender, e, W));
                _model.SynchronizationComplete += SynchronizeHandler;
                _model.SynchronizePlans();
            }
        }
        private void OnSynchronizationComplete(object sender, EventArgs E, Window W)
        {
            _model.SynchronizationComplete -= SynchronizeHandler;
            // Refresh ViewModel
            PatientVM.CalculateOnUpdate = false; // disable automatic calculation while model is updated
            string CurrentStructureSetId = ""; // store these as clearing the list will null this as it is databound.
            string CurrentStructureSetUID = "";
            if (PatientVM.CurrentStructureSet != null)
            {
                CurrentStructureSetId = PatientVM.CurrentStructureSet.StructureSetId; // store these as clearing the list will null this as it is databound.
                CurrentStructureSetUID = PatientVM.CurrentStructureSet.StructureSetUID;
            }
            PatientVM.StructureSets.Clear();
            foreach (var StS in _model.GetAvailableStructureSets())
            {
                PatientVM.StructureSets.Add(new StructureSetSelector(StS));
            }
            if (CurrentStructureSetId != "")
            {
                // Set structure set back to pre-update
                var UnchangedStructureSet = PatientVM.StructureSets.FirstOrDefault(x => x.StructureSetId == CurrentStructureSetId && x.StructureSetUID == CurrentStructureSetUID);
                if (UnchangedStructureSet != null)
                {
                    PatientVM.AutomaticStructureAliasingEnabled = false;
                    PatientVM.CurrentStructureSet = UnchangedStructureSet;
                    PatientVM.AutomaticStructureAliasingEnabled = true;
                }
            }
            foreach (AssessmentView AV in AssessmentsVM.Assessments)
            {
                foreach (AssessmentComponentViewModel ACV in AV.ACVs)
                {
                    ObservableCollection<CourseSelector> UpdatedCourses = new ObservableCollection<CourseSelector>();
                    foreach (string CourseName in _model.GetCourseNames())
                    {
                        UpdatedCourses.Add(new CourseSelector(CourseName));
                    }
                    string PrevSelectedCourseId = "";
                    string PrevSelectedPlanUID = "";
                    if (ACV.SelectedCourse != null)
                        PrevSelectedCourseId = ACV.SelectedCourse.CourseId;
                    if (ACV.SelectedPlan != null)
                        PrevSelectedPlanUID = ACV.SelectedPlan.PlanUID;
                    ACV.Courses = UpdatedCourses;
                    if (PrevSelectedCourseId != "")
                        ACV.SelectedCourse = UpdatedCourses.FirstOrDefault(x => PrevSelectedCourseId == x.CourseId);
                    if (PrevSelectedPlanUID != "")
                        ACV.SelectedPlan = ACV.Plans.FirstOrDefault(x => PrevSelectedPlanUID == x.PlanUID);
                }
            }
            PatientVM.CalculateOnUpdate = true; // disable automatic calculation while model is updated
            //Ctr.UpdateAllConstraints();
            W.Close();
        }


        private void ChangeVisibility(object param = null)
        {
            if (PatientVM.isPIDVisible && _model.PatientOpen)
            {
                var Result = MessageBox.Show("Close current patient and all assessments?", "Close Patient?", MessageBoxButton.OKCancel);
                if (Result == MessageBoxResult.Cancel)
                    return;
                _model.ClosePatient();
                _model.CloseProtocol();
                _model.StartNewSession();
                CloseCheckList();
                ProtocolsVM.UpdateProtocolView();
                PatientVM.PatientId = "";
            }
            else PatientVM.isPIDVisible ^= true;
        }


        private void ExpandStructures(object param = null)
        {
            isStructurePanelVisible = !isStructurePanelVisible;
        }
        private void ExpandUserPanel(object param = null)
        {
            isUserPanelVisible = !isUserPanelVisible;
        }
    }
}
