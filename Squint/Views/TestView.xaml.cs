using Squint.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace Squint.Views
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
          DependencyProperty.Register("TestItems", typeof(ObservableCollection<ITestListItem>), typeof(TestView), new
             PropertyMetadata(new ObservableCollection<ITestListItem>(), new PropertyChangedCallback(OnSetTestItems)));

        public ObservableCollection<ITestListItem> TestItems
        {
            get { return (ObservableCollection<ITestListItem>)GetValue(TestItemsProperty); }
            set { SetValue(TestItemsProperty, value); }
        }

        private static void OnSetTestItems(DependencyObject d,
         DependencyPropertyChangedEventArgs e)
        {
            TestView UserControl1Control = d as TestView;
            UserControl1Control.OnSetTestItems(e);
        }

        private void OnSetTestItems(DependencyPropertyChangedEventArgs e)
        {
            MainTestListView.ItemsSource = (ObservableCollection<ITestListItem>)e.NewValue;
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
