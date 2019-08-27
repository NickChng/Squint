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
    public partial class TestViewGrid : UserControl
    {
        public TestViewGrid()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TestItemsProperty =
          DependencyProperty.Register("TestItems", typeof(ObservableCollection<TestListItem>), typeof(TestViewGrid), new
             PropertyMetadata(new ObservableCollection<TestListItem>(), new PropertyChangedCallback(OnSetTestItems)));

        public ObservableCollection<TestListItem> TestItems
        {
            get { return (ObservableCollection<TestListItem>)GetValue(TestItemsProperty); }
            set { SetValue(TestListTitleProperty, value); }
        }

        private static void OnSetTestItems(DependencyObject d,
         DependencyPropertyChangedEventArgs e)
        {
            TestViewGrid UserControl1Control = d as TestViewGrid;
            UserControl1Control.OnSetTestItems(e);
        }

        private void OnSetTestItems(DependencyPropertyChangedEventArgs e)
        {
            MainTestListView.ItemsSource = (ObservableCollection<TestListItem>)e.NewValue;
        }

        public static readonly DependencyProperty TestListTitleProperty =
           DependencyProperty.Register("TestListTitle", typeof(string), typeof(TestViewGrid), new
              PropertyMetadata("", new PropertyChangedCallback(OnSetTextChanged)));

        public string TestListTitle
        {
            get { return (string)GetValue(TestListTitleProperty); }
            set { SetValue(TestListTitleProperty, value); }
        }

        private static void OnSetTextChanged(DependencyObject d,
           DependencyPropertyChangedEventArgs e)
        {
            TestViewGrid UserControl1Control = d as TestViewGrid;
            UserControl1Control.OnSetTextChanged(e);
        }

        private void OnSetTextChanged(DependencyPropertyChangedEventArgs e)
        {
            TitleBlock.Text = e.NewValue.ToString();
        }

    }
}
