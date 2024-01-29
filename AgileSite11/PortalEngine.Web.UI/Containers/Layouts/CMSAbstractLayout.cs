using System;
using System.Collections;
using System.Web.UI;
using System.Collections.Generic;

using CMS.Base.Web.UI;
using CMS.Base;
using CMS.DocumentEngine;
using CMS.Membership;

namespace CMS.PortalEngine.Web.UI 
{
    using ZonesDictionary = SafeDictionary<string, CMSWebPartZone>;
    using ConditionalLayoutList = List<CMSConditionalLayout>;
#pragma warning disable 618
    using ContentList = List<CMSContent>;
#pragma warning restore 618
    using WebPartZoneList = List<CMSWebPartZone>;

    /// <summary>
    /// Abstract page layout class.
    /// </summary>
    public abstract class CMSAbstractLayout : AbstractUserControl, ICMSPortalControl
    {
        #region "Variables"

        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        protected CMSPagePlaceholder mPagePlaceholder = null;

        /// <summary>
        /// Collection of all the layout web part zones.
        /// </summary>
        protected ZonesDictionary mWebPartZones = null;

        /// <summary>
        /// Web part zones list.
        /// </summary>
        protected WebPartZoneList mWebPartZonesList = null;

        /// <summary>
        /// List of conditional layout controls hosted by this layout
        /// </summary>
        protected ConditionalLayoutList mConditionalLayouts = null;

        /// <summary>
        /// List of the content placeholders.
        /// </summary>
#pragma warning disable 618
        protected ContentList mContents = null;
#pragma warning restore 618

        /// <summary>
        /// Page info for current layout.
        /// </summary>
        protected PageInfo mPageInfo = null;

        /// <summary>
        /// Page template instance.
        /// </summary>
        protected PageTemplateInstance mTemplateInstance = null;

        /// <summary>
        /// True if the web part content was already loaded
        /// </summary>
        protected bool mWebPartsContentLoaded = false;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Parent page placeholder.
        /// </summary>
        public virtual CMSPagePlaceholder PagePlaceholder
        {
            get
            {
                if (mPagePlaceholder == null)
                {
                    mPagePlaceholder = PortalHelper.FindParentPlaceholder(this);
                    if (mPagePlaceholder == null)
                    {
                        throw new Exception("[CMSAbstractLayout.PagePlaceholder]: Parent CMSPagePlaceholder not found.");
                    }
                }
                return mPagePlaceholder;
            }
            set
            {
                mPagePlaceholder = value;
            }
        }


        /// <summary>
        /// Portal manager for the page.
        /// </summary>
        public virtual CMSPortalManager PortalManager
        {
            get
            {
                return PagePlaceholder.PortalManager;
            }
            set
            {
                PagePlaceholder.PortalManager = value;
            }
        }


        /// <summary>
        /// Returns the table of all the inner web part zones.
        /// </summary>
        public ZonesDictionary WebPartZones
        {
            get
            {
                // Ensures the lists of zones
                EnsureZones();

                return mWebPartZones;
            }
        }


        /// <summary>
        /// Returns the list of all the inner web part zones (CMSWebPartZone controls).
        /// </summary>
        public WebPartZoneList WebPartZonesList
        {
            get
            {
                // Ensures the lists of zones
                EnsureZones();

                return mWebPartZonesList;
            }
        }


        /// <summary>
        /// Returns the list of found conditional layouts
        /// </summary>
        public ConditionalLayoutList ConditionalLayouts
        {
            get
            {
                if (mConditionalLayouts == null)
                {
                    // Collect the layouts
                    mConditionalLayouts = PortalHelper.CollectControlsOfType<CMSConditionalLayout, CMSWebPartZone>(this) ?? new ConditionalLayoutList();
                }

                return mConditionalLayouts;
            }
        }


        /// <summary>
        /// Returns the list of found content placeholders.
        /// </summary>
#pragma warning disable 618
        public ContentList Contents
#pragma warning restore 618
        {
            get
            {
                if (mContents == null)
                {
                    // Collect the controls within this layout
#pragma warning disable 618
                    mContents = PortalHelper.CollectControlsOfType<CMSContent, CMSWebPartZone>(this) ?? new ContentList();
#pragma warning restore 618
                }

                return mContents;
            }
        }


        /// <summary>
        /// Page info.
        /// </summary>
        public virtual PageInfo PageInfo
        {
            get
            {
                return mPageInfo;
            }
        }


        /// <summary>
        /// Returns page template instance structure.
        /// </summary>
        public virtual PageTemplateInstance TemplateInstance
        {
            get
            {
                return mTemplateInstance;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Relocates the layout content to the correct locations.
        /// </summary>
        public virtual void RelocateContent()
        {
            // Relocate all found contents
#pragma warning disable 618
            foreach (CMSContent c in Contents)
#pragma warning restore 618
            {
                c.RelocateContent();
            }
        }


        /// <summary>
        /// Checks the visibility of all conditional layouts
        /// </summary>
        public void CheckLayoutsVisibility()
        {
            // Data bind conditional layouts
            foreach (CMSConditionalLayout layout in ConditionalLayouts)
            {
                // Resolve all properties set in markup (including child elements)
                layout.DataBind();
            }
            
            // Evaluate the visibility of conditional layouts
            foreach (CMSConditionalLayout layout in ConditionalLayouts)
            {
                layout.CheckLayoutVisible();
            }
        }


        /// <summary>
        /// Loads the page content to the layout.
        /// </summary>
        /// <param name="pageInfo">Page info object with the page content</param>
        /// <param name="reloadData">Reload the page data</param>
        /// <param name="allowOrphanedZones">If true, orphaned zones should be added to the layout</param>
        public virtual void LoadContent(PageInfo pageInfo, bool reloadData = true, bool allowOrphanedZones = false)
        {
            if (pageInfo != null)
            {
                mPageInfo = pageInfo;

                // Check the visibility of layouts
                CheckLayoutsVisibility();

                // Check if there is a cached personalization for the user (only for live site mode)
                ViewModeEnum viewMode = PagePlaceholder.ViewMode;

                // Load all the MVT/Content personalization variants for the current template if not loaded yet
                if ((pageInfo.UsedPageTemplateInfo != null)
                    && (pageInfo.UsedPageTemplateInfo.TemplateInstance != null))
                {
                    PageTemplateInstance ti = pageInfo.UsedPageTemplateInfo.TemplateInstance;

                    // When changing settings of MVT/CP
                    if ((!PortalContext.MVTVariantsEnabled && ti.MVTVariantsLoaded)
                        || (!PortalContext.ContentPersonalizationEnabled && ti.ContentPersonalizationVariantsLoaded))
                    {
                        // Reload the MVT/Content personalization variants
                        ti.LoadVariants(true, VariantModeEnum.None);

                        // Reset the flag indicating whether MVT/CP variants have been loaded
                        if (!PortalContext.MVTVariantsEnabled)
                        {
                            ti.MVTVariantsLoaded = false;
                        }
                        if (!PortalContext.ContentPersonalizationEnabled)
                        {
                            ti.ContentPersonalizationVariantsLoaded = false;
                        }
                    }
                    else
                    {
                        // If the cached template has not loaded MVT variants yet, then load them now
                        if (PortalContext.MVTVariantsEnabled && !ti.MVTVariantsLoaded)
                        {
                            // Reload the MVT variants
                            ti.LoadVariants(true, VariantModeEnum.MVT);
                        }

                        // If the cached template has not loaded Content personalization variants yet, then load them now
                        if (PortalContext.ContentPersonalizationEnabled && !ti.ContentPersonalizationVariantsLoaded)
                        {
                            // Reload the Content personalization variants
                            ti.LoadVariants(true, VariantModeEnum.ContentPersonalization);
                        }
                    }
                }

                if (PagePlaceholder.ViewMode.IsLiveSite())
                {
                    // Get the personalization for the user
                    mTemplateInstance = PersonalizationInfoProvider.GetPersonalizedTemplateInstance(pageInfo, MembershipContext.AuthenticatedUser.UserID);
                }
                else if (PagePlaceholder.ViewMode == ViewModeEnum.DashboardWidgets)
                {
                    // Get the personalization for the user
                    mTemplateInstance = PersonalizationInfoProvider.GetPersonalizedTemplateInstance(pageInfo, MembershipContext.AuthenticatedUser.UserID, PortalContext.DashboardName, PortalContext.DashboardSiteName);
                }

                // If not found, get just from the page itself
                if (mTemplateInstance == null)
                {
                    if (PortalContext.IsDesignMode(viewMode))
                    {
                        // Design mode - only default template instance
                        PageTemplateInfo pti = pageInfo.UsedPageTemplateInfo;
                        if (pti != null)
                        {
                            mTemplateInstance = pti.TemplateInstance;
                        }
                    }
                    else
                    {
                        // Other modes - combined instance
                        mTemplateInstance = pageInfo.TemplateInstance;
                    }
                }

                // Get the page template instance
                if (mTemplateInstance != null)
                {
                    // Load the zones and keep the list of missing ones
                    List<WebPartZoneInstance> notLoadedZones = null;
                    if (allowOrphanedZones)
                    {
                        notLoadedZones = PortalManager.EnsureNotLoadedZones(mTemplateInstance.WebPartZones);
                    }

                    // Load all zones
                    var zones = new WebPartZoneList(WebPartZonesList);
                    foreach (CMSWebPartZone zone in zones)
                    {
                        LoadZone(zone, reloadData);
                    }

                    // Load the web parts content
                    LoadWebPartsContent(reloadData);
                }
            }
        }


        /// <summary>
        /// Loads the content of the underlying web parts
        /// </summary>
        /// <param name="reloadData">Reload the page data</param>
        public void LoadWebPartsContent(bool reloadData)
        {
            if (mWebPartsContentLoaded && !reloadData)
            {
                return;
            }

            // Load the webparts zones
            ArrayList zones = new ArrayList(WebPartZonesList);
            foreach (CMSWebPartZone zone in zones)
            {
                zone.LoadWebPartsContent(reloadData);
            }

            // Raise on content loaded event
            if (reloadData)
            {
                OnContentLoaded();
            }

            mWebPartsContentLoaded = true;
        }


        /// <summary>
        /// Loads the orphaned zones to the layout.
        /// </summary>
        /// <param name="reloadData">Reload web parts data</param>
        public void LoadOrphanedZones(bool reloadData)
        {
            if (!PortalHelper.ShowOrphanedWebPartZones)
            {
                return;
            }

            // Create orphaned zones
            var notLoadedZones = PortalManager.EnsureNotLoadedZones(mTemplateInstance.WebPartZones);
            if ((notLoadedZones != null) && (notLoadedZones.Count > 0))
            {
                int i = 0;
                WebPartZoneInstance zoneInstance = null;
                var newZones = new WebPartZoneList();

                bool showLayout = PortalHelper.ShowOrphanedLayoutWebPartZones;

                // Go through all not loaded zones
                while (i < notLoadedZones.Count)
                {
                    if (notLoadedZones[i] == zoneInstance)
                    {
                        // Skip the zone (already loaded)
                        i++;
                    }
                    else
                    {
                        // Load the zone
                        zoneInstance = notLoadedZones[i];
                        if ((zoneInstance.WebParts.Count > 0) && (!zoneInstance.LayoutZone || showLayout))
                        {
                            // Add orphaned zone
                            CMSWebPartZone zone = new CMSWebPartZone(true);
                            zone.ID = zoneInstance.ZoneID;

                            Controls.Add(zone);

                            LoadZone(zone, false);

                            // Register the zone in the list
                            RegisterZone(zone);

                            newZones.Add(zone);
                        }
                    }
                }

                // Load the new webparts zones
                foreach (CMSWebPartZone zone in newZones)
                {
                    zone.LoadWebPartsContent(reloadData);
                }

                if (reloadData)
                {
                    // Call OnContentLoaded on all new zones
                    foreach (CMSWebPartZone zone in newZones)
                    {
                        zone.OnContentLoaded();
                    }
                }
            }
        }


        /// <summary>
        /// Registers the zone within the list of zones.
        /// </summary>
        /// <param name="zone">Zone to register</param>
        public void RegisterZone(CMSWebPartZone zone)
        {
            WebPartZones[zone.ID.ToLowerCSafe()] = zone;
            WebPartZonesList.Add(zone);
        }


        /// <summary>
        /// Loads the given web part zone.
        /// </summary>
        /// <param name="zone">Web part zone to load</param>
        /// <param name="reloadData">Reload the zone data</param>
        public void LoadZone(CMSWebPartZone zone, bool reloadData)
        {
            // Load the zones and keep the list of missing ones
            WebPartZoneInstance zoneInstance = mTemplateInstance.GetZone(zone.ID.ToLowerCSafe());

            // Load only if not within conditional layout or the conditional layout is displayed
            if ((zone.ConditionalLayout == null) || zone.ConditionalLayout.Visible)
            {
                zone.LoadWebParts(zoneInstance, reloadData);
            }

            PortalManager.MarkZoneLoaded(zoneInstance);
        }


        /// <summary>
        /// Saves the page content to current page info.
        /// </summary>
        /// <param name="pageInfo">Page info where to save the content</param>
        public virtual void SaveContent(PageInfo pageInfo)
        {
            // Save all zones content
            foreach (CMSWebPartZone zone in WebPartZones.Values)
            {
                zone.SaveContent(pageInfo);
            }
        }


        /// <summary>
        /// Clears the cache of all the web part zones.
        /// </summary>
        public virtual void ClearCache()
        {
            foreach (CMSWebPartZone zone in WebPartZones.Values)
            {
                zone.ClearCache();
            }
        }


        /// <summary>
        /// Returns true if the web part management support is required.
        /// </summary>
        public virtual bool RequiresWebPartManagement()
        {
            foreach (CMSWebPartZone zone in WebPartZones.Values)
            {
                if (zone.WebPartManagementRequired)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Saves the page content to current page info.
        /// </summary>
        public virtual bool Validate()
        {
            bool isValid = true;

            // Save all zones content
            foreach (CMSWebPartZone zone in WebPartZones.Values)
            {
                isValid = isValid & zone.Validate();
            }

            return isValid;
        }


        /// <summary>
        /// Ensures the lists of zones.
        /// </summary>
        protected virtual void EnsureZones()
        {
            if (mWebPartZones == null)
            {
                EnsureChildControls();

                // Collect the zones
                var zones = PortalHelper.CollectWebPartZones(this);
                if (zones == null)
                {
                    zones = new WebPartZoneList();
                }

                // Add to the zones list
                mWebPartZonesList = zones;
                mWebPartZones = ControlsHelper.GetControlsHashtable(zones);

                // Init parent placeholders
                if (mWebPartZones != null)
                {
                    foreach (CMSWebPartZone zone in mWebPartZones.Values)
                    {
                        zone.PagePlaceholder = PagePlaceholder;
                    }
                }
            }
        }


        /// <summary>
        /// Method that is called when the page content is fully loaded.
        /// </summary>
        public virtual void OnContentLoaded()
        {
            // Call OnContentLoaded on all zones
            foreach (CMSWebPartZone zone in WebPartZones.Values)
            {
                zone.OnContentLoaded();
            }
        }


        /// <summary>
        /// Returns the arraylist of the field IDs (Client IDs of the inner controls) that should be spell checked.
        /// </summary>
        public virtual List<string> GetSpellCheckFields()
        {
            List<string> result = new List<string>();

            // Collect from the zones
            foreach (CMSWebPartZone zone in WebPartZones.Values)
            {
                List<string> fields = zone.GetSpellCheckFields();
                if (fields != null)
                {
                    result.AddRange(fields);
                }
            }

            return result;
        }


        /// <summary>
        /// Causes reloading the data, override to implement the data reloading procedure.
        /// </summary>
        public virtual void ReloadData()
        {
            // Reload all the zones
            foreach (CMSWebPartZone zone in WebPartZones.Values)
            {
                zone.ReloadData();
            }
        }


        /// <summary>
        /// Finds the  web part with specific name.
        /// </summary>
        /// <param name="name">Web part name to find</param>
        public virtual CMSAbstractWebPart FindWebPart(string name)
        {
            // Go through all the zones
            foreach (CMSWebPartZone zone in WebPartZones.Values)
            {
                // Find the web part in zone
                CMSAbstractWebPart part = zone.FindWebPart(name);
                if (part != null)
                {
                    return part;
                }
            }

            return null;
        }


        /// <summary>
        /// Finds the  web part with specific type (first web part).
        /// </summary>
        /// <param name="type">Web part type to find</param>
        public virtual CMSAbstractWebPart FindWebPart(Type type)
        {
            // Go through all the zones
            foreach (CMSWebPartZone zone in WebPartZones.Values)
            {
                // Find the web part in zone
                CMSAbstractWebPart part = zone.FindWebPart(type);
                if (part != null)
                {
                    return part;
                }
            }

            return null;
        }


        /// <summary>
        /// Finds all web parts of specified type.
        /// </summary>
        /// <param name="type">Type to find</param>
        public virtual ArrayList FindAllWebParts(Type type)
        {
            ArrayList webParts = new ArrayList();

            // Go through all the zones
            foreach (CMSWebPartZone zone in PagePlaceholder.WebPartZones)
            {
                // Find the web part in zone
                foreach (Control ctrl in zone.WebParts)
                {
                    // Get control instance
                    CMSAbstractWebPart part = PortalHelper.GetWebPartControl(ctrl);
                    if ((part != null) && type.IsInstanceOfType(part))
                    {
                        webParts.Add(part);
                    }
                }
            }

            return webParts;
        }


        /// <summary>
        /// Finds the zone by its ID.
        /// </summary>
        /// <param name="zoneId">Zone ID</param>
        public CMSWebPartZone FindZone(string zoneId)
        {
            if (zoneId != null)
            {
                zoneId = zoneId.ToLowerCSafe();

                foreach (CMSWebPartZone zone in WebPartZones.Values)
                {
                    if ((zone.ZoneInstance != null) && (zone.ZoneInstance.ZoneID.ToLowerCSafe() == zoneId))
                    {
                        return zone;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}