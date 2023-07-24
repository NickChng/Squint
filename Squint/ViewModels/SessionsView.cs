using System;
using System.Collections.ObjectModel;
using PropertyChanged;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class SessionsViewModel
    {
        public MainViewModel ParentView { get; private set; }
        public ObservableCollection<SessionView> SessionViews { get; private set; } = new ObservableCollection<SessionView>();

        public ICommand SaveSessionCommand
        {
            get
            {
                return new DelegateCommand(SaveSession);
            }
        }
        public ICommand LoadSelectedSessionCommand
        {
            get
            {
                return new DelegateCommand(LoadSelectedSession);
            }
        }
        public ICommand DeleteSelectedSessionCommand
        {
            get
            {
                return new DelegateCommand(DeleteSelectedSession);
            }
        }

        private async void SaveSession(object param = null)
        {
            ParentView.LoadingString = "Saving Session...";
            ParentView.isLoading = true;
            bool Success = await Task.Run(() => SquintModel.Save_Session(SessionComment)); // boolean return is in order to delay the "ParentView.isLoading" return to False, so the load menu has a chance to include the latest save
            ParentView.isLoading = false;
            ParentView.SessionSaveVisibility ^= true;
        }

        private async void DeleteSelectedSession(object param = null)
        {
            if (ParentView.isLoading == true)
                return;
            SessionView E = param as SessionView;
            if (param == null)
                return;
            ParentView.isLoading = true;
            ParentView.LoadingString = "Deleting session...";
            try
            {
                await Task.Run(() => SquintModel.Delete_Session(E.ID));
            }
            catch (Exception ex)
            {
                var t = ex;
            }
            ParentView.isLoading = false;
        }
        private async void LoadSelectedSession(object param = null)
        {
            if (ParentView.isLoading == true)
                return;
            SessionView E = param as SessionView;
            if (param == null)
                return;
            ParentView.isLoading = true;
            ParentView.LoadingString = "Loading session...";
            if (await Task.Run(() => SquintModel.Load_Session(E.ID)))
            {
                //ParentView.ProtocolVM.UpdateProtocolView();
                ParentView.ProtocolsVM.isProtocolLoaded = true;
                SquintModel.UpdateConstraints();
                ParentView.AssessmentsVM.isLinkProtocolVisible = true;
            }
            else
                MessageBox.Show("Error loading session");
            ParentView.isLoading = false;
            ParentView.SessionSelectVisibility ^= true;
        }
        public string SessionComment { get; set; }
        public SessionsViewModel(MainViewModel parentView)
        {
            ParentView = parentView;
            SquintModel.SessionsChanged += OnSessionsChanged;
            SessionViews = new ObservableCollection<SessionView>(SquintModel.GetSessionViews());
        }
        public void OnSessionsChanged(object sender, EventArgs e)
        {
            ObservableCollection<SessionView> updatedSV = new ObservableCollection<SessionView>();
            foreach (SessionView E in SquintModel.GetSessionViews())
                updatedSV.Add(E);
            SessionViews = updatedSV;
        }
    }
}
