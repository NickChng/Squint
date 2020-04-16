using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SquintScript
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {

            Helpers.Logger.AddLog(string.Format("{0}\r\n{1}\r\n{2}", args.Exception.Message, args.Exception.InnerException, args.Exception.StackTrace));
            MessageBox.Show("An unexpected exception has occurred. Shutting down the application. Please check the log file for more details.");
            // Prevent default unhandled exception processing
            args.Handled = true;

            Environment.Exit(0);
        }
    }
}
