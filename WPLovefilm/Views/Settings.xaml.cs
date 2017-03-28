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
using Microsoft.Phone.Controls;
using WPLovefilm.Helpers;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;

namespace WPLovefilm.Views
{
    public partial class Settings : PhoneApplicationPage
    {
        private BackgroundAgentHelper agentHelper;
        private bool ignoreCheckBoxAction = true;

        public Settings()
        {
            InitializeComponent();

            App.SetSystemTray(this);

            Loaded += new RoutedEventHandler(Settings_Loaded);
        }

        void Settings_Loaded(object sender, RoutedEventArgs e)
        {
            agentHelper = new BackgroundAgentHelper();

            if (agentHelper.AgentStatus())
            {
                NotificationStatus.IsChecked = true;
            }
            else
            {
                NotificationStatus.IsChecked = false;
            }

            ignoreCheckBoxAction = false;
        }

        private void NotificationStatus_Checked(object sender, RoutedEventArgs e)
        {
            if (ignoreCheckBoxAction)
            {
                return;
            }

            IsolatedStorageSettings.ApplicationSettings["NewTitleNotifications"] = true;
            IsolatedStorageSettings.ApplicationSettings.Save();

            agentHelper.EnableAgent();
        }

        private void NotificationStatus_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ignoreCheckBoxAction)
            {
                return;
            }

            IsolatedStorageSettings.ApplicationSettings["NewTitleNotifications"] = false;
            IsolatedStorageSettings.ApplicationSettings.Save();

            agentHelper.DisableAgent();
        }

        private void TestAgent_Click(object sender, RoutedEventArgs e)
        {
            agentHelper.TestAgent();
        }

    }
}