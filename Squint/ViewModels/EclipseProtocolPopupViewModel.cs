using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged;
using Squint.Interfaces;
using Squint.TestFramework;
using Squint.Extensions;
using System.IO;
using System.Xml.Serialization;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]

    public class EclipseProtocolPopupViewModel
    {

        public bool Loading { get; private set; }
        public MainViewModel ParentView { get; private set; }
        public string ProtocolPath = SquintModel.Config.ClinicalProtocols.FirstOrDefault(x => x.Site == SquintModel.Config.Site.CurrentSite).Path;
        public ObservableCollection<VMS_XML.Protocol> EclipseProtocols { get; set; } = new ObservableCollection<VMS_XML.Protocol>();

        public EclipseProtocolPopupViewModel(MainViewModel parentView)
        {
            ParentView = parentView;
            Loading = true;
            LoadProtocols();
        }


        private async void LoadProtocols()
        {
            var eclipseProtocols = new ObservableCollection<VMS_XML.Protocol>();
            await Task.Run(() =>
            {
                var filenames = Directory.GetFiles(ProtocolPath, @"*.xml");
                XmlSerializer ser = new XmlSerializer(typeof(VMS_XML.Protocol));
                foreach (var f in filenames)
                {
                    using (var data = new StreamReader(f))
                    {
                        var EP = (VMS_XML.Protocol)ser.Deserialize(data);
                        if (EP != null)
                        {
                            if (EP.Preview.ApprovalStatus == VMS_XML.ApprovalStatus.Approved)
                                eclipseProtocols.Add(EP);
                        }

                    }
                }
            });
            foreach (var p in eclipseProtocols.OrderBy(x => x.Preview.ID))
                EclipseProtocols.Add(p);
            Loading = false;
        }

        public ICommand ImportSelectedProtocolCommand
        {
            get { return new DelegateCommand(ImportSelectedProtocol); }
        }

        private async void ImportSelectedProtocol(object param = null)
        {
            VMS_XML.Protocol P = param as VMS_XML.Protocol;
            ParentView.SquintIsBusy = true;
            ParentView.LoadingString = "Loading protocol";
            ParentView.isLoading = true;
            bool Success = false;
            if (P != null)
            {
                Success = await SquintModel.ImportEclipseProtocol(P);
            }
            if (Success)
                ParentView.ProtocolsVM.ViewLoadedProtocol();
            ParentView.SquintIsBusy = false;
            ParentView.LoadingString = "";
            ParentView.isLoading = false;
        }
    }
}