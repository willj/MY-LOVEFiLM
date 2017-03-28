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
    public partial class TitleListing : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;
        private TitleListingViewModel VM;
        public UserControls.FormatDialog FormatDialog;
        public UserControls.HotlistFilterDialog FilterDialog;

        public TitleListing()
        {
            InitializeComponent();

            App.SetSystemTray(this);

            _isNewPageInstance = true; 

            VM = new TitleListingViewModel();
            DataContext = VM;

            VM.TitlesLoaded += VM_TitlesLoaded;
            VM.PropertyChanged += VM_PropertyChanged;
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

        void VM_TitlesLoaded()
        {
            switch (ListingPivot.SelectedIndex)
            {
                case 0:
                    AllTitleListBox.ScrollIntoView(AllTitleListBox.Items[0]);
                    break;
                case 1:
                    FilmTitleListBox.ScrollIntoView(FilmTitleListBox.Items[0]);
                    break;
                case 2:
                    TVTitleListBox.ScrollIntoView(TVTitleListBox.Items[0]);
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_isNewPageInstance)
            {
                VM.InitData(NavigationContext.QueryString);

                VM.RestoreState(State);

                if (State.ContainsKey("ListingPivotIndex"))
                {
                    ListingPivot.SelectedIndex = (int)State["ListingPivotIndex"];
                }
            }

            _isNewPageInstance = false;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                State["ListingPivotIndex"] = ListingPivot.SelectedIndex;

                VM.SaveState(State);
            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (FormatPopup.IsOpen)
            {
                FormatPopup.IsOpen = false;
                ApplicationBar.IsVisible = true;
                e.Cancel = true;
            }

            if (FilterPopup.IsOpen)
            {
                FilterPopup.IsOpen = false;
                ApplicationBar.IsVisible = true;
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private void UpdateSearchType(object sender, SelectionChangedEventArgs e)
        {
            VM.ClearTitles();

            switch (ListingPivot.SelectedIndex)
            {
                case 0:
                    VM.SetSearchType(LFCatalogSearchType.Title);
                    break;
                case 1:
                    VM.SetSearchType(LFCatalogSearchType.Film);
                    break;
                case 2:
                    VM.SetSearchType(LFCatalogSearchType.TV);
                    break;
            }
            VM.LoadTitles();
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
            if(FormatDialog == null){
                FormatDialog = new UserControls.FormatDialog();
                FormatDialog.FormatSettingsSaved += FormatSettingsSaved;
                FormatPopup.Child = FormatDialog;
            }

            ApplicationBar.IsVisible = false;
            FormatPopup.IsOpen = true;
        }

        private void FormatSettingsSaved()
        {
            VM.LoadTitles();
            FormatPopup.IsOpen = false;
            ApplicationBar.IsVisible = true;
        }

        private void TitleSelected(object sender, SelectionChangedEventArgs e)
        {
            ListBox currentListbox = (ListBox)sender;

            if (currentListbox.SelectedItem != null)
            {
                App.SelectedTitle = (LFTitle)currentListbox.SelectedItem;
                NavigationService.Navigate(new Uri("/Views/ViewTitle.xaml", UriKind.Relative));
            }
            currentListbox.SelectedIndex = -1;
        }

        private void OpenFilterDialog(object sender, EventArgs e)
        {
            if (FilterDialog == null)
            {
                FilterDialog = new UserControls.HotlistFilterDialog(VM.RefineType);
                FilterDialog.FilterSettingsSaved += FilterSettingsSaved;
                FilterPopup.Child = FilterDialog;
            }

            ApplicationBar.IsVisible = false;
            FilterPopup.IsOpen = true;
        }

        private void FilterSettingsSaved()
        {
            VM.LoadTitles();
            FilterPopup.IsOpen = false;
            ApplicationBar.IsVisible = true;
        }

    }
}