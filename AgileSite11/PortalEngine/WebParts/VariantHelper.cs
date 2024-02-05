using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

using CMS.DataEngine;
using CMS.Helpers;


namespace CMS.PortalEngine
{
    /// <summary>
    /// Helper class for portal engine variants
    /// </summary>
    public static class VariantHelper
    {
        /// <summary>
        /// Returns variants for given type and documents or template
        /// </summary>
        /// <param name="type">Variant type</param>
        /// <param name="templateID">Template info ID</param>        
        /// <param name="zoneID">Zone instance ID</param>
        /// <param name="instanceGuid">Instance's Guid</param>
        /// <param name="documentID">Document ID </param>
        public static DataSet GetVariants(VariantModeEnum type, int templateID, String zoneID, Guid instanceGuid, int documentID)
        {
            GetVariantsEventArgs args = new GetVariantsEventArgs()
            {
                DocumentID = documentID,
                InstanceGuid = instanceGuid,
                PageTemplateID = templateID,
                VariantType = type,
                ZoneID = zoneID
            };

            PortalEngineEvents.GetVariants.StartEvent(args);

            return args.Variants;
        }


        /// <summary>
        /// Saves the variants
        /// </summary>
        /// <param name="type">Variant type</param>
        /// <param name="variantID">Varaint's (zone or webpart) ID</param>
        /// <param name="xmlWebParts">XML (webpart or zone) definition</param>
        public static void SetVariantWebParts(VariantModeEnum type, int variantID, XmlNode xmlWebParts)
        {
            SetVariantEventArgs args = new SetVariantEventArgs()
            {
                XmlDefinition = xmlWebParts,
                VariantID = variantID,
                VariantType = type
            };

            PortalEngineEvents.SetVariantWebParts.StartEvent(args);
        }


        /// <summary>
        /// Saves the variant.
        /// </summary>
        /// <param name="zone">The zone instance</param>
        /// <param name="variantId">The variant id</param>
        public static void SetVariantWebParts(WebPartZoneInstance zone, int variantId)
        {
            if (zone != null)
            {
                SetVariantWebParts(zone.VariantMode, variantId, zone.GetXmlNode());
            }
        }


        /// <summary>
        /// Saves variant's properties
        /// </summary>
        /// <param name="type">Variant type</param>
        /// <param name="variant">Settings object with variant information</param>
        /// <param name="xmlDefinition">XML node (webpart or zone) definition</param>
        public static int SetVariant(VariantModeEnum type, VariantSettings variant, XmlNode xmlDefinition)
        {
            SetVariantEventArgs args = new SetVariantEventArgs()
            {
                VariantType = type,
                Variant = variant,
                XmlDefinition = xmlDefinition
            };

            PortalEngineEvents.SetVariant.StartEvent(args);

            return args.Variant.ID;
        }


        /// <summary>
        /// Saves the web part variant changes (MVT/Content personalization variants).
        /// </summary>
        /// <param name="webPartInstance">The web part instance</param>
        /// <param name="variantId">The variant id</param>
        /// <param name="zoneVariantId">The zone variant id</param>
        /// <param name="variantMode">The variant mode</param>
        /// <param name="variantProperties">The variant properties</param>
        public static void SaveWebPartVariantChanges(WebPartInstance webPartInstance, int variantId, int zoneVariantId, VariantModeEnum variantMode, Hashtable variantProperties)
        {
            // Save the variant properties
            if ((webPartInstance != null)
                && (webPartInstance.ParentZone != null)
                && (webPartInstance.ParentZone.ParentTemplateInstance != null)
                && (webPartInstance.ParentZone.ParentTemplateInstance.ParentPageTemplate != null)
                && (webPartInstance.ParentZone.ParentTemplateInstance.ParentPageTemplate.ParentPageInfo != null))
            {
                XmlNode xmlWebParts;

                if (zoneVariantId > 0)
                {
                    // This webpart is in a zone variant therefore save the whole variant web parts
                    xmlWebParts = webPartInstance.ParentZone.GetXmlNode();
                    SetVariantWebParts(variantMode, zoneVariantId, xmlWebParts);
                }
                // Web part/widget variant has to have its properties hashtable defined
                else if (((variantId > 0) || (variantProperties != null))
                    && (!webPartInstance.ParentZone.HasVariants)) // Save only if the parent zone does not have any variants
                {
                    xmlWebParts = webPartInstance.GetXmlNode();

                    // Save the variant properties                        
                    VariantSettings variant = new VariantSettings()
                    {
                        ZoneID = webPartInstance.ParentZone.ZoneID,
                        PageTemplateID = webPartInstance.ParentZone.ParentTemplateInstance.ParentPageTemplate.PageTemplateId,
                        DocumentID = (webPartInstance.IsWidget ? webPartInstance.ParentZone.ParentTemplateInstance.ParentPageTemplate.ParentPageInfo.DocumentID : 0),
                        ID = variantId,
                        InstanceGuid = webPartInstance.InstanceGUID
                    };

                    // New variant
                    if (variantProperties != null)
                    {
                        variant.Name = ValidationHelper.GetString(variantProperties["codename"], string.Empty);
                        variant.DisplayName = ValidationHelper.GetString(variantProperties["displayname"], string.Empty);
                        variant.Description = ValidationHelper.GetString(variantProperties["description"], string.Empty);
                        variant.Enabled = ValidationHelper.GetBoolean(variantProperties["enabled"], true);

                        // Content personalization display condition
                        if (variantMode == VariantModeEnum.ContentPersonalization)
                        {
                            variant.Condition = ValidationHelper.GetString(variantProperties["condition"], string.Empty);
                        }
                    }

                    SetVariant(variantMode, variant, xmlWebParts);

                    if ((variantMode == VariantModeEnum.MVT) && (variantProperties != null))
                    {
                        // Update the session with the variant codename resolved
                        variantProperties["codename"] = ProviderHelper.GetInfoById(PredefinedObjectType.MVTVARIANT, variant.ID).Generalized.ObjectCodeName;
                    }

                    // The variants are cached -> Reload
                    webPartInstance.ParentZone.ParentTemplateInstance.LoadVariants(true, VariantModeEnum.None);
                }
            }
        }


        /// <summary>
        /// Removes all widget variants and their combinations.
        /// </summary>
        /// <param name="zoneID">The zone id</param>
        /// <param name="pageTemplateID">The page template id</param>
        /// <param name="documentID">The document id</param>
        public static void DeleteWidgetVariants(String zoneID, int pageTemplateID, int documentID)
        {
            DeleteVariantEventArgs args = new DeleteVariantEventArgs()
            {
                ZoneID = zoneID,
                PageTemplateID = pageTemplateID,
                DocumentID = documentID
            };

            PortalEngineEvents.DeleteWidgetVariants.StartEvent(args);
        }


        /// <summary>
        /// Return variant ID based on name and page template ID
        /// </summary>
        /// <param name="type">Type of variant</param>
        /// <param name="pageTemplateID">Variant's page template ID</param>
        /// <param name="variantName">Variant's name</param>
        /// <param name="documentIdIsNull">If set, adds check to SQL query whether the variant document id is null or not</param>
        public static int GetVariantID(VariantModeEnum type, int pageTemplateID, string variantName, bool? documentIdIsNull = null)
        {
            GetVariantEventArgs args = new GetVariantEventArgs()
            {
                VariantName = variantName,
                PageTemplateID = pageTemplateID,
                VariantType = type,
                DocumentIdIsNull = documentIdIsNull
            };

            PortalEngineEvents.GetVariant.StartEvent(args);

            return args.VariantID;
        }
    }
}
