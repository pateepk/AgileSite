using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Modules;
using CMS.PortalEngine;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// UI context properties.
    /// </summary>
    [RegisterAllProperties]
    public class UIContext : AbstractContext<UIContext>, INotCopyThreadItem
    {
        #region "Variables"

        private UIElementInfo mUIElement;
        private UIContextData mData;
        private UIContextSecure mSecure;
        private object mEditedObject;
        private object mEditedObjectParent;

        private MacroResolver mContextResolver;

        private bool? mIsRootDialog;
        private bool? mIsDialog;
        private List<KeyValuePair<string, string>> mAnchorLinks;

        private bool mErrorLabelAdded;
        private bool mHideControlOnError = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Current UI context
        /// </summary>
        public new static UIContext Current
        {
            get
            {
                return CurrentContext;
            }
        }


        /// <summary>
        /// If true, the context hides the parent control in case of error
        /// </summary>
        public bool HideControlOnError
        {
            get
            {
                return mHideControlOnError;
            }
            set
            {
                mHideControlOnError = value;
            }
        }


        /// <summary>
        /// Override indexer, add fallback to querystring
        /// </summary>
        /// <param name="key">Key name to collection</param>
        public override object this[String key]
        {
            get
            {
                return Data[key];
            }
            set
            {
                Data[key] = value;
            }
        }


        /// <summary>
        /// UI context resolver.
        /// </summary>
        public MacroResolver ContextResolver
        {
            get
            {
                return mContextResolver ?? (mContextResolver = CreateResolver());
            }
            set
            {
                mContextResolver = value;
            }
        }


        /// <summary>
        /// Indicates whether property should be resolved with context's resolver.
        /// </summary>
        public bool ResolveMacros
        {
            get
            {
                return Data.ResolveMacros;
            }
            set
            {
                Data.ResolveMacros = value;
            }
        }


        /// <summary>
        /// Indicates whether data should be loaded from parent context
        /// </summary>
        public bool LoadDataFromParent
        {
            get;
            set;
        }


        /// <summary>
        /// Gets context data for UI.
        /// </summary>
        public UIContextData Data
        {
            get
            {
                return mData ?? (mData = GetCurrentData());
            }
            set
            {
                mData = value;
            }
        }


        /// <summary>
        /// Gets the resource name.
        /// </summary>
        public string ResourceName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the current UI element name.
        /// </summary>
        public string ElementName
        {
            get;
            set;
        }


        /// <summary>
        /// GUID for element to display
        /// </summary>
        public Guid ElementGuid
        {
            get;
            set;
        }


        /// <summary>
        /// UI element for current context
        /// </summary>
        public UIElementInfo UIElement
        {
            get
            {
                // Get UI element based on set properties
                if (mUIElement == null)
                {
                    mUIElement = GetUIElement();

                    // Try to load UI element from upper UI context if not found based on properties
                    if ((mUIElement == null) && LoadDataFromParent)
                    {
                        // If assigned control is set (means this UI context is not top element)
                        var ctrl = AssignedControl;

                        while (ctrl != null)
                        {
                            var context = UIContextHelper.GetUIContext(ctrl.Parent);
                            if (context == null)
                            {
                                break;
                            }

                            // If parent context found, try to use its data
                            if (context.UIElement != null)
                            {
                                mUIElement = context.UIElement;
                                break;
                            }

                            ctrl = context.AssignedControl;
                        }
                    }
                }

                return mUIElement;
            }
            set
            {
                mUIElement = value;
            }
        }


        /// <summary>
        /// Object edited by the current page.
        /// </summary>
        public object EditedObject
        {
            get
            {
                return mEditedObject;
            }
            set
            {
                if (value == null)
                {
                    if (AssignedControl == null)
                    {
                        String infoText = (PortalContext.ViewMode == ViewModeEnum.Design) ? "designmode.cantdisplay" : "editedobject.notexists";
                        URLHelper.SeeOther(AdministrationUrlHelper.GetInformationUrl(infoText));
                    }
                    else
                    {
                        AddErrorLabel();
                    }
                }
                else
                {
                    mEditedObject = value;
                }
            }
        }


        /// <summary>
        /// Parent of the object edited by the current page.
        /// </summary>
        public object EditedObjectParent
        {
            get
            {
                return mEditedObjectParent;
            }
            set
            {

                if (value == null)
                {
                    if (AssignedControl == null)
                    {
                        URLHelper.SeeOther(AdministrationUrlHelper.GetInformationUrl("editedobject.notexists"));
                    }
                    else
                    {
                        AddErrorLabel();
                    }
                }
                else
                {
                    mEditedObjectParent = value;
                }
            }
        }


        /// <summary>
        /// Control to which UI context is assigned. If null context belongs to main page.
        /// </summary>
        public Control AssignedControl
        {
            get;
            set;
        }


        /// <summary>
        /// Site ID
        /// </summary>
        [RegisterColumn]
        public int SiteID
        {
            get
            {
                return ValidationHelper.GetInteger(Data["SiteID"], 0);
            }
        }


        /// <summary>
        /// Indicates whether page is dialog or nested in a dialog. Use <see cref="IsRootDialog"/> property to check the top dialog page.
        /// </summary>
        [RegisterColumn]
        public bool IsDialog
        {
            get
            {
                return mIsDialog ?? ValidationHelper.GetBoolean(Data["Dialog"], false);
            }
            set
            {
                mIsDialog = value;
                Data["Dialog"] = value;
            }
        }


        /// <summary>
        /// Indicates whether page is root dialog (top dialog page with header and footer)
        /// </summary>
        [RegisterColumn]
        public bool IsRootDialog
        {
            get
            {
                return mIsRootDialog ?? ValidationHelper.GetBoolean(Data["IsRootDialog"], false);
            }
            set
            {
                mIsRootDialog = value;
                Data["IsRootDialog"] = value;
            }
        }


        /// <summary>
        /// Parent object ID
        /// </summary>
        [RegisterColumn]
        public int ParentObjectID
        {
            get
            {
                return ValidationHelper.GetInteger(Data["ParentObjectID"], 0);
            }
            set
            {
                Data["ParentObjectID"] = value;
            }
        }


        /// <summary>
        /// Object ID
        /// </summary>
        [RegisterColumn]
        public int ObjectID
        {
            get
            {
                return ValidationHelper.GetInteger(Data["ObjectID"], 0);
            }
            set
            {
                Data["ObjectID"] = value;
            }
        }


        /// <summary>
        /// Root element ID
        /// </summary>
        public int RootElementID
        {
            get
            {
                return ValidationHelper.GetInteger(Data["RootElementID"], 0);
            }
            set
            {
                Data["RootElementID"] = value;
            }
        }


        /// <summary>
        /// If true, the title should be displayed
        /// </summary>
        public bool DisplayTitle
        {
            get
            {
                return ValidationHelper.GetBoolean(Data["DisplayTitle"], true);
            }
            set
            {
                Data["DisplayTitle"] = value;
            }
        }


        /// <summary>
        /// Secure wrapper for properties in special secure mode.
        /// </summary>
        public UIContextSecure Secure
        {
            get
            {
                return mSecure ?? (mSecure = new UIContextSecure(this));
            }
        }


        /// <summary>
        /// If true, the breadcrumbs should be displayed
        /// </summary>
        public bool DisplayBreadcrumbs
        {
            get
            {
                return ValidationHelper.GetBoolean(Data["DisplayBreadcrumbs"], false);
            }
            set
            {
                Data["DisplayBreadcrumbs"] = value;
            }
        }


        /// <summary>
        /// List of anchor links for quick navigation.
        /// </summary>
        public List<KeyValuePair<string, string>> AnchorLinks
        {
            get
            {
                return mAnchorLinks ?? (mAnchorLinks = new List<KeyValuePair<string, string>>());
            }
            set
            {
                mAnchorLinks = value;
            }
        }

        #endregion


        /// <summary>
        /// Event fired when the UI context dynamic data is requested
        /// </summary>
        public event EventHandler<UIContextEventArgs> OnGetValue
        {
            add
            {
                Data.OnGetValue += value;
            }
            remove
            {
                Data.OnGetValue -= value;
            }
        }

        #region "Methods"

        /// <summary>
        /// Returns value of property.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <param name="notNull">If true, the property attempts to return non-null values, at least it returns the empty object of the correct type</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public override bool TryGetProperty(string columnName, out object value, bool notNull)
        {
            if (base.TryGetProperty(columnName, out value, notNull))
            {
                return true;
            }

            value = this[columnName];
            return true;
        }


        /// <summary>
        /// Adds error label to assigned control
        /// </summary>
        private void AddErrorLabel()
        {
            if (!mErrorLabelAdded)
            {
                // Add information label to parent control
                var lbl = new Label
                {
                    CssClass = "WebPartError",
                    Text = ResHelper.GetString("editedobject.notexists")
                };

                // Hide assigned control and add label
                if (HideControlOnError)
                {
                    AssignedControl.Visible = false;
                }

                var parent = AssignedControl.Parent;

                PageContext.PreRender += (sender, args) => parent.Controls.AddAt(0, lbl);

                mErrorLabelAdded = true;
            }
        }


        /// <summary>
        /// Clones current UI context and creates new instance
        /// </summary>
        public UIContext Clone()
        {
            UIContext ui = new UIContext();

            // Clone dictionary data
            ui.Data = Data.Clone();
            ui.UIElement = UIElement;

            if (EditedObject != null)
            {
                ui.EditedObject = EditedObject;
            }

            if (EditedObjectParent != null)
            {
                ui.EditedObjectParent = EditedObjectParent;
            }

            return ui;
        }


        /// <summary>
        /// Gets value from collection
        /// </summary>
        /// <param name="name">Column name</param>
        /// <param name="value">Returned value</param>
        public override bool TryGetProperty(string name, out object value)
        {
            // Call base method first to check defined UIContext properties
            bool result = base.TryGetProperty(name, out value);

            // If UIContext does not contain such direct property search UIContextData
            if (!result)
            {
                value = Data[name];
                if (value != null)
                {
                    return true;
                }
            }

            return result;
        }


        /// <summary>
        /// Creates a macro resolver for this UI context
        /// </summary>
        private MacroResolver CreateResolver()
        {
            var resolver = MacroContext.CurrentResolver.CreateChild();

            resolver.SetNamedSourceData("UIContext", this);
            resolver.SetNamedSourceDataCallback("EditedObject", x => EditedObject, false);
            resolver.SetNamedSourceDataCallback("EditedObjectParent", x => EditedObjectParent, false);

            return resolver;
        }


        /// <summary>
        /// Gets the current UI context data
        /// </summary>
        private UIContextData GetCurrentData()
        {
            UIContextData data = null;

            if (LoadDataFromParent)
            {
                // Get parent context data
                var parentContextData = GetParentContextData();
                if (parentContextData != null)
                {
                    data = parentContextData;
                }
            }

            if (mData == null)
            {
                data = new UIContextData();
                data.ContextResolver = ContextResolver;

                // Load data from UI element
                if (UIElement != null)
                {
                    data.LoadData(UIElement.ElementProperties);
                }
            }

            return data;
        }


        /// <summary>
        /// Gets the parent context data if available
        /// </summary>
        private UIContextData GetParentContextData()
        {
            UIContextData parentContextData = null;

            // If assigned control is set (that is this UI context is not top element)
            // Try to load data from upper UI context
            var ctrl = AssignedControl;

            while (ctrl != null)
            {
                var context = UIContextHelper.GetUIContext(ctrl.Parent);
                if (context == null)
                {
                    break;
                }

                // If parent context found, try to use its data
                if (context.Data != null)
                {
                    parentContextData = context.Data;
                    break;
                }

                ctrl = context.AssignedControl;
            }

            return parentContextData;
        }


        /// <summary>
        /// Gets the current UI element based on UI context properties
        /// </summary>
        private UIElementInfo GetUIElement()
        {
            // First try to load via guid
            if (ElementGuid != Guid.Empty)
            {
                return UIElementInfoProvider.GetUIElementInfo(ElementGuid);
            }
            
            // If guid was not set try to load via resource name and element name
            if ((ElementName != String.Empty) && (ResourceName != String.Empty))
            {
                return UIElementInfoProvider.GetUIElementInfo(ResourceName, ElementName);
            }

            return null;
        }

        #endregion
    }
}