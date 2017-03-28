using System;
using System.Net;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using WPLovefilm.Models;
using WPLovefilm.Service;
using WPLovefilm.Helpers;
using System.IO.IsolatedStorage;

namespace WPLovefilm.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private Title lfTitleService;
        private Queue lfQueueService;
        private bool initComplete = false;

        public string VersionString { get; private set; }

        public ObservableCollection<LFAtHomeTitle> AtHomeTitles { get; private set; }
        public int TitlesAwaitingAllocation { get; private set; }
        public Visibility ShowAwaitingAllocationCount { get; private set; }

        public ObservableCollection<LFQueueBase> MyLists { get; private set; }
        private LFQueueBase rentedTitlesQueue;
        private List<LFQueue> rentalQueues;

        private BackgroundAgentHelper agentHelper;
        public Visibility ShowSettingsMenuItem { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="versionNumber"></param>
        public MainViewModel(string versionNumber) {
            VersionString = "Version " + versionNumber;

            AtHomeTitles = new ObservableCollection<LFAtHomeTitle>();
            ShowAwaitingAllocationCount = Visibility.Collapsed;
            MyLists = new ObservableCollection<LFQueueBase>();
            rentalQueues = new List<LFQueue>();
            ShowSettingsMenuItem = Visibility.Visible;
        }

        public void InitData(){
            if (initComplete == false)
            {
                LoadAtHomeTitles();
                initComplete = true;
            }
        }

        private void LoadAtHomeTitles()
        {
            if (lfTitleService == null)
            {
                lfTitleService = new WPLovefilm.Service.Title();
            }

            SetLoadingStatus(true);

            lfTitleService.GetAtHomeTitles(Account.Instance.GetAtHomeUrl(), (status, atHomeTitles, awaitingAlloc) =>
            {
                SetLoadingStatus(false);

                SetNoContentMessageVisibility(atHomeTitles.Count);

                AtHomeTitles.Clear();

                foreach (LFAtHomeTitle title in atHomeTitles)
                {
                    AtHomeTitles.Add(title);
                }

                SetTitlesAwaitingAllocation(awaitingAlloc);

                // Check Background Agent Status
                if (agentHelper == null)
                {
                    agentHelper = new BackgroundAgentHelper();

                    if (agentHelper.DeviceCanUseAgents())
                    {
                        agentHelper.CheckAgentStatus();
                    }
                    else
                    {
                        ShowSettingsMenuItem = Visibility.Collapsed;
                        NotifyPropertyChanged("ShowSettingsMenuItem");
                    }
                }
            });
        }

        private void SetTitlesAwaitingAllocation(int count)
        {
            if (count > 0)
            {
                TitlesAwaitingAllocation = count;
                ShowAwaitingAllocationCount = Visibility.Visible;
                NotifyPropertyChanged("TitlesAwaitingAllocation");
                NotifyPropertyChanged("ShowAwaitingAllocationCount");
            }
        }

        public void LoadMyLists()
        {
            if (rentalQueues.Count > 0 && App.ForceReloadQueuesList == false)
            {
                return;
            }

            App.ForceReloadQueuesList = false;

            if (lfQueueService == null)
            {
                lfQueueService = new WPLovefilm.Service.Queue();
            }

            SetLoadingStatus(true);

            lfQueueService.GetQueueList(Account.Instance.GetQueuesUrl(), (status, myLists) =>
            {
                SetLoadingStatus(false);
                rentalQueues.Clear();

                if (status)
                {
                    foreach (LFQueue list in myLists)
                    {
                        rentalQueues.Add(list);
                    }
                }

                PopulateMyLists();

            });

        }

        private void PopulateMyLists()
        {
            MyLists.Clear();

            foreach (LFQueue q in rentalQueues)
            {
                MyLists.Add(q);
            }

            LoadRentedTitleCount();

        }

        private void LoadRentedTitleCount()
        {
            if (rentedTitlesQueue != null)
            {
                MyLists.Add(rentedTitlesQueue);
                return;
            }

            if (lfTitleService == null)
            {
                lfTitleService = new WPLovefilm.Service.Title();
            }

            SetLoadingStatus(true);

            lfTitleService.GetRentedTitles(Account.Instance.GetRentedUrl(), 0, 1, (status, titles, meta) =>
            {
                SetLoadingStatus(false);
                if (status)
                {
                    rentedTitlesQueue = new LFQueueBase();
                    rentedTitlesQueue.Count = meta.TotalResults;
                    rentedTitlesQueue.Name = "Rented Titles";
                    rentedTitlesQueue.Type = LFQueueTypes.RentedTitles;

                    MyLists.Add(rentedTitlesQueue);
                }
            });
        }

        public void Logout()
        {
            Account.Instance.Logout();
            initComplete = false;
        }

        #region State

        public void SaveState(IDictionary<string, object> state)
        {
            if (!initComplete)
            {
                return;
            }

            if (AtHomeTitles.Count > 0)
            {
                state["AtHomeTitles"] = AtHomeTitles;
            }

            state["InitComplete"] = initComplete;
            state["TitlesAwaitingAllocation"] = TitlesAwaitingAllocation;

            if (rentedTitlesQueue != null)
            {
                state["RentedTitlesQueue"] = rentedTitlesQueue;
            }

            if (rentalQueues.Count > 0)
            {
                state["RentalQueues"] = rentalQueues;
            }
        }

        public void RestoreState(IDictionary<string, object> state)
        {

            if (state.ContainsKey("AtHomeTitles"))
            {
                this.AtHomeTitles = (ObservableCollection<LFAtHomeTitle>)state["AtHomeTitles"];
            }

            if (state.ContainsKey("InitComplete"))
            {
                this.initComplete = (bool)state["InitComplete"];
            }

            if (state.ContainsKey("TitlesAwaitingAllocation"))
            {
                SetTitlesAwaitingAllocation((int)state["TitlesAwaitingAllocation"]);
            }

            if(state.ContainsKey("RentedTitlesQueue")){
                this.rentedTitlesQueue = (LFQueueBase)state["RentedTitlesQueue"];
            }

            if (state.ContainsKey("RentalQueues"))
            {
                this.rentalQueues = (List<LFQueue>)state["RentalQueues"];

                PopulateMyLists();                
            }

        }

        #endregion

    }
}
