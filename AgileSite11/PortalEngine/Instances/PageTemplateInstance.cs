using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Class holding the page template structure information.
    /// </summary>
    public class PageTemplateInstance : IStructuredData
    {
        #region "Variables"

        /// <summary>
        /// Collection of the webpart zones contained within the page template.
        /// </summary>
        protected List<WebPartZoneInstance> mWebPartZones;

        /// <summary>
        /// Web parts XML.
        /// </summary>
        protected string mWebPartsXml;

        private static Regex mControlIDReplaceRegex;

        private readonly object webPartZonesLock = new Object();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the regular expression for valid control id.
        /// </summary>
        private static Regex ControlIDReplaceRegex
        {
            get
            {
                return mControlIDReplaceRegex ?? (mControlIDReplaceRegex = RegexHelper.GetRegex("\\W"));
            }
        }


        /// <summary>
        /// Web part zones contained within the Page template.
        /// </summary>
        public List<WebPartZoneInstance> WebPartZones
        {
            get
            {
                if (mWebPartZones == null)
                {
                    lock (webPartZonesLock)
                    {
                        if (mWebPartZones == null)
                        {
                            LoadFromXml(mWebPartsXml);
                        }
                    }
                }

                return mWebPartZones;
            }
        }


        /// <summary>
        /// Gets or sets the page template WebParts.
        /// </summary>
        public string WebParts
        {
            get
            {
                return GetZonesXML();
            }
            set
            {
                mWebPartsXml = value;
                LoadFromXml(value);
            }
        }


        /// <summary>
        /// Parent page template.
        /// </summary>
        public PageTemplateInfo ParentPageTemplate
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether MVT variants have been loaded for this page template instance.
        /// </summary>
        public bool MVTVariantsLoaded
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether Content personalization variants have been loaded for this page template instance.
        /// </summary>
        public bool ContentPersonalizationVariantsLoaded
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Empty constructor
        /// </summary>
        public PageTemplateInstance()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="webPartsXml">Web parts XML</param>
        public PageTemplateInstance(string webPartsXml)
        {
            mWebPartsXml = webPartsXml;
        }


        /// <summary>
        /// Loads the data from the given XML element
        /// </summary>
        /// <param name="element">XML element</param>
        public void LoadFromXmlElement(XmlElement element)
        {
            var zones = new List<WebPartZoneInstance>();

            if (element != null)
            {
                // Get the zones from configuration XML
                var zoneNodes = element.SelectNodes("webpartzone");
                if (zoneNodes != null)
                {
                    // Add the zones
                    foreach (XmlNode zoneNode in zoneNodes)
                    {
                        var newZone = new WebPartZoneInstance(zoneNode);
                        newZone.ParentTemplateInstance = this;

                        zones.Add(newZone);
                    }
                }
            }

            // Use complete list
            mWebPartZones = zones;
        }


        /// <summary>
        /// Reloads the zones and webparts info.
        /// </summary>
        /// <param name="webPartsXml">Web parts XML</param>
        public void LoadFromXml(string webPartsXml)
        {
            // Ensure reloading of the MVT/CP variants
            MVTVariantsLoaded = false;
            ContentPersonalizationVariantsLoaded = false;

            // If no data given, exit
            if (!String.IsNullOrEmpty(webPartsXml))
            {
                // Load the document
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(webPartsXml);

                LoadFromXmlElement(xml.DocumentElement);
            }
            else
            {
                // Empty list of zones if context wasn't loaded
                mWebPartZones = new List<WebPartZoneInstance>();
            }
        }


        /// <summary>
        /// Loads all the MVT/Content personalization variants for this template instance.
        /// </summary>
        /// <param name="forceLoad">Indicates if already loaded variants should be reloaded</param>
        /// <param name="variantMode">Specifies which variants should be loaded (MVT/ContentPersonalization/None - means both MVT+CP variants should try to load)</param>
        public void LoadVariants(bool forceLoad, VariantModeEnum variantMode)
        {
            foreach (WebPartZoneInstance zoneInstance in WebPartZones)
            {
                // Reset the variant mode if both MVT/CP variants should be loaded
                if (variantMode == VariantModeEnum.None)
                {
                    zoneInstance.VariantMode = VariantModeEnum.None;
                }

                // Reload all variants
                zoneInstance.LoadVariants(forceLoad, variantMode);
            }
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>
        /// <param name="doc">Parent XML document</param>
        public XmlElement GetXmlElement(XmlDocument doc)
        {
            return GetXmlElement(doc, WidgetZoneTypeEnum.All);
        }


        /// <summary>
        /// Gets the XML element for the data
        /// </summary>
        /// <param name="doc">Parent XML document</param>
        /// <param name="zoneType">Zone type to output</param>
        public XmlElement GetXmlElement(XmlDocument doc, WidgetZoneTypeEnum zoneType)
        {
            var docElem = doc.CreateElement("page");

            // Add the zones
            foreach (WebPartZoneInstance zone in WebPartZones)
            {
                // Add zone node only when all required or if the type matches
                if ((zoneType == WidgetZoneTypeEnum.All) || (zone.WidgetZoneType == zoneType))
                {
                    var zoneElem = zone.GetXmlNode(doc);

                    docElem.AppendChild(zoneElem);
                }
            }

            return docElem;
        }


        /// <summary>
        /// Returns the XML representation of the zones configuration.
        /// </summary>
        /// <param name="zoneType">Zone type to get</param>
        public string GetZonesXML(WidgetZoneTypeEnum zoneType = WidgetZoneTypeEnum.All)
        {
            if (WebPartZones == null)
            {
                return null;
            }

            // Create an output document
            var xml = new XmlDocument();
            var docElem = GetXmlElement(xml, zoneType);

            xml.AppendChild(docElem);

            return xml.InnerXml;
        }


        /// <summary>
        /// Returns true if the page template contains zones of the specific type.
        /// </summary>
        /// <param name="zoneType">Zone type</param>
        public bool ContainsZones(WidgetZoneTypeEnum zoneType)
        {
            // Any zone for all
            if ((zoneType == WidgetZoneTypeEnum.All) && (WebPartZones.Count > 0))
            {
                return true;
            }

            // Try to find zone of the specific type
            foreach (WebPartZoneInstance zone in WebPartZones)
            {
                if (zone.WidgetZoneType == zoneType)
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Combines two page template instances, applies the other page template instance settings (only those matching the required zone type in the current instance) to the current instance. Does not add new zones which are not present in the current instance.
        /// </summary>
        /// <param name="pti">Page template instance with the personalization</param>
        /// <param name="zoneType">Zone type to overwrite</param>
        /// <param name="keepZoneProperties">If true, the original zone properties are kept</param>
        public void CombineWith(PageTemplateInstance pti, WidgetZoneTypeEnum zoneType, bool keepZoneProperties = true)
        {
            if (pti == null)
            {
                return;
            }

            List<WebPartZoneInstance> newZones = new List<WebPartZoneInstance>();

            // Go through all the zones
            foreach (WebPartZoneInstance zone in WebPartZones)
            {
                bool newZoneAdded = false;

                // Customize zone only when all required or if the type matches
                if ((zoneType == WidgetZoneTypeEnum.All) || (zone.WidgetZoneType == zoneType))
                {
                    // Try to find the match in the other page template (only same type of the zone)
                    WebPartZoneInstance newZone = pti.GetZone(zone.ZoneID);
                    if ((newZone != null) && ((zoneType == WidgetZoneTypeEnum.All) || (newZone.WidgetZoneType == zoneType)))
                    {
                        // Copy over the original zone properties
                        if (keepZoneProperties)
                        {
                            newZone.LoadProperties(zone);
                        }

                        newZones.Add(newZone);
                        newZoneAdded = true;
                    }
                }

                // If not overwritten, add current zone
                if (!newZoneAdded)
                {
                    newZones.Add(zone);
                }
            }

            // Load the document widget layout zones
            switch (zoneType)
            {
                case WidgetZoneTypeEnum.Dashboard:
                case WidgetZoneTypeEnum.Editor:
                case WidgetZoneTypeEnum.Group:
                    {
                        // Go through all the zones
                        foreach (WebPartZoneInstance documentZone in pti.WebPartZones)
                        {
                            // Load only the widget editor layout zones
                            if ((documentZone.WidgetZoneType == zoneType) && documentZone.LayoutZone)
                            {
                                // Try to find the match in the page template
                                WebPartZoneInstance templateZone = GetZone(documentZone.ZoneID);

                                // If templateZone was not found, then copy the documentZone to the template
                                if (templateZone == null)
                                {
                                    newZones.Add(documentZone);
                                }
                                else if ((templateZone.WidgetZoneType == zoneType) || templateZone.LayoutZone)
                                {
                                    // If the template already contains the documentZone, replace it.
                                    int templateIndex = newZones.IndexOf(documentZone);
                                    if (templateIndex >= 0)
                                    {
                                        newZones[templateIndex] = documentZone;
                                    }
                                    else
                                    {
                                        newZones.Add(documentZone);
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            mWebPartZones = newZones;
        }


        /// <summary>
        /// Combines the page template instance with a web part instance, applies the web part instance settings to the current page template instance.
        /// </summary>
        /// <param name="wpi">The wpi</param>
        public void CombineWith(WebPartInstance wpi)
        {
            WebPartInstance originalWpi = GetWebPart(wpi.InstanceGUID);
            if (originalWpi != null)
            {
                originalWpi.LoadProperties(wpi);
                if (!originalWpi.IsWidget || PortalContext.ViewMode.IsLiveSite())
                {
                    originalWpi.ControlID = wpi.ControlID;
                }
                originalWpi.CurrentVariantInstance = wpi;
            }
        }


        /// <summary>
        /// Combines the page template instance with a web part zone instance, applies the web part zone instance settings to the current page template instance.
        /// </summary>
        /// <param name="wpzi">Web part zone instance</param>
        public void CombineWith(WebPartZoneInstance wpzi)
        {
            WebPartZoneInstance originalWpzi = GetZone(wpzi.ZoneID);

            if (originalWpzi != null)
            {
                originalWpzi.LoadProperties(wpzi);
                originalWpzi.WebParts = wpzi.WebParts;
            }
        }


        /// <summary>
        /// Clones the instance of page templates.
        /// </summary>
        /// <param name="copyMacroTable">Indicates whether macro values should be cloned</param>
        public PageTemplateInstance Clone(bool copyMacroTable = false)
        {
            // Create new instance object
            var templateInstanceClone = new PageTemplateInstance(mWebPartsXml);

            templateInstanceClone.ParentPageTemplate = ParentPageTemplate;
            templateInstanceClone.MVTVariantsLoaded = MVTVariantsLoaded;
            templateInstanceClone.ContentPersonalizationVariantsLoaded = ContentPersonalizationVariantsLoaded;

            // Clone the zones with a deep clone
            templateInstanceClone.mWebPartZones = new List<WebPartZoneInstance>();
            foreach (WebPartZoneInstance zone in WebPartZones)
            {
                // Clone the zone object
                WebPartZoneInstance newZone = zone.Clone(true, copyMacroTable);

                // Parent template instance of zone and zone variants needs to be updated to reference to the new (cloned) PageTemplateInstance.
                UpdateParentTemplateInstance(newZone, templateInstanceClone);

                templateInstanceClone.mWebPartZones.Add(newZone);
            }

            return templateInstanceClone;
        }


        /// <summary>
        /// Sets <see cref="WebPartZoneInstance.ParentTemplateInstance"/> property of given <paramref name="zoneInstance"/> and all its zone variants to specified <paramref name="templateInstance"/>.
        /// </summary>
        /// <param name="zoneInstance">Web part zone instance to be updated</param>
        /// <param name="templateInstance">Target page template instance</param>
        private static void UpdateParentTemplateInstance(WebPartZoneInstance zoneInstance, PageTemplateInstance templateInstance)
        {
            if (zoneInstance == null)
            {
                return;
            }

            zoneInstance.ParentTemplateInstance = templateInstance;

            if (zoneInstance.ZoneInstanceVariants != null)
            {
                zoneInstance.ZoneInstanceVariants.ForEach(variant => variant.ParentTemplateInstance = templateInstance);
            }
        }

        #endregion


        #region "Zones and web parts methods"

        /// <summary>
        /// Returns the zone instance with specified ID or null when not found.
        /// </summary>
        /// <param name="zoneId">Zone ID to find</param>
        /// <param name="zoneVariantId">The zone variant id</param>
        public WebPartZoneInstance GetZone(string zoneId, int zoneVariantId = 0)
        {
            zoneId = zoneId.ToLowerCSafe();
            foreach (WebPartZoneInstance zone in WebPartZones)
            {
                if (zone.ZoneID.ToLowerCSafe() == zoneId)
                {
                    if (zoneVariantId > 0)
                    {
                        if (zone.ZoneInstanceVariants != null)
                        {
                            return zone.ZoneInstanceVariants.Find(z => z.VariantID.Equals(zoneVariantId));
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return zone;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Ensures that the instance of the given zone exists.
        /// </summary>
        /// <param name="zoneId">Zone ID to ensure</param>
        public WebPartZoneInstance EnsureZone(string zoneId)
        {
            WebPartZoneInstance zone = GetZone(zoneId);
            // If zone instance not exist, add a new one
            if (zone == null)
            {
                zone = new WebPartZoneInstance();
                zone.ZoneID = zoneId;
                zone.ParentTemplateInstance = this;
                WebPartZones.Add(zone);
            }

            return zone;
        }


        /// <summary>
        /// Adds the given zone to the template
        /// </summary>
        /// <param name="zone">Zone to add</param>
        /// <remarks>Returns a new instance of web part zone if the given zone is present within it's parent zone</remarks>
        public WebPartZoneInstance AddZone(WebPartZoneInstance zone)
        {
            if (ZoneExists(zone.ZoneID))
            {
                throw new Exception("[PageTemplateInstance.AddZone]: Cannot add web part zone. Zone with ID '" + zone.ZoneID + "' already exists in this page template.");
            }

            // If present in it's original zone, create a copy
            if ((zone.ParentTemplateInstance != null) && (zone.ParentTemplateInstance.GetZone(zone.ZoneID) != null))
            {
                zone = zone.Clone(false);
            }

            zone.ParentTemplateInstance = this;

            WebPartZones.Add(zone);

            return zone;
        }


        /// <summary>
        /// Returns true if zone with given ID already exists
        /// </summary>
        /// <param name="zoneId">Zone ID to check</param>
        public bool ZoneExists(string zoneId)
        {
            return (GetZone(zoneId) != null);
        }


        /// <summary>
        /// Adds the web part instance to the specified zone.
        /// </summary>
        /// <param name="zoneId">Zone ID</param>
        /// <param name="webPart">Web part</param>
        /// <param name="position">Position index</param>
        /// <remarks>Returns a new instance of web part if the given web part is present within it's parent zone</remarks>
        public WebPartInstance AddWebPart(string zoneId, WebPartInstance webPart, int position = -1)
        {
            WebPartZoneInstance zone = EnsureZone(zoneId);

            return zone.AddWebPart(webPart, position);
        }


        /// <summary>
        /// Adds the web part instance to the specified zone
        /// </summary>
        /// <param name="zoneId">Zone ID</param>
        /// <param name="webPartId">Web part ID</param>
        /// <param name="position">The position of the web part in the target zone</param>
        public WebPartInstance AddWebPart(string zoneId, int webPartId, int position = -1)
        {
            WebPartZoneInstance zone = EnsureZone(zoneId);
            return zone.AddWebPart(webPartId, position);
        }


        /// <summary>
        /// Adds the widget instance to the specified zone.
        /// </summary>
        /// <param name="zoneId">Zone ID</param>
        /// <param name="widgetId">Web part ID</param>
        public WebPartInstance AddWidget(string zoneId, int widgetId)
        {
            WebPartZoneInstance zone = EnsureZone(zoneId);
            return zone.AddWidget(widgetId);
        }


        /// <summary>
        /// Removes zone from template instance
        /// </summary>
        /// <param name="zone">Zone instance to remove</param>
        public void RemoveZone(WebPartZoneInstance zone)
        {
            zone.RemoveAllWebParts();
            WebPartZones.Remove(zone);
        }


        /// <summary>
        /// Removes the nested zones for the given web part
        /// </summary>
        /// <param name="webPart">Web part for which the nested zones will be removed</param>
        internal void RemoveNestedZones(WebPartInstance webPart)
        {
            string zoneId = webPart.ControlID + "_";
            var deleteZones = GetNestedZones(zoneId);

            // Delete underlying web part zones
            foreach (WebPartZoneInstance zone in deleteZones)
            {
                RemoveZone(zone);
            }
        }


        /// <summary>
        /// Gets the list of zones nested in the given web part
        /// </summary>
        /// <param name="webPartId">Web part ID</param>
        public IEnumerable<WebPartZoneInstance> GetNestedZones(string webPartId)
        {
            var zones = new List<WebPartZoneInstance>();

            // Find underlying layout zones
            foreach (WebPartZoneInstance zone in WebPartZones)
            {
                if (zone.LayoutZone && zone.ZoneID.StartsWithCSafe(webPartId, true))
                {
                    zones.Add(zone);
                }
            }

            return zones;
        }


        /// <summary>
        /// Moves the web part between two zones
        /// </summary>
        /// <param name="webPart">Web part to move</param>
        /// <param name="sourceZoneInstance">Source zone</param>
        /// <param name="targetZoneInstance">Target zone instance</param>
        /// <param name="targetPosition">Target position</param>
        /// <param name="targetZoneId">Target zone ID</param>
        public void MoveWebPart(WebPartInstance webPart, WebPartZoneInstance sourceZoneInstance, WebPartZoneInstance targetZoneInstance, string targetZoneId, int targetPosition)
        {
            // Check if the web part can be moved
            if (!CanMoveWebPart(webPart, targetZoneInstance))
            {
                CannotMoveWebPart(webPart);
            }

            if (webPart != null)
            {
                // Remove webpart
                if (sourceZoneInstance.WebParts.Contains(webPart))
                {
                    sourceZoneInstance.WebParts.Remove(webPart);
                }

                targetZoneInstance.AddWebPart(webPart, targetPosition);
            }
        }


        /// <summary>
        /// Throws the exception that the web part cannot be moved to it's own nested zone
        /// </summary>
        /// <param name="wpi">Web part instance</param>
        /// <exception cref="InvalidOperationException">Thrown in all cases</exception>
        internal void CannotMoveWebPart(WebPartInstance wpi)
        {
            throw new InvalidOperationException(String.Format(ResHelper.GetString("Design.CannotMoveWebPart"), wpi.ControlID));
        }


        /// <summary>
        /// Moves all web parts from one zone to another
        /// </summary>
        /// <param name="zone">Source zone</param>
        /// <param name="targetZone">Target zone</param>
        public void MoveAllWebParts(WebPartZoneInstance zone, WebPartZoneInstance targetZone)
        {
            var webParts = new List<WebPartInstance>(zone.WebParts);

            // Check if the web parts can be moved
            foreach (WebPartInstance webPart in webParts)
            {
                if (!CanMoveWebPart(webPart, targetZone))
                {
                    CannotMoveWebPart(webPart);
                }
            }

            // Move all web parts
            foreach (WebPartInstance webPart in webParts)
            {
                // Remove webpart
                if (zone.WebParts.Contains(webPart))
                {
                    zone.WebParts.Remove(webPart);
                }

                // Add to target zone
                targetZone.AddWebPart(webPart);
            }
        }


        /// <summary>
        /// Moves web part up within its zone.
        /// </summary>
        /// <param name="webPart">Web part</param>
        /// <param name="top">If true, the web part is moved to the top</param>
        public void MoveWebPartUp(WebPartInstance webPart, bool top)
        {
            if ((webPart != null) && (webPart.ParentZone != null))
            {
                webPart.ParentZone.MoveWebPartUp(webPart.ControlID, top);
            }
        }


        /// <summary>
        /// Moves web part up within the instance of the specified zone.
        /// </summary>
        /// <param name="zoneId">Zone ID</param>
        /// <param name="webPartControlId">Web part ID</param>
        /// <param name="top">If true, the web part is moved to the bottom</param>
        public void MoveWebPartUp(string zoneId, string webPartControlId, bool top)
        {
            WebPartZoneInstance zone = EnsureZone(zoneId);
            zone.MoveWebPartUp(webPartControlId, top);
        }


        /// <summary>
        /// Moves web part down within its zone.
        /// </summary>
        /// <param name="webPart">Web part</param>
        /// <param name="bottom">If true, the web part is moved to the bottom</param>
        public void MoveWebPartDown(WebPartInstance webPart, bool bottom)
        {
            if ((webPart != null) && (webPart.ParentZone != null))
            {
                webPart.ParentZone.MoveWebPartDown(webPart.ControlID, bottom);
            }
        }


        /// <summary>
        /// Moves web part down within the instance of the specified zone.
        /// </summary>
        /// <param name="zoneId">Zone ID</param>
        /// <param name="webPartControlId">Web part ID</param>
        /// <param name="bottom">If true, the web part is moved to the bottom</param>
        public void MoveWebPartDown(string zoneId, string webPartControlId, bool bottom)
        {
            WebPartZoneInstance zone = EnsureZone(zoneId);
            zone.MoveWebPartDown(webPartControlId, bottom);
        }


        /// <summary>
        /// Moves the web part to the previous zone.
        /// </summary>
        /// <param name="webPart">Web part</param>
        public void MoveWebPartToPreviousZone(WebPartInstance webPart)
        {
            if ((webPart != null) && (webPart.ParentZone != null))
            {
                MoveWebPartToPreviousZone(webPart.ParentZone.ZoneID, webPart.ControlID);
            }
        }


        /// <summary>
        /// Moves the web part to the previous zone.
        /// </summary>
        /// <param name="zoneId">Current web part zone ID</param>
        /// <param name="webPartControlId">Web part control ID</param>
        public void MoveWebPartToPreviousZone(string zoneId, string webPartControlId)
        {
            // If 1 or less zones, exit
            if (WebPartZones.Count <= 1)
            {
                return;
            }
            // Get the zone (source)
            WebPartZoneInstance zone = EnsureZone(zoneId);
            int index = WebPartZones.IndexOf(zone);
            // Get the new zone (target)
            WebPartZoneInstance newZone = null;
            if (index > 0)
            {
                newZone = WebPartZones[index - 1];
            }
            else if (index == 0)
            {
                newZone = WebPartZones[WebPartZones.Count - 1];
            }

            // Move the web part instance between the zones
            if (newZone != null)
            {
                WebPartInstance part = zone.GetWebPart(webPartControlId);
                if (part != null)
                {
                    zone.RemoveWebPart(webPartControlId);
                    newZone.WebParts.Add(part);
                }
            }
        }


        /// <summary>
        /// Moves the web part to the Next zone.
        /// </summary>
        /// <param name="webPart">Web part</param>
        public void MoveWebPartToNextZone(WebPartInstance webPart)
        {
            if ((webPart != null) && (webPart.ParentZone != null))
            {
                MoveWebPartToNextZone(webPart.ParentZone.ZoneID, webPart.ControlID);
            }
        }


        /// <summary>
        /// Moves the web part to the next zone.
        /// </summary>
        /// <param name="zoneId">Current web part zone ID</param>
        /// <param name="webPartControlId">Web part control ID</param>
        public void MoveWebPartToNextZone(string zoneId, string webPartControlId)
        {
            // If 1 or less zones, exit
            if (WebPartZones.Count <= 1)
            {
                return;
            }
            // Get the zone (source)
            WebPartZoneInstance zone = EnsureZone(zoneId);
            int index = WebPartZones.IndexOf(zone);
            // Get the new zone (target)
            WebPartZoneInstance newZone = null;
            if (index < WebPartZones.Count - 1)
            {
                newZone = WebPartZones[index + 1];
            }
            else if (index == WebPartZones.Count - 1)
            {
                newZone = WebPartZones[0];
            }

            // Move the web part instance between the zones
            if (newZone != null)
            {
                WebPartInstance part = zone.GetWebPart(webPartControlId);
                if (part != null)
                {
                    zone.RemoveWebPart(webPartControlId);
                    newZone.WebParts.Add(part);
                }
            }
        }


        /// <summary>
        /// Clones the web part.
        /// </summary>
        /// <param name="webPart">Web part</param>
        public WebPartInstance CloneWebPart(WebPartInstance webPart)
        {
            if ((webPart != null) && (webPart.ParentZone != null))
            {
                return webPart.ParentZone.CloneWebPart(webPart.ControlID);
            }

            return null;
        }


        /// <summary>
        /// Returns the web part with specified ID or null if not found.
        /// </summary>
        /// <param name="webPartControlId">Web part control ID to retrieve</param>
        /// <param name="searchInVariants">Indicates whether to search in the zone variants as well.</param>
        public WebPartInstance GetWebPart(string webPartControlId, bool searchInVariants = false)
        {
            // Look in every zone
            foreach (WebPartZoneInstance zone in WebPartZones)
            {
                WebPartInstance part = zone.GetWebPart(webPartControlId, searchInVariants);
                if (part != null)
                {
                    return part;
                }
            }
            return null;
        }


        /// <summary>
        /// Returns the web part with specified GUID or ID or null if not found.
        /// </summary>
        /// <param name="webPartGuid">Web part GUID</param>
        /// <param name="webPartControlId">Web part control ID to retrieve</param>
        public WebPartInstance GetWebPart(Guid webPartGuid, string webPartControlId)
        {
            return GetWebPart(webPartGuid) ?? GetWebPart(webPartControlId);
        }


        /// <summary>
        /// Finds the web part by its GUID.
        /// </summary>
        /// <param name="instanceGuid">Web part instance GUID</param>
        /// <param name="zoneVariantId">The zone variant id.
        /// 1..: search just in the specific zone variant
        /// 0: search only in the original zones
        /// -1: search in all zones and their variants</param>
        /// <param name="webPartVariantId">The web part variant id</param>
        public WebPartInstance GetWebPart(Guid instanceGuid, int zoneVariantId = 0, int webPartVariantId = 0)
        {
            if (instanceGuid == Guid.Empty)
            {
                return null;
            }

            // Look in every zone
            foreach (WebPartZoneInstance zone in WebPartZones)
            {
                // Find in the zone variants
                WebPartInstance webPart;
                if ((zoneVariantId != 0) && (zone.ZoneInstanceVariants != null))
                {
                    // Try to find in a specific zone variant
                    if (zoneVariantId > 0)
                    {
                        WebPartZoneInstance zoneVariant = zone.ZoneInstanceVariants.Find(z => z.VariantID.Equals(zoneVariantId));
                        if (zoneVariant != null)
                        {
                            // Return the web part
                            return zoneVariant.GetWebPart(instanceGuid);
                        }
                    }
                    // Find in all zone variants (zoneVariantId == -1)
                    else
                    {
                        foreach (WebPartZoneInstance zoneVariant in zone.ZoneInstanceVariants)
                        {
                            // Try to find web part in the zone variant
                            webPart = zoneVariant.GetWebPart(instanceGuid);
                            if (webPart != null)
                            {
                                return webPart;
                            }
                        }
                    }
                }
                else
                {
                    // Get the web part from the original zone
                    webPart = zone.GetWebPart(instanceGuid);

                    if (webPart != null)
                    {
                        // Try to find a specific web part variant
                        if (webPartVariantId > 0)
                        {
                            if (webPart.PartInstanceVariants != null)
                            {
                                // Return the web part variant
                                return webPart.PartInstanceVariants.Find(w => w.VariantID.Equals(webPartVariantId));
                            }
                            else
                            {
                                // Variant not found -> return null
                                return null;
                            }
                        }
                        // Return the web part
                        else
                        {
                            return webPart;
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Returns unique web part ID within this Page template.
        /// </summary>
        /// <param name="baseId">Base web part id (required)</param>
        /// <param name="counterOffset">The counter number which will be used as a starter counter for checking the unique id. Example: counterOffset=2, returns ids starting with number two ('wp_2', 'wp_3', 'wp_4'...)</param>
        public string GetUniqueWebPartId(string baseId, int counterOffset = 0)
        {
            string newId = ControlIDReplaceRegex.Replace(baseId, "_");
            int index = 1;
            bool found;

            baseId = TextHelper.TrimNumberSuffix(newId);

            // Set the web part control id offset
            if (counterOffset > 0)
            {
                newId = baseId + (index + counterOffset).ToString();
                index += counterOffset;
            }

            // Get the unique ID
            do
            {
                found = false;

                // Check the zones
                foreach (WebPartZoneInstance zone in WebPartZones)
                {
                    if (zone.GetWebPart(newId, true) != null)
                    {
                        found = true;
                        newId = baseId + index.ToString();
                        index += 1;
                        break;
                    }
                    else
                    {
                        if (zone.ZoneInstanceVariants != null)
                        {
                            // Check the zone variants
                            foreach (WebPartZoneInstance zoneVariant in zone.ZoneInstanceVariants)
                            {
                                if (zoneVariant.GetWebPart(newId) != null)
                                {
                                    found = true;
                                    newId = baseId + index.ToString();
                                    index += 1;
                                    break;
                                }
                            }
                        }
                    }
                }
            } while (found);

            return newId;
        }


        /// <summary>
        /// Returns true if web part can be moved
        /// </summary>
        /// <param name="webPart">Web part to move</param>
        /// <param name="targetZoneInstance">Target zone instance</param>
        internal bool CanMoveWebPart(WebPartInstance webPart, WebPartZoneInstance targetZoneInstance)
        {
            // If target zone is a layout zone, check if the web part is it's potential parent
            var zone = targetZoneInstance;

            while ((zone != null) && zone.LayoutZone)
            {
                string zoneId = zone.ZoneID;

                // Get the parent web part
                int sepIndex = zoneId.LastIndexOf("_", StringComparison.Ordinal);
                if (sepIndex >= 0)
                {
                    string webPartId = zoneId.Substring(0, sepIndex);

                    var parentWebPart = GetWebPart(webPartId);
                    if (parentWebPart == null)
                    {
                        break;
                    }

                    // If the web part is it's own parent, do not allow to move the web part
                    if (webPartId.EqualsCSafe(webPart.ControlID))
                    {
                        return false;
                    }

                    // Otherwise continue to web part parent zone
                    zone = parentWebPart.ParentZone;
                }
                else
                {
                    zone = null;
                }
            }

            return true;
        }

        #endregion
    }
}
