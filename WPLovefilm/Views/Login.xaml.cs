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
using Microsoft.Phone.Controls;
using WPLovefilm.Helpers;
using WPLovefilm.Service;
using System.IO.IsolatedStorage;
using Madebywill.Helpers;
using Microsoft.Phone.Shell;

/*
 * Add a hidden browser control.
 * whenever webbrowser.navigating url = mobile url
 * cancel - then load mobile=off in the hidden, then reload last url.
 */

namespace WPLovefilm.Views
{
    public partial class Login : PhoneApplicationPage
    {
        string requestTokenUrl = "http://openapi.lovefilm.com/oauth/request_token";
        string accessTokenUrl = "http://openapi.lovefilm.com/oauth/access_token";
        string callbackUrl = "http://www.madebywill.net";

        string tempToken = "";
        string tempSecret = "";
        string activationUrl = "";
        string oauthVerifier = "";

        string userAgentString = "User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0; XBLWP7; ZuneWP7)";

        const string status_requesting_token = "Requesting Token...";
        const string status_loading_lovefilm = "Loading LOVEFiLM...";
        const string status_verifying_account = "Verifying Account...";
        const string status_blank = "";

        public Login()
        {
            InitializeComponent();

            App.SetSystemTray(this);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //Kick it off after the browser control has loaded to avoid an exception on navigate.
            AuthBrowser.Loaded += new RoutedEventHandler(AuthBrowser_Loaded);
        }

        void AuthBrowser_Loaded(object sender, RoutedEventArgs e)
        {
            RequestOAuthToken();
        }

        #region Request Temp oAuth Token

        private void RequestOAuthToken()
        {
            StatusText.Text = status_requesting_token;
            ShowLoading();

            oAuth oauth = new oAuth();
            oauth.ConsumerKey = Account.Instance.GetConsumerKey();
            oauth.ConsumerSecret = Account.Instance.GetConsumerSecret();

            oauth.Callback = oAuth.UrlEncode(callbackUrl);

            string authHeader = oauth.GetAuthHeader(oAuth.HttpMethods.POST, requestTokenUrl);

            WebClient requestClient = new WebClient();
            requestClient.Headers[HttpRequestHeader.Authorization] = authHeader;

            requestClient.UploadStringCompleted += new UploadStringCompletedEventHandler(TempTokenReceived);
            requestClient.UploadStringAsync(new Uri(requestTokenUrl, UriKind.Absolute), "");
        }

        void TempTokenReceived(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ErrorHandler(e);
            }
            else
            {
                string result = e.Result;

                string[] resultParams = result.Split(new Char[] { '&' });

                foreach (string param in resultParams)
                {
                    string[] keyVal = param.Split(new Char[] { '=' });

                    switch (keyVal[0])
                    {
                        case "oauth_token":
                            tempToken = keyVal[1];
                            break;
                        case "oauth_token_secret":
                            tempSecret = keyVal[1];
                            break;
                        case "login_url":
                            activationUrl = HttpUtility.UrlDecode(keyVal[1]);
                            //activationUrl = "https://www.lovefilm.com/visitor/mobileauthorise.html";
                            break;
                        default:
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(tempToken) && !string.IsNullOrEmpty(tempSecret) && !string.IsNullOrEmpty(activationUrl))
                {
                    string act_url = string.Format("{0}?oauth_token={1}&oauth_callback={2}", activationUrl, tempToken, oAuth.UrlEncode(callbackUrl));
                    AuthBrowser.Navigate(new Uri(act_url), null, userAgentString);
                }
                else
                {
                    ErrorHandler();
                }
            }
        }

        #endregion

        #region Verify request & intercept oAuth Tokens

        private void AuthBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            StatusText.Text = status_loading_lovefilm;
            ShowLoading();

            if (e.Uri.Host.Contains("madebywill"))
            {
                StatusText.Text = status_verifying_account;
                e.Cancel = true;
                Verify(e.Uri.OriginalString);

                AuthBrowser.Visibility = Visibility.Collapsed;
            }
        }

        private void AuthBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.LocalPath == "/account/oauth/authorise.html")
            {
                instructions.Text = "Tap Link My Account";
            }

            HideLoading();
        }

        private void Verify(string url)
        {
            string[] resultParams = url.Split(new Char[] { '&' });

            foreach (string param in resultParams)
            {
                string[] keyVal = param.Split(new Char[] { '=' });

                if (keyVal[0] == "oauth_verifier")
                {
                    oauthVerifier = keyVal[1];
                }
            }

            if (!string.IsNullOrEmpty(tempToken) && !string.IsNullOrEmpty(oauthVerifier))
            {
                oAuth oauth = new oAuth();

                oauth.ConsumerKey = Account.Instance.GetConsumerKey();
                oauth.ConsumerSecret = Account.Instance.GetConsumerSecret();
                oauth.Verifier = oauthVerifier;
                oauth.Token = tempToken;
                oauth.TokenSecret = tempSecret;

                string authHeader = oauth.GetAuthHeader(oAuth.HttpMethods.POST, accessTokenUrl);

                WebClient verifyClient = new WebClient();
                verifyClient.Headers[HttpRequestHeader.Authorization] = authHeader;

                verifyClient.UploadStringCompleted += new UploadStringCompletedEventHandler(verifyClient_UploadStringCompleted);
                verifyClient.UploadStringAsync(new Uri(accessTokenUrl, UriKind.Absolute), "");

            }
            else
            {
                ErrorHandler();
            }
        }

        void verifyClient_UploadStringCompleted(object sender, UploadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ErrorHandler(e);
            }
            else
            {
                string oauthToken = "";
                string oauthTokenSecret = "";

                string result = e.Result;

                string[] resultParams = result.Split(new Char[] { '&' });

                foreach (string param in resultParams)
                {
                    string[] keyVal = param.Split(new Char[] { '=' });

                    switch (keyVal[0])
                    {
                        case "oauth_token":
                            oauthToken = keyVal[1];
                            break;
                        case "oauth_token_secret":
                            oauthTokenSecret = keyVal[1];
                            break;
                        default:
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(oauthToken) && !string.IsNullOrEmpty(oauthTokenSecret))
                {
                    IsolatedStorageSettings.ApplicationSettings["oauth_token"] = oauthToken;
                    IsolatedStorageSettings.ApplicationSettings["oauth_secret"] = oauthTokenSecret;

                    Account.Instance.LoadTokens();

                    User lfUser = new User();
                    lfUser.GetCurrentUser((ustatus, userId) =>
                    {
                        if (!ustatus || string.IsNullOrEmpty(userId))
                        {
                            ErrorHandler();
                        }
                        else
                        {
                            lfUser.GetUserProfile(userId, (userProfile, status) =>
                            {
                                if (!status)
                                {
                                    ErrorHandler();
                                }
                                else
                                {
                                    IsolatedStorageSettings.ApplicationSettings["profile_user_id"] = userProfile.Id;
                                    IsolatedStorageSettings.ApplicationSettings["profile_at_home_url"] = userProfile.AtHomeUrl;
                                    IsolatedStorageSettings.ApplicationSettings["profile_queue_url"] = userProfile.QueuesUrl;
                                    IsolatedStorageSettings.ApplicationSettings["profile_ratings_url"] = userProfile.RatingsUrl;
                                    IsolatedStorageSettings.ApplicationSettings["profile_rented_url"] = userProfile.RentedUrl;

                                    IsolatedStorageSettings.ApplicationSettings.Save();

                                    NavigationService.GoBack();
                                }
                            });
                        }
                    });
   
                }
                else
                {
                    ErrorHandler();
                }
            }

        }

        #endregion

        private void ErrorHandler(UploadStringCompletedEventArgs e = null)
        {
            if (e == null)
            {
                DisplayGenericErrorMessage();
                return;
            }

            string errorCode = ServiceBase.GetErrorCode(e);

            switch (errorCode)
            {
                case "NETWORK_UNAVAILABLE":
                    MessageBox.Show("A network connection could not be found, check your wi-fi & mobile network status.", "Network unavailable", MessageBoxButton.OK);
                    NavigationService.GoBack();
                    break;
                case "ERR_401_TIMESTAMP_IS_INVALID":
                    MessageBox.Show("The date or time on your device does not match the server. Check your date & time settings.", "Check your clock", MessageBoxButton.OK);
                    NavigationService.GoBack();
                    break;
                default:
                    DisplayGenericErrorMessage(errorCode);
                    break;
            }
        }

        private void DisplayGenericErrorMessage(string errorCode = "")
        {
            string msg = "We're having trouble reaching the LOVEFiLM service right now, would you like to try again?";

            if (errorCode != "")
            {
                msg += Environment.NewLine + Environment.NewLine + "ERROR CODE: " + errorCode;
            }

            MessageBoxResult errorResult = MessageBox.Show(msg, "Sorry", MessageBoxButton.OKCancel);

            if (errorResult == MessageBoxResult.OK)
            {
                RequestOAuthToken();
            }
            else
            {
                NavigationService.GoBack();
            }
        }

        private void ShowLoading()
        {
            LoadingProgress.IsIndeterminate = true;
            
        }

        private void HideLoading()
        {
            StatusText.Text = status_blank;
            LoadingProgress.IsIndeterminate = false;
        }
    }
}