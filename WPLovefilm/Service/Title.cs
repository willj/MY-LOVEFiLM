using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using WPLovefilm.Helpers;
using WPLovefilm.Models;
using Madebywill.Helpers;
using System.Windows.Threading;

namespace WPLovefilm.Service
{
    public class Title : ServiceBase
    {
        /// <summary>
        /// Load at home titles list. Async callback recieves list of titles plus number of discs awaiting allocation
        /// </summary>
        /// <param name="atHomeUrl"></param>
        /// <param name="callback"></param>
        public void GetAtHomeTitles(string atHomeUrl, Action<bool, List<LFAtHomeTitle>, int> callback)
        {
            List<oAuthParam> queryParams = new List<oAuthParam>();
            queryParams.Add(new oAuthParam("expand", oAuth.UrlEncode("artworks,synopsis,actors,directors")));

            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.GET, atHomeUrl, queryParams);

            HttpGet(atHomeUrl + "?expand=artworks,synopsis,actors,directors", authHeader, (result, status) =>
            {
                int awaitingAllocation = 0;
                List<LFAtHomeTitle> atHomeTitles = new List<LFAtHomeTitle>();

                if (status)
                {
                    XDocument xml = XDocument.Parse(result);

                    //get discs awaiting allocation
                    awaitingAllocation = (int?)xml.Element("at_home").Element("awaiting_allocation") ?? 0;

                    var itemsAtHome = from item in xml.Descendants("at_home_item")
                                      select new LFAtHomeTitle
                                      {
                                          ShipDate = (string)item.Element("shipped") ?? "",
                                          Id = (string)item.Element("catalog_title").Element("id"),
                                          WebUrl = (string)item.Descendants("link").FirstOrDefault(w => w.Attribute("title").Value == "web page").Attribute("href").Value ?? "",
                                          Studio = (string)item.Element("catalog_title").Element("studio") ?? "",
                                          ReleaseDate = (string)item.Element("catalog_title").Element("release_date") ?? "",
                                          Rating = (float?)item.Element("catalog_title").Element("rating") ?? 0,
                                          NumberOfRatings = (int?)item.Element("catalog_title").Element("number_of_ratings") ?? 0,
                                          ProductionYear = (int?)item.Element("catalog_title").Element("production_year") ?? 0,
                                          RunTime = (int?)item.Element("catalog_title").Element("run_time") ?? 0,
                                          TitleName = (string)item.Element("catalog_title").Element("title").Attribute("clean") ?? "",
                                          Format = (string)item.Element("catalog_title").Descendants("category").FirstOrDefault(f => f.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/format").Attribute("term"),
                                          Category = (string)item.Element("catalog_title").Descendants("category").FirstOrDefault(f => f.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/catalog").Attribute("term") ?? "",
                                          CanRent = bool.Parse(item.Element("catalog_title").Element("can_rent").Value),
                                          SmallImage = GetImage(item.Descendants("artwork").FirstOrDefault(a => (string)a.Attribute("type") == "title"), "small"),
                                          MediumImage = GetImage(item.Descendants("artwork").FirstOrDefault(a => (string)a.Attribute("type") == "title"), "medium"),
                                          Synopsis = (string)item.Element("catalog_title").Descendants("synopsis_text").FirstOrDefault(),
                                          GenreList = (from g in item.Element("catalog_title").Elements("category")
                                                       where g.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/genres"
                                                       select g.Attribute("term").Value).ToList(),
                                          ActorList = (from p in
                                                           (from a in item.Element("catalog_title").Descendants("link")
                                                            where a.Attribute("rel").Value == "http://schemas.lovefilm.com/actors"
                                                            select a.Element("people")).Descendants("link")
                                                       select p.Attribute("title").Value).ToList(),
                                          DirectorList = (from p in
                                                              (from a in item.Element("catalog_title").Descendants("link")
                                                               where a.Attribute("rel").Value == "http://schemas.lovefilm.com/directors"
                                                               select a.Element("people")).Descendants("link")
                                                          select p.Attribute("title").Value).ToList(),
                                          Developer = (string)item.Element("catalog_title").Element("developer") ?? "",
                                          Players = (string)item.Element("catalog_title").Element("players") ?? "",
                                          Certificate = (from c in item.Element("catalog_title").Elements("category")
                                                         where c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/certificates/bbfc" || c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/certificates/pegi"
                                                         select c.Attribute("term").Value).Single() ?? ""
                                      };

                    atHomeTitles = itemsAtHome.ToList<LFAtHomeTitle>();
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, atHomeTitles, awaitingAllocation);
                });

            });
        }
       
        /// <summary>
        /// Retrieve a list of titles
        /// </summary>
        /// <param name="type"></param>
        /// <param name="searchTerm"></param>
        /// <param name="startIndex"></param>
        /// <param name="callback"></param>
        public void GetTitles(LFCatalogSearchType type, LFRefineType refine, string searchTerm, string genre, int startIndex, Action<bool, List<LFTitle>, LFCatalogSearchMeta> callback)
        {
            string baseUrl = "http://openapi.lovefilm.com";

            string queryString = "?expand=artworks,synopsis,actors,directors&start_index=" + startIndex;
            string path = string.Empty;
            string format = string.Empty;
            string refineFacets = string.Empty;

            List<oAuthParam> queryParams = new List<oAuthParam>();

            queryParams.Add(new oAuthParam("expand", oAuth.UrlEncode("artworks,synopsis,actors,directors")));
            queryParams.Add(new oAuthParam("start_index", startIndex.ToString()));
            
            //items per page
            queryParams.Add(new oAuthParam("items_per_page", "15"));
            queryString += "&items_per_page=15";


            switch (type)
            {
                case LFCatalogSearchType.Search:
                    path = "/catalog/title";
                    //Add Format filter??
                    break;
                case LFCatalogSearchType.Title:
                    path = "/catalog/title";
                    format = GetDiscFormatString(LFFormatType.Film);
                    break;
                case LFCatalogSearchType.Film:
                    path = "/catalog/film";
                    format = GetDiscFormatString(LFFormatType.Film);
                    break;
                case LFCatalogSearchType.TV:
                    path = "/catalog/tv";
                    format = GetDiscFormatString(LFFormatType.Film);
                    break;
                case LFCatalogSearchType.Games:
                    path = "/catalog/games";
                    format = GetDiscFormatString(LFFormatType.Game);
                    break;
            }

            switch (refine)
	        {
                case LFRefineType.Search:
                    queryParams.Add(new oAuthParam("term", oAuth.UrlEncode(searchTerm)));
                    queryString += "&term=" + oAuth.UrlEncode(searchTerm);
                    break;
		        case LFRefineType.NewReleases:
                    //refineFacets = "hotlist|new_releases";
                    refineFacets = "new_releases|new_releases";
                    break;
                case LFRefineType.ComingSoon:
                    //refineFacets = "hotlist|coming_soon";
                    refineFacets = "coming_soon|coming_soon";
                    break;
                case LFRefineType.MostPopular:
                    refineFacets = "most_popular|most_popular";
                    break;
                case LFRefineType.Genres:
                    queryParams.Add(new oAuthParam("genre", oAuth.UrlEncode(genre)));
                    queryString += "&genre=" + genre;
                    break;
	        }

            if (!string.IsNullOrEmpty(format))
            {
                queryParams.Add(new oAuthParam("format", oAuth.UrlEncode(format)));
                queryString += "&format=" + format;
            }

            if (!string.IsNullOrEmpty(refineFacets))
            {
                queryParams.Add(new oAuthParam("f", oAuth.UrlEncode(refineFacets)));
                queryString += "&f=" + refineFacets;
            }

            //apply filters
            if (refine == LFRefineType.ComingSoon || refine == LFRefineType.NewReleases || refine == LFRefineType.MostPopular)
            {
                LFFilter filter = Filter.Instance.GetFilter(refine);

                if (filter.Decade != "All")
                {
                    queryParams.Add(new oAuthParam("f", oAuth.UrlEncode("production_decade|" + filter.Decade)));
                    queryString += "&f=production_decade|" + filter.Decade;
                }

                string genreString = filter.GetGenreString();

                if (!string.IsNullOrEmpty(genreString))
                {
                    queryParams.Add(new oAuthParam("genre", oAuth.UrlEncode(genreString)));
                    queryString += "&genre=" + genreString;
                }

            }
            else if (refine == LFRefineType.Genres)
            {
                LFFilter filter = Filter.Instance.GetFilter(refine);

                if (filter.Decade != "All")
                {
                    queryParams.Add(new oAuthParam("f", oAuth.UrlEncode("production_decade|" + filter.Decade)));
                    queryString += "&f=production_decade|" + filter.Decade;
                }

                if (!string.IsNullOrEmpty(filter.GetHotlist()))
                {
                    queryParams.Add(new oAuthParam("f", oAuth.UrlEncode("hotlist|" + filter.GetHotlist())));
                    queryString += "&f=hotlist|" + filter.GetHotlist();
                }
            }

            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.GET, baseUrl + path, queryParams);

            HttpGet(baseUrl + path + queryString, authHeader, (result, status) =>
            {
                List<LFTitle> titleList = new List<LFTitle>();
                LFCatalogSearchMeta meta = new LFCatalogSearchMeta();

                if (status)
                {
                    XDocument xml = XDocument.Parse(result);

                    meta = (from m in xml.Descendants("search")
                                select new LFCatalogSearchMeta
                                {
                                    TotalResults = (int?)m.Element("total_results") ?? 0,
                                    ItemsPerPage = (int?)m.Element("items_per_page") ?? 0,
                                    StartIndex = (int?)m.Element("start_index") ?? 0
                                }).SingleOrDefault();

                    var titles = from item in xml.Descendants("catalog_title")
                                 select new LFTitle
                                 {
                                     Id = (string)item.Element("id"),
                                     WebUrl = (string)item.Descendants("link").FirstOrDefault(w => w.Attribute("title").Value == "web page").Attribute("href").Value ?? "",
                                     Studio = (string)item.Element("studio") ?? "",
                                     ReleaseDate = (string)item.Element("release_date") ?? "",
                                     Rating = (float?)item.Element("rating") ?? 0,
                                     NumberOfRatings = (int?)item.Element("number_of_ratings") ?? 0,
                                     ProductionYear = (int?)item.Element("production_year") ?? 0,
                                     RunTime = (int?)item.Element("run_time") ?? 0,
                                     TitleName = (string)item.Element("title").Attribute("clean"),
                                     Format = (string)item.Descendants("category").FirstOrDefault(f => f.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/format").Attribute("term") ?? "",
                                     Category = (string)item.Descendants("category").FirstOrDefault(f => f.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/catalog").Attribute("term") ?? "",
                                     CanRent = bool.Parse(item.Element("can_rent").Value),
                                     SmallImage = GetImage(item.Descendants("artwork").FirstOrDefault(a => (string)a.Attribute("type") == "title"), "small"),
                                     MediumImage = GetImage(item.Descendants("artwork").FirstOrDefault(a => (string)a.Attribute("type") == "title"), "medium"),
                                     Synopsis = (string)item.Descendants("synopsis_text").FirstOrDefault(),
                                     GenreList = (from g in item.Elements("category")
                                                  where g.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/genres"
                                                  select g.Attribute("term").Value).ToList(),
                                     ActorList = (from p in
                                                      (from a in item.Descendants("link")
                                                       where a.Attribute("rel").Value == "http://schemas.lovefilm.com/actors"
                                                       select a.Element("people")).Descendants("link")
                                                  select p.Attribute("title").Value).ToList(),
                                     DirectorList = (from p in
                                                         (from a in item.Descendants("link")
                                                          where a.Attribute("rel").Value == "http://schemas.lovefilm.com/directors"
                                                          select a.Element("people")).Descendants("link")
                                                     select p.Attribute("title").Value).ToList(),
                                     Developer = (string)item.Element("developer") ?? "",
                                     Players = (string)item.Element("players") ?? "",
                                     Certificate = (from c in item.Elements("category")
                                                    where c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/certificates/bbfc" || c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/certificates/pegi"
                                                    select c.Attribute("term").Value).Single() ?? ""
                                 };

                    titleList = titles.ToList<LFTitle>();
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, titleList, meta);
                });

            });

        }

        /// <summary>
        /// Get Format String
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private string GetDiscFormatString(LFFormatType type)
        {
            return Format.Instance.GetFormatString(type);
        }

        /// <summary>
        /// Rate a title
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="rating"></param>
        /// <param name="callback"></param>
        public void RateTitle(string titleId, float rating, Action<bool> callback)
        {
            List<oAuthParam> queryParams = new List<oAuthParam>();
            queryParams.Add(new oAuthParam("title_ref", oAuth.UrlEncode(titleId)));
            queryParams.Add(new oAuthParam("rating", oAuth.UrlEncode(rating.ToString())));

            string ratingUrl = Account.Instance.GetRatingUrl();

            //Seems to be missing an important ending....
            ratingUrl += "/title";

            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.POST, ratingUrl, queryParams);

            string requestData = "title_ref=" + oAuth.UrlEncode(titleId) + "&rating=" + rating;

            HttpPost(ratingUrl, authHeader, requestData, (result, status) =>
            {
                bool ratingStatus = false;

                if (status)
                {
                    XDocument xml = XDocument.Parse(result);

                    int successTitles = xml.Element("resources_created").Descendants().Count();

                    if (successTitles == 1)
                    {
                        ratingStatus = true;
                    }
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(ratingStatus);
                });
            });

        }

        /// <summary>
        /// Return titles rented in the last 90 days
        /// </summary>
        /// <param name="rentedUrl"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="startIndex"></param>
        /// <param name="callback"></param>
        public void GetRentedTitles(string rentedUrl, int itemsPerPage, int startIndex, Action<bool, List<LFTitle>, LFCatalogSearchMeta> callback)
        {
            List<oAuthParam> queryParams = new List<oAuthParam>();
            queryParams.Add(new oAuthParam("expand", oAuth.UrlEncode("artworks,synopsis,actors,directors")));
            queryParams.Add(new oAuthParam("items_per_page", oAuth.UrlEncode(itemsPerPage.ToString())));
            queryParams.Add(new oAuthParam("start_index", startIndex.ToString()));

            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.GET, rentedUrl, queryParams);

            HttpGet(rentedUrl + "?expand=artworks,synopsis,actors,directors&items_per_page=" + itemsPerPage.ToString() + "&start_index=" + startIndex.ToString(), authHeader, (result, status) =>
            {
                List<LFTitle> titleList = new List<LFTitle>();
                LFCatalogSearchMeta meta = new LFCatalogSearchMeta();

                if (status)
                {
                    XDocument xml = XDocument.Parse(result);

                    meta = (from m in xml.Descendants("rented")
                                select new LFCatalogSearchMeta
                                {
                                    TotalResults = (int?)m.Element("total_results") ?? 0,
                                    ItemsPerPage = (int?)m.Element("items_per_page") ?? 0,
                                    StartIndex = (int?)m.Element("start_index") ?? 0
                                }).SingleOrDefault();

                    var rentedTitles = from item in xml.Descendants("catalog_title")
                                       select new LFTitle
                                       {
                                           Id = (string)item.Element("id"),
                                           WebUrl = (string)item.Descendants("link").FirstOrDefault(w => w.Attribute("title").Value == "web page").Attribute("href").Value ?? "",
                                           Studio = (string)item.Element("studio") ?? "",
                                           ReleaseDate = (string)item.Element("release_date") ?? "",
                                           Rating = (float?)item.Element("rating") ?? 0,
                                           NumberOfRatings = (int?)item.Element("number_of_ratings") ?? 0,
                                           ProductionYear = (int?)item.Element("production_year") ?? 0,
                                           RunTime = (int?)item.Element("run_time") ?? 0,
                                           TitleName = (string)item.Element("title").Attribute("clean"),
                                           Format = (string)item.Descendants("category").FirstOrDefault(f => f.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/format").Attribute("term") ?? "",
                                           Category = (string)item.Descendants("category").FirstOrDefault(f => f.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/catalog").Attribute("term") ?? "",
                                           CanRent = bool.Parse(item.Element("can_rent").Value),
                                           SmallImage = GetImage(item.Descendants("artwork").FirstOrDefault(a => (string)a.Attribute("type") == "title"), "small"),
                                           MediumImage = GetImage(item.Descendants("artwork").FirstOrDefault(a => (string)a.Attribute("type") == "title"), "medium"),
                                           Synopsis = (string)item.Descendants("synopsis_text").FirstOrDefault(),
                                           GenreList = (from g in item.Elements("category")
                                                        where g.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/genres"
                                                        select g.Attribute("term").Value).ToList(),
                                           ActorList = (from p in
                                                            (from a in item.Descendants("link")
                                                             where a.Attribute("rel").Value == "http://schemas.lovefilm.com/actors"
                                                             select a.Element("people")).Descendants("link")
                                                        select p.Attribute("title").Value).ToList(),
                                           DirectorList = (from p in
                                                               (from a in item.Descendants("link")
                                                                where a.Attribute("rel").Value == "http://schemas.lovefilm.com/directors"
                                                                select a.Element("people")).Descendants("link")
                                                           select p.Attribute("title").Value).ToList(),
                                           Developer = (string)item.Element("developer") ?? "",
                                           Players = (string)item.Element("players") ?? "",
                                           Certificate = (from c in item.Elements("category")
                                                          where c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/certificates/bbfc" || c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/certificates/pegi"
                                                          select c.Attribute("term").Value).Single() ?? ""
                                       };

                    titleList = rentedTitles.ToList<LFTitle>();
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, titleList, meta);
                });

            });
        }
    
        /*
        public void GetTitle(string titleId, Action<bool, LFTitle> callback)
        {
            string queryString = "?expand=artworks,synopsis,actors,directors";

            List<oAuthParam> queryParams = new List<oAuthParam>();
            queryParams.Add(new oAuthParam("expand", oAuth.UrlEncode("artworks,synopsis,actors,directors")));

            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.GET, titleId, queryParams);

            HttpGet(titleId + queryString, authHeader, (result, status) =>
            {
                if (status)
                {
                    XDocument xml = XDocument.Parse(result);

                    LFTitle title = (from item in xml.Descendants("catalog_title")
                                     select new LFTitle
                                     {
                                         Id = (string)item.Element("id"),
                                         Studio = (string)item.Element("studio") ?? "",
                                         ReleaseDate = (string)item.Element("release_date") ?? "",
                                         Rating = (float?)item.Element("rating") ?? 0,
                                         ProductionYear = (int?)item.Element("production_year") ?? 0,
                                         RunTime = (int?)item.Element("run_time") ?? 0,
                                         TitleName = (string)item.Element("title").Attribute("clean"),
                                         Format = (string)item.Descendants("category").FirstOrDefault(f => f.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/format").Attribute("term") ?? "",
                                         Category = (string)item.Descendants("category").FirstOrDefault(f => f.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/catalog").Attribute("term") ?? "",
                                         CanRent = bool.Parse(item.Element("can_rent").Value),
                                         SmallImage = GetImage(item.Descendants("artwork").FirstOrDefault(a => (string)a.Attribute("type") == "title"), "small"),
                                         MediumImage = GetImage(item.Descendants("artwork").FirstOrDefault(a => (string)a.Attribute("type") == "title"), "medium"),
                                         Synopsis = (string)item.Descendants("synopsis_text").FirstOrDefault(),
                                         GenreList = (from g in item.Elements("category")
                                                      where g.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/genres"
                                                      select g.Attribute("term").Value).ToList(),
                                         ActorList = (from p in
                                                          (from a in item.Descendants("link")
                                                           where a.Attribute("rel").Value == "http://schemas.lovefilm.com/actors"
                                                           select a.Element("people")).Descendants("link")
                                                      select p.Attribute("title").Value).ToList(),
                                         DirectorList = (from p in
                                                             (from a in item.Descendants("link")
                                                              where a.Attribute("rel").Value == "http://schemas.lovefilm.com/directors"
                                                              select a.Element("people")).Descendants("link")
                                                         select p.Attribute("title").Value).ToList(),
                                         Developer = (string)item.Element("developer") ?? "",
                                         Players = (string)item.Element("players") ?? "",
                                         Certificate = (from c in item.Elements("category")
                                                        where c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/certificates/bbfc" || c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/certificates/pegi"
                                                        select c.Attribute("term").Value).Single() ?? ""
                                     }).SingleOrDefault();

                    callback(true, title);
                }
                else
                {
                    callback(false, new LFTitle());
                }
            });

        }
         */

    }
}
