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
    public class Queue : ServiceBase
    {
        /// <summary>
        /// Get a list of all users queues.
        /// </summary>
        /// <param name="queueListingUrl"></param>
        /// <param name="callback"></param>
        public void GetQueueList(string queueListingUrl, Action<bool, List<LFQueue>> callback, bool loadFromCache = false)
        {
            if (loadFromCache)
            {
                //If we've done this once this session then load it from the cache
                if (App.QueueList != null && App.QueueList.Count > 0)
                {
                    callback(true, App.QueueList);
                    return;
                }
            }

            Random r = new Random();
            int rand = r.Next(50000);

            List<oAuthParam> queryParams = new List<oAuthParam>();
            queryParams.Add(new oAuthParam("rand", rand.ToString()));

            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.GET, queueListingUrl, queryParams);

            HttpGet(queueListingUrl + "?rand=" + rand, authHeader, (result, status) =>
            {
                List<LFQueue> queueList = new List<LFQueue>();

                if (status)
                {
                    XDocument xml = XDocument.Parse(result);

                    var queues = from queue in xml.Descendants("queue")
                                 select new LFQueue
                                 {
                                     Type = LFQueueTypes.RentalQueue,
                                     Count = (int?)queue.Element("count") ?? 0,
                                     Name = (string)queue.Element("name"),
                                     Default = bool.Parse(queue.Element("default").Value),
                                     Allocation = (int)queue.Element("allocation"),
                                     Url = (string)queue.Element("link").Attribute("href").Value
                                 };

                    queueList = queues.ToList<LFQueue>();
                    App.QueueList = queues.ToList<LFQueue>();
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, queueList);
                });
            });
        }

        /// <summary>
        /// Get Queue Titles
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="startIndex"></param>
        /// <param name="callback"></param>
        public void GetQueue(string queueId, int startIndex, Action<bool, List<LFQueueTitle>, LFCatalogSearchMeta> callback)
        {
            Random r = new Random();
            int rand = r.Next(50000);

            List<oAuthParam> queryParams = new List<oAuthParam>();
            queryParams.Add(new oAuthParam("expand", oAuth.UrlEncode("artworks,synopsis,actors,directors")));
            queryParams.Add(new oAuthParam("start_index", startIndex.ToString()));
            queryParams.Add(new oAuthParam("rand", rand.ToString()));

            //items per page
            queryParams.Add(new oAuthParam("items_per_page", "15"));

            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.GET, queueId, queryParams);

            string url = queueId + "?expand=artworks,synopsis,actors,directors&start_index=" + startIndex + "&items_per_page=15&rand=" + rand;

            HttpGet(url, authHeader, (result, status) =>
            {
                LFCatalogSearchMeta meta = new LFCatalogSearchMeta();
                List<LFQueueTitle> queueTitleList = new List<LFQueueTitle>();

                if (status)
                {
                    XDocument xml = XDocument.Parse(result);

                    meta = (from m in xml.Descendants("queue")
                            select new LFCatalogSearchMeta
                            {
                                TotalResults = (int?)m.Element("total_results") ?? 0,
                                ItemsPerPage = (int?)m.Element("items_per_page") ?? 0,
                                StartIndex = (int?)m.Element("start_index") ?? 0
                            }).SingleOrDefault();

                    var queueItems = from item in xml.Descendants("queue_item")
                                     select new LFQueueTitle
                                     {
                                         InQueueId = (string)item.Element("id"),
                                         QueueId = (string)item.Elements("link").FirstOrDefault(q => q.Attribute("rel").Value == "http://schemas.lovefilm.com/queue").Attribute("href") ?? "",
                                         Priority = (int?)item.Element("priority") ?? 2,
                                         Availability = (string)item.Elements("category").FirstOrDefault(c => c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/availability").Attribute("term") ?? "",
                                         EstimatedWait = (string)item.Elements("category").FirstOrDefault(c => c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/estimated_wait").Attribute("term") ?? "",
                                         NumberOfDiscs = (int)item.Elements("queue_disc").Count(),

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

                    queueTitleList = queueItems.ToList<LFQueueTitle>();
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status, queueTitleList, meta);
                });

            });

        }

        /// <summary>
        /// Add Title To Queue
        /// </summary>
        /// <param name="queueId"></param>
        /// <param name="titleId"></param>
        /// <param name="callback"></param>
        public void AddToQueue(string queueId, string titleId, int priority, Action<bool> callback)
        {
            List<oAuthParam> queryParams = new List<oAuthParam>();
            queryParams.Add(new oAuthParam("title_refs", oAuth.UrlEncode(titleId)));
            queryParams.Add(new oAuthParam("priority", oAuth.UrlEncode(priority.ToString())));

            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.POST, queueId, queryParams);

            HttpPost(queueId, authHeader, "priority=" + priority.ToString() + "&title_refs=" + titleId, (result, status) =>
            {
                bool addToQueueStatus = false;

                if (status)
                {
                    XDocument xml = XDocument.Parse(result);

                    int failedTitles = xml.Element("response").Element("failed_title_refs").Descendants().Count();
                    int successTitles = xml.Element("response").Element("resources_created").Descendants().Count();

                    if (failedTitles == 0 && successTitles > 0)
                    {
                        addToQueueStatus = true;
                    }
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(addToQueueStatus);
                });

            });

        }

        /// <summary>
        /// Remove a title from Rental Queue
        /// </summary>
        /// <param name="queueItemId"></param>
        /// <param name="callback"></param>
        public void RemoveFromQueue(string queueItemId, Action<bool> callback)
        {
            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.DELETE, queueItemId);

            HttpDelete(queueItemId, authHeader, (result, status) =>
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status);
                });
            });
        }

        public void SaveQueueTitle(string queueTitleId, string queueId, int priority, Action<bool> callback)
        {
            List<oAuthParam> queryParams = new List<oAuthParam>();
            queryParams.Add(new oAuthParam("queue_ref", oAuth.UrlEncode(queueId)));
            queryParams.Add(new oAuthParam("priority", oAuth.UrlEncode(priority.ToString())));

            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.PUT, queueTitleId, queryParams);

            string url = queueTitleId + "?priority=" + priority + "&queue_ref=" + oAuth.UrlEncode(queueId);

            HttpPut(url, authHeader, "", (result, status) =>
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(status);
                });
            });

        }

        public void CheckQueueTitle(string everyQueueId, string titleId, Action<bool, LFQueueTitle> callback)
        {
            Random r = new Random();
            int rand = r.Next(50000);

            List<oAuthParam> queryParams = new List<oAuthParam>();
            queryParams.Add(new oAuthParam("start_index", "1"));
            queryParams.Add(new oAuthParam("title_refs", oAuth.UrlEncode(titleId)));
            queryParams.Add(new oAuthParam("rand", rand.ToString()));

            //items per page
            queryParams.Add(new oAuthParam("items_per_page", "1"));

            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.GET, everyQueueId, queryParams);

            string url = everyQueueId + "?start_index=1&items_per_page=1&title_refs=" + oAuth.UrlEncode(titleId) + "&rand=" + rand;

            HttpGet(url, authHeader, (result, status) =>
            {
                LFQueueTitle queueItem = new LFQueueTitle();
                bool queueItemStatus = false;

                if (status)
                {
                    XDocument xml = XDocument.Parse(result);

                    var queueTitle = (from item in xml.Descendants("queue_item")
                                    select new LFQueueTitle
                                    {
                                        InQueueId = (string)item.Element("id"),
                                        QueueId = (string)item.Elements("link").FirstOrDefault(q => q.Attribute("rel").Value == "http://schemas.lovefilm.com/queue").Attribute("href") ?? "",
                                        Priority = (int?)item.Element("priority") ?? 2,
                                        Availability = (string)item.Elements("category").FirstOrDefault(c => c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/availability").Attribute("term") ?? "",
                                        EstimatedWait = (string)item.Elements("category").FirstOrDefault(c => c.Attribute("scheme").Value == "http://openapi.lovefilm.com/categories/estimated_wait").Attribute("term") ?? "",
                                    }).SingleOrDefault();

                    if (queueTitle != null)
                    {
                        queueItem = queueTitle;
                        queueItemStatus = true;
                    }
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(queueItemStatus, queueItem);
                });

            });

        }

    }
}
