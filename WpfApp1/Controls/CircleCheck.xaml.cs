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

namespace SquintScript.Controls
{
    /// <summary>
    /// Interaction logic for CircleCheck.xaml
    /// </summary>
    public partial class CircleCheck : UserControl
    {
        public static DependencyProperty GridHeightProperty =
        DependencyProperty.Register("CheckHeight", typeof(double), typeof(CircleCheck), new
           PropertyMetadata(50.0, new PropertyChangedCallback(OnSetHeightChanged)));

        public double CheckHeight
        {
            get { return (double)GetValue(GridHeightProperty); }
            set { SetValue(GridHeightProperty, value); }
        }

        private static void OnSetHeightChanged(DependencyObject d,
          DependencyPropertyChangedEventArgs e)
        {
            CircleCheck UserControl1Control = d as CircleCheck;
            UserControl1Control.OnSetHeightChanged(e);
        }

        private void OnSetHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            ControlGrid.Height = (double)e.NewValue;
        }

        public static DependencyProperty PassProperty =
        DependencyProperty.Register("Pass", typeof(bool), typeof(CircleCheck), new
         PropertyMetadata(false, new PropertyChangedCallback(OnPassChanged)));

        public bool Pass
        {
            get { return (bool)GetValue(PassProperty); }
            set { SetValue(PassProperty, value); }
        }

        private static void OnPassChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            CircleCheck UserControl1Control = d as CircleCheck;
            UserControl1Control.OnPassChanged(e);
        }

        private void OnPassChanged(DependencyPropertyChangedEventArgs e)
        {
            //if ((bool)e.NewValue)
            //{
            //    PassGrid.Visibility = Visibility.Visible;
            //    FailGrid.Visibility = Visibility.Hidden;
            //}
            //else
            //{
            //    PassGrid.Visibility = Visibility.Hidden;
            //    FailGrid.Visibility = Visibility.Visible;
            //}
        }

        public static DependencyProperty GridWidthProperty =
       DependencyProperty.Register("GridWidth", typeof(double), typeof(CircleCheck), new
          PropertyMetadata(50.0, new PropertyChangedCallback(OnSetWidthChanged)));

        public double CheckWidth
        {
            get { return (double)GetValue(GridWidthProperty); }
            set { SetValue(GridWidthProperty, value); }
        }

        private static void OnSetWidthChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            CircleCheck UserControl1Control = d as CircleCheck;
            UserControl1Control.OnSetWidthChanged(e);
        }

        private void OnSetWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            ControlGrid.Width = (double)e.NewValue;
        }

        public CircleCheck()
        {
            InitializeComponent();
        }
    }
}
