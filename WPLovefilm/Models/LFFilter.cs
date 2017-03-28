using System;
using System.Net;
using System.Windows;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace WPLovefilm.Models
{
    public class LFFilterItem
    {
        public LFFilterItem() { }

        public LFFilterItem(string name, bool active)
        {
            Name = name;
            Active = active;
        }

        [XmlAttribute()]
        public string Name { get; set; }

        [XmlAttribute()]
        public bool Active { get; set; }
    }

    public class LFFilter
    {
        public LFFilter()
        {
            this.Genres = new List<LFFilterItem>();
        }

        [XmlAttribute()]
        public LFRefineType FilterType { get; set; }

        [XmlElement()]
        public string Decade { get; set; }

        [XmlElement()]
        public string Hotlist { get; set; }

        [XmlElement()]
        public List<LFFilterItem> Genres { get; set; }

        public string GetGenreString()
        {
            int notActiveCount = 0;
            string filterString = string.Empty;

            foreach (LFFilterItem genre in Genres)
            {
                if (genre.Active == true)
                {
                    if (string.IsNullOrEmpty(filterString))
                    {
                        filterString += genre.Name;
                    }
                    else
                    {
                        filterString += " OR " + genre.Name;
                    }
                }
                else
                {
                    notActiveCount++;
                }
            }

            if (notActiveCount > 0)
            {
                return filterString;
            }

            return string.Empty;
        }

        public string GetHotlist()
        {
            string hot = string.Empty;

            switch (this.Hotlist)
            {
                case "New Releases":
                    hot = "new_releases";
                    break;
                case "Coming Soon":
                    hot = "coming_soon";
                    break;
                case "Just Added":
                    hot = "re_releases";
                    break;
                case "Most Watched":
                    hot = "most_watched";
                    break;
                case "Highest Rated":
                    hot = "highest_rated";
                    break;
                default:
                    break;
            }

            return hot;
        }

    }
}
