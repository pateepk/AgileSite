namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Data;
    using System.Collections.Specialized;
    using System.Web.Script.Serialization;
    using System.Net;
    using System.Web.Routing;

    public static class Utils
    {

        #region RequestCommon // Enum, Class, Public Functions to get parameter values from Query String, Form, Session
        // all types of class that need convertion from string to bool/date/integer/float, etc
        public enum getTypes
        {
            QueryString = 1,
            Form = 2,
            Session = 3,
            Cookie = 4,
            AppSeetings = 5

        }

        /// <summary>
        /// Helper to make something like get getQueryString(key).ToSomething else!
        /// </summary>
        public class CastTo
        {
            private string _val = String.Empty;

            public CastTo(string key, getTypes gt)
            {
                switch (gt)
                {
                    case getTypes.QueryString:
                        _val = HttpContext.Current.Request.QueryString[key];
                        break;
                    case getTypes.Form:
                        _val = HttpContext.Current.Request.Form[key];
                        break;
                    case getTypes.Session:
                        if (HttpContext.Current.Session[key] != null)
                        {
                            _val = (string)HttpContext.Current.Session[key];
                        }
                        break;
                    case getTypes.Cookie:
                        HttpCookie cookie = HttpContext.Current.Request.Cookies[key];
                        if (cookie == null)
                        {
                            _val = string.Empty;
                        }
                        else
                        {
                            _val = HttpContext.Current.Server.UrlDecode(cookie.Value);
                        }
                        break;
                    case getTypes.AppSeetings:
                        _val = AppSettings.Data(key).ToString();
                        break;
                }


                if (_val == null)
                {
                    _val = String.Empty;
                }

            }

            public bool ToBool()
            {
                bool bresult = false;
                bool.TryParse(_val, out bresult);
                return bresult;
            }

            public override string ToString()
            {
                return _val == null ? "" : _val;
            }

            public int ToInt()
            {
                int iresult = 0;
                int.TryParse(_val, out iresult);
                return iresult;
            }

            public decimal ToDecimal()
            {
                decimal dresult = 0;
                decimal.TryParse(_val, out dresult);
                return dresult;
            }

            public double ToDouble()
            {
                double dresult = 0;
                double.TryParse(_val, out dresult);
                return dresult;
            }

            public DateTime ToDateTime()
            {
                DateTime dresult = DateTime.MinValue;
                DateTime.TryParse(_val, out dresult);
                return dresult;
            }

            public Int64 ToInt64()
            {
                Int64 iresult = 0;
                Int64.TryParse(_val, out iresult);
                return iresult;
            }

        }


        /// <summary>
        /// Get parameter from Query String! All request has to use this function!
        /// </summary>
        /// <param name="key">pass the key (parameter inside the query string)</param>
        /// <returns>result still need a conversion to type needed (bool, date, integer, etc)</returns>
        public static CastTo getQueryString(string key)
        {
            CastTo myCTT = new CastTo(key, getTypes.QueryString);

            return myCTT;

        }

        /// <summary>
        /// Get the whole string (for error manager). Space & for line break
        /// </summary>
        /// <returns></returns>
        public static string getQueryStringForLogs()
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Request.ServerVariables[SV.ServerVariables.QUERY_STRING].Replace("&", " &");
        }

        public static string getCompleteQueryString()
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Request.ServerVariables[SV.ServerVariables.QUERY_STRING];
        }

        public static string getUserAgent()
        {
            if (HttpContext.Current != null)
            {
                string ua = HttpContext.Current.Request.ServerVariables[SV.ServerVariables.HTTP_USER_AGENT];
                if (ua != null)
                {
                    return ua;
                }
            }
            return "";
        }

        public static bool IsFacebookAgent()
        {
            if (HttpContext.Current != null)
            {
                string ua = HttpContext.Current.Request.ServerVariables[SV.ServerVariables.HTTP_USER_AGENT];
                if ((ua != null) && (ua.IndexOf(SV.UserAgentDetectTokens.FacebookBot) > -1))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsBrowserTouchScreen()
        {
            if (HttpContext.Current != null)
            {
                string ua = HttpContext.Current.Request.ServerVariables[SV.ServerVariables.HTTP_USER_AGENT];
                if ((ua != null) && ((ua.IndexOf(SV.UserAgentDetectTokens.Android) > -1) ||
                    (ua.IndexOf(SV.UserAgentDetectTokens.Blackberry) > -1) ||
                    (ua.IndexOf(SV.UserAgentDetectTokens.IEMobile) > -1) ||
                    (ua.IndexOf(SV.UserAgentDetectTokens.iPad) > -1) ||
                    (ua.IndexOf(SV.UserAgentDetectTokens.iPhone) > -1)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get parameter from FORM! All request has to use this function!
        /// </summary>
        /// <param name="key">pass the key (parameter inside the query string)</param>
        /// <returns>result still need a conversion to type needed (bool, date, integer, etc)</returns>
        public static CastTo getForm(string key)
        {
            CastTo myCTT = new CastTo(key, getTypes.Form);

            return myCTT;

        }

        /// <summary>
        /// Get all data from form for error manager
        /// </summary>
        /// <returns></returns>
        public static string getFormForLogs()
        {
            string allData = string.Empty;
            try
            {
                if (getHttpMethod() == HttpMethods.POST)
                {
                    foreach (string key in HttpContext.Current.Request.Form.AllKeys)
                    {
                        if (key.IndexOf("__") == -1)
                        {
                            allData += String.Format("{0}={1} &", key, HttpContext.Current.Request.Form[key]);
                        }
                    }
                }
            }
            catch { }
            return allData;

        }

        public static HttpMethods getHttpMethod()
        {
            if ((HttpContext.Current != null) && (HttpContext.Current.Request.HttpMethod.ToUpper() == "POST"))
            {
                return HttpMethods.POST;
            }
            else
            {
                return HttpMethods.GET;
            }
        }

        /// <summary>
        /// Set session as string! If you need object, use getSessionObject. All reaquest of Session, use this.
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>result still need a conversion to type needed (bool, date, integer, etc)</returns>
        internal static CastTo getSession(string key)
        {
            CastTo myCTT = new CastTo(key, getTypes.Session);
            return myCTT;
        }


        /// <summary>
        /// Reset all session keys to empty string
        /// On logout, after you reset session, you may need to create new user manager right away
        /// because page may still need to access user manager that has blank user
        /// </summary>
        public static void resetSession()
        {
            for (int i = 0; i < HttpContext.Current.Session.Count; i++)
            {
                HttpContext.Current.Session.RemoveAt(i);
            }
        }

        public static void resetSession(string key)
        {
            if ((HttpContext.Current.Session != null) && (HttpContext.Current.Session[key] != null))
            {
                HttpContext.Current.Session[key] = null;
            }
        }

        /// <summary>
        /// Get Cookie value. Centralize using this function!
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static CastTo getCookie(string key)
        {
            CastTo myCTT = new CastTo(key, getTypes.Cookie);

            return myCTT;

        }

        /// <summary>
        /// Get value from Application Settings
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static CastTo getAppSettings(string key)
        {
            CastTo myCTT = new CastTo(key, getTypes.AppSeetings);

            return myCTT;

        }

        public static void resetCookies()
        {
            HttpContext.Current.Request.Cookies.Clear();
        }

        #endregion

        public static string getServerName()
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Request.ServerVariables[SV.ServerVariables.SERVER_NAME];
        }

        public static string getApplicationPath()
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Request.ApplicationPath;

        }

        public static void rewritePath(string path)
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.RewritePath(path);
            }
        }

        public static string getPath()
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Request.Path;

        }

        public static string getApplicationPath(string path)
        {
            string ap = getApplicationPath();
            if ((ap != null) && (!ap.EndsWith("/")))
            {
                ap += "/";
            }
            return ap + path;
        }


        public static string getClientIPAddress()
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Request.ServerVariables[SV.ServerVariables.REMOTE_ADDR];
        }

        public static string getHttpReferer()
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Request.ServerVariables[SV.ServerVariables.HTTP_REFERER];
        }

        public static string getHttpHost()
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Request.ServerVariables[SV.ServerVariables.HTTP_HOST];
        }

        public static string getScriptName()
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Request.ServerVariables[SV.ServerVariables.SCRIPT_NAME];
        }

        public static string getRawURL()
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Request.RawUrl;
        }

        /// <summary>
        /// Return Server.MapPath and forcing with \ at the end if there is no \
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getMapPath(string path)
        {
            if (HttpContext.Current != null)
            {
                string sp = HttpContext.Current.Server.MapPath(path);
                if (!sp.EndsWith("\\"))
                {
                    sp += "\\";
                }
                return sp;
            }
            else
            {
                return "";
            }
        }

        public static string getMapFile(string fileName)
        {
            string sp = HttpContext.Current.Server.MapPath(fileName);
            return sp;
        }

        public static string getAttribute(XmlNode xn, string key)
        {
            string s = string.Empty;
            XmlNode xnk = xn.Attributes[key];
            if (xnk != null)
            {
                s = xnk.Value;
                if (String.IsNullOrEmpty(s))
                {
                    s = string.Empty;
                }
            }
            return s;

        }

        public static int getAttributeInteger(XmlNode xn, string key)
        {
            string s = getAttribute(xn, key);
            int ri = 0;
            if (int.TryParse(s, out ri))
            {
                return ri;
            }
            else
            {
                return 0;
            }
        }

        public static bool getAttributeBool(XmlNode xn, string key)
        {
            bool rbool = false;
            string s = getAttribute(xn, key);
            bool.TryParse(s, out rbool);
            return rbool;
        }

        public static void setSessionObject(string key, object obj)
        {
            if (HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[key] = obj;
            }
            else
            {
                ErrorManager.logMessage(SV.ErrorMessages.NoSessionObject);
            }
        }

        public static void setSessionDataTable(string key, object obj)
        {
            setSessionObject(key, obj);
        }

        public static object getSessionObject(string key)
        {
            if (HttpContext.Current.Session != null)
            {
                return HttpContext.Current.Session[key];
            }
            else
            {
                string[] exKey = new string[] { SV.Sessions.UserManager }; // put execption here
                if (!exKey.Contains(key))
                {
                    ErrorManager.logMessage(String.Format(SV.ErrorMessages.NullSession, key));
                }
                return null;
            }
        }

        public static DataTable getSessionDataTable(string key)
        {
            DataTable dt = null;
            object obj = getSessionObject(key);
            if (obj != null)
            {
                dt = (DataTable)obj;
            }
            return dt;
        }

        public static void setPageItem(string key, object obj)
        {
            if (HttpContext.Current.Items.Contains(key))
            {
                HttpContext.Current.Items.Remove(key);
            }
            HttpContext.Current.Items.Add(key, obj);
        }

        public static string getSessionID()
        {
            if (HttpContext.Current.Session != null)
            {
                return HttpContext.Current.Session.SessionID;
            }
            else
            {
                return String.Empty;
            }
        }

        public static object getPageItem(string key)
        {
            return HttpContext.Current == null ? String.Empty : HttpContext.Current.Items[key];
        }

        public static XDocument modelToXDocument(object model)
        {
            XDocument xdoc = new XDocument();
            XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(model.GetType());
            MemoryStream memoryStream = new MemoryStream();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xs.Serialize(xmlTextWriter, model);
            memoryStream.Position = 0;
            xdoc = XDocument.Load(memoryStream);
            return xdoc;
        }

        public static object xmlFileToModel(string fileName, Type modelType)
        {
            // Create an instance of the XmlSerializer specifying type and namespace.
            XmlSerializer serializer = new
            XmlSerializer(modelType);

            // A FileStream is needed to read the XML document.
            FileStream fs = new FileStream(fileName, FileMode.Open);
            XmlReader reader = new XmlTextReader(fs);

            // Declare an object variable of the type to be deserialized.
            object model;

            // Use the Deserialize method to restore the object's state.
            model = serializer.Deserialize(reader);

            fs.Close();

            return model;
        }


        public static void XDocumentAttributeSet(XDocument xdoc, string xpath, string attribute, string value)
        {
            if (xdoc.Root != null)
            {
                XNamespace nspace = xdoc.Root.Name.Namespace;
                string[] path = xpath.Split('/');
                XElement xe = xdoc.Root;
                int i = 0;
                do
                {
                    if (path[i].Length > 0)
                    {
                        xe = xe.Element(nspace + path[i]);
                    }
                    i++;
                } while ((i < path.Length) || (xe == null));
                if (xe != null)
                {
                    XAttribute xa = xe.Attribute(attribute);
                    if (xa != null)
                    {
                        xa.SetValue(value);
                    }
                }
            }
        }

        public static void XDocumentElementSet(XDocument xdoc, string xpath, string value)
        {
            if (xdoc.Root != null)
            {
                XNamespace nspace = xdoc.Root.Name.Namespace;
                string[] path = xpath.Split('/');
                XElement xe = xdoc.Root;
                int i = 0;
                do
                {
                    if (path[i].Length > 0)
                    {
                        xe = xe.Element(nspace + path[i]);
                    }
                    i++;
                } while ((i < path.Length) || (xe == null));
                if (xe != null)
                {
                    xe.SetValue(value);
                }
            }
        }

        public static void responseRedirect(string redirectURL)
        {
            responseRedirect(redirectURL, false);
        }

        public static void responseRedirect(string redirectURL, bool isExternalURL)
        {
            if (!isExternalURL)
            {
                HttpContext.Current.Response.Redirect(ResolveURL(redirectURL));
            }
            else
            {
                HttpContext.Current.Response.Redirect(redirectURL);
            }
        }

        public static void responseRedirectPermanent(string redirectPermanentURL)
        {
            HttpContext.Current.Response.RedirectPermanent(redirectPermanentURL);
        }

        public static string ResolveURL(string aspxfilename)
        {
            string apppath = getApplicationPath();
            if (apppath.Length == 1)
            {
                return '/' + aspxfilename;
            }
            else
            {
                return apppath + '/' + aspxfilename;
            }
        }

        public static string ResolveFullPathURL(string aspxfilename)
        {
            NameValueCollection nv = HttpContext.Current.Request.ServerVariables;
            string sv = nv[SV.ServerVariables.SERVER_NAME];
            string port = nv[SV.ServerVariables.SERVER_PORT] == "80" ? "" : nv[SV.ServerVariables.SERVER_PORT];
            bool isSecure = nv[SV.ServerVariables.SERVER_PORT_SECURE] == "1";
            string ap = Utils.getApplicationPath();
            if ((ap != null) && (ap.EndsWith("/")))
            {
                ap = ap.Substring(0, ap.Length - 1);
            }

            return (isSecure ? SV.Common.https : SV.Common.http) + "://" + sv + (String.IsNullOrEmpty(port) ? "" : ":") + port + ap + (aspxfilename.StartsWith("/") ? "" : "/") + aspxfilename;
        }

        public static string AddHelpTooltip(string message, string imageName)
        {
            return String.Format("<a rel=\"tooltip\" title=\"{0}\"><img alt=\"help\" border=\"0\" src=\"{1}\" /></a>", message, ResolveURL("images/" + (imageName.Length > 0 ? imageName : "question.png")));
        }

        public static string AddHelpTooltip(string message)
        {
            return AddHelpTooltip(message, String.Empty);
        }


        public static string DisableFocusOnFirstTextbox()
        {
            return String.Format("<input type=\"\">");
        }

        public static string AddRefreshPageEvery(int milliseconds)
        {
            // setRefreshPageEvery JS function is in ssch.js
            return "<script type=\"text/javascript\">$(document).ready(function() { setRefreshPageEvery(" + milliseconds.ToString() + "); });</script>";
        }

        public static void DisablePageCaching()
        {
            //Used for disabling page caching 
            HttpContext.Current.Response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
            HttpContext.Current.Response.Cache.SetValidUntilExpires(false);
            HttpContext.Current.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.Cache.SetNoStore();
        }

        public static string ObjectToJSON(object obj)
        {
            string sJSON = string.Empty;
            try
            {
                JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                sJSON = oSerializer.Serialize(obj);
            }
            catch (Exception ex)
            {
                ErrorManager.logError(String.Format(SV.ErrorMessages.ErrorJSONSerialize, obj.ToString()), ex);
            }
            return sJSON;
        }

        public static object JSONToObject(string json, Type targetType)
        {
            object obj = null;
            try
            {
                JavaScriptSerializer oSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                obj = oSerializer.Deserialize(json, targetType);
            }
            catch (Exception ex)
            {
                ErrorManager.logError(String.Format(SV.ErrorMessages.ErrorJSONDeserialize, targetType.ToString()), ex);
            }
            return obj;
        }

        public static double HoursCounting(DateTime dtime)
        {
            TimeSpan ts1 = dtime.Subtract(DateTime.Now.AddHours(3));
            double d = (int)ts1.TotalHours;
            return d;
        }

        public static string FormatTimeAfter(DateTime dtime1, DateTime dtime2)
        {
            string tago = string.Empty;
            TimeSpan ts1 = dtime2.Subtract(dtime1);
            int tdays = (int)ts1.TotalDays;
            if (tdays == 0)
            {
                int thours = (int)ts1.TotalHours;
                if (thours == 0)
                {
                    int tminutes = (int)ts1.TotalMinutes;
                    if (tminutes == 0)
                    {
                        tago = "seconds after";
                    }
                    else
                    {
                        tago = String.Format("{0} minutes after", tminutes);
                    }
                }
                else
                {
                    if (thours == 1)
                    {
                        tago = "1 hour after";
                    }
                    else
                    {
                        tago = String.Format("{0} hours after", thours);
                    }
                }
            }
            else
            {
                if (tdays == 1)
                {
                    tago = "1 day after";
                }
                else
                {
                    tago = String.Format("{0} days after", tdays);
                }
            }
            return tago;
        }

        public static string FormatTimeAgo(DateTime dtime)
        {
            string tago = string.Empty;
            TimeSpan ts1 = DateTime.Now.Subtract(dtime);
            int tdays = (int)ts1.TotalDays;
            if (tdays == 0)
            {
                int thours = (int)ts1.TotalHours;
                if (thours == 0)
                {
                    int tminutes = (int)ts1.TotalMinutes;
                    if (tminutes == 0)
                    {
                        tago = "seconds ago";
                    }
                    else
                    {
                        tago = String.Format("{0} minutes ago", tminutes);
                    }
                }
                else
                {
                    if (thours == 1)
                    {
                        tago = "1 hour ago";
                    }
                    else
                    {
                        tago = String.Format("{0} hours ago", thours);
                    }
                }
            }
            else
            {
                if (tdays == 1)
                {
                    tago = "yesterday";
                }
                else
                {
                    tago = String.Format("{0} days ago", tdays);
                }
            }
            return tago;
        }

        public static string FormatTimeFromNow(DateTime dtime)
        {
            string tago = string.Empty;
            TimeSpan ts1 = dtime.Subtract(DateTime.Now);
            int tdays = (int)ts1.TotalDays;
            if (tdays == 0)
            {
                int thours = (int)ts1.TotalHours;
                if (thours == 0)
                {
                    int tminutes = (int)ts1.TotalMinutes;
                    if (tminutes == 0)
                    {
                        tago = "seconds from now";
                    }
                    else
                    {
                        tago = String.Format("{0} minutes from now", tminutes);
                    }
                }
                else
                {
                    if (thours == 1)
                    {
                        tago = "1 hour from now";
                    }
                    else
                    {
                        tago = String.Format("{0} hours from now", thours);
                    }
                }
            }
            else
            {
                if (tdays == 1)
                {
                    tago = "tomorrow";
                }
                else
                {
                    tago = String.Format("{0} days from now", tdays);
                }
            }
            return tago;
        }

        public static string DatesCompareTypeDescription(int DatesCompareType)
        {
            switch (DatesCompareType)
            {
                case 1:
                    return "Left Outside";
                case 2:
                    return "Left Match";
                case 3:
                    return "Match";
                case 4:
                    return "Right Match";
                case 5:
                    return "Right Outside";
                case 6:
                    return "Left Right Match";
                default:
                    return "";
            }
        }

        public static string UrlEncode(string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        public static string UrlDecode(string str)
        {
            return HttpUtility.UrlDecode(str);
        }

        public static string HtmlEncode(string str)
        {
            return HttpUtility.HtmlEncode(str);
        }

        public static string StringToHTML(string str)
        {
            return str.Replace("\r\n", "<br>");
        }

        public static string GetViewMIDs(string MID, string OtherMIDs)
        {
            return MID + "<br />" + OtherMIDs.Replace(",", "<br />");
        }
        public static string XMLToHTML(XmlDocument xdoc)
        {
            // this is not generic xml. only one level xml
            StringBuilder strHTML = new StringBuilder();
            if (xdoc != null)
            {
                XmlNode xroot = xdoc.FirstChild;
                if (xroot.HasChildNodes)
                {
                    XmlNode xtitle = xroot.FirstChild;
                    strHTML.Append("<table border=0>");
                    strHTML.AppendFormat("<tr><td colspan=\"2\"><b>{0}</b></td></tr>", xtitle.Name);
                    string strTitle = xtitle.Name;
                    if (xtitle.HasChildNodes)
                    {
                        foreach (XmlNode item in xtitle.ChildNodes)
                        {
                            strHTML.AppendFormat("<tr><td valign=top align=left>{0}: </td><td valign=top align=left>{1}</td></tr>", item.Name, item.InnerText);
                        }
                    }
                    strHTML.Append("</table>");
                }
            }
            return strHTML.ToString();
        }

        public static string CountNumberToEnglish(int n)
        {
            switch (n)
            {
                case 0:
                    return "zeroth";
                case 1:
                    return "first";
                case 2:
                    return "second";
                case 3:
                    return "third";
                case 4:
                    return "fourth";
                case 5:
                    return "fifth";
                case 6:
                    return "sixth";
                case 7:
                    return "seventh";
                case 8:
                    return "eighth";
                case 9:
                    return "nineth";
                default:
                    return n.ToString();
            }
        }

        public static string getCorrectLocalPath(string path)
        {
            if ((path.IndexOf(':') > -1) || (path.StartsWith("\\\\")))
            {
                return path;
            }
            else
            {
                return Utils.getMapPath(Utils.getApplicationPath(path));
            }
        }

        public static string GenerateValidCharactersFileName(string fileName)
        {
            string fn = String.Empty;
            for (int i = 0; i < fileName.Length; i++)
            {
                char c = fileName.Substring(i, 1).ToUpper()[0];
                if (((c >= 'A') && (c <= 'Z')) || ((c >= '0') && (c <= '9')))
                {
                    fn += fileName.Substring(i, 1);
                }
            }
            return fn;
        }

        public static string CleanPhoneNumbers(string phoneNumber)
        {
            string fn = String.Empty;
            for (int i = 0; i < phoneNumber.Length; i++)
            {
                char c = phoneNumber.Substring(i, 1).ToUpper()[0];
                if (((c >= 'A') && (c <= 'Z')) || ((c >= '0') && (c <= '9')))
                {
                    fn += phoneNumber.Substring(i, 1);
                }
            }
            if (fn.Length == 7)
            {
                fn = fn.Substring(0, 3) + "-" + fn.Substring(3);
            }
            else if (fn.Length >= 10)
            {
                fn = "(" + fn.Substring(0, 3) + ")" + fn.Substring(3, 3) + "-" + fn.Substring(6);
            }

            return fn;
        }

        public static string CleanNonAlphaNumeric(string name)
        {
            string fn = String.Empty;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name.Substring(i, 1).ToUpper()[0];
                if ((c == ' ') || (c == '-') || ((c >= 'A') && (c <= 'Z')) || ((c >= '0') && (c <= '9')))
                {
                    fn += name.Substring(i, 1);
                }
            }

            return fn;
        }


        public static string getValueFromFile(string fileName)
        {
            string v = String.Empty;
            if (File.Exists(fileName))
            {
                try
                {
                    StreamReader streamReader = new StreamReader(fileName);
                    v = streamReader.ReadToEnd();
                    streamReader.Close();
                }
                catch (Exception ex)
                {
                    ErrorManager.logError(String.Format(SV.ErrorMessages.ErrorLoadingValueFromFile, fileName), ex);
                }
            }
            return v;
        }

        public static void generateResetPasswordKey(int UserID, out string key, out Guid guid)
        {
            guid = Guid.NewGuid();
            key = guid.ToString() + '#' + UserID.ToString();
            key = EncryptionManager.EncryptResetPassword(key);
        }

        public static void decryptPasswordKey(string key, out Guid guid, out int UserID)
        {
            UserID = 0;
            guid = new Guid();
            if (!String.IsNullOrEmpty(key))
            {
                try
                {
                    key = EncryptionManager.DecryptResetPassword(key);
                    string[] k = key.Split('#');
                    if (k.Length == 2)
                    {
                        if (Guid.TryParse(k[0], out guid))
                        {
                            int.TryParse(k[1], out UserID);
                        }
                    }
                }
                catch
                {
                    // do nothing
                }
            }
        }


        public static string SilverlightInitParams(string initParams, string paramName, string paramValue)
        {
            return string.Format("{0}{1} {2}={3}", initParams, initParams.Length > 0 ? "," : "", paramName, paramValue);
        }


        /// <summary>
        /// Convert 1 to Jan, 2 to Feb, 3 to March, etc
        /// format, i.e. MMM
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string IntegerToMonthName(int i, string format)
        {
            return new DateTime(2000, 12, 1).AddMonths(i).ToString(format);
        }

        /// <summary>
        /// This is User Interface replacement for SQL Error Message, in the furture may be using XML/database/etc
        /// </summary>
        /// <param name="ErrorCode"></param>
        /// <param name="ErrorMessage"></param>
        /// <returns></returns>
        public static string ErrorSQLUserInterface(int ErrorCode, string ErrorMessage)
        {
            switch (ErrorCode)
            {
                case 1230:
                    ErrorMessage = "You cannot update Respite Hours Allowance to an amount that is less that the number of respite hours that have been already used.";
                    break;
            }
            return ErrorMessage;
        }

        public static int CleaningFileNameFromCommasAndSingleQuote(string[] fs)
        {
            #region replacing commas and single quote in file name
            int x = 0;
            for (int i = 0; i < fs.Length; i++)
            {

                int t2 = fs[i].LastIndexOf("\\");
                if (t2 > -1)
                {
                    if ((fs[i].IndexOf(",", t2) > -1) || (fs[i].IndexOf("'", t2) > -1))
                    {
                        try
                        {
                            File.Move(fs[i], fs[i].Substring(0, t2) + fs[i].Substring(t2).Replace(",", " ").Replace("'", "").Trim());
                            x++;
                        }
                        catch { }
                    }
                }
            }
            return x; // total replacement
            #endregion
        }


        public static string getLogonUser()
        {
            return String.Empty;
        }

        public static string CleanHTMLTag(string html)
        {
            return Regex.Replace(html, @"<[^>]*>", String.Empty);
        }

        public static string CleanHTMLRN(string p)
        {
            return p.Replace("\r", "").Replace("\n", "");
        }

        public static int MinutesAgo(DateTime oldDate)
        {
            TimeSpan ts = DateTime.Now - oldDate;
            int minutes = ts.Minutes;
            minutes += ts.Hours * 60;
            if (ts.Days > 0)
            {
                minutes = 0;
            }
            return minutes;
        }

        public static int IntToOppositeColor(int i)
        {
            if (i == 1)
            {
                return 2;
            }
            else if (i == 2)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }



        public static void ProvSpliter(string html, out int Rating, out int Prov)
        {
            html = html.Replace('(', ' ').Replace(')', ' ');
            string SMarker15 = "P";
            Rating = 0;
            Prov = 0;
            int t1 = html.IndexOf(SMarker15);
            if (t1 > -1)
            {
                int.TryParse(html.Substring(0, t1), out Rating);
                int.TryParse(html.Substring(t1 + 1), out Prov);
                if (Prov == 0)
                {
                    Prov = 1; // unknown P number
                }
            }
            else
            {
                int.TryParse(html, out Rating);
            }
        }


        public static DateTime? NullIfMinValue(DateTime? d)
        {
            if (d == DateTime.MinValue)
            {
                return null;
            }
            else
            {
                return d;
            }
        }


        /// <summary>
        /// Split one line of string from TR to /TR, and get TD and /TD
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string[] TDColumnSpliter(string html)
        {
            string TMarker9 = "<TD>";
            string TMarker10 = "</TD>";
            List<string> cols = new List<string>();
            html = CleanHTMLRN(html);
            while (html.Length > 0)
            {
                int t1 = html.IndexOf(TMarker9);
                int t2 = html.IndexOf(TMarker10, t1 + 1);
                if ((t1 > -1) && (t2 > -1) && (t2 > t1))
                {
                    cols.Add(html.Substring(t1 + TMarker9.Length, t2 - t1 - TMarker9.Length).Trim());
                    html = html.Substring(t2 + TMarker10.Length);
                }
                else
                {
                    html = String.Empty;
                }
            }
            return cols.ToArray();
        }

        public static void SplitFirstLastName(string FullName, out string FirstName, out string LastName)
        {
            string[] suffix = { "JR", "SR", "I", "II", "III", "IV" };

            FirstName = "";
            LastName = "";
            FullName = FullName.Trim().Trim('.').ToUpper();
            int t1 = FullName.LastIndexOf(" ");
            if (t1 > -1)
            {
                LastName = FullName.Substring(t1).Trim();
                FullName = FullName.Substring(0, t1).Trim();
                if (Array.IndexOf(suffix, LastName) > -1)
                {
                    int t2 = FullName.LastIndexOf(" ");
                    if (t2 > -1)
                    {
                        LastName = FullName.Substring(t2).Trim();
                        FullName = FullName.Substring(0, t2).Trim();
                    }
                }
            }
            FirstName = FullName;
        }


        public static string AddToCalendarHTML(string eventName, string location, string link, DateTime startDate, DateTime endDate)
        {
            string html = "";

            html += "<a href=\"" + link + "\" title=\"Add to Calendar\" class=\"addthisevent\">";
            html += "Add to Calendar";
            html += String.Format("<span class=\"_start\">{0}</span>", startDate.ToShortDateString());
            html += String.Format("<span class=\"_end\">{0}</span>", endDate.ToShortDateString());
            html += "<span class=\"_zonecode\">15</span>";
            html += "<span class=\"_summary\">" + eventName + "</span>";
            html += "<span class=\"_description\">" + eventName + "</span>";
            html += "<span class=\"_location\">" + location + "</span>";
            html += "<span class=\"_all_day_event\">true</span>";
            html += "<span class=\"_date_format\">MM/DD/YYYY</span>";
            html += "</a>";

            return html;
        }

        public static string SupNumbers(string text)
        {
            return text.Replace("1st", "1<sup>st</sup>").Replace("2nd", "2<sup>nd</sup>").Replace("3rd", "3<sup>rd</sup>").Replace("4th", "4<sup>th</sup>").Replace("5th", "5<sup>th</sup>").Replace("6th", "6<sup>th</sup>").Replace("7th", "7<sup>th</sup>").Replace("8th", "8<sup>th</sup>").Replace("9th", "9<sup>th</sup>").Replace("10th", "10<sup>th</sup>");
        }



        public static bool IsRoleIDsHasAdmin(string RoleIDs)
        {
            RoleIDs = RoleIDs.Trim() + ",";
            return RoleIDs.IndexOf(((int)roleIDs.Administrator).ToString() + ",") > -1;
        }

        public static string getFileNameOnlyFromPath(string PathAndFile)
        {
            string fn = "";
            int t1 = PathAndFile.LastIndexOfAny(new char[] { '/', '\\' });
            if (t1 > -1)
            {
                fn = PathAndFile.Substring(t1 + 1);
            }
            else
            {
                fn = PathAndFile;
            }
            return fn;
        }
    }

}
