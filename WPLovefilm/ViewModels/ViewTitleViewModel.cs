using System;
using System.Net;
using System.Windows;
using WPLovefilm.Models;
using WPLovefilm.Service;
using WPLovefilm.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Net.NetworkInformation;

namespace WPLovefilm.ViewModels
{
    public class ViewTitleViewModel : ViewModelBase
    {
        public LFTitle Title { get; set; }
        public LFQueueTitle QueueTitle { get; set; }
        public float UserRating { get; set; }
        public ObservableCollection<LFQueue> Queues { get; private set; }
        public bool QueueListLoaded { get; private set; }
        public int SelectedQueueIndex { get; set; }
        protected Queue queueService;
        private bool QueueTitleLoaded { get; set; }
        public bool QueueTitleDisplayed { get; set; }
        public Visibility ShowQueuePriority { get; set; }
        private bool referrerIsQueue;
        public bool ReferrerIsQueue
        {
            get
            {
                return referrerIsQueue;
            }
            set
            {
                referrerIsQueue = value;
                if (value == true)
                {
                    ShowQueuePriority = Visibility.Visible;
                    NotifyPropertyChanged("ShowQueuePriority");
                }
            }
        }
        

        public ViewTitleViewModel()
        {
            ShowQueuePriority = Visibility.Collapsed;
            ReferrerIsQueue = false;
            SelectedQueueIndex = -1;
            QueueListLoaded = false;
            QueueTitleLoaded = false;
            QueueTitleDisplayed = false;
            QueueTitle = new LFQueueTitle();
            QueueTitle.Priority = 2;
            Queues = new ObservableCollection<LFQueue>();
            queueService = new Queue();
        }

        public void GetTrailer()
        {
            if (Title.Trailer == null)
            {
                PNTrailerService.GetTrailer(Title.Id, (trailer) =>
                {
                    if (!string.IsNullOrEmpty(trailer.LowTrailer) || !string.IsNullOrEmpty(trailer.HighTrailer))
                    {
                        Title.Trailer = trailer;
                        NotifyPropertyChanged("Trailer");
                    }
                });
            }
            else
            {
                NotifyPropertyChanged("Trailer");
            }

        }

        public void IsTitleInQueue()
        {
            if (QueueTitleLoaded)
            {
                return;
            }

            queueService.CheckQueueTitle(Account.Instance.GetQueuesUrl() + "/every", Title.Id, (status, queueTitle) =>
            {
                QueueTitleLoaded = true;
                
                if (!string.IsNullOrEmpty(queueTitle.InQueueId))
                {
                    DisplayQueueTitle(queueTitle);
                }
            });
        }

        private void DisplayQueueTitle(LFQueueTitle queueTitle)
        {
            QueueTitle = queueTitle;

            NotifyPropertyChanged("QueueTitle");

            ShowQueuePriority = Visibility.Visible;
            NotifyPropertyChanged("ShowQueuePriority");

            LoadQueueList(queueTitle.QueueId);
        }

        public void LoadQueueList(string selectedQueue = "default")
        {
            if (!QueueListLoaded)
                {
                    SetLoadingStatus(true);

                    if (queueService == null)
                    {
                        queueService = new Queue();
                    }

                    queueService.GetQueueList(Account.Instance.GetQueuesUrl(), (status, myLists) =>
                    {
                        SetLoadingStatus(false);
                        Queues.Clear();

                        int i = 0;

                        foreach (LFQueue list in myLists)
                        {
                            Queues.Add(list);

                            if (selectedQueue == "default")
                            {
                                if (list.Default)
                                {
                                    SelectedQueueIndex = i;
                                }
                            }
                            else
                            {
                                if (list.Url == selectedQueue)
                                {
                                    SelectedQueueIndex = i;
                                }
                            }
                            i++;
                        }

                        QueueListLoaded = true;
                        NotifyPropertyChanged("QueueListLoaded");
                        NotifyPropertyChanged("SelectedQueueIndex");
                    }, true);
                }
        }

        public void AddToQueue()
        {
            SetLoadingStatus(true);

            if (queueService == null)
            {
                queueService = new Queue();
            }

            queueService.AddToQueue(Queues[SelectedQueueIndex].Url, Title.Id, QueueTitle.Priority, (status) =>
            {
                SetLoadingStatus(false);

                if (status){

                    SetStatusMessage(Title.TitleName + " was added to your list");

                    QueueTitleLoaded = false;
                    IsTitleInQueue();
                }
                else
                {
                    SetStatusMessage(Title.TitleName + " could not be added to your list, have another go");
                }

            });
        }

        public void RateTitle()
        {
            SetLoadingStatus(true);

            Service.Title ts = new Title();
            ts.RateTitle(Title.Id, UserRating, (status) =>
            {
                SetLoadingStatus(false);

                if (status)
                {
                    SetStatusMessage("You rated " + Title.TitleName + " " + UserRating.ToString() + " stars");
                }
                else
                {
                    SetStatusMessage("Oops, your rating didn't get saved, have another go");
                }
            });
        }

        
        public void PlayTrailer()
        {
            string trailerUrl = string.Empty;

            if(!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Please connect to a network to play a video");
                return;
            }

            switch (NetworkInterface.NetworkInterfaceType)
	        {
                case NetworkInterfaceType.Ethernet:
                case NetworkInterfaceType.Wireless80211:
                      if(!string.IsNullOrEmpty(Title.Trailer.HighTrailer)){
                          trailerUrl = Title.Trailer.HighTrailer;
                      }
                      else if (!string.IsNullOrEmpty(Title.Trailer.LowTrailer))
                      {
                          trailerUrl = Title.Trailer.LowTrailer;
                      }
                break;

                case NetworkInterfaceType.MobileBroadbandCdma:
                case NetworkInterfaceType.MobileBroadbandGsm:
                    if (!string.IsNullOrEmpty(Title.Trailer.LowTrailer))
                    {
                        trailerUrl = Title.Trailer.LowTrailer;
                    }
                    else if (!string.IsNullOrEmpty(Title.Trailer.HighTrailer))
                    {
                    trailerUrl = Title.Trailer.HighTrailer;
                    }
                break;
                default:
                    return;
	        }

            if (string.IsNullOrEmpty(trailerUrl))
            {
                MessageBox.Show("Sorry, we don't have a trailer for this title");
                return;
            }

            if (!MediaPlayer.GameHasControl)
            {
                MessageBoxResult result = MessageBox.Show("Watching a trailer will pause the music currently playing, do you want to continue?", "", MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            try
            {
                MediaPlayerLauncher mp = new MediaPlayerLauncher();
                mp.Media = new Uri(trailerUrl, UriKind.Absolute);
                FrameworkDispatcher.Update();
                mp.Show();
            }
            catch (Exception)
            {
                MessageBox.Show("Oops, we couldn't load that trailer, try again.");
            }
        }

        public void RemoveTitle(Action<bool> callback)
        {
            if (queueService == null)
            {
                queueService = new Queue();
            }

            SetLoadingStatus(true);

            queueService.RemoveFromQueue(QueueTitle.InQueueId, (status) =>
            {
                SetLoadingStatus(false);
                callback(status);
            });
        }

        public void SaveQueueTitle()
        {
            if (queueService == null)
            {
                queueService = new Queue();
            }

            SetLoadingStatus(true);

            queueService.SaveQueueTitle(QueueTitle.InQueueId, Queues[SelectedQueueIndex].Url, QueueTitle.Priority, (status) =>
            {
                SetLoadingStatus(false);

                if (status)
                {
                    SetStatusMessage("Your changes have been saved");
                    NotifyPropertyChanged("QueueTitle");
                }
                else
                {
                    SetStatusMessage("Your changes could not be saved");
                }
            });
        }

        public void ShareTitle()
        {
            if (Title.WebUrl == "")
            {
                MessageBox.Show("Sorry, this title can't be shared.");
                return;
            }

            try
            {
                ShareLinkTask st = new ShareLinkTask();
                st.Title = Title.TitleName;
                st.LinkUri = new Uri(Title.WebUrl, UriKind.Absolute);

                if (UserRating > 0)
                {
                    st.Message = "I rated " + Title.TitleName + " " + UserRating.ToString() + "/5";
                }
                else
                {
                    st.Message = Title.TitleName;    
                }

                st.Show();
            }
            catch (Exception)
            {
                MessageBox.Show("Oops, something went wrong, try that again");
            }
        }

        #region State

        public void SaveState(IDictionary<string, object> state)
        {
            state["Trailer"] = Title.Trailer;
            state["QueueTitleLoaded"] = QueueTitleLoaded;

            if (!ReferrerIsQueue && !string.IsNullOrEmpty(QueueTitle.InQueueId))
            {
                state["QueueTitle"] = QueueTitle;
            }
        }

        public void RestoreState(IDictionary<string, object> state)
        {
            if (state.ContainsKey("Trailer"))
            {
                this.Title.Trailer = (PNTrailer)state["Trailer"];
                NotifyPropertyChanged("Trailer");
            }

            if (state.ContainsKey("QueueTitle"))
            {
                LFQueueTitle queueTitle = (LFQueueTitle)state["QueueTitle"];

                DisplayQueueTitle(queueTitle);
            }

            if (state.ContainsKey("QueueTitleLoaded"))
            {
                this.QueueTitleLoaded = (bool)state["QueueTitleLoaded"];
            }
        }

        #endregion

    }
}
