using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace WPLovefilm.UserControls
{
    public partial class StatusMessageControl : UserControl
    {
        public StatusMessageControl()
        {
            InitializeComponent();
        }

        private void StatusChanged(object sender, TextChangedEventArgs e)
        {
            ShowStatus.Completed += ShowStatus_Completed;
            
            ShowStatus.Begin();
        }

        void ShowStatus_Completed(object sender, EventArgs e)
        {
            StatusPopup.IsOpen = false;
            HideStatus.Begin();
            ShowStatus.Completed -= ShowStatus_Completed;
        }
    }
}
