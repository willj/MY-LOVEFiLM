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
    public class User : ServiceBase
    {
        /// <summary>
        /// Requests the current user.
        /// Callback recieves URL for the current user profile
        /// Method will callback with an empty string if it fails.
        /// </summary>
        /// <param name="callback"></param>
        public void GetCurrentUser(Action<bool, string> callback)
        {
            string requestUrl = "http://openapi.lovefilm.com/users";
            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.GET, requestUrl);

            HttpGet(requestUrl, authHeader, (result, status) =>
            {
                string userUrl = string.Empty;
                bool userStatus = false;

                if (status)
                {
                    userUrl = XDocument.Parse(result).Element("resource").Element("link").Attribute("href").Value ?? "";

                    if (!string.IsNullOrEmpty(userUrl))
                    {
                        userStatus = true;
                    }
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(userStatus, userUrl);
                });
            });
        }

        /// <summary>
        /// Request current user profile.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="callback"></param>
        public void GetUserProfile(string userId, Action<LFUser, bool> callback)
        {
            string authHeader = oAuth.GetAuthHeader(oAuth.HttpMethods.GET, userId);

            HttpGet(userId, authHeader, (result, status) =>
            {
                LFUser profile = new LFUser();

                if (status)
                {
                    XDocument xml = XDocument.Parse(result);

                    profile = (from u in xml.Descendants("user")
                                select new LFUser
                                {
                                    Id = (string)u.Element("id") ?? "",
                                    FirstName = (string)u.Element("first_name") ?? "",
                                    LastName = (string)u.Element("last_name") ?? "",
                                    AtHomeUrl = (string)u.Elements("link").FirstOrDefault(l => l.Attribute("rel").Value == "http://schemas.lovefilm.com/at_home").Attribute("href").Value ?? "",
                                    QueuesUrl = (string)u.Elements("link").FirstOrDefault(l => l.Attribute("rel").Value == "http://schemas.lovefilm.com/queues").Attribute("href").Value ?? "",
                                    RentedUrl = (string)u.Elements("link").FirstOrDefault(l => l.Attribute("rel").Value == "http://schemas.lovefilm.com/rented").Attribute("href").Value ?? "",
                                    RatingsUrl = (string)u.Elements("link").FirstOrDefault(l => l.Attribute("rel").Value == "http://schemas.lovefilm.com/ratings").Attribute("href").Value ?? ""
                                }).SingleOrDefault();
                }

                SmartDispatcher.BeginInvoke(() =>
                {
                    callback(profile, status);
                });

            });

        }


    }
}
