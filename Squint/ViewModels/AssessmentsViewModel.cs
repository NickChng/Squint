﻿using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using PropertyChanged;
using System.Windows.Controls;
using System.Windows;
using wpfcolors = System.Windows.Media.Colors;
using wpfcolor = System.Windows.Media.Color;
using Squint.Views;
using System.Linq;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class AssessmentsViewModel : ObservableObject
    {
        public AssessmentsViewModel()
        { 
        }
        public AssessmentsViewModel(MainViewModel parentView, SquintModel model)
        {
            ParentView = parentView;
            _model = model;
            _model.ProtocolClosed += Ctr_ProtocolClosed;
        }

        private void Ctr_ProtocolClosed(object sender, EventArgs e)
        {
            Assessments = new ObservableCollection<AssessmentView>();
            AssessmentColumns =  new ObservableCollection<SquintDataColumn>();
            isLinkProtocolVisible = false;
            SelectedAssessment = null;

        }

        public Progress<int> Progress { get; set; }
        public Thickness ColHeaderMargin { get; set; } = new Thickness(10, 5, 10, 5);
        //public DataGridLength RowHeaderWidth { get; set; } = new DataGridLength(1, DataGridLengthUnitType.Auto);
        public double RowHeaderWidth { get; set; } = double.NaN;
        public int AssessmentCounter { get; private set; } = 1;
        public bool WaitingForUpdate { get; set; } = false;
        public string WaitingDescription { get; set; } = "";

        // Visibility Toggles
        public bool isLinkProtocolVisible { get; set; } = false;

        private SquintModel _model;
        public MainViewModel ParentView { get; set; }
        public double FontSize { get; set; } = 12;
        private List<wpfcolor> DefaultAssessmentColors = new List<wpfcolor> { wpfcolors.LightSteelBlue, wpfcolors.AliceBlue, wpfcolors.PapayaWhip, wpfcolors.PaleGoldenrod };
        private List<wpfcolor> DefaultAssessmentTextColors = new List<wpfcolor> { wpfcolors.White, wpfcolors.Black, wpfcolors.Black, wpfcolors.Black };

        public AssessmentView SelectedAssessment { get; set; }
        public ObservableCollection<AssessmentView> Assessments { get; set; } = new ObservableCollection<AssessmentView>(); // { new AssessmentView("DesignTime") 
        public ObservableCollection<SquintDataColumn> AssessmentColumns { get; set; } = new ObservableCollection<SquintDataColumn>();
        public void AddAssessment()
        {
            if (_model.PatientOpen && _model.ProtocolLoaded)
            {
                int colindex = (AssessmentCounter - 1) % DefaultAssessmentColors.Count;
                AssessmentView AV = new AssessmentView(DefaultAssessmentColors[colindex], DefaultAssessmentTextColors[colindex], this, _model);
                Assessments.Add(AV);
                //Add new column
                SquintDataColumn dgtc = new SquintDataColumn(AV)
                {
                    //HeaderTemplate = (DataTemplate)Resources["myColumnHeaderTemplate"],
                    HeaderStyle = (Style)Application.Current.FindResource("SquintColumnHeaderStyle"),
                    CellTemplate = (DataTemplate)Application.Current.FindResource("SquintCellTemplate"),
                    CellStyle = (Style)Application.Current.FindResource("SquintCellStyle"),
                    Header = AV.AssessmentName,
                    Width = DataGridLength.Auto,
                };
                AssessmentColumns.Add(dgtc);
                AssessmentCounter++;
                isLinkProtocolVisible = true;
            }
            else
            {
                MessageBox.Show("Please load patient and protocol first", "No open Protocol/Patient");
                return;
            }

        }
        public void LoadAssessmentViews()
        {
            if (_model.PatientOpen && _model.ProtocolLoaded)
            {
                foreach (AssessmentViewModel A in _model.GetAssessmentList())
                {
                    int colindex = (AssessmentCounter - 1) % DefaultAssessmentColors.Count;
                    AssessmentView AV = new AssessmentView(A, DefaultAssessmentColors[colindex], DefaultAssessmentTextColors[colindex], this, _model);
                    Assessments.Add(AV);
                    //Add new column
                    SquintDataColumn dgtc = new SquintDataColumn(AV)
                    {
                        HeaderStyle = (Style)Application.Current.FindResource("SquintColumnHeaderStyle"),
                        CellTemplate = (DataTemplate)Application.Current.FindResource("SquintCellTemplate"),
                        CellStyle = (Style)Application.Current.FindResource("SquintCellStyle"),
                        Header = AV.AssessmentName,
                        Width = DataGridLength.Auto,
                    };
                    AssessmentColumns.Add(dgtc);
                    AssessmentCounter++;
                }
            }

            //else
            //{
            //    MessageBox.Show("Please load patient and protocol first", "No open Protocol/Patient");
            //    return;
            //}
        }
        public ICommand DeleteAssessmentCommand
        {
            get { return new DelegateCommand(DeleteAssessment); }
        }
        private void DeleteAssessment(object param = null)
        {
            var AS = (AssessmentView)param;
            DeleteAssessment(AS);
            AS.Delete();
        }
        public void DeleteAssessment(AssessmentView AS)
        {
            var UpdatedAssessments = new ObservableCollection<AssessmentView>();
            foreach (AssessmentView AV in Assessments)
            {
                if (AV != AS)
                    UpdatedAssessments.Add(AV);
            }
            Assessments = UpdatedAssessments;
            AssessmentColumns.Remove(AssessmentColumns.FirstOrDefault(x => x.Header.ToString() == AS.AssessmentName));
        }
        private void AddAssessment(object param = null)
        {
            AddAssessment();
        }
        public ICommand AddAssessmentCommand
        {
            get { return new DelegateCommand(AddAssessment); }
        }

        public ICommand ChangeLinkVisibilityCommand
        {
            get { return new DelegateCommand(ChangeLinkProtocolVisibility); }
        }
        private void ChangeLinkProtocolVisibility(object param = null)
        {
            isLinkProtocolVisible = !isLinkProtocolVisible;
            // Add an assessment if there aren't any
            if (Assessments.Count == 0 && isLinkProtocolVisible == true)
                AddAssessment();
        }
        //private void UpdateAssessmentsView()
        //{
        //    AssessmentPresenter = new AssessmentsView(this);
        //    AssessmentPresenter.LoadAssessmentViews();
        //}
        public ICommand FontSizeIncreaseCommand
        {
            get { return new DelegateCommand(FontSizeIncrease); }
        }
        public ICommand FontSizeDecreaseCommand
        {
            get { return new DelegateCommand(FontSizeDecrease); }
        }
        private void FontSizeIncrease(object param = null)
        {
            var AP = param as AssessmentsViewModel;
            if (AP != null)
                AP.FontSize = AP.FontSize + 1;
        }
        private void FontSizeDecrease(object param = null)
        {
            var AP = param as AssessmentsViewModel;
            if (AP != null)
            {
                AP.FontSize = AP.FontSize - 1;
                foreach (SquintDataColumn C in AssessmentColumns)
                {
                    C.Width = new DataGridLength(0);
                    C.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }
                RowHeaderWidth = 0;
                RowHeaderWidth = double.NaN;
            }
        }
        public ICommand ViewPlanCheckCommand
        {
            get
            {
                return new DelegateCommand(ViewPlanCheck);
            }
        }
        private async void ViewPlanCheck(object param = null)
        {
            try
            {
                var p = (param as PlanSelector);
                if (param == null)
                    return;
                else
                {
                    ParentView.ProtocolCheckVisible = false;
                    ParentView.PlanCheckVisible = true;
                    ParentView.isPlanCheckCalculating = true;
                    ParentView.Loading_ViewModel = new LoadingViewModel() { LoadingMessage = @"Checking plan, please wait..." };

                    await ParentView.ProtocolsVM.ChecklistViewModel.DisplayChecksForPlan(p);
                    ParentView.isPlanCheckCalculating = false;
                }
            }
            catch (Exception ex)
            {
                Helpers.Logger.AddLog(ex.Message);
                MessageBox.Show("Error in ViewPlanCheck");
            }
        }

        public ICommand ShowAssessmentCommand
        {
            get { return new DelegateCommand(ShowAssessment); }
        }
        private void ShowAssessment(object param = null)
        {
            var AS = (AssessmentView)param;
            AS.Pinned = !AS.Pinned;
        }

    }
}
