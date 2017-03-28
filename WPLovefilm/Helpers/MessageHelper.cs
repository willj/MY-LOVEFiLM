using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Phone.Tasks;
using System.IO.IsolatedStorage;

namespace WPLovefilm.Helpers
{
    class MessageHelper
    {
        private static string messageUrl = "http://lfapp.s3.amazonaws.com/messages.xml";
        private bool isAccountLoggedIn;

        public MessageHelper(bool isAccountLoggedIn)
        {
            this.isAccountLoggedIn = isAccountLoggedIn;
        }

        public void GetMessages()
        {
            BackgroundWorker bw = new BackgroundWorker();

            bw.DoWork += (s, e) =>
            {
                WebClient wc = new WebClient();
                wc.DownloadStringCompleted += wc_DownloadStringCompleted;
                wc.DownloadStringAsync(new Uri(messageUrl));
            };

            bw.RunWorkerAsync();
        }

        void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }

            ProcessMessages(e.Result);
        }

        private void ProcessMessages(string messageString)
        {
            XDocument xml = XDocument.Parse(messageString);

            var messages = from message in xml.Descendants("message")
                           select new Message
                           {
                               Id = (string)message.Element("id") ?? "",
                               Title = (string)message.Element("title") ?? "",
                               Body = (string)message.Element("body") ?? "",
                               Link = (string)message.Element("link") ?? "",
                               Frequency = (string)message.Element("frequency") ?? "",
                               AccountStatus = (string)message.Element("account_status") ?? ""
                           };

            if (messages.Count() < 1)
            {
                return;
            }

            //We're going to be lazy and assume "show always" messages will appear first
            foreach (Message message in messages)
            {
                if (isAccountLoggedIn)
                {
                    if (message.AccountStatus == "Any" || message.AccountStatus == "LoggedIn")
                    {
                        if (message.Frequency == "Always")
                        {
                            DisplayMessage(message);
                            return;
                        }
                        else if (message.Frequency == "Once")
                        {
                            List<string> readMessages = new List<string>();

                            //has it been shown before?
                            if (IsolatedStorageSettings.ApplicationSettings.Contains("ReadMessages")){
                                readMessages = (List<string>)IsolatedStorageSettings.ApplicationSettings["ReadMessages"];

                                if (readMessages.Contains(message.Id))
                                {
                                    continue;
                                }
                            }

                            //show it
                            DisplayMessage(message);

                            //mark as shown
                            readMessages.Add(message.Id);
                            IsolatedStorageSettings.ApplicationSettings["ReadMessages"] = readMessages;
                            IsolatedStorageSettings.ApplicationSettings.Save();
                            
                            return;
                        }
                    }
                }
                else
                {
                    if (message.Frequency == "Always" && (message.AccountStatus == "Any" || message.AccountStatus == "LoggedOut"))
                    {
                        DisplayMessage(message);
                        return;
                    }
                }
            }
        }

        private void DisplayMessage(Message m)
        {
            SmartDispatcher.BeginInvoke(() =>
            {
                if (m.Link == string.Empty)
                {
                    MessageBox.Show(m.Body, m.Title, MessageBoxButton.OK);
                }
                else
                {
                    MessageBoxResult r = MessageBox.Show(m.Body, m.Title, MessageBoxButton.OKCancel);

                    if (r == MessageBoxResult.OK)
                    {
                        try
                        {
                            WebBrowserTask t = new WebBrowserTask();
                            t.Uri = new Uri(m.Link);
                            t.Show();
                        }
                        catch (Exception)
                        {
                            //can't do anything about that
                        }
                    }
                }
            });
        }

    }

    class Message
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Link { get; set; }
        public string Frequency { get; set; }
        public string AccountStatus { get; set; }
    }
}
