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
            var VM = (MainViewModel)DataContext;
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
        
        // Constraints dragging
        private void DragImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var VM = DataContext as MainViewModel;
            ConstraintListView.AllowDrop = true;
            VM.ProtocolVM.DragSelected = true;

            var ImageContentPresenter = (ContentPresenter)(sender as System.Windows.Controls.Image).TemplatedParent;
            var SelectedListBoxItem = (ListBoxItem)VisualTreeHelper.GetParent(ImageContentPresenter.Parent);
            SelectedListBoxItem.MouseLeave += ConstraintsListView_MouseLeave;

        }
        private void ListView_DragOver(object sender, DragEventArgs e)
        {
            var VM = DataContext as MainViewModel;
            var SelectedIndex = GetCurrentIndex(e.GetPosition);
            //

            var DropIndex = VM.ProtocolVM.SelectedIndex;
            if (SelectedIndex < 0)
                return;
            if (DropIndex < 0)
                return;
            if (SelectedIndex == DropIndex)
                return;
            if (VM.ProtocolVM.DragSelected)
            {
                int OldIndex = VM.ProtocolVM.Constraints[SelectedIndex].DisplayOrder;
                int inc = 1;
                if (SelectedIndex < DropIndex)
                    inc = -1;
                int CurrentIndex = DropIndex;
                while (CurrentIndex != SelectedIndex)
                {
                    int Switch  = VM.ProtocolVM.Constraints[CurrentIndex + inc].DisplayOrder;
                    VM.ProtocolVM.Constraints[CurrentIndex + inc].DisplayOrder = VM.ProtocolVM.Constraints[CurrentIndex].DisplayOrder;
                    VM.ProtocolVM.Constraints[CurrentIndex].DisplayOrder = Switch;
                    VM.ProtocolVM.Constraints.Move(CurrentIndex + inc, CurrentIndex);
                    CurrentIndex = CurrentIndex + inc;
                }
                
            }
        }
        int GetCurrentIndex(GetPositionDelegate getPosition)
        {
            int index = -1;
            for (int i = 0; i < ConstraintListView.Items.Count; ++i)
            {
                ListViewItem item = GetListViewItem(i, ConstraintListView);
                if (item != null)
                    if (this.IsMouseOverTarget(item, getPosition))
                    {
                        index = i;
                        break;
                    }
            }
            return index;
        }

        private void ConstraintsListView_MouseLeave(object sender, MouseEventArgs e)
        {
            // This method disables drag after the mouse leaves the listbox, so user needs to click on drag icon again to restart
            // However, if the mouse doen't leave the listbox, drag can still be accomplished by clicking the listboxitem

            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                var LV = ItemsControl.ItemsControlFromItemContainer((sender as ListViewItem)) as ListView; // all this to get the listbox!
                LV.AllowDrop = false;
                var VM = DataContext as MainViewModel;
                VM.ProtocolVM.DragSelected = false; // this needs to be at protocol level as it's used to suppress selection/expansion of constraint in XAML
                (sender as ListViewItem).MouseLeave += ConstraintsListView_MouseLeave;
            }

        }



        // Structures draggin
        private void AliasList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ImageContentPresenter = (ContentPresenter)VisualTreeHelper.GetParent((sender as System.Windows.Controls.Image).Parent);
            var SelectedListBoxItem = (ListBoxItem)VisualTreeHelper.GetParent(ImageContentPresenter.Parent);
            var listbox = ItemsControl.ItemsControlFromItemContainer(SelectedListBoxItem) as ListBox; // all this to get the listbox!
            listbox.AllowDrop = true;

            (listbox.Parent as Grid).MouseLeave += AliasList_MouseLeave;
            (listbox.DataContext as StructureSelector).DragSelected = true;
        }
        private void AliasList_MouseLeave(object sender, MouseEventArgs e)
        {
            // This method disables drag after the mouse leaves the listbox, so user needs to click on drag icon again to restart
            // However, if the mouse doen't leave the listbox, drag can still be accomplished by clicking the listboxitem
            
            if (!(sender as Grid).IsMouseOver && Mouse.LeftButton == MouseButtonState.Released)
            {
                ListBox LB = (sender as Grid).Children.OfType<ListBox>().FirstOrDefault();
                LB.AllowDrop = false;
                var VM = (sender as Grid).DataContext as StructureSelector;
                VM.DragSelected = false;
                (sender as Grid).MouseLeave -= AliasList_MouseLeave;
            }
            
        }
        private void AliasList_DragOver(object sender, DragEventArgs e)
        {
            var listbox = (sender as ListBox);

            var VM = listbox.DataContext as StructureSelector;
            var SelectedIndex = GetCurrentAliasIndex(e.GetPosition, listbox);
            //

            var DropIndex = VM.SelectedAliasIndex;
            if (SelectedIndex < 0)
                return;
            if (DropIndex < 0)
                return;
            if (SelectedIndex == DropIndex)
                return;
            if (VM.DragSelected)
            {
                //int OldIndex = VM.Aliases[SelectedIndex].DisplayOrder;
                int inc = 1;
                if (SelectedIndex < DropIndex)
                    inc = -1;
                int CurrentIndex = DropIndex;
                while (CurrentIndex != SelectedIndex)
                {
                    VM.Aliases.Move(CurrentIndex + inc, CurrentIndex);
                    CurrentIndex = CurrentIndex + inc;
                }
            }
        }
        int GetCurrentAliasIndex(GetPositionDelegate getPosition, ListBox listbox)
        {
            int index = -1;
            for (int i = 0; i < listbox.Items.Count; ++i)
            {
                ListBoxItem item = GetListBoxItem(i, listbox);
                if (item != null)
                    if (this.IsMouseOverTarget(item, getPosition))
                    {
                        index = i;
                        break;
                    }
            }
            return index;
        }
        //private void StructureAliasList_MouseEnter(object sender, MouseEventArgs e)
        //{

        //    var VM = (sender as ListBox).DataContext as StructureSelector;
        //    if (e.LeftButton == MouseButtonState.Released && VM.DragSelected)
        //    {
        //        VM.DragSelected = false;
        //        (sender as ListBox).AllowDrop = false;
        //    }
        //}

        ListViewItem GetListViewItem(int index, ListView LV)
        {
            if (LV.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return LV.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        ListBoxItem GetListBoxItem(int index, ListBox LB)
        {
            if (LB.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return LB.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        }

        delegate Point GetPositionDelegate(IInputElement element);

        bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
        }

        private List<DependencyObject> hitResults = new List<DependencyObject>();
        //void ConstraintListView_DragLeave(object sender, DragEventArgs e)
        //{
        //    var gd = sender as Grid;

        //    if (gd == null) return;
        //    Point pt = e.GetPosition(this);
        //    hitResults.Clear();
        //    VisualTreeHelper.HitTest(gd, null, GridHitTestResultCallback,
        //        new PointHitTestParameters(pt));

        //    if (!hitResults.Contains(gd))
        //    {
        //        (DataContext as Presenter).Protocol.DragSelected = false;
        //        ConstraintListView.AllowDrop = false;
        //    }
        //}

        HitTestResultBehavior GridHitTestResultCallback(HitTestResult result)
        {
            hitResults.Add(result.VisualHit);
            return HitTestResultBehavior.Continue;
        }

        //private void ConstraintListView_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    if (this.IsMouseOverTarget(this, e.GetPosition))
        //    {
        //        (DataContext as Presenter).Protocol.DragSelected = false;
        //        ConstraintListView.AllowDrop = false;
        //    }

        //}
        //private void ConstraintListView_MouseEnter(object sender, MouseEventArgs e)
        //{

        //    if (e.LeftButton == MouseButtonState.Released && (DataContext as Presenter).Protocol.DragSelected)
        //    {
        //        (DataContext as Presenter).Protocol.DragSelected = false;
        //        ConstraintListView.AllowDrop = false;
        //    }
        //}
        //private void ConstraintListView_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    var VM = (sender as ListView).DataContext as Presenter;
        //    VM.Protocol.DragSelected = false;
        //    ConstraintListView.AllowDrop = false;
        //}
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
