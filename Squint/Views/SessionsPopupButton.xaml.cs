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

namespace Squint.Views
{
    /// <summary>
    /// Interaction logic for PopupButton.xaml
    /// </summary>
    public partial class SessionsPopupButton : UserControl
    {
        public SessionsPopupButton()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(HorizontalPopup_Loaded);
        }
        private void HorizontalPopup_Loaded(object sender, RoutedEventArgs e)
        {
            Window w = Window.GetWindow(OpenSessionsButton);
            // w should not be Null now!
            if (null != w)
            {
                w.LocationChanged += delegate (object sender2, EventArgs args)
                {
                    var offset = myPopup.HorizontalOffset;
                    // "bump" the offset to cause the popup to reposition itself
                    //   on its own
                    myPopup.HorizontalOffset = offset + 1;
                    myPopup.HorizontalOffset = offset;
                };
                // Also handle the window being resized (so the popup's position stays
                //  relative to its target element if the target element moves upon 
                //  window resize)
                w.SizeChanged += delegate (object sender3, SizeChangedEventArgs e2)
                {
                    var offset = myPopup.HorizontalOffset;
                    myPopup.HorizontalOffset = offset + 1;
                    myPopup.HorizontalOffset = offset;
                };
            }
        }
    }
}
