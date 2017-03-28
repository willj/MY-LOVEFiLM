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
using WPLovefilm.Models;
using WPLovefilm.Service;

namespace WPLovefilm.UserControls
{
    public partial class HotlistFilterDialog : UserControl
    {
        static readonly string[] ProductionDecades = { "All", "2010", "2000", "1990", "1980", "1970", "1960", "1950", "1940", "1930", "1920", "1910", "1900" };

        static readonly string[] HotlistFilters = { "All", "New Releases", "Coming Soon", "Just Added", "Most Watched", "Highest Rated" };

        public LFFilter CurrentFilter;
        public LFRefineType RefineType;

        public delegate void SettingsSavedDelegate();
        public event SettingsSavedDelegate FilterSettingsSaved;

        public HotlistFilterDialog(LFRefineType type)
        {
            InitializeComponent();

            RefineType = type;

            CurrentFilter = Filter.Instance.GetFilter(RefineType);

            init();

            DecadeListbox.Loaded += new RoutedEventHandler(DecadeListbox_Loaded);

        }

        void DecadeListbox_Loaded(object sender, RoutedEventArgs e)
        {
            DecadeListbox.ScrollIntoView(DecadeListbox.SelectedItem);
        }


        private void init()
        {

            if (RefineType == LFRefineType.Genres)
            {
                GenreListBox.Visibility = Visibility.Collapsed;
                GenreTitle.Visibility = Visibility.Collapsed;

                HotlisListbox.Visibility = Visibility.Visible;
                HotlisListbox.ItemsSource = HotlistFilters;
                HotlisListbox.SelectedItem = CurrentFilter.Hotlist;
            }
            else
            {
                GenreTitle.Visibility = Visibility.Visible;
                GenreListBox.Visibility = Visibility.Visible;
                GenreListBox.ItemsSource = CurrentFilter.Genres;

                HotlisListbox.Visibility = Visibility.Collapsed;
            }

            DecadeListbox.ItemsSource = ProductionDecades;
            DecadeListbox.SelectedItem = CurrentFilter.Decade;
        }

        private void save()
        {
            Filter.Instance.SetFilter(CurrentFilter);

            if (this.FilterSettingsSaved != null)
            {
                this.FilterSettingsSaved();
            }
        }

        private void Apply(object sender, RoutedEventArgs e)
        {
            CurrentFilter.Decade = DecadeListbox.SelectedItem.ToString();

            if (RefineType == LFRefineType.Genres)
            {
                CurrentFilter.Hotlist = HotlisListbox.SelectedItem.ToString();
            }

            save();
        }

        private void Remove(object sender, RoutedEventArgs e)
        {
            foreach (LFFilterItem item in CurrentFilter.Genres)
            {
                item.Active = true;
            }

            CurrentFilter.Decade = "All";
            CurrentFilter.Hotlist = "All";

            save();

            GenreListBox.ItemsSource = null;

            init();
        }

    }
}
