using System;
using System.Net;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using WPLovefilm.Models;
using WPLovefilm.Service;
using WPLovefilm.Helpers;

namespace WPLovefilm.ViewModels
{
    public class QueueViewModel : ViewModelBase
    {
        private bool initComplete = false;

        public ObservableCollection<LFQueueTitle> QueueTitles { get; private set; }
        public LFCatalogSearchMeta PageMeta { get; private set; }
        private Queue lfQueueService;

        public LFQueue CurrentQueue { get; set; }
        public bool CanPageBack { get; private set; }
        public bool CanPageForward { get; private set; }

        public delegate void TitlesLoadedHandler();
        public event TitlesLoadedHandler TitlesLoaded;

        public QueueViewModel()
        {
            QueueTitles = new ObservableCollection<LFQueueTitle>();
        }

        public void NextPage()
        {
            if (PageMeta != null)
            {
                int nextStartIndex = PageMeta.StartIndex + PageMeta.ItemsPerPage;

                if (nextStartIndex <= PageMeta.TotalResults)
                {
                    loadTitles(nextStartIndex);
                }
            }
        }

        public void PrevPage()
        {
            if (PageMeta != null)
            {
                int prevStartIndex = PageMeta.StartIndex - PageMeta.ItemsPerPage;

                if (prevStartIndex > 0)
                {
                    loadTitles(prevStartIndex);
                }
            }
        }

        public void LoadTitles()
        {
            if (!initComplete)
            {
                loadTitles(1);
            }
        }

        public void ReloadTitles()
        {
            loadTitles(PageMeta.StartIndex);
        }

        private void loadTitles(int startIndex)
        {
            if(lfQueueService == null){
                lfQueueService = new WPLovefilm.Service.Queue();
            }

            SetLoadingStatus(true);

            lfQueueService.GetQueue(CurrentQueue.Url, startIndex, (status, titles, meta) =>{
                
                SetLoadingStatus(false);

                SetNoContentMessageVisibility(titles.Count);

                QueueTitles.Clear();

                PageMeta = meta;

                if (titles.Count > 0)
                {
                    foreach (LFQueueTitle title in titles)
                    {
                        QueueTitles.Add(title);
                    }

                    if (TitlesLoaded != null)
                    {
                        TitlesLoaded();
                    }
                }

                toggleNavigationButtons();

                initComplete = true;
            });

        }

        private void toggleNavigationButtons()
        {
            if (PageMeta.StartIndex > 1)
            {
                CanPageBack = true;
            }
            else
            {
                CanPageBack = false;
            }

            if ((PageMeta.StartIndex + PageMeta.ItemsPerPage) <= PageMeta.TotalResults)
            {
                CanPageForward = true;
            }
            else
            {
                CanPageForward = false;
            }
            NotifyPropertyChanged("CanPageBack");
            NotifyPropertyChanged("CanPageForward");
        }

        public void SaveState(IDictionary<string, object> state)
        {
            if (!initComplete)
            {
                return;
            }

            state["InitComplete"] = initComplete;
            state["QueueTitles"] = QueueTitles;
            state["PageMeta"] = PageMeta;
        }

        public void RestoreState(IDictionary<string, object> state)
        {
            if (state.ContainsKey("InitComplete"))
            {
                this.initComplete = (bool)state["InitComplete"];
            }

            if (!initComplete)
            {
                return;
            }

            if (state.ContainsKey("QueueTitles"))
            {
                this.QueueTitles = (ObservableCollection<LFQueueTitle>)state["QueueTitles"];

                // Not sure we should have to do this, but it doesn't work if we don't
                NotifyPropertyChanged("QueueTitles");
            }

            SetNoContentMessageVisibility(QueueTitles.Count);

            if (state.ContainsKey("PageMeta"))
            {
                this.PageMeta = (LFCatalogSearchMeta)state["PageMeta"];
            }

            toggleNavigationButtons();
        }

    }
}
