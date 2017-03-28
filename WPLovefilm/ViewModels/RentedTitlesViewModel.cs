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
    public class RentedTitlesViewModel : ViewModelBase
    {
        private bool initComplete = false;

        public ObservableCollection<LFTitle> RentedTitles { get; private set; }
        public LFCatalogSearchMeta PageMeta { get; private set; }
        private Title lfTitleService;

        public bool CanPageBack { get; private set; }
        public bool CanPageForward { get; private set; }

        public RentedTitlesViewModel()
        {
            RentedTitles = new ObservableCollection<LFTitle>();
        }

        public void LoadTitles()
        {
            if (!initComplete)
            {
                loadTitles(1);
            }
        }

        private void loadTitles(int startIndex)
        {
            if (lfTitleService == null)
            {
                lfTitleService = new WPLovefilm.Service.Title();
            }

            SetLoadingStatus(true);

            lfTitleService.GetRentedTitles(Account.Instance.GetRentedUrl(), 15, startIndex, (status, titles, meta) =>
            {
                SetLoadingStatus(false);

                SetNoContentMessageVisibility(titles.Count);

                RentedTitles.Clear();

                PageMeta = meta;

                if (titles.Count > 0)
                {
                    foreach (LFTitle title in titles)
                    {
                        RentedTitles.Add(title);
                    }
                }

                toggleNavigationButtons();

                initComplete = true;
            });
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
            state["RentedTitles"] = RentedTitles;
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

            if (state.ContainsKey("RentedTitles"))
            {
                this.RentedTitles = (ObservableCollection<LFTitle>)state["RentedTitles"];
                
                // Not sure we should have to do this, but it doesn't work if we don't
                NotifyPropertyChanged("RentedTitles");
            }

            SetNoContentMessageVisibility(RentedTitles.Count);

            if (state.ContainsKey("PageMeta"))
            {
                this.PageMeta = (LFCatalogSearchMeta)state["PageMeta"];
            }

            toggleNavigationButtons();
        }

    }
}
