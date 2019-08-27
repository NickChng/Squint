using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for CalculationView.xaml
    /// </summary>
    public partial class TestView : UserControl
    {
        public TestView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TestItemsProperty =
          DependencyProperty.Register("TestItems", typeof(ObservableCollection<TestListItem>), typeof(TestView), new
             PropertyMetadata(new ObservableCollection<TestListItem>(), new PropertyChangedCallback(OnSetTestItems)));

        public ObservableCollection<TestListItem> TestItems
        {
            get { return (ObservableCollection<TestListItem>)GetValue(TestItemsProperty); }
            set { SetValue(TestListTitleProperty, value); }
        }

        private static void OnSetTestItems(DependencyObject d,
         DependencyPropertyChangedEventArgs e)
        {
            TestView UserControl1Control = d as TestView;
            UserControl1Control.OnSetTestItems(e);
        }

        private void OnSetTestItems(DependencyPropertyChangedEventArgs e)
        {
            MainTestListView.ItemsSource = (ObservableCollection<TestListItem>)e.NewValue;
        }

        public static readonly DependencyProperty TestListTitleProperty =
           DependencyProperty.Register("TestListTitle", typeof(string), typeof(TestView), new
              PropertyMetadata("", new PropertyChangedCallback(OnSetTextChanged)));

        public string TestListTitle
        {
            get { return (string)GetValue(TestListTitleProperty); }
            set { SetValue(TestListTitleProperty, value); }
        }

        private static void OnSetTextChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            TestView UserControl1Control = d as TestView;
            UserControl1Control.OnSetTextChanged(e);
        }

        private void OnSetTextChanged(DependencyPropertyChangedEventArgs e)
        {
            TitleBlock.Text = e.NewValue.ToString();
        }

        public static readonly DependencyProperty ReferenceHeaderProperty =
           DependencyProperty.Register("ReferenceHeaderProperty", typeof(string), typeof(TestView), new
              PropertyMetadata("", new PropertyChangedCallback(OnReferenceHeaderChanged)));

        public string SetReferenceHeader
        {
            get { return (string)GetValue(ReferenceHeaderProperty); }
            set { SetValue(ReferenceHeaderProperty, value); }
        }

        private static void OnReferenceHeaderChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            TestView UserControl1Control = d as TestView;
            UserControl1Control.OnReferenceHeaderChanged(e);
        }

        private void OnReferenceHeaderChanged(DependencyPropertyChangedEventArgs e)
        {
            ReferenceHeader.Text = e.NewValue.ToString();
        }
    }
}
