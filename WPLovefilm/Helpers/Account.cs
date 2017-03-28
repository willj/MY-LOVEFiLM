using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.IO;
using Madebywill.Helpers;

namespace WPLovefilm.Helpers
{
    public sealed class Account
    {
        private static readonly Account instance = new Account();

        private Account() {
            LoadTokens();
        }

        /// <summary>
        /// Get instance of the Account Singleton
        /// </summary>
        public static Account Instance
        {
            get
            {
                return instance;
            }
        }

        #region oAuth Keys

        private string oAuthToken;
        private string oAuthTokenSecret;

        /// <summary>
        /// Get Consumer Key
        /// </summary>
        /// <returns></returns>
        public string GetConsumerKey()
        {
            return Keys.GetConsumerKey();
        }

        /// <summary>
        /// Get Consumer Secret
        /// </summary>
        /// <returns></returns>
        public string GetConsumerSecret()
        {
            return Keys.GetConsumerSecret();
        }

        /// <summary>
        /// Get the users oAuth Token
        /// </summary>
        /// <returns></returns>
        public string GetToken()
        {
            return oAuthToken;
        }

        /// <summary>
        /// Get the users oAuth Token Secret
        /// </summary>
        /// <returns></returns>
        public string GetTokenSecret()
        {
            return oAuthTokenSecret;
        }

        /// <summary>
        /// Loads users oAuth tokens from App Settings
        /// </summary>
        public void LoadTokens()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("oauth_token") && IsolatedStorageSettings.ApplicationSettings.Contains("oauth_secret"))
            {
                oAuthToken = IsolatedStorageSettings.ApplicationSettings["oauth_token"].ToString();
                oAuthTokenSecret = IsolatedStorageSettings.ApplicationSettings["oauth_secret"].ToString();
            }
        }

        /// <summary>
        /// Check an active oAuth account token & secret exist for user.
        /// </summary>
        /// <returns></returns>
        public bool AccountTokenExists()
        {
            if (!string.IsNullOrEmpty(oAuthToken) && !string.IsNullOrEmpty(oAuthTokenSecret))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Lovefilm API Endpoints

        private string atHomeUrl;
        private string queuesUrl;
        private string ratingsUrl;
        private string rentedUrl;

        public string GetAtHomeUrl()
        {
            if (string.IsNullOrEmpty(atHomeUrl))
            {
                if(IsolatedStorageSettings.ApplicationSettings.Contains("profile_at_home_url"))
                {
                    atHomeUrl = IsolatedStorageSettings.ApplicationSettings["profile_at_home_url"].ToString();
                }
            }
            
            return atHomeUrl;
        }

        public string GetQueuesUrl()
        {
            if (string.IsNullOrEmpty(queuesUrl))
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("profile_queue_url"))
                {
                    queuesUrl = IsolatedStorageSettings.ApplicationSettings["profile_queue_url"].ToString();
                }
            }

            return queuesUrl;
        }

        public string GetRatingUrl()
        {
            if (string.IsNullOrEmpty(ratingsUrl))
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("profile_ratings_url"))
                {
                    ratingsUrl = IsolatedStorageSettings.ApplicationSettings["profile_ratings_url"].ToString();
                }
            }

            return ratingsUrl;
        }

        public string GetRentedUrl()
        {
            if (string.IsNullOrEmpty(rentedUrl))
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("profile_rented_url"))
                {
                    rentedUrl = IsolatedStorageSettings.ApplicationSettings["profile_rented_url"].ToString();
                }    
            }

            return rentedUrl;
        }

        #endregion

        public void Logout()
        {
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }

        #region Version Logging

        private bool checkedVersion = false;

        public void LogVersion(string VersionNumber)
        {
            if (checkedVersion == true)
            {
                return;
            }

            string loggedVersionNumber = string.Empty;

            if (IsolatedStorageSettings.ApplicationSettings.Contains("VersionLog"))
            {
                loggedVersionNumber = IsolatedStorageSettings.ApplicationSettings["VersionLog"].ToString();
            }

            if (loggedVersionNumber == VersionNumber)
            {
                checkedVersion = true;
                return;
            }
            else
            {
                string url = "http://phone.madebywill.net/?app=lovefilm&v=" + VersionNumber;

                if (loggedVersionNumber != string.Empty)
                {
                    url += "&upgrade=1";
                }

                //Grab device info
                string osVersion = Environment.OSVersion.ToString();
                string deviceName = Microsoft.Phone.Info.DeviceStatus.DeviceManufacturer + " " + Microsoft.Phone.Info.DeviceStatus.DeviceName;

                url += "&osv=" + osVersion + "&device=" + deviceName;

                HttpWebRequest request = HttpWebRequest.CreateHttp(url);

                request.BeginGetResponse((result) =>
                {
                    ResponseHandler(VersionNumber, result);
                }, request);
            }
        }

        private static void ResponseHandler(string version, IAsyncResult asyncResult)
        {
            try
            {
                var request = (HttpWebRequest)asyncResult.AsyncState;
                var response = request.EndGetResponse(asyncResult);

                using (var rs = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(rs))
                    {
                        //string result = sr.ReadToEnd();
                        //This seems to be causing an exception, although it works fine...?

                        string result = sr.ReadLine();

                        if (result == version)
                        {
                            IsolatedStorageSettings.ApplicationSettings["VersionLog"] = version;
                        }
                    }
                }
            }
            catch
            {
                return;
            }
        }

        #endregion

    }
}