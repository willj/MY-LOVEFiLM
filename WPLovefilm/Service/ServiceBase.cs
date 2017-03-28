using System;
using System.Windows;
using System.Net;
using WPLovefilm.Helpers;
using System.Xml.Linq;
using System.Linq;
using System.Net.NetworkInformation;
using System.IO;
using Madebywill.Helpers;
using System.ComponentModel;
using System.Windows.Threading;

namespace WPLovefilm.Service
{
    public class ServiceBase
    {
        protected oAuth oAuth;

        public ServiceBase()
        {
            oAuth = new oAuth();

            oAuth.ConsumerKey = Account.Instance.GetConsumerKey();
            oAuth.ConsumerSecret = Account.Instance.GetConsumerSecret();

            if (Account.Instance.AccountTokenExists())
            {
                oAuth.Token = Account.Instance.GetToken();
                oAuth.TokenSecret = Account.Instance.GetTokenSecret();
            }
        }

        /// <summary>
        /// Execute async HTTP GET Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authHeader"></param>
        /// <param name="resultCallback"></param>
        protected void HttpGet(string url, string authHeader, Action<string, bool> resultCallback)
        {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (s,e) =>
            {
                WebClient getClient = new WebClient();

                if (!string.IsNullOrEmpty(authHeader))
                {
                    getClient.Headers[HttpRequestHeader.Authorization] = authHeader;
                }

                getClient.DownloadStringCompleted += getClient_DownloadStringCompleted;
                getClient.DownloadStringAsync(new Uri(url), resultCallback);
            };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Execute async HTTP POST request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authHeader"></param>
        /// <param name="requestData"></param>
        /// <param name="resultCallback"></param>
        protected void HttpPost(string url, string authHeader, string requestData, Action<string, bool> resultCallback)
        {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (s, e) =>
            {
                WebClient postClient = new WebClient();

                if (!string.IsNullOrEmpty(authHeader))
                {
                    postClient.Headers[HttpRequestHeader.Authorization] = authHeader;
                }
                postClient.Headers["Content-Type"] = "application/x-www-form-urlencoded";

                postClient.UploadStringCompleted += webClient_UploadStringCompleted;
                postClient.UploadStringAsync(new Uri(url), "POST", requestData, resultCallback);
            };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Execute async HTTP DELETE request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authHeader"></param>
        /// <param name="resultCallback"></param>
        protected void HttpDelete(string url, string authHeader, Action<string, bool> resultCallback)
        {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (s, e) =>
            {
                WebClient deleteClient = new WebClient();

                if (!string.IsNullOrEmpty(authHeader))
                {
                    deleteClient.Headers[HttpRequestHeader.Authorization] = authHeader;
                }

                deleteClient.UploadStringCompleted += webClient_UploadStringCompleted;
                deleteClient.UploadStringAsync(new Uri(url), "DELETE", "", resultCallback);
            };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Execute async HTTP PUT request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="authHeader"></param>
        /// <param name="requestData"></param>
        /// <param name="resultCallback"></param>
        protected void HttpPut(string url, string authHeader, string requestData, Action<string, bool> resultCallback)
        {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (s, e) =>
            {
                WebClient putClient = new WebClient();

                if (!string.IsNullOrEmpty(authHeader))
                {
                    putClient.Headers[HttpRequestHeader.Authorization] = authHeader;
                }
                //putClient.Headers["Content-Type"] = "application/x-www-form-urlencoded";

                putClient.UploadStringCompleted += webClient_UploadStringCompleted;
                putClient.UploadStringAsync(new Uri(url), "PUT", requestData, resultCallback);
            };

            bw.RunWorkerAsync();
        }

        /// <summary>
        /// Handle WebClient Download complete events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void getClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Action<string, bool> resultAction = e.UserState as Action<string, bool>;

            if (resultAction != null)
            {
                if (e.Error != null)
                {
                    resultAction(e.Error.Message, false);

                    SmartDispatcher.BeginInvoke(() =>
                    {
                        HandleHTTPError(e);
                    });
                }
                else
                {
                    resultAction(e.Result, true);
                }
            }
        }

        /// <summary>
        /// Handle WebClient Upload completed events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webClient_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            Action<string, bool> resultAction = e.UserState as Action<string, bool>;

            if (resultAction != null)
            {
                if (e.Error != null)
                {
                    resultAction(e.Error.Message, false);
                }
                else
                {
                    resultAction(e.Result, true);
                }
            }
        }

        /// <summary>
        /// Helper method to get artwork from title results
        /// </summary>
        /// <param name="artworkElement"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public string GetImage(XElement artworkElement, string size)
        {
            string defaultImage = "";

            if (size == "small")
            {
                defaultImage = "/Images/NoCoverSmall.jpg";
            }
            else if (size == "medium")
            {
                defaultImage = "/Images/NoCoverMedium.jpg";
            }

            if (artworkElement != null)
            {
                XElement imageElement = artworkElement.Descendants("image").FirstOrDefault(i => (string)i.Attribute("size") == size);
                if (imageElement != null)
                {
                    return imageElement.Attribute("href").Value ?? defaultImage;
                }
            }

            return defaultImage;
        }

        /// <summary>
        /// Handle HTTP Error
        /// </summary>
        /// <param name="e"></param>
        public static void HandleHTTPError(DownloadStringCompletedEventArgs e)
        {
            string errorCode = GetErrorCode(e);

            switch (errorCode)
            {
                case "NETWORK_UNAVAILABLE":
                    MessageBox.Show("A network connection could not be found, check your wi-fi & mobile network status.", "Network unavailable", MessageBoxButton.OK);
                break;
                case "ERR_401_INVALID_OR_EXPIRED_TOKEN":
                    MessageBox.Show("Your login token has expired or become invalid. Please restart the application and login again.", "Expired account token", MessageBoxButton.OK);
                    Account.Instance.Logout();
                break;
                case "ERR_401_TIMESTAMP_IS_INVALID":
                MessageBox.Show("The date or time on your device does not match the server. Check your date & time settings.", "Check your clock", MessageBoxButton.OK);
                break;
                default:
                    MessageBox.Show("We're having an issue reaching LOVEFiLM right now, try again later." + Environment.NewLine + Environment.NewLine + "ERROR CODE: " + errorCode, "Sorry", MessageBoxButton.OK);
                break;
            }
        }

        /// <summary>
        /// Get Error Code for Downloads
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetErrorCode(DownloadStringCompletedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return "NETWORK_UNAVAILABLE";
            }

            string errorCode = "UNKNOWN_NETWORK_ERROR";

            try
            {
                string result = e.Result;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    if (ex.Response.Headers["X-Mashery-Error-Code"] != null)
                    {
                        errorCode = ex.Response.Headers["X-Mashery-Error-Code"];
                    }
                }
            }

            return errorCode;
        }

        /// <summary>
        /// Get Error Code for Uploads
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string GetErrorCode(UploadStringCompletedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return "NETWORK_UNAVAILABLE";
            }

            string errorCode = "UNKNOWN_NETWORK_ERROR";

            try
            {
                string result = e.Result;
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    if (ex.Response.Headers["X-Mashery-Error-Code"] != null)
                    {
                        errorCode = ex.Response.Headers["X-Mashery-Error-Code"];
                    }
                }
            }

            return errorCode;
        }
    }
}
