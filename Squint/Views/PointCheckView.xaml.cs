using SquintScript.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace SquintScript.Views
{
    /// <summary>
    /// Interaction logic for CalculationView.xaml
    /// </summary>
    public partial class PointCheckView : UserControl
    {
        public PointCheckView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TestItemsProperty =
          DependencyProperty.Register("TestItems", typeof(ObservableCollection<ITestListItem>), typeof(PointCheckView), new
             PropertyMetadata(new ObservableCollection<ITestListItem>(), new PropertyChangedCallback(OnSetTestItems)));

        public ObservableCollection<ITestListItem> TestItems
        {
            get { return (ObservableCollection<ITestListItem>)GetValue(TestItemsProperty); }
            set { SetValue(TestItemsProperty, value); }
        }

        private static void OnSetTestItems(DependencyObject d,
         DependencyPropertyChangedEventArgs e)
        {
            PointCheckView UserControl1Control = d as PointCheckView;
            UserControl1Control.OnSetTestItems(e);
        }

        private void OnSetTestItems(DependencyPropertyChangedEventArgs e)
        {
            MainTestListView.ItemsSource = (ObservableCollection<ITestListItem>)e.NewValue;
        }

    }
}
