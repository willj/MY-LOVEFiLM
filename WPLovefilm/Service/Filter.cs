using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using WPLovefilm.Models;

namespace WPLovefilm.Service
{
    public sealed class Filter
    {
        private static readonly Filter instance = new Filter();

        public static Filter Instance
        {
            get
            {
                return instance;
            }
        }

        private Filter()
        {
            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!store.FileExists("Filters.xml"))
                    {
                        //first run, setup the defaults
                        createFilterList();
                    }
                    else
                    {
                        //There's already a file, load it and deserialize
                        using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream("Filters.xml", FileMode.Open, store))
                        {
                            XmlSerializer s = new XmlSerializer(typeof(List<LFFilter>));

                            filters = (List<LFFilter>)s.Deserialize(fs);

                            if (filters.Count(f => f.FilterType == LFRefineType.MostPopular) < 1)
                            {
                                createFilterList();
                            }
                        }
                    }
                }
            }
            //If these occur we won't retrieve user prefs. They should never happen!
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }
            catch (IsolatedStorageException) { }
        }

        private List<LFFilter> filters;

        public LFFilter GetFilter(LFRefineType filterType)
        {
            //return filters.Select(f => f.FilterType = filterType) as LFFilter ?? new LFFilter();
            
            foreach (LFFilter filter in filters)
            {
                if (filter.FilterType == filterType)
                {
                    return filter;
                }
            }
            
            return new LFFilter();
        }

        public void SetFilter(LFFilter filter)
        {
            int filterIndex = -1;

            foreach (LFFilter f in filters)
            {
                if (filter.FilterType == f.FilterType)
                {
                    filterIndex = filters.IndexOf(f);
                }
            }

            if (filterIndex >= 0)
            {
                filters[filterIndex] = filter;
            }

            saveFilters();
        }

        private void saveFilters()
        {
            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream fs = new IsolatedStorageFileStream("Filters.xml", FileMode.Create, store))
                    {
                        XmlSerializer s = new XmlSerializer(typeof(List<LFFilter>));

                        s.Serialize(fs, filters);
                    }
                }
            }
            //There really is nothing we can do here, but it's not the end of the world so we'll just swallow it, gulp!
            catch (IsolatedStorageException) { }
        }

        private void createFilterList()
        {
            filters = new List<LFFilter>();

            LFFilter newReleases = new LFFilter();

            newReleases.Decade = "All";
            newReleases.FilterType = LFRefineType.NewReleases;
            newReleases.Hotlist = "All";    //Just a default value, never used!

            newReleases.Genres.Add(new LFFilterItem("Action/Adventure", true));
            newReleases.Genres.Add(new LFFilterItem("Adult", true));
            newReleases.Genres.Add(new LFFilterItem("Animated", true));
            newReleases.Genres.Add(new LFFilterItem("Anime", true));
            newReleases.Genres.Add(new LFFilterItem("Audio Descriptive", true));
            newReleases.Genres.Add(new LFFilterItem("Bollywood", true));
            newReleases.Genres.Add(new LFFilterItem("Children", true));
            newReleases.Genres.Add(new LFFilterItem("Comedy", true));
            newReleases.Genres.Add(new LFFilterItem("Documentary", true));
            newReleases.Genres.Add(new LFFilterItem("Drama", true));
            newReleases.Genres.Add(new LFFilterItem("Family", true));
            newReleases.Genres.Add(new LFFilterItem("Gay/Lesbian", true));
            newReleases.Genres.Add(new LFFilterItem("Horror", true));
            newReleases.Genres.Add(new LFFilterItem("Music/Musical", true));
            newReleases.Genres.Add(new LFFilterItem("Romance", true));
            newReleases.Genres.Add(new LFFilterItem("Sci-Fi/Fantasy", true));
            newReleases.Genres.Add(new LFFilterItem("Special Interest", true));
            newReleases.Genres.Add(new LFFilterItem("Sport", true));
            newReleases.Genres.Add(new LFFilterItem("Teen", true));
            newReleases.Genres.Add(new LFFilterItem("Television", true));
            newReleases.Genres.Add(new LFFilterItem("Thriller", true));
            newReleases.Genres.Add(new LFFilterItem("World Cinema", true));

            LFFilter comingSoon = new LFFilter();

            comingSoon.Decade = "All";
            comingSoon.FilterType = LFRefineType.ComingSoon;
            comingSoon.Hotlist = "All";    //Just a default value, never used!

            comingSoon.Genres.Add(new LFFilterItem("Action/Adventure", true));
            comingSoon.Genres.Add(new LFFilterItem("Adult", true));
            comingSoon.Genres.Add(new LFFilterItem("Animated", true));
            comingSoon.Genres.Add(new LFFilterItem("Anime", true));
            comingSoon.Genres.Add(new LFFilterItem("Audio Descriptive", true));
            comingSoon.Genres.Add(new LFFilterItem("Bollywood", true));
            comingSoon.Genres.Add(new LFFilterItem("Children", true));
            comingSoon.Genres.Add(new LFFilterItem("Comedy", true));
            comingSoon.Genres.Add(new LFFilterItem("Documentary", true));
            comingSoon.Genres.Add(new LFFilterItem("Drama", true));
            comingSoon.Genres.Add(new LFFilterItem("Family", true));
            comingSoon.Genres.Add(new LFFilterItem("Gay/Lesbian", true));
            comingSoon.Genres.Add(new LFFilterItem("Horror", true));
            comingSoon.Genres.Add(new LFFilterItem("Music/Musical", true));
            comingSoon.Genres.Add(new LFFilterItem("Romance", true));
            comingSoon.Genres.Add(new LFFilterItem("Sci-Fi/Fantasy", true));
            comingSoon.Genres.Add(new LFFilterItem("Special Interest", true));
            comingSoon.Genres.Add(new LFFilterItem("Sport", true));
            comingSoon.Genres.Add(new LFFilterItem("Teen", true));
            comingSoon.Genres.Add(new LFFilterItem("Television", true));
            comingSoon.Genres.Add(new LFFilterItem("Thriller", true));
            comingSoon.Genres.Add(new LFFilterItem("World Cinema", true));

            LFFilter mostPopular = new LFFilter();

            mostPopular.Decade = "All";
            mostPopular.FilterType = LFRefineType.MostPopular;
            mostPopular.Hotlist = "All";    //Just a default value, never used!

            mostPopular.Genres.Add(new LFFilterItem("Action/Adventure", true));
            mostPopular.Genres.Add(new LFFilterItem("Adult", true));
            mostPopular.Genres.Add(new LFFilterItem("Animated", true));
            mostPopular.Genres.Add(new LFFilterItem("Anime", true));
            mostPopular.Genres.Add(new LFFilterItem("Audio Descriptive", true));
            mostPopular.Genres.Add(new LFFilterItem("Bollywood", true));
            mostPopular.Genres.Add(new LFFilterItem("Children", true));
            mostPopular.Genres.Add(new LFFilterItem("Comedy", true));
            mostPopular.Genres.Add(new LFFilterItem("Documentary", true));
            mostPopular.Genres.Add(new LFFilterItem("Drama", true));
            mostPopular.Genres.Add(new LFFilterItem("Family", true));
            mostPopular.Genres.Add(new LFFilterItem("Gay/Lesbian", true));
            mostPopular.Genres.Add(new LFFilterItem("Horror", true));
            mostPopular.Genres.Add(new LFFilterItem("Music/Musical", true));
            mostPopular.Genres.Add(new LFFilterItem("Romance", true));
            mostPopular.Genres.Add(new LFFilterItem("Sci-Fi/Fantasy", true));
            mostPopular.Genres.Add(new LFFilterItem("Special Interest", true));
            mostPopular.Genres.Add(new LFFilterItem("Sport", true));
            mostPopular.Genres.Add(new LFFilterItem("Teen", true));
            mostPopular.Genres.Add(new LFFilterItem("Television", true));
            mostPopular.Genres.Add(new LFFilterItem("Thriller", true));
            mostPopular.Genres.Add(new LFFilterItem("World Cinema", true));
            
            LFFilter genreFilter = new LFFilter();

            genreFilter.Decade = "All";
            genreFilter.Hotlist = "All";
            genreFilter.FilterType = LFRefineType.Genres;

            filters.Add(newReleases);
            filters.Add(comingSoon);
            filters.Add(mostPopular);
            filters.Add(genreFilter);

            saveFilters();
        }

        public bool IsFilterApplied(LFRefineType type)
        {
            bool filterApplied = false;

            LFFilter filter = GetFilter(type);

            if (filter.Decade != "All" || filter.Hotlist != "All")
            {
                filterApplied = true;
            }

            foreach (LFFilterItem genre in filter.Genres)
            {
                if (genre.Active == false)
                {
                    filterApplied = true;
                }
            }
            return filterApplied;
        }

    }
}
