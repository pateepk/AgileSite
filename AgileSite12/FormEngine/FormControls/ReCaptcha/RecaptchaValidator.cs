using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;

using CMS.EventLog;

using Newtonsoft.Json;

namespace CMS.FormEngine
{
    /// <summary>
    /// Calls the reCAPTCHA server to validate the answer to a reCAPTCHA challenge.
    /// </summary>
    public class RecaptchaValidator
    {
        private const string VERIFYURL = "https://www.google.com/recaptcha/api/siteverify";

        private string mRemoteIp;


        /// <summary>
        /// The shared key between the site and reCAPTCHA.
        /// </summary>
        public string PrivateKey
        {
            get;
            set;
        }


        /// <summary>
        /// The user's IP address.
        /// </summary>
        public string RemoteIP
        {
            get
            {
                return mRemoteIp;
            }

            set
            {
                IPAddress ip = IPAddress.Parse(value);

                if (ip == null ||
                    (ip.AddressFamily != AddressFamily.InterNetwork &&
                    ip.AddressFamily != AddressFamily.InterNetworkV6))
                {
                    throw new ArgumentException("Expecting an IP address, got " + ip);
                }

                mRemoteIp = ip.ToString();
            }
        }


        /// <summary>
        /// The user response token provided by reCAPTCHA, verifying the user on your site.
        /// </summary>
        public string Response
        {
            get;
            set;
        }


        /// <summary>
        /// Validate reCAPTCHA response
        /// </summary>
        public RecaptchaResponse Validate()
        {
            // Prepare web request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(VERIFYURL);
            request.ProtocolVersion = HttpVersion.Version10;
            request.Timeout = 30 * 1000 /* 30 seconds */;
            request.Method = "POST";
            request.UserAgent = "reCAPTCHA/ASP.NET";
            request.ContentType = "application/x-www-form-urlencoded";

            // Prepare form data
            string formdata = $"secret={HttpUtility.UrlEncode(PrivateKey)}&remoteip={HttpUtility.UrlEncode(RemoteIP)}&response={HttpUtility.UrlEncode(Response)}";

            // Write data to request
            byte[] formbytes = Encoding.ASCII.GetBytes(formdata);
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(formbytes, 0, formbytes.Length);
            }

            // Get validation response
            try
            {
                using (WebResponse httpResponse = request.GetResponse())
                {
#pragma warning disable BH1014 // Do not use System.IO
                    using (StreamReader readStream = new StreamReader(httpResponse.GetResponseStream(), Encoding.UTF8))
#pragma warning restore BH1014 // Do not use System.IO
                    {
                        var responseJson = readStream.ReadToEnd();
                        return JsonConvert.DeserializeObject<RecaptchaResponse>(responseJson);
                    }
                }
            }
            catch (WebException ex)
            {
                EventLogProvider.LogException("ReCAPTCHA", "VALIDATE", ex);
                return null;
            }
        }
    }
}
