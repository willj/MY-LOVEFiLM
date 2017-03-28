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
using WPLovefilm.Models;
using Microsoft.Phone.Shell;

namespace WPLovefilm.Views
{
    public partial class RentedTitles : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;
        private RentedTitlesViewModel VM;

        public RentedTitles()
        {
            InitializeComponent();

            App.SetSystemTray(this);

            _isNewPageInstance = true;
            
            VM = new RentedTitlesViewModel();
            DataContext = VM;

            VM.PropertyChanged += VM_PropertyChanged;

            Loaded += new RoutedEventHandler(RentedTitles_Loaded);

        }

        void RentedTitles_Loaded(object sender, RoutedEventArgs e)
        {
            if (VM.RentedTitles.Count < 1)
            {
                VM.LoadTitles();
            }
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

        private void AppBarPrevPage(object sender, EventArgs e)
        {
            VM.PrevPage();
        }

        private void AppBarNextPage(object sender, EventArgs e)
        {
            VM.NextPage();
        }

        private void TitleSelected(object sender, SelectionChangedEventArgs e)
        {
            if (RentedListBox.SelectedItem != null)
            {
                App.SelectedTitle = (LFTitle)RentedListBox.SelectedItem;
                NavigationService.Navigate(new Uri("/Views/ViewTitle.xaml", UriKind.Relative));
            }
            RentedListBox.SelectedIndex = -1;
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

    }
}