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

namespace Squint.Views
{
    /// <summary>
    /// Interaction logic for LoadingControl.xaml
    /// </summary>
    public partial class LoadingControl : UserControl
    {
        public LoadingControl()
        {
            InitializeComponent();
        }

        public static DependencyProperty LoadingMessageProperty =
          DependencyProperty.Register("LoadingMessage", typeof(string), typeof(LoadingControl), new
             PropertyMetadata("", new PropertyChangedCallback(OnSetTextChanged)));

        public string LoadingMessage
        {
            get { return (string)GetValue(LoadingMessageProperty); }
            set { SetValue(LoadingMessageProperty, value); }
        }

        private static void OnSetTextChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            LoadingControl UserControl1Control = d as LoadingControl;
            UserControl1Control.OnSetTextChanged(e);
        }

        private void OnSetTextChanged(DependencyPropertyChangedEventArgs e)
        {
            LoadingMessageBlock.Text = e.NewValue.ToString();
        }
    }
}
