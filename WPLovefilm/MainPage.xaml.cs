using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using WPLovefilm.ViewModels;
using WPLovefilm.Models;
using WPLovefilm.Helpers;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using System.Text;

namespace WPLovefilm
{
    public partial class MainPage : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;
        public MainViewModel VM;
        private bool initialised = false;
        private bool messagesChecked = false;

        // Constructor
        public MainPage()
        {
            _isNewPageInstance = true;
            InitializeComponent();

            App.SetSystemTray(this);

            VM = new MainViewModel(App.VersionNumber);
            DataContext = VM;

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Account.Instance.AccountTokenExists())
            {
                VM.InitData();
                Account.Instance.LogVersion(App.VersionNumber);
                initialised = true;
            }
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                //Save State

                State["HomePanoramaSelectedIndex"] = HomePanorama.SelectedIndex;
                State["SearchTextBox"] = SearchTextBox.Text;
                State["MessagesChecked"] = messagesChecked;

                VM.SaveState(State);
            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_isNewPageInstance)
            {
                if (State.ContainsKey("MessagesChecked"))
                {
                    messagesChecked = (bool)State["MessagesChecked"];
                }

                if (!messagesChecked)
                {
                    MessageHelper m = new MessageHelper(Account.Instance.AccountTokenExists());
                    m.GetMessages();
                    messagesChecked = true;
                }
            }

            if (!initialised)
            {
                //Show Login Screen
                if (!Account.Instance.AccountTokenExists())
                {
                    LoginPopup.IsOpen = true;
                    //Should we return here so nothing below is executed???
                    base.OnNavigatedTo(e);
                    return;
                }
            }

            if (_isNewPageInstance)
            {
                int selectedIndex = 0;

                if (State.ContainsKey("HomePanoramaSelectedIndex"))
                {
                    selectedIndex = (int)State["HomePanoramaSelectedIndex"];
                    HomePanorama.DefaultItem = HomePanorama.Items[selectedIndex];
                }
                else
                {
                    selectedIndex = HomePanorama.SelectedIndex;
                }

                VM.RestoreState(State);

                if (selectedIndex == 2)
                {
                    VM.LoadMyLists();
                }

                if (State.ContainsKey("SearchTextBox"))
                {
                    SearchTextBox.Text = (string)State["SearchTextBox"];
                }
            }
            else
            {
                if (HomePanorama.SelectedIndex == 2)
                {
                    VM.LoadMyLists();
                }
            }

            _isNewPageInstance = false;

            base.OnNavigatedTo(e);
        }

        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HomePanorama.SelectedIndex == 2)
            {
                VM.LoadMyLists();
            }
        }

        #region Selected Items

        private void QueueSelected(object sender, SelectionChangedEventArgs e)
        {
            if (MyQueuesListBox.SelectedItem != null)
            {
                LFQueueBase SelectedQueue = (LFQueueBase)MyQueuesListBox.SelectedItem;

                if (SelectedQueue.Type == LFQueueTypes.RentalQueue)
                {
                    App.SelectedQueue = (LFQueue)MyQueuesListBox.SelectedItem;

                    NavigationService.Navigate(new Uri("/Views/ViewQueue.xaml", UriKind.Relative));
                }
                else if (SelectedQueue.Type == LFQueueTypes.RentedTitles)
                {
                    NavigationService.Navigate(new Uri("/Views/RentedTitles.xaml", UriKind.Relative));
                }
            }

            MyQueuesListBox.SelectedIndex = -1;
        }

        private void AtHomeItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (AtHomeListBox.SelectedItem != null)
            {
                App.SelectedAtHomeTitle = (LFAtHomeTitle)AtHomeListBox.SelectedItem;

                NavigationService.Navigate(new Uri("/Views/ViewAtHomeTitle.xaml", UriKind.Relative));
            }
            AtHomeListBox.SelectedIndex = -1;
        }

        #endregion

        #region search

        private void Search(object sender, RoutedEventArgs e)
        {
            StartSearch();
        }

        private void StartSearch()
        {
            NavigationService.Navigate(new Uri("/Views/SearchResults.xaml?type=Search&refine=Search&search=" + HttpUtility.UrlEncode(SearchTextBox.Text), UriKind.Relative));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Enter & !string.IsNullOrEmpty(SearchTextBox.Text))
            {
                StartSearch();
            }
        }

        #endregion

        #region Login/Logout

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/Login.xaml", UriKind.Relative));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult logoutResult = MessageBox.Show("Are you sure you want to logout of LOVEFiLM?" + Environment.NewLine + "You will need to login again to access your content.", "", MessageBoxButton.OKCancel);

            if (logoutResult == MessageBoxResult.OK)
            {
                VM.Logout();
                LoginPopup.IsOpen = true;
            }
        }

        #endregion  

        #region Review Feedback Help

        private void SubmitReview(object sender, RoutedEventArgs e)
        {
            try
            {
                MarketplaceReviewTask review = new MarketplaceReviewTask();
                review.Show();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Oops, something went wrong, try that again");
            }

        }

        private void FeedbackClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult r = MessageBox.Show("This address is for application support & feedback, to contact LOVEFiLM directly use the details at www.lovefilm.com. Do you want to continue?", "Note", MessageBoxButton.OKCancel);

            if (r == MessageBoxResult.OK)
            {
                try
                {
                    StringBuilder debugInfo = new StringBuilder();
                    debugInfo.AppendLine();
                    debugInfo.AppendLine();
                    debugInfo.AppendFormat("Current Date/Time: {0}", DateTime.Now.ToString());
                    debugInfo.AppendLine();
                    debugInfo.AppendFormat("OS Version: {0}", Environment.OSVersion.ToString());
                    debugInfo.AppendLine();
                    debugInfo.AppendFormat("Device: {0} {1}", Microsoft.Phone.Info.DeviceStatus.DeviceManufacturer, Microsoft.Phone.Info.DeviceStatus.DeviceName);

                    EmailComposeTask email = new EmailComposeTask();
                    email.To = "";	// Removed
                    email.Subject = "MY LOVEFiLM MOBILE Feedback/Support Request " + App.VersionNumber;
                    email.Body = debugInfo.ToString();
                    email.Show();
                }
                catch (InvalidOperationException)
                {
                    MessageBox.Show("Oops, something went wrong, try that again");
                }
            }
        }

        private void HelpClick(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserTask wb = new WebBrowserTask();
                wb.Uri = new Uri("http://www.madebywill.net/lovefilm/mob_faq.htm");
                wb.Show();
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Oops, something went wrong, try that again");
            }

        }

        #endregion

    }
}