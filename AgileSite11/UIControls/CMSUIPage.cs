using System;
using System.Web;
using System.Web.UI;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Modules;
using CMS.PortalEngine.Web.UI;
using CMS.SiteProvider;

namespace CMS.UIControls
{
    /// <summary>
    /// Base page class for UI 
    /// </summary>
    public class CMSUIPage : CMSPage
    {
        #region "Variables"

        private MacroResolver mContextResolver;

        /// <summary>
        /// Main object ID
        /// </summary>
        protected int objectID;

        private UIElementInfo mUIElement;

        #endregion


        #region "Properties"

        /// <summary>
        /// Provides access to a dialog's footer. Is only rendered if the page is a dialog.
        /// </summary>
        public virtual IDialogFooter DialogFooter
        {
            get;
            protected set;
        }


        /// <summary>
        /// Web part context resolver.
        /// </summary>
        public MacroResolver ContextResolver
        {
            get
            {
                if (mContextResolver == null)
                {
                    var resolver = MacroContext.CurrentResolver.CreateChild();

                    resolver.SetNamedSourceData("Page", Page);
                    resolver.SetNamedSourceData("UIContext", UIContext);

                    resolver.SetNamedSourceDataCallback("EditedObject", x => UIContext.EditedObject, false);
                    resolver.SetNamedSourceDataCallback("EditedObjectParent", x => UIContext.EditedObjectParent, false);

                    mContextResolver = resolver;
                }

                return mContextResolver;
            }
        }


        /// <summary>
        /// Gets the current UI element object.
        /// </summary>
        protected UIElementInfo UIElement
        {
            get
            {
                if (mUIElement == null)
                {
                    mUIElement = UIContext.UIElement;

                    if (mUIElement == null)
                    {
                        RedirectToInformation(String.Format(GetString("uielement.elementnotfound"), UIContext.ElementName));
                    }
                }

                return mUIElement;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// PreInit event handler.
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            CheckAdministrationInterface();

            // Culture set
            SetCulture();

            UIContext.ElementName = QueryHelper.GetString("ElementName", String.Empty);
            UIContext.ResourceName = QueryHelper.GetString("ResourceName", String.Empty);
            UIContext.ElementGuid = QueryHelper.GetGuid("elementguid", Guid.Empty);

            // Init dialog properties
            base.OnPreInit(e);

            UIContext.ContextResolver = ContextResolver;

            // Set EditedObject and ParentEditedObject. Add before InitPage to prepare EditedObject for page extender.
            objectID = EnsureObject(QueryHelper.GetString("objectID", String.Empty));
            EnsureParentObject();

            InitPage(this, UIContext);
            SetContextParameters();
        }


        /// <summary>
        /// On load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            RegisterJavaScript();
            AddNoCacheTag();
            RedirectToSecured();

            base.OnLoad(e);
        }


        /// <summary>
        /// Render event handler.
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            // Link component style sheets
            PortalHelper.AddComponentsCSS(this);

            base.Render(writer);
        }


        /// <summary>
        /// Redirects the current page to a specific URL
        /// </summary>
        /// <param name="url">URL to redirect</param>
        protected static void RedirectToUrl(string url)
        {
            // Trim keywords (resource name, element name, element guid)
            string currentQuery = URLHelper.GetQuery(RequestContext.CurrentURL);

            currentQuery = URLHelper.RemoveParametersFromUrl(currentQuery, "resourcename", "elementname", "elementguid", "displaytitle");

            // Append actual query
            url = URLHelper.AppendQuery(url, URLHelper.GetQuery(currentQuery));

            // Ensure hash
            url = URLHelper.EnsureHashToQueryParameters(url);

            URLHelper.Redirect(url);
        }


        /// <summary>
        /// Registers javascript
        /// </summary>
        private void RegisterJavaScript()
        {
            // Register element's script  
            String js = UIContext["Javascript"].ToString(String.Empty);
            String jsFile = UIContext["JavascriptLinkedFile"].ToString(String.Empty);

            if ((js != String.Empty) || (jsFile != String.Empty))
            {
                // Load javascript web part to register scripts
                CMSAbstractWebPart ctrl = Page.LoadControl("~/CMSWebParts/General/javascript.ascx") as CMSAbstractWebPart;
                if (ctrl != null)
                {
                    ctrl.SetValue("InlineScript", js);
                    ctrl.SetValue("LinkedFile", jsFile);

                    Controls.Add(ctrl);
                }
            }
        }


        /// <summary>
        /// Create URL suffix for wrapper controls
        /// </summary>
        protected void SetContextParameters()
        {
            // Resource name            
            UIContext["resourcename"] = ApplicationUrlHelper.GetResourceName(UIElement.ElementResourceID);

            // Object ID
            if (objectID > 0)
            {
                UIContext.ObjectID = objectID;
            }

            // For parent object not set manually by current element
            if (UIContext["parentobjectid"] == null)
            {
                int parentObjectID = QueryHelper.GetInteger("parentobjectid", 0);
                if (parentObjectID > 0)
                {
                    UIContext.ParentObjectID = parentObjectID;
                }
            }

            // Display title
            if (UIContext["displaytitle"] == null)
            {
                bool displayTitle = QueryHelper.GetBoolean("displaytitle", true);
                UIContext.DisplayTitle = displayTitle;
            }

            // Parent title, used for _parent breadcrumbs
            if (QueryHelper.Contains("parenttitle"))
            {
                UIContext["parenttitle"] = QueryHelper.GetBoolean("parenttitle", false);
            }

            // Tab index
            int tabIndex = QueryHelper.GetInteger("tabIndex", 0);
            if (tabIndex != 0)
            {
                UIContext["TabIndex"] = tabIndex;
            }

            // Proceed the return javascript handler
            string returnHandler = QueryHelper.GetString("returnhandler", String.Empty);
            if (!String.IsNullOrEmpty(returnHandler))
            {
                UIContext["returnhandler"] = returnHandler;

                // Specify return column type (id, codename...)
                string returnType = QueryHelper.GetString("returntype", String.Empty);
                if (!String.IsNullOrEmpty(returnType))
                {
                    UIContext["returntype"] = returnType;
                }
            }

            // Store query string
            UIContext[UIContextHelper.QUERY_STRING_KEY] = HttpContext.Current.Request.QueryString;
        }

        #endregion


        #region "Init methods"

        /// <summary>
        /// Initializes the page with current context
        /// </summary>
        /// <param name="page">Page to initialize</param>
        /// <param name="ctx">UI Context</param>
        /// <param name="validateDialogHash">Indicates whether hash should be validated</param>
        /// <param name="checkPermissions">Indicates whether permission should be checked (incl. UI personalization, Permissions, Resource on site)</param>
        internal static void InitPage(Page page, UIContext ctx, bool validateDialogHash = true, bool checkPermissions = true)
        {
            // Check license
            CheckLicense(ctx);

            // Validates dialog hash
            if (validateDialogHash)
            {
                ValidateDialogHash(ctx);
            }

            EnsureRootElementId(ctx);

            // Check list permissions
            if (checkPermissions)
            {
                CheckViewAndUIPermissions(ctx);
            }

            // Load page extender
            LoadPageExtender(page, ctx);
        }


        /// <summary>
        /// Checks element's license feature
        /// </summary>
        /// <param name="ctx">UI Context</param>
        protected static void CheckLicense(UIContext ctx)
        {
            var feature = UIContextHelper.FindElementFeature(ctx.UIElement);

            if (!String.IsNullOrEmpty(feature) && !String.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, feature.ToEnum<FeatureEnum>());
            }
        }


        /// <summary>
        /// Validates dialog hash
        /// </summary>
        protected static void ValidateDialogHash(UIContext ctx)
        {
            if (ctx.IsDialog)
            {
                // Validate hash
                QueryHelper.ValidateHash("hash", "objectid;" + ctx["excludedhashparameters"]);
            }
        }


        /// <summary>
        /// Ensures root element ID in UIContext
        /// </summary>
        /// <param name="ctx"></param>
        protected static void EnsureRootElementId(UIContext ctx)
        {
            if (ctx.IsDialog)
            {
                // Root UI element - this variable holds the very top UIElement which is open in a dialog.
                // The top UIElement uses this information and behaves a bit differently then its child elements (i.e. hides breadcrumbs, uses dialog header...) 
                int rootUIElementId = QueryHelper.GetInteger("rootelementid", 0);
                ctx.RootElementID = (rootUIElementId != 0) ? rootUIElementId : ctx.UIElement.ElementID;
            }
        }


        /// <summary>
        /// Checks view and UI personalization permissions
        /// </summary>
        protected static void CheckViewAndUIPermissions(UIContext ctx)
        {
            var elem = ctx.UIElement;
            if (elem == null)
            {
                return;
            }

            String resourceName = ApplicationUrlHelper.GetResourceName(elem.ElementResourceID);

            // Check module is on site
            if (!ApplicationUIHelper.IsAccessibleOnlyByGlobalAdministrator(elem) && !ResourceSiteInfoProvider.IsResourceOnSite(resourceName, SiteContext.CurrentSiteName))
            {
                RedirectToResourceNotAvailableOnSite(resourceName);
            }

            // Check module loaded 
            if (!ResourceInfoProvider.IsResourceAvailable(elem.ElementResourceID))
            {
                RedirectToUINotAvailable();
            }

            // Check UI element access
            CheckUIElementAccessHierarchical(elem, ctx.ContextResolver, ctx.RootElementID);
        }


        /// <summary>
        /// Loads page extender
        /// </summary>
        protected static void LoadPageExtender(Page page, UIContext ctx)
        {
            // Load page extender
            var extenderAssemblyName = ValidationHelper.GetString(ctx["PageExtenderAssemblyName"], String.Empty);
            var extenderClassName = ValidationHelper.GetString(ctx["PageExtenderClassName"], String.Empty);

            if (!String.IsNullOrEmpty(extenderAssemblyName) && !String.IsNullOrEmpty(extenderClassName))
            {
                try
                {
                    var extender = (PageExtender)ClassHelper.GetClass(extenderAssemblyName, extenderClassName);

                    extender.Init(page);
                }
                catch (Exception ex)
                {
                    // Log exception
                    CoreServices.EventLog.LogException("CMSUIPage", "OnInit", ex);

                    // Add error web part
                    var errorWebPart = new WebPartError
                    {
                        ErrorTitle = ex.Message
                    };

                    page.Controls.Add(errorWebPart);
                }
            }
        }

        #endregion


        #region "Object methods"

        /// <summary>
        /// If edited object parent is not set, try to set it 
        /// </summary>
        protected void EnsureParentObject()
        {
            UIContext context = UIContext;
            if (context.EditedObjectParent == null)
            {
                int parentID = ValidationHelper.GetInteger(context["parentobjectid"], 0);
                if (parentID != 0)
                {
                    // Some parent ID found, try to get parent object type
                    String parentObjectType = GetParentObjectType(context);
                    if (parentObjectType != String.Empty)
                    {
                        BaseInfo bi = ProviderHelper.GetInfoById(parentObjectType, parentID);
                        if (bi != null)
                        {
                            context.EditedObjectParent = bi;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Tries to set <see cref="UIContext.EditedObject"/> based on object type and object identifier <paramref name="objectId"/>.
        /// </summary>
        /// <param name="objectId">Object identifier - ID or code name</param>
        protected int EnsureObject(String objectId)
        {
            int ret = 0;
            int intId = ValidationHelper.GetInteger(objectId, -1);

            // Return if objectId not specified or 0
            if (String.IsNullOrEmpty(objectId) || (intId == 0))
            {
                return ret;
            }

            UIContext context = UIContext;
            String objectType = UIContextHelper.GetObjectType(context);

            // Store edited object
            if (!String.IsNullOrEmpty(objectType))
            {
                BaseInfo bi = null;
                IInfoProvider provider;
                if (intId > 0)
                {
                    bi = ProviderHelper.GetInfoById(objectType, intId);
                }
                else if ((provider = InfoProviderLoader.GetInfoProvider(objectType, false)) != null)
                {
                    // Check if object name is in full name format
                    if (provider is IFullNameInfoProvider)
                    {
                        // Get object by full name
                        bi = ProviderHelper.GetInfoByFullName(objectType, objectId);
                    }
                    
                    if (bi == null)
                    {
                        // Get site object by code name, if not found, try to get it from the global objects
                        bi = ProviderHelper.GetInfoByName(objectType, objectId, SiteContext.CurrentSiteID) ?? ProviderHelper.GetInfoByName(objectType, objectId, 0);
                    }
                }

                if (bi != null)
                {
                    context.EditedObject = bi;

                    ret = bi.Generalized.ObjectID;

                    // Object site ID
                    if (bi.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        // Use '0' for NULL
                        context["ObjectSiteID"] = ValidationHelper.GetInteger(bi.GetValue(bi.TypeInfo.SiteIDColumn), 0);
                    }
                }
                else
                {
                    // No edited object found for objectId -> redirect to object was not found page
                    context.EditedObject = null;
                }
            }

            return ret;
        }


        /// <summary>
        /// Try to find parent object type. Parent object type may be:        
        /// A. First parent's object type different from current object type
        /// B. First parameter 'parentobjecttype' in any parent UI element
        /// </summary>
        /// <param name="context">Control's UI context</param>
        protected string GetParentObjectType(UIContext context)
        {
            String parObjType = ValidationHelper.GetString(context["parentobjecttype"], String.Empty);

            if (parObjType != String.Empty)
            {
                return parObjType;
            }

            // If UI element does not hold parent object type, try TYPEINFO of edited object
            BaseInfo bi = context.EditedObject as BaseInfo;
            if (bi != null)
            {
                parObjType = bi.Generalized.ParentObjectType;
            }

            // If object type not set, try find from parent
            if ((parObjType == String.Empty) && (context.UIElement != null))
            {
                // Current object type
                String objectType = ValidationHelper.GetString(context["objecttype"], String.Empty);

                // Find first parent
                UIElementInfo parent = UIElementInfoProvider.GetUIElementInfo(context.UIElement.ElementParentID);

                while ((parent != null) && (parObjType == String.Empty))
                {
                    // Load parent data
                    UIContextData data = new UIContextData();
                    data.LoadData(parent.ElementProperties);

                    // Load parent's objecttype and parent's parentobjecttype
                    parObjType = ValidationHelper.GetString(data["parentobjecttype"], String.Empty);
                    String objCurrent = ValidationHelper.GetString(data["objecttype"], String.Empty);

                    // If object type not already found, set first one
                    if (objectType == String.Empty)
                    {
                        objectType = objCurrent;
                    }

                    // Parent objecttype is either objecttype different from current objecttype
                    if ((objectType != String.Empty) && (objCurrent != String.Empty) && (objectType != objCurrent))
                    {
                        return objCurrent;
                    }

                    // Or parent's object type
                    if (parObjType != String.Empty)
                    {
                        return parObjType;
                    }

                    parent = UIElementInfoProvider.GetUIElementInfo(parent.ElementParentID);
                }
            }

            return parObjType;
        }

        #endregion
    }
}