using System;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.ServiceModel.Activation;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.DocumentEngine;
using CMS.TranslationServices;
using CMS.WebServices;

namespace CMS.DocumentWebServices
{
    /// <summary>
    /// REST service to access and manage translations of objects.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ObjectTranslationRESTService : RESTService
    {
        /// <summary>
        /// Returns proper stream from given object.
        /// </summary>
        /// <param name="obj">Info object or TreeNode to get into the stream</param>
        /// <param name="objectType">Original object type</param>
        /// <param name="settings">Export settings</param>
        protected override Stream GetStream(object obj, string objectType, TraverseObjectSettings settings)
        {
            return settings.Translate ? GetTranslation(obj, CurrentSiteName) : base.GetStream(obj, objectType, settings);
        }


        /// <summary>
        /// Creates ExportObjectSettings object from query string parameters.
        /// </summary>
        /// <param name="rootName">Name of the root</param>
        protected override TraverseObjectSettings GetExportSettings(string rootName)
        {
            var settings = base.GetExportSettings(rootName);

            var context = WebOperationContext.Current;
            if (context == null)
            {
                return settings;
            }

            var query = context.IncomingRequest.UriTemplateMatch.QueryParameters;
            settings.Translate = ValidationHelper.GetBoolean(query["translate"], false);

            return settings;
        }


        /// <summary>
        /// Returns XLIFF translation of the given object.
        /// </summary>
        /// <param name="obj">Object to translate</param>
        /// <param name="currentSiteName">Name of the current site</param>
        public static Stream GetTranslation(object obj, string currentSiteName)
        {
            var context = WebOperationContext.Current;
            if (context == null)
            {
                return null;
            }

            // There is no object to translate
            if (obj == null)
            {
                context.OutgoingResponse.SetStatusAsNotFound();
                return null;
            }

            var settings = GetTranslationSettings(currentSiteName);

            // Get document
            var data = obj as InfoDataSet<TreeNode>;
            if ((data != null) && data.Items.Count == 1)
            {
                obj = data.Items[0];
            }

            // REST supports translation only to a single language
            var targetLanguage = settings.TargetLanguages.First();

            string result = null;
            if (obj is TreeNode)
            {
                result = TranslationServiceHelper.GetXLIFF((TreeNode)obj, settings, targetLanguage);
            }
            else if (obj is GeneralizedInfo)
            {
                result = TranslationServiceHelper.GetXLIFF((GeneralizedInfo)obj, settings, targetLanguage);
            }

            // No translation available
            if (result == null)
            {
                context.OutgoingResponse.SetStatusAsNotFound();
                return null;
            }

            // Set translation encoding
            var encoding = TranslationServiceHelper.GetTranslationsEncoding(currentSiteName);
            context.OutgoingResponse.ContentType = TranslationServiceHelper.XLIFFMIME + "; charset=" + encoding.WebName;

            // Prepare stream
            var stream = new MemoryStream(encoding.GetBytes(result));
            stream.Position = 0;
            return stream;
        }


        /// <summary>
        /// Creates TranslationSettings object from query string parameters.
        /// </summary>
        /// <param name="currentSiteName">Name of the current site</param>
        private static TranslationSettings GetTranslationSettings(string currentSiteName)
        {
            var context = WebOperationContext.Current;
            if (context == null)
            {
                return null;
            }

            var query = context.IncomingRequest.UriTemplateMatch.QueryParameters;
            var settings = new TranslationSettings();

            settings.SourceLanguage = query["sourcelanguage"];
            settings.TargetLanguages.Add(query["targetlanguage"]);

            settings.TranslateDocCoupledData = ValidationHelper.GetBoolean(query["translatedoccoupleddata"], true);
            settings.TranslateEditableItems = ValidationHelper.GetBoolean(query["translateeditableitems"], true);
            settings.TranslateWebpartProperties = ValidationHelper.GetBoolean(query["translatewebpartproperties"], SettingsKeyInfoProvider.GetBoolValue(currentSiteName + ".CMSTranslateWebpartProperties"));
            settings.TranslateAttachments = ValidationHelper.GetBoolean(query["translateattachments"], false);

            return settings;
        }
    }
}