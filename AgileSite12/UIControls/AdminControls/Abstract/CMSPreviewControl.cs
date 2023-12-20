using System;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Abstract preview control class
    /// </summary>
    public abstract class CMSPreviewControl : CMSAdminControl
    {
        #region "Constants"

        /// <summary>
        /// Transformation constant
        /// </summary>
        public const String TRANSFORMATION = "t";

        /// <summary>
        /// Css stylesheet constant
        /// </summary>
        public const String CSSSTYLESHEET = "css";

        /// <summary>
        /// Web part container constant
        /// </summary>
        public const String WEBPARTCONTAINER = "wpc";

        /// <summary>
        /// Web part css constant
        /// </summary>
        public const String WEBPARTCSS = "wpcss";

        /// <summary>
        /// web part layout constant
        /// </summary>
        public const String WEBPARTLAYOUT = "wpl";

        /// <summary>
        /// Page layout constant
        /// </summary>
        public const String PAGELAYOUT = "pl";

        /// <summary>
        /// Page template layout constant
        /// </summary>
        public const String PAGETEMPLATELAYOUT = "ptl";

        /// <summary>
        /// Device layout constant
        /// </summary>
        public const String DEVICELAYUOT = "dl";

        /// <summary>
        /// Maser page constant
        /// </summary>
        public const String MASTERPAGE = "mp";

        /// <summary>
        /// Theme constant
        /// </summary>
        public const String THEME = "theme";

        #endregion


        #region "Variables"

        private bool mShowPreview = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// If true, the control is shown in preview mode
        /// </summary>
        public bool IsInPreview
        {
            get;
            set;
        }


        /// <summary>
        /// If true, preview can be shown.
        /// </summary>
        public bool ShowPreview
        {
            get
            {
                return mShowPreview;
            }
            set
            {
                mShowPreview = value;
            }
        }


        /// <summary>
        /// If set this path is automatically used when first opened.
        /// </summary>
        public String DefaultPreviewPath
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DefaultPreviewPath"), String.Empty);
            }
            set
            {
                SetValue("DefaultPreviewPath", value);
            }
        }


        /// <summary>
        /// Object name, used for session key, under preview information is stored
        /// </summary>
        public String PreviewObjectName
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PreviewObjectName"), String.Empty);
            }
            set
            {
                SetValue("PreviewObjectName", value);
            }
        }


        /// <summary>
        /// Default preview settings for object
        /// </summary>
        public String[] PreviewObjectPreferredDocument
        {
            get
            {
                if (UIContext.EditedObject is TransformationInfo)
                {
                    TransformationInfo ti = (TransformationInfo)UIContext.EditedObject;
                    if (!String.IsNullOrEmpty(ti.TransformationPreferredDocument))
                    {
                        return ti.GetPreferredPreviewDocument();
                    }
                }

                return null;
            }
        }


        /// <summary>
        /// Part of URL parameters, added to preview iframe URL
        /// </summary>
        public String PreviewURLSuffix
        {
            get
            {
                return ValidationHelper.GetString(GetValue("PreviewURLSuffix"), String.Empty);
            }
            set
            {
                SetValue("PreviewURLSuffix", value);
            }
        }


        /// <summary>
        /// If true title is added to toolbar in vertical position
        /// </summary>
        public bool DisplayTitlePane
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("DisplayTitlePane"), false);
            }
            set
            {
                SetValue("DisplayTitlePane", value);
            }
        }


        /// <summary>
        /// Default alias path for preview
        /// </summary>
        public String DefaultAliasPath
        {
            get
            {
                return ValidationHelper.GetString(GetValue("DefaultAliasPath"), String.Empty);
            }
            set
            {
                SetValue("DefaultAliasPath", value);
            }
        }


        /// <summary>
        /// ClientID of parent wrapper element (pane content)
        /// </summary>
        public String ParentClientID
        {
            get
            {
                return ValidationHelper.GetString(GetValue("ParentClientID"), String.Empty);
            }
            set
            {
                SetValue("ParentClientID", value);
            }
        }


        /// <summary>
        /// Indicates whether page is in dialog mode
        /// </summary>
        public bool DialogMode
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("DialogMode"), false);
            }
            set
            {
                SetValue("DialogMode", value);
            }
        }


        /// <summary>
        /// If true, preview was initialized
        /// </summary>
        public bool PreviewInitialized
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("PreviewInitialized"), false);
            }
            set
            {
                SetValue("PreviewInitialized", value);
            }
        }


        /// <summary>
        /// Indicates whether session values should be ignored when first load.
        /// </summary>
        public bool IgnoreSessionValues
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("IgnoreSessionValues"), false);
            }
            set
            {
                SetValue("IgnoreSessionValues", value);
            }
        }


        /// <summary>
        /// Indicates whether session values should be ignored. This is used when edited in dialog (or special cases) mode to read actual aliaspath and culture.
        /// </summary>
        public bool LoadSessionValues
        {
            get
            {
                return !(IgnoreSessionValues && (!RequestHelper.IsPostBack() || PreviewInitialized));
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns url for preview, based on object type and parameters
        /// </summary>
        protected String GetPreviewURL()
        {
            // Get parameters from session
            String[] parameters = null;

            // For dialog mode first time load or when preview is initialized - set actual settings, not stored 
            if (LoadSessionValues)
            {
                parameters = SessionHelper.GetValue(PreviewObjectName) as String[];

                // If parameters not in session, try load from DB
                if (parameters == null)
                {
                    parameters = PreviewObjectPreferredDocument;
                }
            }

            // If still null, create new
            if ((parameters == null) || (parameters.Length != 4))
            {
                parameters = new String[4];

                // First try get alias path from property
                String aliasPath = DefaultAliasPath;
                if (String.IsNullOrEmpty(aliasPath))
                {
                    // Then get path settings from query String (used in CMS Desk)
                    aliasPath = QueryHelper.GetString("aliaspath", String.Empty);
                }

                parameters[0] = String.IsNullOrEmpty(aliasPath) ? DefaultPreviewPath : aliasPath;
                parameters[3] = DeviceContext.CurrentDeviceProfileName;
            }

            String pagePreview = parameters[0];

            String src = String.Empty;
            if (String.IsNullOrEmpty(pagePreview))
            {
                src = UrlResolver.ResolveUrl(AdministrationUrlHelper.GetInformationUrl("transformation.preview.selectpage"));
            }
            else
            {
                String siteName = SiteInfoProvider.GetSiteName(ValidationHelper.GetInteger(parameters[1], SiteContext.CurrentSiteID));
                String culture = ValidationHelper.GetString(parameters[2], LocalizationContext.PreferredCultureCode);
                String deviceCode = ValidationHelper.GetString(parameters[3], String.Empty);

                // Get path             
                String applicationUrl = String.Empty;
                TreeNode node = null;
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                node = tree.SelectSingleNode(siteName, pagePreview, culture, false, null, false, false, false);

                if (node != null)
                {
                    applicationUrl = DocumentURLProvider.GetPermanentDocUrl(node.NodeGUID, node.NodeAlias, siteName, PageInfoProvider.PREFIX_CMS_GETDOC, "");
                    applicationUrl = (siteName != SiteContext.CurrentSiteName) ? DocumentURLProvider.GetUrl(applicationUrl, String.Empty, siteName) : URLHelper.GetAbsoluteUrl(applicationUrl);

                    // Add .aspx to end if not found
                    if (!applicationUrl.EndsWithCSafe(".aspx"))
                    {
                        applicationUrl += ".aspx";
                    }
                }

                if (applicationUrl != String.Empty)
                {
                    // Get query
                    string query = String.Format("?clientcache=0&previewobjectname={0}&objectlifetime={1}&viewmode={2}&lang={3}{4}", GetObjectName(), ObjectLifeTimeFunctions.ObjectLifeTimeToString(ObjectLifeTimeEnum.Request),
                        (int)ViewModeEnum.Preview, culture, PreviewURLSuffix);

                    // Finalize URL
                    src = String.Format("{0}{1}", applicationUrl, query);

                    // If non default device is used -> redirect to special preview page
                    DeviceProfileInfo profile = DeviceProfileInfoProvider.GetDeviceProfileInfo(deviceCode);

                    String profileName = String.Empty;
                    if ((profile != null) && (profile.ProfilePreviewWidth > 0) && (profile.ProfilePreviewHeight > 0))
                    {
                        src = AddUrlToQueryString("viewpage", src, "~/CMSPages/DevicePreview.aspx");
                        profileName = profile.ProfileName;
                    }

                    src += "&deviceName=" + profileName;
                    
                    // Add hash for clickjacking protection
                    string hash = ValidationHelper.GetHashString(URLHelper.GetQuery(src), new HashSettings(RequestContext.UserName));

                    src += "&refreshtoken=0";
                    src += string.Format("&clickjackinghash={0}", hash);
                    src += "#previewanchor";

                }
                else
                {
                    src = UrlResolver.ResolveUrl(AdministrationUrlHelper.GetInformationUrl("editform.documentnotfound"));
                }
            }

            return src;
        }


        /// <summary>
        /// Appends one URL to another as a query parameter.
        /// </summary>
        /// <param name="queryParam">Name of query parameter under which the original URL will be stored </param>
        /// <param name="originalUrl">Original URL</param>
        /// <param name="newUrl">URL to which the original URL will be appended as a parameter</param>
        private String AddUrlToQueryString(String queryParam, String originalUrl, String newUrl)
        {
            String queryString = URLHelper.GetQuery(originalUrl);
            String url = URLHelper.AppendQuery(UrlResolver.ResolveUrl(newUrl), queryString);
            url = URLHelper.AddParameterToUrl(url, queryParam, HttpUtility.UrlEncode(UrlResolver.ResolveUrl(originalUrl)));
            return url;
        }


        /// <summary>
        /// Save new preview document settings
        /// </summary>
        /// <param name="parameters">New document settings</param>
        protected void StoreNewPreferredDocument(String[] parameters)
        {
            TransformationInfo ti = UIContext.EditedObject as TransformationInfo;
            if (ti != null)
            {
                String newPreferredDoc = parameters[0] + ";" + parameters[1] + ";" + parameters[2] + ";" + parameters[3];
                if (newPreferredDoc != ti.TransformationPreferredDocument)
                {
                    ti.TransformationPreferredDocument = newPreferredDoc;
                    TransformationInfoProvider.SetTransformation(ti);
                }
            }
        }


        /// <summary>
        /// Insert (or replace) new preview state value based on given key
        /// </summary>
        /// <param name="key">Key of wanted value</param>
        /// <param name="state">New preview state value</param>
        public void SetPreviewStateToCookies(String key, int state)
        {
            String input = ValidationHelper.GetString(CookieHelper.GetValue(CookieName.PreviewState), String.Empty);
            String newValue = key + "=" + state;

            String pattern = key + @"=(\d)";
            Regex reg = RegexHelper.GetRegex(pattern);
            MatchCollection matches = reg.Matches(input);

            // If key already in cookies - replace
            if (matches.Count > 0)
            {
                input = reg.Replace(input, newValue);
            }
            else
            {
                // Else insert as new
                input += newValue;
            }
            CookieHelper.SetValue(CookieName.PreviewState, input, DateTime.Now.AddYears(1));
        }


        /// <summary>
        /// Parse preview state from cookies and returns value based on given key
        /// </summary>
        /// <param name="key">Key of wanted value</param>
        public int GetPreviewStateFromCookies(String key)
        {
            String input = ValidationHelper.GetString(CookieHelper.GetValue(CookieName.PreviewState), String.Empty);

            String pattern = key + @"=(\d)";
            Regex reg = RegexHelper.GetRegex(pattern);
            MatchCollection matches = reg.Matches(input);

            if (matches.Count > 0)
            {
                return ValidationHelper.GetInteger(matches[0].Groups[1].Value, 0);
            }

            return 0;
        }


        /// <summary>
        /// Returns String representation of object type based on previewobject
        /// </summary>
        private String GetObjectName()
        {
            BaseInfo bi = UIContext.EditedObject as BaseInfo;
            return (bi != null) ? bi.TypeInfo.ObjectType : PreviewObjectName;
        }


        /// <summary>
        /// Registers script for refresh preview
        /// </summary>
        protected void RegisterRefreshScript()
        {
            ScriptHelper.RegisterStartupScript(Page, typeof(String), "RefreshPreviewCall", ScriptHelper.GetScript("refreshPreview();"));
        }


        /// <summary>
        /// Register initialization scripts
        /// </summary>
        /// <param name="bodyID">Body div clientID</param>
        /// <param name="menuID">Menu div clientID</param>
        /// <param name="startWithFullScreen">If true, default editable area is shown in fullscreen mode</param>
        public void RegisterInitScripts(String bodyID, String menuID, bool startWithFullScreen)
        {
            String script = @"
$cmsj(document).ready(function () { 
     InitPreview('" + bodyID + "', " + startWithFullScreen.ToString().ToLowerCSafe() + @");
});";

            ScriptHelper.RegisterStartupScript(this, typeof(String), "StartUpRefreshScript", ScriptHelper.GetScript(script));
        }

        #endregion
    }
}
