using SquintScript.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace SquintScript.Views
{
    /// <summary>
    /// Interaction logic for CalculationView.xaml
    /// </summary>
    public partial class HighResolutionView : UserControl
    {
        public HighResolutionView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TestItemsProperty =
          DependencyProperty.Register("TestItems", typeof(ObservableCollection<ITestListItem>), typeof(HighResolutionView), new
             PropertyMetadata(new ObservableCollection<ITestListItem>(), new PropertyChangedCallback(OnSetTestItems)));

        public ObservableCollection<ITestListItem> TestItems
        {
            get { return (ObservableCollection<ITestListItem>)GetValue(TestItemsProperty); }
            set { SetValue(TestItemsProperty, value); }
        }

        private static void OnSetTestItems(DependencyObject d,
         DependencyPropertyChangedEventArgs e)
        {
            HighResolutionView UserControl1Control = d as HighResolutionView;
            UserControl1Control.OnSetTestItems(e);
        }

        private void OnSetTestItems(DependencyPropertyChangedEventArgs e)
        {
            MainTestListView.ItemsSource = (ObservableCollection<ITestListItem>)e.NewValue;
        }
    }
}
