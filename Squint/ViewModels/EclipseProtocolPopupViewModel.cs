using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged;
using SquintScript.Interfaces;
using SquintScript.ViewModelClasses;
using SquintScript.Extensions;
using System.IO;
using System.Xml.Serialization;

namespace SquintScript.ViewModels
{
    [AddINotifyPropertyChangedInterface]

    public class EclipseProtocolPopupViewModel
    {

        public MainViewModel ParentView { get; private set; }
        public string ProtocolPath = Ctr.Config.ClinicalProtocols.FirstOrDefault(x => x.Site == Ctr.Config.Site.CurrentSite).Path;
        public ObservableCollection<VMSTemplates.Protocol> EclipseProtocols { get; set; } = new ObservableCollection<VMSTemplates.Protocol>();

        public EclipseProtocolPopupViewModel(MainViewModel parentView)
        {
            ParentView = parentView;
            var filenames = Directory.GetFiles(ProtocolPath, @"*.xml");
            XmlSerializer ser = new XmlSerializer(typeof(VMSTemplates.Protocol));
            foreach (var f in filenames)
            {
                using (var data = new StreamReader(f))
                {
                    var EP = (VMSTemplates.Protocol)ser.Deserialize(data);
                    if (EP != null)
                    {
                        if (EP.Preview.ApprovalStatus.ToLower().Equals(@"approved"))
                            EclipseProtocols.Add(EP);
                    }

                }
            }
            EclipseProtocols = new ObservableCollection<VMSTemplates.Protocol>(EclipseProtocols.OrderBy(x => x.Preview.ID).ToList());

        }

        public ICommand ImportSelectedProtocolCommand
        {
            get { return new DelegateCommand(ImportSelectedProtocol); }
        }

        private async void ImportSelectedProtocol(object param = null)
        {
            VMSTemplates.Protocol P = param as VMSTemplates.Protocol;
            ParentView.SquintIsBusy = true;
            ParentView.LoadingString = "Loading protocol";
            ParentView.isLoading = true;
            bool Success = false;
            if (P != null)
            {
                Success = await Ctr.ImportEclipseProtocol(P);
            }
            if (Success)
                ParentView.ProtocolVM.ViewLoadedProtocol();
            ParentView.SquintIsBusy = false;
            ParentView.LoadingString = "";
            ParentView.isLoading = false;
        }
    }
}