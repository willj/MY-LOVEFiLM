using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace Madebywill.Helpers
{
    public class oAuthParam
    {
        private string key = null;
        private string value = null;

        public oAuthParam(string key, string value)
        {
            this.key = key;
            this.value = value;
        }

        public string Key{
            get { return key; }
        }

        public string Value
        {
            get { return value; }
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", key, value);
        }

    }

    public class oAuth
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }

        public string Token { get; set; }
        public string TokenSecret { get; set; }

        public string Verifier { get; set; }
        public string Callback { get; set; }

        private int timeStamp;
        private string nonce;

        public enum HttpMethods { GET, POST, PUT, DELETE}

        public string GetAuthHeader(HttpMethods method, string url, List<oAuthParam> userParams = null)
        {
            setTimestamp();
            setNonce();

            List<oAuthParam> oAuthParams = new List<oAuthParam>();

            oAuthParams.Add(new oAuthParam("oauth_consumer_key", ConsumerKey));
            oAuthParams.Add(new oAuthParam("oauth_nonce", nonce));
            oAuthParams.Add(new oAuthParam("oauth_signature_method","HMAC-SHA1"));
            oAuthParams.Add(new oAuthParam("oauth_timestamp",timeStamp.ToString()));
            oAuthParams.Add(new oAuthParam("oauth_version", "1.0"));

            if (!string.IsNullOrEmpty(Token))
            {
                oAuthParams.Add(new oAuthParam("oauth_token", Token));
            }            
            
            if (!string.IsNullOrEmpty(Verifier))
            {
                oAuthParams.Add(new oAuthParam("oauth_verifier", Verifier));
            }

            if (!string.IsNullOrEmpty(Callback))
            {
                oAuthParams.Add(new oAuthParam("oauth_callback", Callback));
            }       

            string baseString = buildBaseString(method, url, oAuthParams, userParams);
            string signature = getSignature(baseString, ConsumerSecret + "&" + TokenSecret);

            oAuthParams.Add(new oAuthParam("oauth_signature", signature));

            StringBuilder authHeader = new StringBuilder();

            authHeader.Append("OAuth ");

            //oAuthParams.Sort();

            int i = 0;

            foreach (oAuthParam param in oAuthParams)
            {
                if (i == 0)
                {
                    authHeader.AppendFormat("{0}=\"{1}\"", param.Key, param.Value);
                    i++;
                }
                else
                {
                    authHeader.AppendFormat(", {0}=\"{1}\"", param.Key, param.Value);
                }    
            }

            //Reset these, they should be set for each request they're needed on.
            Verifier = "";
            Callback = "";

            return authHeader.ToString();
        }

        private string buildBaseString(HttpMethods method, string url, List<oAuthParam> oAuthParams, List<oAuthParam> userParams)
        {
            List<string> allParams = new List<string>();

            foreach (oAuthParam param in oAuthParams)
            {
                allParams.Add(param.ToString());
            }

            if (userParams != null)
            {
                foreach (oAuthParam param in userParams)
                {
                    allParams.Add(param.ToString());
                }
            }

            allParams.Sort();

            StringBuilder paramString = new StringBuilder();

            int i = 0;

            foreach (string param in allParams)
            {
                if (i == 0)
                {
                    paramString.Append(param);
                    i++;
                }
                else
                {
                    paramString.AppendFormat("&{0}", param);
                }
            }

            StringBuilder baseString = new StringBuilder();
            baseString.AppendFormat("{0}&",method.ToString());
            baseString.AppendFormat("{0}&", UrlEncode(url));
            baseString.Append(UrlEncode(paramString.ToString()));

            return baseString.ToString();

        }

        private void setNonce()
        {
            string guid = System.Guid.NewGuid().ToString();
            nonce = guid.Replace("-", "");
        }

        private void setTimestamp()
        {
            TimeSpan tspan = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            timeStamp = (int)tspan.TotalSeconds;
        }

        public static string UrlEncode(string Input)
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

        private string getSignature(string base_string, string key)
        {
            UTF8Encoding stringEncoder = new UTF8Encoding();

            HMACSHA1 hmac = new HMACSHA1(stringEncoder.GetBytes(key));

            byte[] hashval = hmac.ComputeHash(stringEncoder.GetBytes(base_string));

            return UrlEncode(Convert.ToBase64String(hashval));
        }

    }
}
