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
    public partial class ViewTitle : PhoneApplicationPage
    {
        bool _isNewPageInstance = false;

        private ViewTitleViewModel VM;

        public ViewTitle()
        {
            InitializeComponent();

            App.SetSystemTray(this);

            _isNewPageInstance = true;

            VM = new ViewTitleViewModel();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (_isNewPageInstance)
            {
                string referrer = string.Empty;

                if (NavigationContext.QueryString.TryGetValue("referrer", out referrer))
                {
                    if (referrer == "queue")
                    {
                        VM.ReferrerIsQueue = true;
                    }
                }

                if (VM.ReferrerIsQueue)
                {
                    VM.Title = App.SelectedQueueTitle;
                    VM.QueueTitle = App.SelectedQueueTitle;
                    SwitchToInQueueTitle();
                }
                else
                {
                    VM.Title = App.SelectedTitle;
                }

                DataContext = VM;

                VM.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(VM_PropertyChanged);

                VM.RestoreState(State);

                VM.GetTrailer();

                if (!VM.ReferrerIsQueue)
                {
                    VM.IsTitleInQueue();
                }

                ApplicationBarIconButton rentButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
                rentButton.IsEnabled = VM.Title.CanRent;
            }

            _isNewPageInstance = false;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                VM.SaveState(State);
            }

            base.OnNavigatedFrom(e);
        }

        void VM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Trailer")
            {
                ApplicationBarIconButton trailerButton = ApplicationBar.Buttons[3] as ApplicationBarIconButton;
                trailerButton.IsEnabled = true;
            }

            if (VM.QueueTitleDisplayed == false)
            {
                if (e.PropertyName == "QueueTitle")
                {
                    if (!string.IsNullOrEmpty(VM.QueueTitle.InQueueId))
                    {
                        SwitchToInQueueTitle();
                    }
                }
            }

            //Only do this if it would ever do anything!
            if (VM.SelectedQueueIndex > 1)
            {
                if (e.PropertyName == "SelectedQueueIndex")
                {
                    QueueListBox.ScrollIntoView(QueueListBox.Items[VM.SelectedQueueIndex]);
                    QueueListBox.UpdateLayout();
                    QueueListBox.ScrollIntoView(QueueListBox.Items[VM.SelectedQueueIndex]);
                }
            }
        }

        private void SwitchToInQueueTitle()
        {
            ApplicationBarIconButton actionButton = ApplicationBar.Buttons[0] as ApplicationBarIconButton;
            actionButton.Text = "remove";
            actionButton.IconUri = new Uri("/Images/appbar.stop.rest.png", UriKind.Relative);

            ApplicationBarMenuItem manageMenu = new ApplicationBarMenuItem("manage");
            manageMenu.Click += ShowAddToListPopup;
            manageMenu.IsEnabled = true;

            ApplicationBar.MenuItems.Add(manageMenu);
            ApplicationBar.IsMenuEnabled = true;

            AddOrSaveButton.Content = "save";

            VM.QueueTitleDisplayed = true;
        }

        private void AddOrRemoveFromQueue(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(VM.QueueTitle.InQueueId))
            {
                // It's already in our queue, so remove it.

                MessageBoxResult confirm = MessageBox.Show("Are you sure you want to remove " + VM.QueueTitle.TitleName + " from your list?", "", MessageBoxButton.OKCancel);

                if (confirm == MessageBoxResult.OK)
                {
                    VM.RemoveTitle((status) =>
                    {
                        if (status)
                        {
                            App.ForceReloadQueuesList = true;
                            if (VM.ReferrerIsQueue)
                            {
                                App.ForceReloadQueue = true;
                            }

                            NavigationService.GoBack();
                        }
                    });
                }
            }
            else
            {
                ShowAddToListPopup(sender, e);
            }
        }

        private void ShowAddToListPopup(object sender, EventArgs e)
        {
            if (VM.ReferrerIsQueue)
            {
                VM.LoadQueueList(VM.QueueTitle.QueueId);
            }
            else
            {
                VM.LoadQueueList();
            }
            

            if (!AddToListPopup.IsOpen)
            {
                AddToListPopup.IsOpen = true;
                AddToListTransitionIn.Begin();
            }
            else
            {
                AddToListPopup.IsOpen = false;
            }
        }



        private void AddToQueue(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(VM.QueueTitle.InQueueId))
            {
                VM.SaveQueueTitle();
                if (VM.ReferrerIsQueue)
                {
                    App.ForceReloadQueue = true;
                }
            }
            else
            {
                VM.AddToQueue();
            }

            App.ForceReloadQueuesList = true;
            AddToListPopup.IsOpen = false;
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

            if (AddToListPopup.IsOpen)
            {
                AddToListPopup.IsOpen = false;
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private void PlayTrailer(object sender, EventArgs e)
        {
            VM.PlayTrailer();
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