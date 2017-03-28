using System;
using System.Net;
using System.Windows;
using System.ComponentModel;

namespace WPLovefilm.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public ViewModelBase()
        {
            SetNoContentMessageVisibility(1);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool IsLoading { get; private set; }

        protected void SetLoadingStatus(bool status)
        {
            if (IsLoading != status)
            {
                IsLoading = status;
                NotifyPropertyChanged("IsLoading");
            }
        }

        public string StatusMessage { get; set; }
        public bool ShowStatusMessage { get; set; }

        protected void SetStatusMessage(string message)
        {
            ShowStatusMessage = true;
            NotifyPropertyChanged("ShowStatusMessage");

            StatusMessage = message;
            NotifyPropertyChanged("StatusMessage");
        }

        public Visibility NoContentMessageVisibility { get; private set; }

        protected void SetNoContentMessageVisibility(int itemCount)
        {
            if (itemCount < 1)
            {
                NoContentMessageVisibility = Visibility.Visible;
            }
            else
            {
                NoContentMessageVisibility = Visibility.Collapsed;
            }
            NotifyPropertyChanged("NoContentMessageVisibility");
        }

    }
}
