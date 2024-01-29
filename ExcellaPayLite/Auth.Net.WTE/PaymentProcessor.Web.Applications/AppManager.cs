namespace PaymentProcessor.Web.Applications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Timers;
    using System.Xml;
    using System.Net;
    using System.IO;
    using System.Collections;
    using System.Web;
    using System.Data.SqlTypes;


    public static class AppManager
    {
        private static Dictionary<string, object> _cache = new Dictionary<string, object>();
        private static Dictionary<string, FileSystemWatcher> _pgnwatcher = new Dictionary<string, FileSystemWatcher>();
        private static Dictionary<string, DateTime> _pgnwatcherTimeStamp = new Dictionary<string, DateTime>();
        private static Dictionary<string, string> _urlRewriter = new Dictionary<string, string>();
        private static Dictionary<string, string> _ip2State = new Dictionary<string, string>();

        private static bool _isupgrading = false;
        private static int _upgradingbyuserid = 0;
        private static string _upgradingbyuserfullname = string.Empty;
        private static bool _isstarted = false;
        private static bool _isjuststarted = false;

        private static int _timerCounter = 0;
        private static Timer _timer = new Timer();
        private static Timer _timerFileWatcher = null;
        private static DateTime _timerNext = DateTime.MinValue;
        private static string _timerTaskOn = String.Empty;
        private static int _timerMaxTaskOnExecute = 1;
        private static bool _isexternal = false;
        private static char[] EndURLChars = { '/', '?', ' ', '&' };

        private static void InitTimer()
        {
            bool IsTaskExecutorInAppManager = Utils.getAppSettings(SV.AppSettings.IsTaskExecutorInAppManager).ToBool();
            if (IsTaskExecutorInAppManager)
            {
                double interval = Utils.getAppSettings(SV.AppSettings.TaskTimerInterval).ToDouble();
                if (interval > 0)
                {
                    _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                    _timer.Interval = (1000) * (interval);
                    _timerNext = DateTime.Now.AddMilliseconds(_timer.Interval);
                    _timer.Enabled = true;
                    _timerCounter++;
                    _timer.Start();
                }
                else
                {
                    _timer.Enabled = false;
                }
            }
        }

        public static void setApplicationObject(string key, object obj)
        {
            _cache[key] = obj;
        }

        public static void reloadURLRewriters()
        {
            _urlRewriter.Clear();
        }

        public static void addURLRewriter(string key, string path)
        {
            if (_urlRewriter.ContainsKey(key))
            {
                _urlRewriter[key] = path;
            }
            else
            {
                _urlRewriter.Add(key, path);
            }
        }

        public static void clearURLRewriter()
        {
            _urlRewriter.Clear();
        }

        public static void clearObjectWithStartKey(string startKey)
        {
            // cleaning cache with starting key
            List<string> ks = new List<string>();
            foreach (string key in _cache.Keys)
            {
                if (key.StartsWith(startKey))
                {
                    ks.Add(key);
                }
            }
            if (ks.Count > 0)
            {
                foreach (string k in ks)
                {
                    clearApplicationObject(k);
                }
            }
        }

        public static void clearApplicationObject(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }
        }

        public static object getApplicationObject(string key)
        {
            if (_cache.ContainsKey(key))
            {
                return _cache[key];
            }
            else
            {
                return null;
            }
        }

        private static bool IsTaskWorkingOn()
        {
            return !String.IsNullOrEmpty(_timerTaskOn);
        }

        private static void OnTimedWatchEvent(object source, ElapsedEventArgs e)
        {
            
        }

        private static void SetWatchTimer()
        {
            if (_timerFileWatcher == null)
            {
                int tt = 5000;
                _timerFileWatcher = new Timer();
                _timerFileWatcher.Elapsed += new ElapsedEventHandler(OnTimedWatchEvent);
                _timerFileWatcher.Interval = tt;
                _timerFileWatcher.Enabled = true;
                _timerFileWatcher.Start();
            }
        }


        public static string TasksExecution()
        {
            string strHTML = String.Empty;
            if (!IsTaskWorkingOn())
            {
                DRspAppTask_Get task = SQLData.spAppTask_Get();
                if ((task != null) && (task.Count > 0))
                {
                    for (int i = 0; i < task.Count; i++)
                    {
                        int TaskID = task.AppTaskID(i);
                        _timerTaskOn = task.URL(i);
                        int t1 = _timerTaskOn.LastIndexOf("/");
                        if (t1 > -1)
                        {
                            _timerTaskOn = _timerTaskOn.Substring(t1 + 1);
                        }

                        string URL = task.URL(i);
                        // only Tasks that has URL that will be executed here
                        // Tasks that has Stored Procedure will be executed inside SQL Server Tasks
                        if (!String.IsNullOrEmpty(URL))
                        {
                            try
                            {
                                if (!URL.StartsWith(SV.Common.http))
                                {
                                    URL = Utils.getAppSettings(SV.AppSettings.EnvironmentURL).ToString() + (URL[0] == '/' ? "" : "/") + URL;
                                }
                                HttpWebRequest taskRequest = (HttpWebRequest)WebRequest.Create(URL);
                                taskRequest.Method = SV.Common.POSTMethod;
                                taskRequest.Timeout = 1000 * Utils.getAppSettings(SV.AppSettings.TaskExecutor_Timeout).ToInt();
                                string username = Utils.getAppSettings(SV.AppSettings.TaskExecutor_Username).ToString();
                                if (!String.IsNullOrEmpty(username))
                                {
                                    taskRequest.Credentials = new NetworkCredential(
                                        Utils.getAppSettings(SV.AppSettings.TaskExecutor_Username).ToString(),
                                        Utils.getAppSettings(SV.AppSettings.TaskExecutor_Password).ToString(),
                                        Utils.getAppSettings(SV.AppSettings.TaskExecutor_Domain).ToString());
                                }
                                taskRequest.ContentLength = 0;
                                HttpWebResponse taskResponse = (HttpWebResponse)taskRequest.GetResponse();

                                StreamReader sr = new StreamReader(taskResponse.GetResponseStream());
                                String result = sr.ReadToEnd();
                                strHTML += result;
                                sr.Close();
                                taskResponse.Close();
                                if (i >= _timerMaxTaskOnExecute - 1)
                                {
                                    i = task.Count;
                                }
                            }
                            catch (Exception ex)
                            {
                                ErrorManager.logError(String.Format(SV.ErrorMessages.WebRequestURL, URL), ex);
                            }

                            DRspAppTask_UpdateLastExecuted update = SQLData.spAppTask_UpdateLastExecuted(TaskID);

                        }
                        _timerTaskOn = String.Empty;
                    }
                }
            }
            return strHTML;
        }

        #region TaskTimer_Properties
        public static double TaskTimerInterval
        {
            get
            {
                return _timer.Interval;
            }
        }

        public static bool TaskTimerEnable
        {
            get
            {
                return _timer.Enabled;
            }
        }

        public static DateTime TaskTimerNextExecute
        {
            get
            {
                return _timerNext;
            }
        }

        public static int TaskTimerCounter
        {
            get
            {
                return _timerCounter;
            }
        }
        public static string TimerTaskOn
        {
            get
            {
                return _timerTaskOn;
            }
        }
        #endregion

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _timerNext = DateTime.Now.AddMilliseconds(_timer.Interval);
            _timerCounter++;
            TasksExecution();
        }



        public static Dictionary<string, object> Cache
        {
            get
            {
                return _cache;
            }
        }

        public static void Init()
        {
            if (!_isstarted)
            {
                AppSettings.setWebServerEnvironment();
                setApplicationSettings();
                _isjuststarted = true;
                _isstarted = true;
                string exmode = Utils.getAppSettings(SV.AppSettings.ExternalMode).ToString().ToUpper();

                if (exmode == SV.Common.ExternalModeExternal)
                {
                    _isexternal = true;
                }
                else if (exmode == SV.Common.ExternalModeAuto)
                {
                    // detect windows login
                    if (Utils.getLogonUser().Length == 0)
                    {
                        _isexternal = true;
                    }
                }

                InitTimer();
                DRspWebsites_GetByID web = SQLData.spWebsites_GetByID(websiteIDs.PaymentProcessor);
                if (!web.isError)
                {
                    _isupgrading = web.IsUpgrading;
                    _upgradingbyuserid = web.UpgradingByUserID;
                    _upgradingbyuserfullname = web.UserFullName;
                    DRspWebsitesRestart_Insert webStart = SQLData.spWebsitesRestart_Insert(websiteIDs.PaymentProcessor);
                }

                AppManager.reloadURLRewriters();


            }
            else
            {
                _isjuststarted = false;
            }
        }

        private static void setApplicationSettings()
        {
            AppSettings.clearAllData();
            ProcessAppSettingsDirectory(Utils.getMapPath(SV.ApplicationPath.AppSettings));
            ProcessOverwrittingAppSettingsFromDatabase();
            ProcessOverwrittingDefaultUserSettingsFromDatabase();
        }

        /// <summary>
        /// This process is reading all app settings from XML and then check all that has flag CanBeEdited
        /// All Application Settings that can be edited will be put in SQL table (Application Settings)
        /// </summary>
        private static void ProcessOverwrittingAppSettingsFromDatabase()
        {
            DRspAppSettings_GetByKeywords dbassets = SQLData.spAppSettings_GetByKeywords(string.Empty); // get all
            Hashtable assets = AppSettings.data;
            List<string> keys = new List<string>();
            foreach (string key in assets.Keys)
            {
                keys.Add(key);
            }
            for (int j = 0; j < keys.Count; j++)
            {
                string dt = AppSettings.getDataType(keys[j]);
                if ((dt == SV.appSettingData.Application) || (dt == SV.appSettingData.ConnectionString) || (dt == SV.appSettingData.Links))
                {
                    if (AppSettings.isDataCanBeEdited(keys[j]))
                    {
                        bool isOnDB = false;
                        string DBValue = string.Empty;
                        for (int i = 0; i < dbassets.Count; i++)
                        {
                            if (dbassets.Name(i).ToLower() == keys[j].ToLower())
                            {
                                isOnDB = true;
                                DBValue = dbassets.Value(i);
                                break;
                            }
                        }
                        if (isOnDB)
                        {
                            AppSettings.overwriteData(keys[j], DBValue);
                        }
                        else
                        {
                            DRspAppSettings_Insert setasset = SQLData.spAppSettings_Insert(keys[j], assets[keys[j]].ToString());
                        }
                    }
                }
            }
        }

        private static void ProcessOverwrittingDefaultUserSettingsFromDatabase()
        {
            DRspUserSettings_GetAllDefaults dbassets = SQLData.spUserSettings_GetAllDefaults();
            Hashtable assets = AppSettings.data;
            List<string> keys = new List<string>();
            foreach (string key in assets.Keys)
            {
                keys.Add(key);
            }
            for (int j = 0; j < keys.Count; j++)
            {
                string dt = AppSettings.getDataType(keys[j]);
                if (dt == SV.appSettingData.Users)
                {
                    bool isOnDB = false;
                    string DBValue = string.Empty;
                    for (int i = 0; i < dbassets.Count; i++)
                    {
                        if (dbassets.Name(i).ToLower() == keys[j].ToLower())
                        {
                            isOnDB = true;
                            DBValue = dbassets.Value(i);
                            break;
                        }
                    }
                    if (isOnDB)
                    {
                        AppSettings.overwriteData(keys[j], DBValue);
                    }
                    else
                    {
                        // default value of UserID = 0
                        DRspUserSettings_Insert setasset = SQLData.spUserSettings_Insert(0, keys[j], assets[keys[j]].ToString());
                    }
                }
            }
        }

        private static void ProcessAppSettingsDirectory(string sourcePath)
        {
            // Process the list of files found in the directory. 
            getAppSettingsInPath(sourcePath); // process one directory

            // Recurse into subdirectories of this directory.
            string[] subdirEntries = Directory.GetDirectories(sourcePath);
            foreach (string subdir in subdirEntries)
            {
                // Do not iterate through reparse points
                if ((File.GetAttributes(subdir) &
                        FileAttributes.ReparsePoint) !=
                            FileAttributes.ReparsePoint)

                    ProcessAppSettingsDirectory(subdir);
            }
        }

        private static void getAppSettingsInPath(string sourcePath)
        {
            string[] filePaths = Directory.GetFiles(sourcePath, SV.Common.xmls);
            for (int x = 0; x < filePaths.Length; x++)
            {
                processAppSettingsOneFile(filePaths[x], String.Empty, false);
            }
        }

        private static void processAppSettingsOneFile(string fileName, string selectedVarName, bool isReload)
        {
            // if selectedVarName is null or empty string, load the file and reload the value
            bool isXMLLoaded = true;
            XmlDocument xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.Load(fileName);
            }
            catch (Exception e)
            {
                isXMLLoaded = false;
                string otherMessages = string.Empty;
                otherMessages = String.Format(SV.ErrorMessages.LoadingAppSettingXML, fileName);
                ErrorManager.logError(otherMessages, e);
            }

            if (isXMLLoaded)
            {
                XmlNodeList xlist = null;
                if (selectedVarName.Length > 0)
                {
                    // select only one node
                    xlist = xmlDoc.SelectNodes(String.Format(SV.Common.AppSetting.XPathWithName, selectedVarName));
                }
                else
                {
                    // select all data in app settings
                    xlist = xmlDoc.SelectNodes(SV.Common.AppSetting.XPath);
                }

                if (xlist.Count > 0)
                {

                    for (int i = 0; i < xlist.Count; i++)
                    {
                        appSettingData dataType = appSettingData.Application;
                        string dataName = string.Empty;
                        string dataValue = string.Empty;
                        bool canBeInJavascript = false;
                        bool canBeEdited = false;
                        bool isXMLData = false;
                        bool isEncrypted = false;
                        processAppSettingsOneNode(xlist[i], out dataType, out dataName, out dataValue, out canBeInJavascript, out isXMLData, out canBeEdited, out isEncrypted);
                        if (isEncrypted)
                        {
                            dataValue = EncryptionManager.DecryptString(dataValue);
                        }
                        //bool isNewLoad = selectedVarName
                        if (dataName.Length > 0)
                        {
                            setSingleAppSetting(dataType, dataName, dataValue, canBeInJavascript, isXMLData, fileName, isReload, canBeEdited);
                        }
                    }
                }
            }
        }


        private static void setSingleAppSetting(appSettingData dataType, string dataName, string dataValue, bool canBeInJavascript, bool isXMLData, string filePath, bool isForce, bool canBeEdited)
        {
            // filepath is for realoading purpose
            // for connection strings
            switch (dataType)
            {
                case appSettingData.Application:
                    AppSettings.setData(dataType, dataName, dataValue, isForce);
                    AppSettings.setDataJS(dataName, canBeInJavascript);
                    AppSettings.setDataCanBeEdited(dataName, canBeEdited);
                    AppSettings.setDataIsXML(dataName, isXMLData);
                    AppSettings.setFilePath(dataName, filePath);
                    break;

                case appSettingData.ConnectionString:
                    if (dataName.Length > 0)
                    {
                        databaseServer db = databaseServer.na;
                        try
                        {
                            db = (databaseServer)Enum.Parse(typeof(databaseServer), dataName);
                        }
                        catch
                        {
                            db = databaseServer.na;
                        }
                        AppSettings.setConnectionString(db, dataValue);
                        // we still put here to show that the system have read it
                        AppSettings.setData(dataType, dataName, SV.Common.ConnectionStringNoShow, isForce);
                        AppSettings.setDataJS(dataName, false); // overwrite to false
                        AppSettings.setDataCanBeEdited(dataName, false); // overwrite to false
                        AppSettings.setDataIsXML(dataName, false); // overwrite to false
                        AppSettings.setFilePath(dataName, filePath);
                    }
                    break;

                case appSettingData.Links:
                    // to do: Link Manager
                    break;
                case appSettingData.Users:
                    AppSettings.setData(dataType, dataName, dataValue, isForce);
                    AppSettings.setDataJS(dataName, false);             // overwrite to false
                    AppSettings.setDataCanBeEdited(dataName, false);    // overwrite to false
                    AppSettings.setDataIsXML(dataName, false);          // overwrite to false
                    AppSettings.setFilePath(dataName, filePath);
                    break;
            }


        }


        private static void processAppSettingsOneNode(XmlNode xlist,
            out appSettingData dataType,
            out string dataName,
            out string dataValue,
            out bool canBeInJavascript,
            out bool isXMLData,
            out bool canBeEdited,
            out bool isEncrypted
        )
        {
            dataType = appSettingData.Application;
            dataName = string.Empty;
            dataValue = string.Empty;
            canBeInJavascript = false;
            isXMLData = false;
            canBeEdited = false;
            isEncrypted = false;


            int intCurrentWebserver = (int)AppSettings.EnvironmentCode;
            string strCurrentWebserver = intCurrentWebserver.ToString();
            if (xlist.HasChildNodes)
            {
                XmlNodeList xdata = xlist.ChildNodes;
                int priorityValue = -1;



                for (int j = 0; j < xdata.Count; j++)
                {
                    string nName = xdata[j].Name;
                    switch (nName)
                    {
                        // picking correct value based on WebServer
                        // 0 or none: is default value (lowest priority)
                        // 1,2,3,4: localhost, dev, staging, production (medium priority)
                        // machine name: highest priority
                        case SV.Common.AppSetting.Value:
                            bool isGet = false;
                            string webServer = Utils.getAttribute(xdata[j], SV.Common.AppSetting.WebServer);
                            if (webServer == AppSettings.MachineName)
                            {
                                if (priorityValue < int.MaxValue)
                                {
                                    isGet = true; // get it whatever it is
                                    priorityValue = int.MaxValue; // highest priority
                                }
                            }
                            else if ((webServer == string.Empty) || (webServer == "0"))
                            {
                                if (priorityValue < 0)
                                {
                                    isGet = true; // get it
                                    priorityValue = 0; // default priority
                                }
                            }
                            else if (webServer == strCurrentWebserver)
                            {
                                if (priorityValue < intCurrentWebserver)
                                {
                                    // 1,2,3,4 is not a priority. it is just between 0 - highest priority
                                    isGet = true; // get it
                                    priorityValue = intCurrentWebserver;
                                }
                            }

                            if (isGet)
                            {
                                dataValue = Utils.getAttribute(xdata[j], SV.Common.AppSetting.Value);
                                canBeInJavascript = Utils.getAttributeBool(xdata[j], SV.Common.AppSetting.CanBeInJavascript);
                                canBeEdited = Utils.getAttributeBool(xdata[j], SV.Common.AppSetting.CanBeEdited);
                                isXMLData = Utils.getAttributeBool(xdata[j], SV.Common.AppSetting.IsXMLData);
                                isEncrypted = Utils.getAttributeBool(xdata[j], SV.Common.AppSetting.IsEncrypted);
                                // if it is file, replace value with content of the file
                                if (Utils.getAttributeBool(xdata[j], SV.Common.AppSetting.IsFile))
                                {
                                    dataValue = Utils.getValueFromFile(Utils.getMapPath(SV.ApplicationPath.AppFiles) + dataValue);
                                }
                            }
                            break;

                        case SV.Common.AppSetting.Name:
                            dataName = xdata[j].InnerText.Trim();
                            break;

                        case SV.Common.AppSetting.Notes:
                            // do nothing
                            break;

                        case SV.Common.AppSetting.Type:
                            string dtype = xdata[j].InnerText.Trim();
                            dataType = appSettingData.Application;
                            try
                            {
                                dataType = (appSettingData)Enum.Parse(typeof(appSettingData), dtype);
                            }
                            catch
                            {
                                dataType = appSettingData.Application;
                            }
                            break;
                    }
                }

            }
        }

        public static void ApplicationError()
        {
            // Code that runs when an unhandled error occurs
            if (HttpContext.Current.Server.GetLastError() != null)
            {
                Exception ex = HttpContext.Current.Server.GetLastError().GetBaseException();
                ErrorManager.logError(SV.ErrorMessages.Application_Error, ex);
            }
            else
            {
                ErrorManager.logError(SV.ErrorMessages.Application_Error, null);
            }
            HttpContext.Current.Server.ClearError();
        }

        public static RequestManager requestManager
        {
            get
            {
                RequestManager request = null;
                try
                {
                    request = (RequestManager)Utils.getPageItem(SV.PageItems.RequestManager);
                }
                catch
                {
                    ErrorManager.logMessage(String.Format(SV.ErrorMessages.NullRequestManager, "NULL"));
                    request = null;
                }
                return request;
            }
        }

        public static UserManager userManager
        {
            get
            {
                UserManager user = null;
                try
                {
                    user = (UserManager)Utils.getPageItem(SV.PageItems.UserManager);
                }
                catch (Exception e)
                {
                    user = null;
                    ErrorManager.logError(String.Format(SV.ErrorMessages.CastUserManager, Utils.getSessionID()), e);
                }
                // during request it refers to Page Item, not the one in session
                if (user != null)
                {
                    return user;
                }
                else
                {
                    //ErrorManager.logMessage(String.Format(SV.ErrorMessages.NullUserManager, "NULL Session:" + Utils.getSessionID()));
                    return null;
                }
            }
        }

        public static void RequestStart()
        {
            AppManager.timeIn();
            AppManager.Init();
            AppManager.loadRequestManager();

            bool isRewrite = false;
            if (!isRewrite)
            {
                isRewrite = AppManager.InvoicePDFDisable();
            }
        }

        private static bool InvoicePDFDisable()
        {
            bool rw = false;
            if (HttpContext.Current != null)
            {
                string path = Utils.getPath().ToLower();
                if (path.StartsWith(SV.ApplicationPath.AppInvoiceFiles.Substring(1), StringComparison.OrdinalIgnoreCase))
                {
                    rw = true;
                    ErrorManager.logMessage(String.Format(SV.ErrorMessages.PDFNotDirectAccess, path));
                    Utils.rewritePath("/Invoices/DownloadInvoicePDF.ashx");
                }
            }
            return rw;
        }

        public static void RequestEnd()
        {
            TimeSpan ts = AppManager.timeOut();
            SQLData.spRequestLogs_Insert(ts, Utils.getClientIPAddress(), Utils.getScriptName());
        }

        public static void PageStart(System.Web.UI.Page thePage)
        {
            AppManager.loadUserManager();
        }

        public static void PageEnd(System.Web.UI.Control theControl)
        {
            AppManager.saveUserManager();
        }

        public static void PageEnd(System.Web.UI.Page thePage)
        {
            AppManager.saveUserManager();
        }


        public static void PagePreInit(out UserManager user, out RequestManager request)
        {
            user = null;
            request = null;
            if (AppManager.IsJustStarted)
            {
                // check requirements
            }
            // there are 2 attempts for page start. On base page and base control
            AppManager.PageStart(null);
            user = AppManager.userManager;
            request = AppManager.requestManager;
            user.AuthorizeUser();
            if (!user.isLogin)
            {
                // do not redirect to a page that has BasePage, otherwise it will do loopback
            }
            else
            {
                if (AppManager.IsUpgrading)
                {
                    // preventing from loopback
                    string currentPage = Utils.getScriptName().ToLower();
                    string redirectPage = Utils.getAppSettings(SV.AppSettings.UpgradingURL).ToString().ToLower();

                    if (user.userID == AppManager.UpgradingByUserID)
                    {
                        if (currentPage.EndsWith(redirectPage))
                        {
                            // do nothing ... admin need to be able to finish upgrading
                        }
                        else
                        {
                            Utils.responseRedirect(Utils.getAppSettings(SV.AppSettings.UpgradingURL).ToString());
                        }
                    }
                    else
                    {
                        Utils.responseRedirect(Utils.getAppSettings(SV.AppSettings.UnderConstructionPageURL).ToString());
                    }
                }
            }
        }

        public static bool ValidateAuthKey(string authKey)
        {
            bool r = false;
            if (!String.IsNullOrEmpty(authKey))
            {
                // todo: put logic of auth key
                r = true;
            }
            return r;
        }

        /// <summary>
        /// Make it private, for only AppManager
        /// </summary>
        private static void loadRequestManager()
        {
            RequestManager request = new RequestManager();
            Utils.setPageItem(SV.PageItems.RequestManager, request);
        }


        /// <summary>
        /// Make it private, for only AppManager
        /// Modified: now public for loading when user login in external application
        /// </summary>
        public static void loadUserManager()
        {
            UserManager user = null;
            object um = Utils.getSessionObject(SV.Sessions.UserManager);
            if (um != null)
            {
                try
                {
                    // convert object taken from session to UserManager
                    user = (UserManager)um;
                }
                catch
                {
                    um = null;
                }

            }

            if (um == null)
            {
                // UserManager not found inside session, create a new one
                user = new UserManager(); // please ONLY create user manager here
                user.IsUsingTouchScreen = Utils.IsBrowserTouchScreen();
                user.VisitorState = GetCurrentStateLocation(Utils.getClientIPAddress());
            }

            Utils.setPageItem(SV.PageItems.UserManager, user);
        }

        private static string GetCurrentStateLocation(string IPAddress)
        {
            return String.Empty;
        }

        /// <summary>
        /// Make it private, for only AppManager
        /// </summary>
        private static void saveUserManager()
        {
            Utils.setSessionObject(SV.Sessions.UserManager, AppManager.userManager);
        }

        /// <summary>
        /// Make it private, for only AppManager
        /// </summary>
        private static void timeIn()
        {
            DateTime t = DateTime.Now;
            Utils.setPageItem(SV.PageItems.ExecutionTime, t);
        }

        public static bool IsJustStarted
        {
            get
            {
                return _isjuststarted;
            }
        }

        public static bool IsUpgrading
        {
            get
            {
                return _isupgrading;
            }
        }

        public static int UpgradingByUserID
        {
            get
            {
                return _upgradingbyuserid;
            }
        }

        public static string UpgradingByUserFullName
        {
            get
            {
                return _upgradingbyuserfullname;
            }
        }

        public static bool IsStarted
        {
            get
            {
                return _isstarted;
            }
        }

        public static bool IsExternal
        {
            get
            {
                return _isexternal;
            }
        }

        public static Dictionary<string, string> IP2State
        {
            get
            {
                return _ip2State;
            }

        }

        public static void ApplicationLock(int UserID, string FullName)
        {
            _isupgrading = true;
            _upgradingbyuserid = UserID;
            _upgradingbyuserfullname = FullName;
        }

        public static void ApplicationUnlock(int UserID, string FullName)
        {
            _isupgrading = false;
            _upgradingbyuserid = UserID;
            _upgradingbyuserfullname = FullName;
        }

        /// <summary>
        /// Make it private, for only AppManager
        /// </summary>
        private static TimeSpan timeOut()
        {
            object t = Utils.getPageItem(SV.PageItems.ExecutionTime);
            if (t != null)
            {
                DateTime dt = (DateTime)t;
                TimeSpan ts = DateTime.Now.Subtract(dt);
                return ts;
            }
            else
            {
                return new TimeSpan(0, 0, 0, 0, 0);
            }
        }


        public static ElapsedEventHandler OnTimedFileWatcherEvent { get; set; }


    }
}
