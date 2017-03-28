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
using System.Windows.Navigation;
using WPLovefilm.Service;
using WPLovefilm.Models;
using WPLovefilm.ViewModels;
using Microsoft.Phone.Shell;

namespace WPLovefilm.Views
{
    public partial class GameListing : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;
        private TitleListingViewModel VM;
        public UserControls.FormatDialog FormatDialog;

        private bool hasInitialised = false;

        public GameListing()
        {
            InitializeComponent();

            App.SetSystemTray(this);

            _isNewPageInstance = true;

            VM = new TitleListingViewModel();
            DataContext = VM;

            VM.TitlesLoaded += VM_TitlesLoaded;
            VM.PropertyChanged += VM_PropertyChanged;
            this.Loaded += GameListing_Loaded;
        }

        void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CanPageBack")
            {
                ApplicationBarIconButton back = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                back.IsEnabled = VM.CanPageBack;
            }
            else if (e.PropertyName == "CanPageForward")
            {
                ApplicationBarIconButton fwd = ApplicationBar.Buttons[1] as ApplicationBarIconButton;
                fwd.IsEnabled = VM.CanPageForward;
            }
        }

        void GameListing_Loaded(object sender, RoutedEventArgs e)
        {
            if (!hasInitialised)
            {
                VM.LoadTitles();
                hasInitialised = true;
            }
        }

        void VM_TitlesLoaded()
        {
            GameListBox.ScrollIntoView(GameListBox.Items[0]);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                VM.SaveState(State);
            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isNewPageInstance)
            {
                VM.InitData(NavigationContext.QueryString);

                VM.RestoreState(State);
            }

            _isNewPageInstance = false;

            base.OnNavigatedTo(e);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (FormatPopup.IsOpen)
            {
                FormatPopup.IsOpen = false;
                ApplicationBar.IsVisible = true;
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private void AppBarPrevPage(object sender, EventArgs e)
        {
            VM.PrevPage();
        }

        private void AppBarNextPage(object sender, EventArgs e)
        {
            VM.NextPage();
        }

        private void OpenFormatDialog(object sender, EventArgs e)
        {
            if (FormatDialog == null)
            {
                FormatDialog = new UserControls.FormatDialog();
                FormatDialog.FormatSettingsSaved += FormatSettingsSaved;
                FormatPopup.Child = FormatDialog;
            }

            ApplicationBar.IsVisible = false;
            FormatPopup.IsOpen = true;
        }

        void FormatSettingsSaved()
        {
            VM.LoadTitles();
            FormatPopup.IsOpen = false;
            ApplicationBar.IsVisible = true;
        }

        private void TitleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (GameListBox.SelectedItem != null)
            {
                App.SelectedTitle = (LFTitle)GameListBox.SelectedItem;
                NavigationService.Navigate(new Uri("/Views/ViewTitle.xaml", UriKind.Relative));
            }
            GameListBox.SelectedIndex = -1;
        }

    }
}