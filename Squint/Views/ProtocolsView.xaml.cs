using Squint.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for ProtocolsView.xaml
    /// </summary>
    public partial class ProtocolsView : UserControl
    {
        public ProtocolsView()
        {
            InitializeComponent();
        }

        private void DragStructure_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var VM = DataContext as ProtocolsViewModel;
            StructuresTree.AllowDrop = true;
            VM.DragSelected = true;

            //var ImageContentPresenter = (ContentPresenter)(sender as System.Windows.Controls.Image).TemplatedParent;
            //var SelectedListBoxItem = (ListBoxItem)VisualTreeHelper.GetParent(ImageContentPresenter.Parent);

        }
        //private void DragStructure_ListView_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    // This method disables drag after the mouse leaves the listbox, so user needs to click on drag icon again to restart
        //    // However, if the mouse doen't leave the listbox, drag can still be accomplished by clicking the listboxitem

        //    if (Mouse.LeftButton == MouseButtonState.Released)
        //    {
        //        var LV = ItemsControl.ItemsControlFromItemContainer((sender as ListViewItem)) as ListView; // all this to get the listbox!
        //        LV.AllowDrop = false;
        //        var VM = DataContext as MainViewModel;
        //        VM.ProtocolVM.DragSelected = false; // this needs to be at protocol level as it's used to suppress selection/expansion of constraint in XAML
        //        (sender as ListViewItem).MouseLeave -= DragStructure_ListView_MouseLeave;
        //        (sender as ListViewItem).IsSelected = false;
        //    }
        //}

        delegate Point GetPositionDelegate(IInputElement element);
        int DragStructure_GetCurrentIndex(GetPositionDelegate getPosition)
        {
            int index = -1;
            for (int i = 0; i < StructuresTree.Items.Count; ++i)
            {
                ListViewItem item = GetListViewItem(i, StructuresTree);
                if (item != null)
                    if (this.IsMouseOverTarget(item, getPosition))
                    {
                        index = i;
                        break;
                    }
            }
            return index;
        }

        // Constraints dragging
        private async void DragConstraint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var VM = DataContext as ProtocolsViewModel;
            ConstraintListView.AllowDrop = true;
            VM.DragSelected = true;

            //var ImageContentPresenter = (ContentPresenter)(sender as System.Windows.Controls.Image).TemplatedParent;
            //var SelectedListBoxItem = (ListBoxItem)VisualTreeHelper.GetParent(ImageContentPresenter.Parent);
        }

        private void DragConstraint_Drop(object sender, DragEventArgs e)
        {
            var LV = sender as ListView; // all this to get the listbox!
            if (LV.AllowDrop && Mouse.LeftButton == MouseButtonState.Released)
            {
                LV.AllowDrop = false;
                var VM = DataContext as ProtocolsViewModel;
                VM.DragSelected = false; // this needs to be at protocol level as it's used to suppress selection/expansion of constraint in XAML
            }
        }

        private void DragConstraint_ListView_DragOver(object sender, DragEventArgs e)
        {
            var VM = DataContext as ProtocolsViewModel;
            var SelectedIndex = DragConstraint_GetCurrentIndex(e.GetPosition);
            //

            var DropIndex = VM.SelectedIndex;
            if (SelectedIndex < 0)
                return;
            if (DropIndex < 0)
                return;
            if (SelectedIndex == DropIndex)
                return;
            if (VM.DragSelected)
            {
                int OldIndex = VM.ConstraintViewModels[SelectedIndex].DisplayOrder;
                int inc = 1;
                if (SelectedIndex < DropIndex)
                    inc = -1;
                int CurrentIndex = DropIndex;
                while (CurrentIndex != SelectedIndex)
                {
                    int Switch = VM.ConstraintViewModels[CurrentIndex + inc].DisplayOrder;
                    VM.ConstraintViewModels[CurrentIndex + inc].DisplayOrder = VM.ConstraintViewModels[CurrentIndex].DisplayOrder;
                    VM.ConstraintViewModels[CurrentIndex].DisplayOrder = Switch;
                    VM.ConstraintViewModels.Move(CurrentIndex + inc, CurrentIndex);
                    CurrentIndex = CurrentIndex + inc;
                }

            }
        }
        int DragConstraint_GetCurrentIndex(GetPositionDelegate getPosition)
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

        private void Constraint_ListView_MouseLeave(object sender, MouseEventArgs e)
        {
            // This method disables drag after the mouse leaves the listbox, so user needs to click on drag icon again to restart
            // However, if the mouse doen't leave the listbox, drag can still be accomplished by clicking the listboxitem

            var LV = sender as ListView; // all this to get the listbox!
            if (LV.AllowDrop && Mouse.LeftButton == MouseButtonState.Released)
            {
                LV.AllowDrop = false;
                var VM = DataContext as ProtocolsViewModel;
                VM.DragSelected = false; // this needs to be at protocol level as it's used to suppress selection/expansion of constraint in XAML
            }

        }



        // Alias list dragging
        private void AliasList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ImageContentPresenter = (ContentPresenter)VisualTreeHelper.GetParent((sender as System.Windows.Controls.Image).Parent);
            var SelectedListBoxItem = (ListBoxItem)VisualTreeHelper.GetParent(ImageContentPresenter.Parent);
            var listbox = ItemsControl.ItemsControlFromItemContainer(SelectedListBoxItem) as ListBox; // all this to get the listbox!
            listbox.AllowDrop = true;

            (listbox.Parent as Grid).MouseLeave += AliasList_MouseLeave;
            (listbox.DataContext as StructureViewModel).DragSelected = true;
        }
        private void AliasList_MouseLeave(object sender, MouseEventArgs e)
        {
            // This method disables drag after the mouse leaves the listbox, so user needs to click on drag icon again to restart
            // However, if the mouse doen't leave the listbox, drag can still be accomplished by clicking the listboxitem

            if (!(sender as Grid).IsMouseOver && Mouse.LeftButton == MouseButtonState.Released)
            {
                ListBox LB = (sender as Grid).Children.OfType<ListBox>().FirstOrDefault();
                LB.AllowDrop = false;
                var VM = (sender as Grid).DataContext as StructureViewModel;
                VM.DragSelected = false;
                (sender as Grid).MouseLeave -= AliasList_MouseLeave;
            }

        }
        private void AliasList_DragOver(object sender, DragEventArgs e)
        {
            var listbox = (sender as ListBox);

            var VM = listbox.DataContext as StructureViewModel;
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

        bool IsMouseOverTarget(Visual target, GetPositionDelegate getPosition)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = getPosition((IInputElement)target);
            return bounds.Contains(mousePos);
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


        private void DragStructure_ListView_DragOver(object sender, DragEventArgs e)
        {
            var VM = DataContext as ProtocolsViewModel;
            var SelectedIndex = DragStructure_GetCurrentIndex(e.GetPosition);
            //

            var DropIndex = VM.SelectedStructureIndex;
            if (SelectedIndex < 0)
                return;
            if (DropIndex < 0)
                return;
            if (SelectedIndex == DropIndex)
                return;
            if (VM.DragSelected)
            {
                int OldIndex = VM.Structures[SelectedIndex].DisplayOrder;
                int inc = 1;
                if (SelectedIndex < DropIndex)
                    inc = -1;
                int CurrentIndex = DropIndex;
                while (CurrentIndex != SelectedIndex)
                {
                    int Switch = VM.Structures[CurrentIndex + inc].DisplayOrder;
                    VM.Structures[CurrentIndex + inc].DisplayOrder = VM.Structures[CurrentIndex].DisplayOrder;
                    VM.Structures[CurrentIndex].DisplayOrder = Switch;
                    VM.Structures.Move(CurrentIndex + inc, CurrentIndex);
                    CurrentIndex = CurrentIndex + inc;
                }

            }
        }
    }
}
