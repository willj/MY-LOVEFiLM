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
    public partial class ViewQueue : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;
        private QueueViewModel VM;

        public ViewQueue()
        {
            InitializeComponent();

            App.SetSystemTray(this);

            _isNewPageInstance = true;

            VM = new QueueViewModel();
            VM.CurrentQueue = App.SelectedQueue;
            DataContext = VM;

            VM.TitlesLoaded += VM_TitlesLoaded;
            VM.PropertyChanged += VM_PropertyChanged;

            Loaded += ViewQueue_Loaded;
        }

        void ViewQueue_Loaded(object sender, RoutedEventArgs e)
        {
            if (VM.QueueTitles.Count < 1)
            {
                VM.LoadTitles();
                App.ForceReloadQueue = false;
            }

            if (App.ForceReloadQueue == true)
            {
                VM.ReloadTitles();
                App.ForceReloadQueue = false;
            }

        }

        void VM_TitlesLoaded()
        {
            QueueListBox.ScrollIntoView(QueueListBox.Items[0]);
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
            if (QueueListBox.SelectedItem != null)
            {
                App.SelectedQueueTitle = (LFQueueTitle)QueueListBox.SelectedItem;
                NavigationService.Navigate(new Uri("/Views/ViewTitle.xaml?referrer=queue", UriKind.Relative));
            }
            QueueListBox.SelectedIndex = -1;
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