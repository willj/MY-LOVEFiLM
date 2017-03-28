using System.Windows;
using Microsoft.Phone.Scheduler;
using System;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Net;
using System.Collections.Generic;
using Madebywill.Helpers;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace LFBackgroundAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        DateTime LastRunTime;
        DateTime AgentExpirationTime;
        string filename = "AtHomeTitles.xml";
        bool LastResultsExist = false;

        private string consumerKey;
        private string consumerSecret;
        private string token;
        private string tokenSecret;
        private string atHomeUrl;

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            // We use this later to check how long we've got left...
            AgentExpirationTime = task.ExpirationTime;

            if (!TimeIsGoForLaunch())
            {
                NotifyComplete();
                return;
            }

            if (!LoadAccountKeys())
            {
                Abort();
                return;
            }

            //Go go request

            oAuth oauth = new oAuth();
            oauth.ConsumerKey = consumerKey;
            oauth.ConsumerSecret = consumerSecret;
            oauth.Token = token;
            oauth.TokenSecret = tokenSecret;

            Random r = new Random();

            int rand = r.Next(50000);

            List<oAuthParam> paramList = new List<oAuthParam>();
            paramList.Add(new oAuthParam("rand", rand.ToString()));

            WebClient wc = new WebClient();
            wc.Headers[HttpRequestHeader.Authorization] = oauth.GetAuthHeader(oAuth.HttpMethods.GET, atHomeUrl, paramList);

            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);

            wc.DownloadStringAsync(new Uri(atHomeUrl + "?rand=" + rand, UriKind.Absolute));
        }

        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<AtHomeTitle> currentResults;
                List<AtHomeTitle> lastResults;

                //Parse current result
                try
                {
                     currentResults = ParseAtHomeTitleXml(e.Result);
                }
                catch (Exception)
                {
                    NotifyComplete();
                    return;
                }

                //Load last set of results & parse
                try
                {
                    lastResults = GetLastTitles();
                }
                catch (Exception)
                {
                    //Remove the Last Results, they're probably bad!
                    using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if(store.FileExists(filename)){
                            store.DeleteFile(filename);
                        }
                    }

                    NotifyComplete();
                    return;
                }

                //Then store these results
                StoreFileString(e.Result);

                //If this is first run then just store these.
                if (!LastResultsExist)
                {
                    NotifyComplete();
                    return;
                }

                List<AtHomeTitle> newTitles = new List<AtHomeTitle>();

                //And Compare
                foreach (AtHomeTitle c in currentResults)
                {
                    bool match = false;

                    foreach (AtHomeTitle t in lastResults)
                    {
                        if (c.Id == t.Id)
                        {
                            match = true;
                            break;
                        }
                    }

                    if (!match)
                    {
                        newTitles.Add(c);
                    }
                }

                //Are there any new titles?
                if (newTitles.Count > 0)
                {
                    DisplayNewTitleShippedNotification(newTitles.Count);
                    IsolatedStorageSettings.ApplicationSettings["TileBackDataSet"] = true;
                }

                //Store last run
                IsolatedStorageSettings.ApplicationSettings["LastAgentRuntime"] = DateTime.Now;
                IsolatedStorageSettings.ApplicationSettings.Save();

                if (newTitles.Count < 1)
                {
                    CheckAgentExpiration();
                }

                #if DEBUG

                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debug.WriteLine("Peak Memory: " + Microsoft.Phone.Info.DeviceStatus.ApplicationPeakMemoryUsage.ToString());
                }

                #endif    
            }

            NotifyComplete();
        }

        private void DisplayNewTitleShippedNotification(int count)
        {
            string notificationString = "Your next rental is ready";

            if (count > 1)
            {
                notificationString = "Your next rentals are ready";
            }

            ShellToast t = new ShellToast();
            t.Title = "LOVEFiLM";
            t.Content = notificationString;
            t.NavigationUri = new Uri("/MainPage.xaml", UriKind.Relative);
            t.Show();

            ShellTile tile = ShellTile.ActiveTiles.First();

            if (tile != null)
            {
                StandardTileData tileData = new StandardTileData
                {
                    Count = count,
                    BackContent = notificationString
                };

                tile.Update(tileData);
            }

        }

        private List<AtHomeTitle> GetLastTitles()
        {
            string xmlString = LoadStoredResults();

            if (string.IsNullOrEmpty(xmlString))
            {
                return new List<AtHomeTitle>();
            }

            return ParseAtHomeTitleXml(xmlString);
        }

        private string LoadStoredResults()
        {
            string data = string.Empty;

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(filename))
                {
                    LastResultsExist = true;
                    using (IsolatedStorageFileStream stream = store.OpenFile(filename, System.IO.FileMode.Open))
                    {
                        using (StreamReader sr = new StreamReader(stream))
                        {
                            data = sr.ReadToEnd();
                        }
                    }
                }
            }

            return data;
        }

        private void StoreFileString(string data)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = store.CreateFile(filename))
                {
                    using (StreamWriter sw = new StreamWriter(stream))
                    {
                        sw.Write(data);
                    }
                }
            }
        }

        private List<AtHomeTitle> ParseAtHomeTitleXml(string xmlString)
        {
            List<AtHomeTitle> titles = new List<AtHomeTitle>();

            XDocument xml = XDocument.Parse(xmlString);

            titles = (from item in xml.Descendants("at_home_item")
                              select new AtHomeTitle
                              {
                                  Id = (string)item.Element("catalog_title").Element("id"),
                                  Title = (string)item.Element("catalog_title").Element("title").Attribute("clean") ?? ""
                              }).ToList();

            return titles;
        }

        #region Time Related Bits

        private bool TimeIsGoForLaunch()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                return true;
            }

            //Is the hour between 1am - 8am don't bother running
            if (DateTime.Now.Hour > 1 && DateTime.Now.Hour < 8)
            {
                return false;
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("LastAgentRuntime"))
            {
                LastRunTime = (DateTime)IsolatedStorageSettings.ApplicationSettings["LastAgentRuntime"];
            }

            // Check last run time, if it's less than 90 mins then exit.
            if (LastRunTime != null)
            {
                if (DateTime.Now.Subtract(LastRunTime).TotalMinutes < 90)
                {
                    return false;
                }
            }

            return true;
        }

        public void CheckAgentExpiration()
        {
            if (AgentExpirationTime == null)
            {
                return;
            }

            // If the agent expires in the next 48 hours
            if(AgentExpirationTime.Subtract(DateTime.Now).TotalHours < 48){

                // we'll assume it's true if we don't know
                bool backDataSet = true;

                if (IsolatedStorageSettings.ApplicationSettings.Contains("TileBackDataSet"))
                {
                    backDataSet = (bool)IsolatedStorageSettings.ApplicationSettings["TileBackDataSet"];
                }

                if (!backDataSet)
                {
                    ShellTile tile = ShellTile.ActiveTiles.First();

                    if (tile != null)
                    {
                        StandardTileData tileData = new StandardTileData
                        {
                            BackContent = "Alerts expire soon, open me to reset"
                        };

                        tile.Update(tileData);
                    }

                    IsolatedStorageSettings.ApplicationSettings["TileBackDataSet"] = true;
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
            }
        }

        #endregion

        #region Account Data

        private bool LoadAccountKeys()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("oauth_token")
                || !IsolatedStorageSettings.ApplicationSettings.Contains("oauth_secret")
                || !IsolatedStorageSettings.ApplicationSettings.Contains("profile_at_home_url"))
            {
                return false;
            }

            token = (string)IsolatedStorageSettings.ApplicationSettings["oauth_token"];
            tokenSecret = (string)IsolatedStorageSettings.ApplicationSettings["oauth_secret"];
            atHomeUrl = (string)IsolatedStorageSettings.ApplicationSettings["profile_at_home_url"];

            //Test URLS
            //atHomeUrl = "http://phone.madebywill.net/athometest.xml";
            //atHomeUrl = "http://phone.madebywill.net/bigathometest.xml";

            consumerKey = Keys.GetConsumerKey();
            consumerSecret = Keys.GetConsumerSecret();

            return true;
        }

        #endregion

    }

    public class AtHomeTitle
    {
        public string Id { get; set; }
        public string Title { get; set; }
    }
}