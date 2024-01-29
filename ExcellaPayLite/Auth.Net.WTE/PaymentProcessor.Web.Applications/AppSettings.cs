namespace PaymentProcessor.Web.Applications
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Collections;
    using System.Xml;
    using System.Configuration;

    public static class AppSettings
    {
        private static environmentCodes _webserver = environmentCodes.NA;
        private static string _machinename = string.Empty;
        private static string[] _connstring = new string[Enum.GetValues(typeof(databaseServer)).Length - 1]; // -1 for na length
        private static List<string> _filePath = new List<string>();
        private static List<DateTime> _fileDate = new List<DateTime>();
        private static Hashtable _dataType = new Hashtable();
        // to do these following data may need to ba in a class
        private static Hashtable _data = new Hashtable();
        private static Hashtable _isdatajs = new Hashtable();
        private static Hashtable _canbeedited = new Hashtable();
        private static Hashtable _isxmldata = new Hashtable();
        private static Hashtable _xmldata = new Hashtable();
        private static Hashtable _filepathindex = new Hashtable();


        public static void setWebServerEnvironment()
        {
            char[] delimiterChars = { ',' }; // delimiter of spliter
            char[] secondDelimiterChars = { '|' }; // delimiter of spliter in application path
            _machinename = System.Environment.MachineName;
            string webserverNames = string.Empty;
            string[] web = null;
            string[] webSNames = null;

            // prepare for looping from production to localhost
            int count = Enum.GetValues(typeof(environmentCodes)).Length - 1;
            while (count > 0) // looping until found or until 0
            {
                environmentCodes wb = (environmentCodes)count; // will be Production, Staging, Development, Localhost
                // the the machine names set on web config
                webserverNames = ConfigurationManager.AppSettings[wb.ToString()];
                if (!String.IsNullOrEmpty(webserverNames)) // if it is not null or empty
                {
                    bool isFound = false;
                    web = webserverNames.Split(delimiterChars); // splited by commas
                    // if there is no second identification
                    if (webserverNames.IndexOf(secondDelimiterChars[0].ToString()) == -1)
                    {
                        if (Array.IndexOf(web, _machinename) > -1) // check if the machine name in the array
                        {
                            isFound = true;

                        }
                    }
                    else // if there is second identification in host header (server_name)
                    {
                        string requestAppPath = Utils.getApplicationPath().ToLower();
                        if (requestAppPath.StartsWith("/"))
                        {
                            requestAppPath = requestAppPath.Substring(1);
                        }
                        for (int i = 0; i < web.Length; i++)
                        {
                            string idString = string.Empty;
                            webSNames = web[i].Split(secondDelimiterChars);
                            if (webSNames.Length > 1)
                            {
                                idString = webSNames[1].ToLower(); // get the identification
                            }
                            if ((_machinename == webSNames[0]) && ((idString == string.Empty) || (requestAppPath == idString)))
                            {
                                isFound = true;
                            }
                        }
                    }

                    if (isFound)
                    {
                        setWebServer(wb);
                        count = 0;
                        break;
                    }

                }
                count--; // looping til the end of found
            }
        }

        public static void getAllXMLs()
        {
            foreach (string key in _data.Keys)
            {
                if ((bool)_isxmldata[key])
                {
                    string strURL = (string)_data[key];
                    if (strURL.Length > 0)
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        try
                        {
                            xmlDoc.Load(strURL);
                            // lesson: we used to save XML in the hashtable
                            // the problem was when we modify the data, the library will be also modified
                            // so, we save it now, in string
                            // when we send it out, we convert it to New XmlDocument
                            _xmldata[key] = xmlDoc.InnerXml.ToString();
                        }
                        catch (Exception e)
                        {
                            _isxmldata[key] = false;
                            ErrorManager.logError(String.Format(SV.ErrorMessages.LoadingXMLFromLib, key), e);
                        }
                    }
                }
            }
        }

        private static string ReplaceEncrypted(string cs)
        {
            string p1 = SV.Common.PasswordOnConString;
            int t1 = cs.LastIndexOf(p1, StringComparison.OrdinalIgnoreCase);
            if (t1 > -1)
            {
                int t2 = cs.IndexOf(";", t1 + 1);
                if (t2 > -1)
                {
                    string pwd = cs.Substring(t1 + p1.Length, t2 - t1 - p1.Length);
                    pwd = EncryptionManager.DecryptConnectionString(pwd);
                    cs = cs.Substring(0, t1 + p1.Length) + pwd + cs.Substring(t2);
                }
            }
            return cs;
        }

        public static void clearAllData()
        {
            _dataType.Clear();
            _filePath.Clear();
            _fileDate.Clear();
            _data.Clear();
            _isdatajs.Clear();
            _canbeedited.Clear();
            _isxmldata.Clear();
            _xmldata.Clear();

        }

        public static void setDataDB(string key, string value)
        {
            if (!_data.ContainsKey(key))
            {
                _data[key] = value;
            }
            else
            {
                ErrorManager.logMessage(String.Format(SV.ErrorMessages.DuplicateAppSetting, key));
            }
        }

        public static void overwriteData(string key, string value)
        {
            if (_data.ContainsKey(key) && _canbeedited.ContainsKey(key) && ((bool)_canbeedited[key]))
            {
                _data[key] = value;
            }
        }

        public static void setData(appSettingData dataType, string key, string value, bool isForceSet)
        {
            if ((!_data.ContainsKey(key)) || isForceSet)
            {
                _dataType[key] = dataType;
                _data[key] = value;
            }
            else
            {
                ErrorManager.logMessage(String.Format(SV.ErrorMessages.DuplicateAppSetting, key));
            }
        }

        public static string getDataType(string key)
        {
            if (_dataType.ContainsKey(key))
            {
                return _dataType[key].ToString(); ;
            }
            else
            {
                return "";
            }
        }

        public static void setDataJS(string key, bool value)
        {
            _isdatajs[key] = value;
        }

        public static void setDataCanBeEdited(string key, bool value)
        {
            _canbeedited[key] = value;
        }

        public static void setDataIsXML(string key, bool value)
        {
            _isxmldata[key] = value;
        }

        public static object Data(string key)
        {
            if (_data.ContainsKey(key))
            {
                return _data[key];
            }
            else
            {
                return String.Empty;
            }
        }

        public static XmlDocument XMLData(string key)
        {
            if ((bool)_isxmldata[key])
            {
                try
                {
                    string origDocString = (string)_xmldata[key];
                    XmlDocument copyDoc = new XmlDocument();
                    copyDoc.LoadXml(origDocString);
                    return copyDoc;
                }
                catch
                {
                    return new XmlDocument(); // return empty one
                }

            }
            else
            {
                return new XmlDocument(); // return empty one
            }
        }

        public static bool isDataJS(string key)
        {
            return (bool)_isdatajs[key];
        }

        public static bool isDataCanBeEdited(string key)
        {
            return (bool)_canbeedited[key];
        }

        public static string connString()
        {
            return connString(databaseServer.DefaultDB);
        }

        public static string connString(databaseServer db)
        {
            return _connstring[(int)db];
        }

        public static void setConnectionString(databaseServer db, string connstring)
        {
            int dbIndex = (int)db;
            if (dbIndex >= 0) // now including the default one
            {
                // do encryption if needed here
                switch (AppSettings.EnvironmentCode)
                {
                    case environmentCodes.Localhost:
                        break;
                    case environmentCodes.Development:
                        break;
                    case environmentCodes.Staging:
                        //connstring = ReplaceEncrypted(connstring);
                        break;
                    case environmentCodes.Production:
                        connstring = ReplaceEncrypted(connstring);
                        break;
                }
                _connstring[dbIndex] = connstring;
            }
        }

        public static bool isDevelopment()
        {
            return (_webserver == environmentCodes.Development);
        }

        public static bool isProduction()
        {
            return (_webserver == environmentCodes.Production);
        }

        public static bool isStaging()
        {
            return (_webserver == environmentCodes.Staging);
        }

        public static bool isLocalhost()
        {
            return (_webserver == environmentCodes.Localhost);
        }

        public static environmentCodes EnvironmentCode
        {
            get
            {
                return _webserver;
            }
        }

        public static string MachineName
        {
            get
            {
                return _machinename;
            }
        }

        public static void setWebServer(environmentCodes webserver)
        {
            _webserver = webserver;
        }


        /// <summary>
        /// Property of App Setting. Used outside to loop all keys
        /// </summary>
        public static Hashtable data
        {
            get
            {
                return _data;
            }
        }

        private static int setFilePathIndex(string filePath)
        {
            int i = 0;
            i = _filePath.IndexOf(filePath);
            if (i == -1)
            {
                _filePath.Add(filePath);
                _fileDate.Add(File.GetLastWriteTime(filePath));
                i = _filePath.Count - 1; // last index

            }
            return i;
        }

        public static void setFilePath(string dataName, string filePath)
        {
            int i = setFilePathIndex(filePath);
            if (i > -1)
            {
                _filepathindex[dataName] = i;
            }
        }

        public static List<string> expiredXMLFiles()
        {
            List<string> ef = new List<string>();
            for (int i = 0; i < _filePath.Count; i++)
            {
                DateTime nf = File.GetLastWriteTime(_filePath[i]);
                if (nf.CompareTo(_fileDate[i]) == 1)
                {
                    ef.Add(_filePath[i]);
                }
            }
            return ef;
        }

        public static string getFilePath(string dataName)
        {
            if (_filepathindex.Contains(dataName))
            {
                return _filePath[(int)_filepathindex[dataName]];
            }
            else
            {
                return "";
            }
        }

        public static void updateFileDate(string filePath)
        {
            int i = _filePath.IndexOf(filePath);
            if (i > -1)
            {
                _fileDate[i] = File.GetLastWriteTime(_filePath[i]);
            }
        }

    }

}
