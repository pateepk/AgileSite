using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.WebAnalytics;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// MV testing event handlers.
    /// </summary>
    internal class MVTHandlers
    {
        #region "Constants"

        /// <summary>
        /// Cookie prefix.
        /// </summary>
        private const string MVTCOOKIE_PREFIX = "CMSMVT";


        /// <summary>
        /// Conversion name.
        /// </summary>
        private const string MVTCONVERSION = "mvtconversion";


        /// <summary>
        /// Used as a query parameter when logging analytics via JavaScript. 
        /// </summary>
        private const string PARAM_MVT_COMBINATION_NAME = "MVTestCombinationName";

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            WebAnalyticsEvents.LogConversion.Before += LogMVTConversion;
            WebAnalyticsEvents.ProcessLogRecord.Before += HandleMVTestConversion;

            WebPartEvents.WebPartLoadVariant.Execute += LoadVariants;
            WebPartEvents.WebPartZoneLoadVariant.Execute += LoadZoneVariants;
            WebPartEvents.MoveAllWebParts.Before += MoveAllWebParts;
            WebPartEvents.MoveWebPart.After += MoveWebPart;
            WebPartEvents.RemoveWebPart.Before += RemoveWebPart;
            WebPartEvents.RemoveAllWebParts.Before += RemoveAllWebParts;
            WebPartEvents.ChangeLayoutZoneId.Execute += ChangeLayoutZoneId;

            PortalEngineEvents.GetVariants.Execute += GetVariants;
            PortalEngineEvents.GetVariant.Execute += GetVariant;
            PortalEngineEvents.SetVariantWebParts.Execute += SetVariantWebParts;
            PortalEngineEvents.SetVariant.Execute += SetVariant;
            PortalEngineEvents.DeleteWidgetVariants.Execute += DeleteWidgetVariants;

            PageTemplateInfo.TYPEINFO.Events.Delete.After += PageTemplateDelete;

            DocumentEvents.Move.Before += MoveMVTests;

            WebAnalyticsEvents.InsertAnalyticsJS.After += InsertMVTestJavaScriptWebServiceParameters;
        }


        /// <summary>
        /// Inserts additional query parameters to the web analytics Javascript snippet.
        /// </summary>
        private static void InsertMVTestJavaScriptWebServiceParameters(object sender, AnalyticsJSEventArgs e)
        {
            var combinationName = MVTContext.CurrentMVTCombinationName;
            Dictionary<string, string> queryParams = e.QueryParameters;

            if (string.IsNullOrEmpty(combinationName) || (queryParams == null))
            {
                return;
            }

            queryParams.Add(PARAM_MVT_COMBINATION_NAME, combinationName);
        }


        /// <summary>
        /// Moves MVT tests together with a document
        /// </summary>
        private static void MoveMVTests(object sender, DocumentEventArgs e)
        {
            var originalNode = e.Node;
            var originalAliasPath = originalNode.NodeAliasPath;
            var originalParentId = originalNode.NodeParentID;

            e.CallWhenFinished(() =>
            {
                var node = e.Node;
                var parentChanged = originalParentId != node.NodeParentID;
                if (parentChanged)
                {
                    // Move tests if parent changed
                    MVTestInfoProvider.MoveMVTests(node.NodeAliasPath, originalAliasPath, node.NodeSiteID);
                }
            });
        }


        /// <summary>
        /// Logs AB and MVT conversions.
        /// </summary>
        private static void LogMVTConversion(object sender, CMSEventArgs<LogRecord> processLogRecordEventArgs)
        {
            var logRecord = processLogRecordEventArgs.Parameter;

            // Check whether the context is available so we can use cookies
            if (HttpContext.Current != null)
            {
                string siteName = logRecord.SiteName;
                string culture = logRecord.Culture;
                string objectName = logRecord.ObjectName;
                int objectId = logRecord.ObjectId;
                int count = logRecord.Hits;
                double value = logRecord.Value;

                // Check whether MV testing is enabled and try log MV testing conversion
                if (MVTestInfoProvider.MVTestingEnabled(siteName))
                {
                    LogMVTestConversion(siteName, culture, objectName, objectId, count, value);
                }
            }
        }


        /// <summary>
        /// Checks cookies and logs MVT conversions.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="culture">Culture</param>
        /// <param name="conversionName">Conversion name</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="count">Conversions count</param>
        /// <param name="value">Conversions value</param>
        private static void LogMVTestConversion(string siteName, string culture, string conversionName, int objectId, int count, double value)
        {
            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            var cookies = CookieHelper.GetDistinctCookieNames();

            // Loop through all available cookies
            foreach (string cookieName in cookies)
            {
                // Try to get MVT cookies
                if (cookieName.StartsWithCSafe(MVTCOOKIE_PREFIX, true))
                {
                    // Get the test name
                    string testName = cookieName.Substring(MVTCOOKIE_PREFIX.Length);

                    string combination = CookieHelper.GetValue(cookieName);
                    if (!string.IsNullOrEmpty(combination))
                    {
                        // Log MVT conversion
                        HitLogProvider.LogHit(MVTCONVERSION + ";" + testName + ";" + combination, siteName, culture, conversionName, objectId, count, value);
                    }
                }
            }
        }


        /// <summary>
        /// Checks if conversion is applicable for MV Test and saves it.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="processLogRecordEventArgs">Log record data</param>
        private static void HandleMVTestConversion(object sender, CMSEventArgs<LogRecord> processLogRecordEventArgs)
        {
            var logRecord = processLogRecordEventArgs.Parameter;

            // Check MVT conversion validity
            if (!logRecord.CodeName.StartsWithCSafe(MVTCONVERSION, true))
            {
                return;
            }

            // If conversion does not exist - no log
            if (!ConversionExists(logRecord.ObjectName, logRecord.SiteName))
            {
                processLogRecordEventArgs.Cancel();
                return;
            }

            // Split items in codename
            string[] items = logRecord.CodeName.Split(';');

            // Check whether format is correct
            if (items.Length != 3)
            {
                // Format is not valid => MVT conversion value is not valid
                processLogRecordEventArgs.Cancel();
                return;
            }
            string mvTestName = items[1];
            string combination = items[2];

            // Check whether MVT combination is valid
            if (!MVTCombinationInfoProvider.IsMVTCombinationValid(combination, mvTestName, logRecord.SiteName, logRecord.Culture))
            {
                processLogRecordEventArgs.Cancel();
                return;
            }

            // Increase number of conversion for variant
            int storedHits = MVTestInfoProvider.IncreaseConversions(mvTestName, combination, logRecord.SiteName, logRecord.Culture, logRecord.Hits);
            if (storedHits < logRecord.Hits)
            {
                logRecord.Value = GetNewValue(storedHits, logRecord.ValuesSet);
            }
        }


        /// <summary>
        /// Returns false if conversion does not exists
        /// </summary>
        /// <param name="conversionName">Conversion name</param>
        /// <param name="siteName">Site name</param>
        private static bool ConversionExists(string conversionName, string siteName)
        {
            // Find conversion by conversion code name 
            ConversionInfo ci = ConversionInfoProvider.GetConversionInfo(conversionName, siteName);
            return (ci != null);
        }


        /// <summary>
        /// Returns value according to stored conversions.
        /// </summary>
        private static double GetNewValue(int storedHits, IList<double> valuesSet)
        {
            double value = 0;
            while (storedHits > 0)
            {
                value += valuesSet[0];
                valuesSet.RemoveAt(0);
                storedHits--;
            }
            return value;
        }


        /// <summary>
        /// Loads all the MVT/Content personalization variants for this web part instance.
        /// </summary>
        private static void LoadVariants(object sender, WebPartLoadVariantsArgs args)
        {
            MVTHelper.LoadVariants(args.ForceLoad, args.VariantMode, args.WebPartInstance, args.DocumentID);
        }


        /// <summary>
        /// Loads all the MVT/Content personalization variants for this zone instance.
        /// </summary>
        private static void LoadZoneVariants(object sender, WebPartLoadVariantsArgs args)
        {
            MVTHelper.LoadZoneVariants(args.ForceLoad, args.VariantMode, args.WebPartZoneInstance);
        }


        /// <summary>
        /// Updates web parts variants after all webparts are moved
        /// </summary>
        private static void MoveAllWebParts(object sender, MoveWebPartsArgs args)
        {
            MVTHelper.UpdateWebPartsVariants(args.Zone, args.TargetZone, args.TemplateInfo);
        }


        /// <summary>
        /// Updates web parts variants after webpart move        
        /// </summary>
        private static void MoveWebPart(object sender, MoveWebPartsArgs args)
        {
            WebPartZoneInstance zone = args.Zone;
            WebPartZoneInstance targetZone = args.TargetZone;
            PageTemplateInfo pti = args.TemplateInfo;
            WebPartInstance webPart = args.WebPartInstance;

            if ((zone.ZoneID != targetZone.ZoneID) || ((targetZone.ZoneID == zone.ZoneID) && (targetZone.VariantID != zone.VariantID)))
            {
                if ((pti != null) && (pti.TemplateInstance != null))
                {
                    if (webPart.HasVariants)
                    {
                        // Update the web part variants with the target zone id
                        if (webPart.VariantMode == VariantModeEnum.MVT)
                        {
                            MVTVariantInfoProvider.UpdateWebPartVariants(pti.PageTemplateId, zone.ZoneID, webPart.InstanceGUID, targetZone.ZoneID);
                        }
                        else if (webPart.VariantMode == VariantModeEnum.ContentPersonalization)
                        {
                            ContentPersonalizationVariantInfoProvider.UpdateWebPartVariants(pti.PageTemplateId, zone.ZoneID, webPart.InstanceGUID, targetZone.ZoneID);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Remove MVT a CP variants
        /// </summary>
        private static void RemoveWebPart(object sender, RemoveWebPartsArgs args)
        {
            WebPartInstance webPart = args.WebPartInstance;

            if (webPart.HasVariants)
            {
                // Remove all web part variants
                var array = new List<WebPartInstance>();
                array.Add(webPart);

                using (new CMSActionContext
                {
                    TouchParent = false
                })
                {
                    if (webPart.VariantMode == VariantModeEnum.MVT)
                    {
                        MVTVariantInfoProvider.RemoveWebPartsVariants(array);
                    }
                    else if (webPart.VariantMode == VariantModeEnum.ContentPersonalization)
                    {
                        ContentPersonalizationVariantInfoProvider.RemoveWebPartsVariants(array);
                    }
                }
            }
        }


        /// <summary>
        /// Remove MVT a CP variants for all webparts
        /// </summary>
        private static void RemoveAllWebParts(object sender, RemoveWebPartsArgs args)
        {
            WebPartZoneInstance zone = args.Zone;
            List<WebPartInstance> webParts = args.Zone.WebParts;

            if (zone.WebPartsContainVariants)
            {
                ContentPersonalizationVariantInfoProvider.RemoveWebPartsVariants(webParts);
                MVTVariantInfoProvider.RemoveWebPartsVariants(webParts);
            }
        }


        /// <summary>
        /// Event raised after page template is deleted.
        /// </summary>
        private static void PageTemplateDelete(object sender, ObjectEventArgs oea)
        {
            ProviderHelper.ClearHashtables(MVTVariantInfo.OBJECT_TYPE, true);
            ProviderHelper.ClearHashtables(MVTCombinationInfo.OBJECT_TYPE, true);
        }


        /// <summary>
        /// Event raised after template's variants are requested
        /// </summary>
        private static void GetVariants(object sender, GetVariantsEventArgs variantEventArgs)
        {
            GetVariantsEventArgs args = variantEventArgs;

            // MVT
            if ((args.VariantType == VariantModeEnum.MVT) && MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.MVTest", "Read"))
            {
                args.Variants = MVTVariantInfoProvider.GetMVTVariants(args.PageTemplateID, args.ZoneID, args.InstanceGuid, args.DocumentID);
            }

            // Content personalization
            if ((args.VariantType == VariantModeEnum.ContentPersonalization) && MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("CMS.ContentPersonalization", "Read"))
            {
                args.Variants = ContentPersonalizationVariantInfoProvider.GetContentPersonalizationVariants(args.PageTemplateID, args.ZoneID, args.InstanceGuid, args.DocumentID);
            }
        }


        /// <summary>
        /// Raised when variant is requested.
        /// </summary>
        private static void GetVariant(object sender, GetVariantEventArgs args)
        {
            if (args.VariantType == VariantModeEnum.MVT)
            {
                args.VariantID = MVTVariantInfoProvider.GetMVTVariantId(args.PageTemplateID, args.VariantName);
            }
            else if (args.VariantType == VariantModeEnum.ContentPersonalization)
            {
                var variant = ContentPersonalizationVariantInfoProvider.GetContentPersonalizationVariants()
                                                                       .WhereEquals("VariantPageTemplateID", args.PageTemplateID);

                if (!String.IsNullOrEmpty(args.VariantName))
                {
                    variant.WhereEquals("VariantName", args.VariantName);
                }

                if (args.DocumentIdIsNull.HasValue)
                {
                    variant = args.DocumentIdIsNull.Value ?
                        variant.WhereNull("VariantDocumentID") :
                        variant.WhereNotNull("VariantDocumentID");
                }

                args.VariantID = variant.Select(c => c.VariantID).FirstOrDefault();
            }
        }


        /// <summary>
        /// Event raised when variant is to be saved
        /// </summary>
        private static void SetVariantWebParts(object sender, SetVariantEventArgs setVariantEventArgs)
        {
            SetVariantEventArgs args = setVariantEventArgs;
            if (args.VariantID > 0)
            {
                if (args.VariantType == VariantModeEnum.MVT)
                {
                    MVTVariantInfoProvider.SaveVariantWebparts(args.VariantID, args.XmlDefinition);
                }
                else if (args.VariantType == VariantModeEnum.ContentPersonalization)
                {
                    ContentPersonalizationVariantInfoProvider.SaveVariantWebparts(args.VariantID, args.XmlDefinition);
                }
            }
        }


        /// <summary>
        /// Event raised when variant's properties are saved
        /// </summary>
        private static void SetVariant(object sender, SetVariantEventArgs setVariantEventArgs)
        {
            SetVariantEventArgs args = setVariantEventArgs;
            VariantSettings variant = args.Variant;

            if (args.VariantType == VariantModeEnum.MVT)
            {
                // Save MVT variant properties
                variant.ID = MVTVariantInfoProvider.SaveVariant(variant.ID, variant.Name, variant.DisplayName, variant.Description, variant.Enabled, variant.ZoneID, variant.InstanceGuid, variant.PageTemplateID, variant.DocumentID, args.XmlDefinition);
            }
            else if (args.VariantType == VariantModeEnum.ContentPersonalization)
            {
                // Save Content personalization variant properties
                variant.ID = ContentPersonalizationVariantInfoProvider.SaveVariant(variant.ID, variant.Name, variant.DisplayName, variant.Description, variant.Enabled, variant.Condition, variant.ZoneID, variant.InstanceGuid, variant.PageTemplateID, variant.DocumentID, args.XmlDefinition);
            }
        }


        /// <summary>
        /// Event raised when variants are to be deleted.
        /// </summary>
        private static void DeleteWidgetVariants(object sender, DeleteVariantEventArgs args)
        {
            MVTVariantInfoProvider.DeleteWidgetVariants(args.ZoneID, args.PageTemplateID, args.DocumentID);
            ContentPersonalizationVariantInfoProvider.DeleteWidgetVariants(args.ZoneID, args.PageTemplateID, args.DocumentID);
        }


        /// <summary>
        /// Updates web parts variants after their parent layout zone id is changed
        /// </summary>
        private static void ChangeLayoutZoneId(object sender, ChangeLayoutZoneIdArgs args)
        {
            string oldZoneId = args.OldZoneId;
            string newZoneId = args.NewZoneId;
            int pageTemplateId = args.PageTemplateId;
            var webParts = args.ZoneWebParts;

            if ((pageTemplateId == 0) || (webParts == null))
            {
                return;
            }

            if (!String.Equals(oldZoneId, newZoneId, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var webPart in webParts)
                {
                    if (webPart.HasVariants)
                    {
                        // Update the web part variants with the new zone id
                        if (webPart.VariantMode == VariantModeEnum.MVT)
                        {
                            MVTVariantInfoProvider.UpdateWebPartVariants(pageTemplateId, oldZoneId, webPart.InstanceGUID, newZoneId);
                        }
                        else if (webPart.VariantMode == VariantModeEnum.ContentPersonalization)
                        {
                            ContentPersonalizationVariantInfoProvider.UpdateWebPartVariants(pageTemplateId, oldZoneId, webPart.InstanceGUID, newZoneId);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
