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
    public class TitleListingViewModel : ViewModelBase
    {
        private bool initComplete = false;

        public ObservableCollection<LFTitle> TitleCollection { get; private set; }
        public LFCatalogSearchMeta PageMeta { get; private set; }

        public bool TombStoneActivation = false;

        private Title lfTitleService;

        private LFCatalogSearchType searchType;
        public LFRefineType RefineType { get; private set; }
        
        public string Genre { get; private set; }
        public string SearchTerm { get; set; }
        public string PageTitle { get; private set; }
        public bool CanPageBack { get; private set; }
        public bool CanPageForward { get; private set; }
        public Visibility FilterActive { get; private set; }

        public delegate void TitlesLoadedHandler();
        public event TitlesLoadedHandler TitlesLoaded;

        public TitleListingViewModel()
        {
            lfTitleService = new Title();
            TitleCollection = new ObservableCollection<LFTitle>();
        }

        public void InitData(IDictionary<string,string> querystring){
            string type = "";
            string refine = "";
            string genre = "";
            string search = "";

            if (querystring.TryGetValue("type", out type))
            {
                searchType = (LFCatalogSearchType)Enum.Parse(typeof(LFCatalogSearchType), type, true);
            }

            if (querystring.TryGetValue("refine", out refine))
            {
                SetRefineType((LFRefineType)Enum.Parse(typeof(LFRefineType), refine, true));
            }

            if (RefineType == LFRefineType.Genres)
            {
                querystring.TryGetValue("genre", out genre);
                SetGenre(genre);
            }

            if (searchType == LFCatalogSearchType.Search)
            {
                if (string.IsNullOrEmpty(SearchTerm))
                {
                    querystring.TryGetValue("search", out search);
                    SearchTerm = search;
                    NotifyPropertyChanged("SearchTerm");
                }
            }

        }

        public void LoadTitles()
        {
            if (!TombStoneActivation)
            {
                loadTitles(1);
            }
            else
            {
                TombStoneActivation = false;
            }
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

        public void ClearTitles()
        {
            if (!TombStoneActivation)
            {
                TitleCollection.Clear();
            }
        }

        private void loadTitles(int startIndex)
        {
            SetLoadingStatus(true);

            SetFilterStatus();

            lfTitleService.GetTitles(searchType, RefineType, SearchTerm, Genre, startIndex, (status, titles, meta) =>
            {
                SetLoadingStatus(false);

                SetNoContentMessageVisibility(titles.Count);

                TitleCollection.Clear();

                PageMeta = meta;

                if (titles.Count > 0)
                {
                    foreach (LFTitle title in titles)
                    {
                        TitleCollection.Add(title);
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

        public void SetSearchType(LFCatalogSearchType type)
        {
            searchType = type;            
        }

        public void SetRefineType(LFRefineType type)
        {
            RefineType = type;

            switch (type)
            {
                case LFRefineType.Search:
                    PageTitle = "SEARCH";
                    break;
                case LFRefineType.NewReleases:
                    PageTitle = "NEW RELEASES";
                    break;
                case LFRefineType.ComingSoon:
                    PageTitle = "COMING SOON";
                    break;
                case LFRefineType.MostPopular:
                    PageTitle = "MOST POPULAR";
                    break;
                case LFRefineType.Genres:
                    break;
            }
            NotifyPropertyChanged("PageTitle");
        }

        public void SetGenre(string genre)
        {
            Genre = genre;
            if (RefineType == LFRefineType.Genres)
            {
                PageTitle = Genre;
                NotifyPropertyChanged("PageTitle");
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

        private void SetFilterStatus()
        {
            if (Filter.Instance.IsFilterApplied(RefineType))
            {
                FilterActive = Visibility.Visible;
            }
            else
            {
                FilterActive = Visibility.Collapsed;
            }
            NotifyPropertyChanged("FilterActive");
        }

        #region state

        public void SaveState(IDictionary<string, object> state)
        {
            if (!initComplete)
            {
                return;
            }

            state["InitComplete"] = initComplete;
            state["TitleCollection"] = TitleCollection;
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

            TombStoneActivation = true;

            SetFilterStatus();
            
            if (state.ContainsKey("TitleCollection"))
            {
                this.TitleCollection = (ObservableCollection<LFTitle>)state["TitleCollection"];

                // Not sure we should have to do this, but it doesn't work if we don't
                NotifyPropertyChanged("TitleCollection");

            }
            
            SetNoContentMessageVisibility(TitleCollection.Count);

            if (state.ContainsKey("PageMeta"))
            {
                this.PageMeta = (LFCatalogSearchMeta)state["PageMeta"];
            }

            toggleNavigationButtons();
        }

        #endregion

    }
}
