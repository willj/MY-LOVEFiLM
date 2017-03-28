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
using WPLovefilm.ViewModels;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace WPLovefilm.Views
{
    public partial class ViewAtHomeTitle : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;

        private AtHomeTitleViewModel VM;

        public ViewAtHomeTitle()
        {
            InitializeComponent();

            App.SetSystemTray(this);

            _isNewPageInstance = true;

            VM = new AtHomeTitleViewModel();
            VM.Title = App.SelectedAtHomeTitle;
            VM.AtHomeTitle = App.SelectedAtHomeTitle;

            VM.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(VM_PropertyChanged);

            VM.GetTrailer();

            DataContext = VM;
        }

        void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Trailer")
            {
                ApplicationBarIconButton trailerButton = ApplicationBar.Buttons[2] as ApplicationBarIconButton;
                trailerButton.IsEnabled = true;
            }
        }

        private void RateTitle(object sender, EventArgs e)
        {
            RatingPopup.IsOpen = !RatingPopup.IsOpen;
        }

        private void SubmitRating(object sender, RoutedEventArgs e)
        {
            RatingPopup.IsOpen = false;
            VM.RateTitle();
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (RatingPopup.IsOpen)
            {
                RatingPopup.IsOpen = false;
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private void PlayTrailer(object sender, EventArgs e)
        {
            VM.PlayTrailer();
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                VM.SaveState(State);
            }

            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_isNewPageInstance)
            {
                VM.RestoreState(State);
            }

            _isNewPageInstance = false;

            base.OnNavigatedTo(e);
        }

        private void ShowMoreSynopsis_Click(object sender, RoutedEventArgs e)
        {
            Synopsis.MaxHeight = 2000;
            ShowMoreSynopsis.Visibility = Visibility.Collapsed;
        }

        private void ShareTitle(object sender, EventArgs e)
        {
            VM.ShareTitle();
        }

    }
}