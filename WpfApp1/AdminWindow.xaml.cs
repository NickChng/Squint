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
using ESAPI = VMS.TPS.Common.Model.API.Application;
using System.Windows.Controls.Primitives;
using GongSolutions.Wpf.DragDrop;

namespace SquintScript
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                string debugme = "hi";
            }
            //StructuresTree.SelectedItemChanged += OnSelectedStructureChanged;
            //DbUser User = SquintDb.Context.DbUsers.Where(x => x.ARIA_ID == Ctr.SquintUser).SingleOrDefault();
            //if (User == null) // create new user
            //{
            //    MessageBox.Show("User not found, please contact Nick to add you...");
            //    //User = SquintDb.Context.DbUsers.Create();
            //    //User.ARIA_ID = Ctr.SquintUser;
            //    //User.PermissionGroupID = 2; // Dosimetry by default
            //    //SquintDb.Context.DbUsers.Add(User);
            //    //SquintDb.Context.SaveChanges();
            //}
        }

        private void OnSelectedStructureChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var VM = (Presenter)DataContext;
            var test = VM.Protocol.Structures;
            string debugme = "hi";
        }

        public int counter = 0;

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;

            if (tvi == null || e.Handled) return;

            tvi.IsExpanded = !tvi.IsExpanded;
            e.Handled = true;
        }

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
        //Import / Export of protocols
        private async void ImportProtocolDirectory(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog f = new System.Windows.Forms.FolderBrowserDialog();
            f.Description = "Please select import folder...";
            f.SelectedPath = @"\\srvnetapp02\bcca\docs\physics\cn\software\squint\xml protocol library\";
            Cursor = Cursors.Wait;
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string[] filenames = System.IO.Directory.GetFiles(f.SelectedPath);
                    List<Task> importTasks = new List<Task>();
                    int count = 0;
                    double total = filenames.Count();
                    foreach (string file in filenames)
                    {
                        string ext = System.IO.Path.GetExtension(file);
                        if (ext == ".xml")
                            try
                            {
                                count++;
                                importTasks.Add(Task.Run(() =>
                                {
                                    Ctr.ImportProtocolFromXML(file);
                                }));
                            }
                            catch
                            {
                                MessageBox.Show(string.Format("Error importing {0}", file));
                            }
                    }
                    await Task.WhenAll(importTasks);
                }
                catch (Exception ex)
                {
                    string debugme = "hi";
                }
            }
            Cursor = Cursors.Arrow;
            MessageBox.Show("Complete!");
        }

        private List<HitTestResult> hitResultsList = new List<HitTestResult>();

        private async void ImportProtocol(object sender, RoutedEventArgs e)
        {
            if (Ctr.PatientLoaded || Ctr.NumAssessments > 0)
            {
                System.Windows.Forms.DialogResult DR = System.Windows.Forms.MessageBox.Show("This will close the current protocol and any assessments. Any unsaved changes will be lost. Continue?", "Import from XML", System.Windows.Forms.MessageBoxButtons.YesNo);
                if (DR == System.Windows.Forms.DialogResult.No)
                    return;
            }
            Ctr.ClosePatient();
            Ctr.CloseProtocol();
            Cursor = Cursors.Wait;
            System.Windows.Forms.OpenFileDialog d = new System.Windows.Forms.OpenFileDialog();
            d.Title = "Open Ctr.GetProtocolView() File";
            d.Filter = "XML files|*.xml";
            d.InitialDirectory = @"\\srvnetapp02\bcca\docs\Physics\CN\Software\Squint\XML Protocol Library\v0.5 Library";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MessageBox.Show(d.FileName.ToString());
            }
            bool ImportSuccessful = await Task.Run(() => Ctr.ImportProtocolFromXML(d.FileName));
            if (ImportSuccessful)
                Cursor = Cursors.Arrow;
            else
            {
                Cursor = Cursors.Arrow;
                MessageBox.Show("Error in importing protocol, please review XML file");
            }
        }

        /// <summary>
        /// Real time update of the constraint position.  Most of the work is in getting DragSelected flag back to false under various conditions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void DragImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var VM = DataContext as Presenter;
            ConstraintListView.AllowDrop = true;
            VM.Protocol.DragSelected = true;
        }

        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            hitResultsList.Add(result);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }
        public HitTestFilterBehavior MyHitTestFilter(DependencyObject o)
        {
            // Test for the object value you want to filter.
            if (o.GetType() == typeof(System.Windows.Controls.Image))
            {
                // Visual object and descendants are NOT part of hit test results enumeration.
                return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
            }
            else
            {
                // Visual object is part of hit test results enumeration.
                return HitTestFilterBehavior.Continue;
            }
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
    }
}

