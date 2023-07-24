using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using VMS.TPS.Common.Model;
using VMS.TPS.Common.Model.API;
using System.IO;
using System.Windows.Controls.Primitives;
using ESAPI = VMS.TPS.Common.Model.API.Application;
using Squint.ViewModels;
using System.Windows.Threading;

namespace Squint.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                ((MainViewModel)DataContext).SquintIsInitializing = true;
                //System.Threading.Thread.Sleep(3000); // 
                // Note that Squint is initialized when the windows Loaded Event is fired, (see OnLoaded)
                // This is necessary so any message boxes and exceptions that get thrown by Squint on start are visible to the user.
            }
            catch (Exception ex)
            {
                var test = ex;
            }

        }



        private void OnSelectedStructureChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var VM = (MainViewModel)DataContext;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            SquintModel.Dispose();
        }

        public int counter = 0;

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void ScrollViewer_PreviewMouseWheel2(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        // Structure draggin
     

        

       
        delegate Point GetPositionDelegate(IInputElement element);

     

        
      

        

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var uiDispatcher = Dispatcher.CurrentDispatcher;
            Task.Run(() => SquintModel.Initialize(uiDispatcher));
        }

    
    }
}
