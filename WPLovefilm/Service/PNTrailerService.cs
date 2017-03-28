using System;
using System.Net;
using System.Windows;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Windows.Threading;
using System.Xml.Linq;
using System.Linq;
using WPLovefilm.Helpers;
using WPLovefilm.Models;
using Madebywill.Helpers;

namespace WPLovefilm.Service
{
    public static class PNTrailerService
    {
        public static void GetTrailer(string TitleId, Action<PNTrailer> callback)
        {
            if (string.IsNullOrEmpty(TitleId))
            {
                return;
            }

            string url = BuildQuery(TitleId);

            HttpWebRequest request = HttpWebRequest.CreateHttp(url);

            request.BeginGetResponse((result) =>
            {
                ResponseHandler(callback, result);
            }, request);

        }

        private static void ResponseHandler(Action<PNTrailer> resultCallback, IAsyncResult asyncResult)
        {
            try
            {
                var request = (HttpWebRequest)asyncResult.AsyncState;
                var response = request.EndGetResponse(asyncResult);

                using (var rs = response.GetResponseStream())
                {
                    using (var sr = new StreamReader(rs))
                    {
                        string result = sr.ReadToEnd();

                        XDocument xml = XDocument.Parse(result);

                        XNamespace ns = "http://sdb.amazonaws.com/doc/2009-04-15/";

                        PNTrailer trailer = (from t in xml.Descendants(ns + "Item")
                                           select new PNTrailer
                                           {
                                               LowTrailer = (string)t.Elements(ns + "Attribute").FirstOrDefault(a => a.Element(ns + "Name").Value == "low_trailer").Element(ns + "Value").Value ?? "",
                                               HighTrailer = (string)t.Elements(ns + "Attribute").FirstOrDefault(a => a.Element(ns + "Name").Value == "high_trailer").Element(ns + "Value").Value ?? ""
                                           }).SingleOrDefault();

                        if (trailer != null)
                        {
                            SmartDispatcher.BeginInvoke(() =>
                            {
                                resultCallback(trailer);
                            });
                        }
                    }
                }
            }
            catch
            {
                return;
            }
        }

        private static string BuildQuery(string TitleId)
        {
            string query = "SELECT low_trailer, high_trailer FROM LFTrailers WHERE dvd_link = '" + TitleId + "' OR bluray_link = '" + TitleId + "' OR disc3_link = '" + TitleId + "' OR disc4_link = '" + TitleId + "' LIMIT 1";

            StringBuilder ts = new StringBuilder();
            ts.AppendFormat("{0:yyyy-MM-ddTHH:mm:ssZ}", DateTime.Now.ToUniversalTime().ToUniversalTime());

            StringBuilder stringToSign = new StringBuilder();
            stringToSign.Append("GET\nsdb.amazonaws.com\n/\n");
            stringToSign.AppendFormat("AWSAccessKeyId={0}", UrlEncode(GetAWSKey()));
            stringToSign.Append("&Action=Select");
            stringToSign.AppendFormat("&SelectExpression={0}", UrlEncode(query));
            stringToSign.Append("&SignatureMethod=HmacSHA1");
            stringToSign.Append("&SignatureVersion=2");
            stringToSign.AppendFormat("&Timestamp={0}", UrlEncode(ts.ToString()));
            stringToSign.Append("&Version=2009-04-15");

            string sig = GetSignature(stringToSign.ToString());

            StringBuilder url = new StringBuilder();
            url.Append("https://sdb.amazonaws.com?Action=Select");
            url.AppendFormat("&SelectExpression={0}", UrlEncode(query));
            url.Append("&SignatureMethod=HmacSHA1&SignatureVersion=2");
            url.AppendFormat("&Timestamp={0}", UrlEncode(ts.ToString()));
            url.AppendFormat("&Version=2009-04-15&AWSAccessKeyId={0}", UrlEncode(GetAWSKey()));
            url.AppendFormat("&Signature={0}", sig);

            return url.ToString();
        }

        private static string GetAWSKey()
        {
			// Removed
        }

        private static string GetAWSSecret()
        {
            // Removed
        }

        private static string GetSignature(string stringToSign)
        {
            UTF8Encoding stringEncoder = new UTF8Encoding();

            HMACSHA1 hmac = new HMACSHA1(stringEncoder.GetBytes(GetAWSSecret()));

            byte[] hashval = hmac.ComputeHash(stringEncoder.GetBytes(stringToSign));

            return UrlEncode(Convert.ToBase64String(hashval));

        }

        private static string UrlEncode(string Input)
        {
            string UnreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

            StringBuilder Result = new StringBuilder();

            for (int x = 0; x < Input.Length; x++)
            {
                if (UnreservedChars.IndexOf(Input[x]) != -1)
                {
                    Result.Append(Input[x]);
                }
                else
                {
                    Result.Append("%" + String.Format("{0:X2}", (int)Input[x]));
                }
            }
            return Result.ToString();
        }
    }
}
