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
using SquintScript.ViewModels;
using System.Windows.Threading;

namespace SquintScript.Views
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
                Ctr.Initialize(Dispatcher.CurrentDispatcher);
                InitializeComponent();
            }
            catch (Exception ex)
            {
                var test = ex;
            }
            
        }

        private void OnSelectedStructureChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var VM = (Presenter)DataContext;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            Ctr.Dispose();
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
        

        private void DragImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var VM = DataContext as Presenter;
            ConstraintListView.AllowDrop = true;
            VM.Protocol.DragSelected = true;
        }
        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            var VM = DataContext as Presenter;
            var SelectedIndex = GetCurrentIndex(e.GetPosition);
            //

            var DropIndex = VM.Protocol.SelectedIndex;
            if (SelectedIndex < 0)
                return;
            if (DropIndex < 0)
                return;
            if (SelectedIndex == DropIndex)
                return;
            if (VM.Protocol.DragSelected)
            {
                int OldIndex = VM.Protocol.Constraints[SelectedIndex].DisplayOrder;
                VM.Protocol.Constraints[SelectedIndex].DisplayOrder = VM.Protocol.Constraints[DropIndex].DisplayOrder;
                VM.Protocol.Constraints[DropIndex].DisplayOrder = OldIndex;
                VM.Protocol.Constraints.Move(SelectedIndex, DropIndex);
            }
        }


        int GetCurrentIndex(GetPositionDelegate getPosition)
        {
            int index = -1;
            for (int i = 0; i < ConstraintListView.Items.Count; ++i)
            {
                ListViewItem item = GetListViewItem(i);
                if (item != null)
                    if (this.IsMouseOverTarget(item, getPosition))
                    {
                        index = i;
                        break;
                    }
            }
            return index;
        }

        ListViewItem GetListViewItem(int index)
        {
            if (ConstraintListView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return ConstraintListView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        delegate Point GetPositionDelegate(IInputElement element);

        bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }

        private void ConstraintListView_MouseLeave(object sender, MouseEventArgs e)
        {
            //hitResultsList.Clear();
            //VisualTreeHelper.HitTest(ConstraintListView,
            //                  new HitTestFilterCallback(MyHitTestFilter),
            //                  new HitTestResultCallback(MyHitTestResult),
            //                  new PointHitTestParameters(e.GetPosition((UIElement)sender)));

            if (e.LeftButton == MouseButtonState.Released)
            {
                (DataContext as Presenter).Protocol.DragSelected = false;
                ConstraintListView.AllowDrop = false;
            }

        }
        private void ConstraintListView_MouseEnter(object sender, MouseEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Released && (DataContext as Presenter).Protocol.DragSelected)
            {
                (DataContext as Presenter).Protocol.DragSelected = false;
                ConstraintListView.AllowDrop = false;
            }
        }
        private void ConstraintListView_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var VM = (sender as ListView).DataContext as Presenter;
            VM.Protocol.DragSelected = false;
            ConstraintListView.AllowDrop = false;
        }
        private void TextBox_KeyEnterUpdate(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tBox = (TextBox)sender;
                DependencyProperty prop = TextBox.TextProperty;

                BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
                if (binding != null) { binding.UpdateSource(); }
            }
        }
    }
}
