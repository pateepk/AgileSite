using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.SiteProvider;

using SystemIO = System.IO;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Class containing methods for manipulating with Translation Services (XLIFF format).
    /// </summary>
    public static class TranslationServiceHelper
    {
        #region "Variables"

        private static bool? mUseCDATAForTranslationUnit;
        private const string TRANSLATION_SOURCENAME = "Translation services";
        private static HashSet<string> allowedAttachmentExtensions;

        #endregion


        #region "Constants"

        /// <summary>
        /// XLIFF file extension
        /// </summary>
        public const string XLIFFEXTENSION = "xlf";

        /// <summary>
        /// XLIFF MIME type
        /// </summary>
        public const string XLIFFMIME = "application/x-xliff+xml";

        private const string WEBPARTIDPREFIX = "webpart;";
        private const string WIDGETIDPREFIX = "widget;";
        private const string DEFAULTWIDGETIDPREFIX = "defaultwidget;";
        private const string EDITABLEPREFIX = "editable;";
        private const string EDITABLEREGIONPREFIX = "editableregion;";
        private const string EDITABLEWEBPARTPREFIX = "editablewebpart;";
        private const string SUBMISSION_PAGE_URL = "~/CMSModules/Translations/CMSPages/SubmitTranslation.aspx";

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if CDATA section should be used for translation units.
        /// </summary>
        public static bool UseCDATAForTranslationUnit
        {
            get
            {
                return mUseCDATAForTranslationUnit ?? (mUseCDATAForTranslationUnit = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSTranslationServicesUseCDATAForTransUnit"], true)).Value;
            }
            set
            {
                mUseCDATAForTranslationUnit = value;
            }
        }


        /// <summary>
        /// If true, submissions are processed automatically (without explicit approval in administration) when the submission is ready to translate.
        /// </summary>
        public static bool AutoImportEnabled
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSTranslationsAutoImport");
            }
        }

        #endregion


        #region "XLIFF Export methods"

        /// <summary>
        /// Returns XLIFF export for given IDataContainer object.
        /// </summary>
        /// <param name="data">DataContainer object to export</param>
        /// <param name="settings">Translation settings</param>
        /// <param name="targetLanguage">Target language</param>
        public static string GetXLIFF(IDataContainer data, TranslationSettings settings, string targetLanguage)
        {
            int wordCount, charCount;
            return GetXLIFF(data, settings, targetLanguage, out wordCount, out charCount);
        }


        /// <summary>
        /// Returns XLIFF export for given IDataContainer object.
        /// </summary>
        /// <param name="data">DataContainer object to export</param>
        /// <param name="settings">Translation settings</param>
        /// <param name="targetLanguage">Target language</param>
        /// <param name="charCount">Number of characters to translate within exported XLIFF</param>
        /// <param name="wordCount">Number of words to translate within exported XLIFF</param>
        public static string GetXLIFF(IDataContainer data, TranslationSettings settings, string targetLanguage, out int wordCount, out int charCount)
        {
            CheckLicense();

            wordCount = 0;
            charCount = 0;

            if (data == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            XmlWriter xml = GetXmlWriter(sb);

            string sourceLang = settings.SourceLanguage;
            if (string.IsNullOrEmpty(sourceLang))
            {
                sourceLang = CultureHelper.DefaultUICultureCode;
            }

            string dataType = settings.DataType;
            if (string.IsNullOrEmpty(dataType))
            {
                dataType = "plaintext";
            }

            WriteHeader(xml, settings.ItemIdentifier, sourceLang, targetLanguage, dataType);

            // Write the fields
            foreach (string col in data.ColumnNames)
            {
                // Skip binary fields if not required
                object val = data.GetValue(col);
                if (val is byte[])
                {
                    continue;
                }

                int wCount, chCount;
                WriteTransUnit(xml, col, val, sourceLang, settings.GenerateTargetTag, null, out wCount, out chCount);

                wordCount += wCount;
                charCount += chCount;
            }

            WriteEnd(xml);
            xml.Flush();

            return sb.ToString();
        }


        /// <summary>
        /// Returns XLIFF export for given object instance.
        /// </summary>
        /// <param name="info">BaseInfo object to export</param>
        /// <param name="settings">Translation settings</param>
        /// <param name="targetLanguage">Target language</param>
        public static string GetXLIFF(BaseInfo info, TranslationSettings settings, string targetLanguage)
        {
            int wordCount, charCount;
            return GetXLIFF(info, settings, targetLanguage, out wordCount, out charCount);
        }


        /// <summary>
        /// Returns XLIFF export for given TreeNode object.
        /// </summary>
        /// <param name="info">BaseInfo object to export</param>
        /// <param name="settings">Translation settings</param>
        /// <param name="targetLanguage">Target language</param>
        /// <param name="charCount">Number of characters to translate within exported XLIFF</param>
        /// <param name="wordCount">Number of words to translate within exported XLIFF</param>
        public static string GetXLIFF(BaseInfo info, TranslationSettings settings, string targetLanguage, out int wordCount, out int charCount)
        {
            CheckLicense();

            wordCount = 0;
            charCount = 0;

            // Clear allowed extension to ensure setting value for given site
            allowedAttachmentExtensions = null;

            if (info == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            XmlWriter xml = GetXmlWriter(sb);

            string sourceLang = settings.SourceLanguage;
            if (string.IsNullOrEmpty(sourceLang))
            {
                sourceLang = CultureHelper.DefaultUICultureCode;
            }

            var ti = info.TypeInfo;

            string original = settings.ItemIdentifier;
            if (string.IsNullOrEmpty(original))
            {
                original = ti.ObjectType + ";" + info.Generalized.ObjectID;
            }

            string dataType = settings.DataType;
            if (string.IsNullOrEmpty(dataType))
            {
                dataType = "plaintext";
            }

            WriteHeader(xml, original, sourceLang, targetLanguage, dataType);

            string mimeType = null;
            if (ti.MimeTypeColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                mimeType = ValidationHelper.GetString(info.GetValue(ti.MimeTypeColumn), "");
            }

            // Write the fields
            var fi = FormHelper.GetFormInfo(ti.ObjectClassName, false);
            if (fi != null)
            {
                var fields = fi.GetFields(true, true);
                WriteFields(xml, info, fields, sourceLang, settings.TranslateAttachments, settings.GenerateTargetTag, mimeType, out wordCount, out charCount);
            }

            WriteEnd(xml);
            xml.Flush();

            return sb.ToString();
        }


        /// <summary>
        /// Returns XLIFF export for given TreeNode object.
        /// </summary>
        /// <param name="node">TreeNode object to export</param>
        /// <param name="settings">Export settings</param>
        /// <param name="targetLanguage">Target language</param>
        public static string GetXLIFF(TreeNode node, TranslationSettings settings, string targetLanguage)
        {
            int wordCount, charCount;
            return GetXLIFF(node, settings, targetLanguage, out wordCount, out charCount);
        }


        /// <summary>
        /// Returns XLIFF export for given TreeNode object.
        /// </summary>
        /// <param name="node">TreeNode object to export</param>
        /// <param name="settings">Export settings</param>
        /// <param name="targetLanguage">Target language</param>
        /// <param name="charCount">Number of characters to translate within exported XLIFF</param>
        /// <param name="wordCount">Number of words to translate within exported XLIFF</param>
        public static string GetXLIFF(TreeNode node, TranslationSettings settings, string targetLanguage, out int wordCount, out int charCount)
        {
            CheckLicense();

            wordCount = 0;
            charCount = 0;

            // Clear allowed extension to ensure setting value for given site
            allowedAttachmentExtensions = null;

            if (node == null)
            {
                return null;
            }

            int wCount, chCount;

            StringBuilder sb = new StringBuilder();
            XmlWriter xml = GetXmlWriter(sb);

            // Create the file header
            string sourceLanguage = settings.SourceLanguage;
            if (string.IsNullOrEmpty(sourceLanguage))
            {
                sourceLanguage = node.DocumentCulture;
            }

            string original = settings.ItemIdentifier;
            if (string.IsNullOrEmpty(original))
            {
                original = "cms.document;" + node.DocumentID;
            }

            string dataType = settings.DataType;
            if (string.IsNullOrEmpty(dataType))
            {
                dataType = "htmlbody";
            }

            WriteHeader(xml, original, sourceLanguage, targetLanguage, dataType);

            // Export document-specific info
            if (settings.TranslateDocCoupledData)
            {
                WriteCoupledData(xml, node, sourceLanguage, settings, ref wordCount, ref charCount);
            }

            // Export editable regions
            if (settings.TranslateEditableItems)
            {
                var items = new EditableItems();
                items.LoadContentXml(node.GetStringValue("DocumentContent", ""));

                var regions = items.EditableRegions;
                WriteEditableItems(xml, regions, sourceLanguage, settings, EDITABLEREGIONPREFIX, ref wordCount, ref charCount);

                var webParts = items.EditableWebParts;
                WriteEditableItems(xml, webParts, sourceLanguage, settings, EDITABLEWEBPARTPREFIX, ref wordCount, ref charCount);
            }

            // Load page info from node
            PageInfo pageInfo = new PageInfo();
            pageInfo.LoadVersion(node);

            // Export web part properties
            if (settings.TranslateWebpartProperties)
            {
                ViewModeEnum viewMode = PortalContext.ViewMode;
                PortalContext.ViewMode = ViewModeEnum.Edit;
                PageTemplateInstance template = pageInfo.TemplateInstance;
                PortalContext.ViewMode = viewMode;

                if (template != null)
                {
                    List<WebPartZoneInstance> zones = template.WebPartZones;
                    foreach (WebPartZoneInstance zone in zones)
                    {
                        // Process only web parts and editor widgets
                        if ((zone.WidgetZoneType != WidgetZoneTypeEnum.Editor) && (zone.WidgetZoneType != WidgetZoneTypeEnum.None))
                        {
                            continue;
                        }

                        foreach (WebPartInstance webPart in zone.WebParts)
                        {
                            var formInfo = GetComponentFormInfo(webPart);
                            if (formInfo == null)
                            {
                                continue;
                            }

                            string controlName = ValidationHelper.GetString(webPart.GetValue("ControlID"), "");

                            var fields = formInfo.GetFields(true, false);
                            foreach (FormFieldInfo field in fields)
                            {
                                if (!IsFieldForTranslation(webPart, field))
                                {
                                    continue;
                                }

                                var prefix = webPart.IsWidget ? WIDGETIDPREFIX : WEBPARTIDPREFIX;
                                WriteTransUnit(xml, prefix + controlName + "/" + field.Name, webPart.Properties[field.Name], sourceLanguage, settings.GenerateTargetTag, null, out wCount, out chCount);

                                wordCount += wCount;
                                charCount += chCount;
                            }
                        }
                    }
                }

                // Include default editor widgets
                if (pageInfo.UsedPageTemplateInfo != null)
                {
                    template = pageInfo.UsedPageTemplateInfo.TemplateInstance;
                    if (template != null)
                    {
                        List<WebPartZoneInstance> zones = template.WebPartZones;
                        foreach (WebPartZoneInstance zone in zones)
                        {
                            // Process only editor widgets
                            if (zone.WidgetZoneType != WidgetZoneTypeEnum.Editor)
                            {
                                continue;
                            }

                            foreach (WebPartInstance webPart in zone.WebParts)
                            {
                                var formInfo = GetComponentFormInfo(webPart);
                                if (formInfo == null)
                                {
                                    continue;
                                }

                                string controlName = ValidationHelper.GetString(webPart.GetValue("ControlID"), "");
                                var fields = formInfo.GetFields(true, false);
                                foreach (FormFieldInfo field in fields)
                                {
                                    if (!IsFieldForTranslation(webPart, field))
                                    {
                                        continue;
                                    }

                                    WriteTransUnit(xml, DEFAULTWIDGETIDPREFIX + controlName + "/" + field.Name, webPart.Properties[field.Name], sourceLanguage, settings.GenerateTargetTag, null, out wCount, out chCount);

                                    wordCount += wCount;
                                    charCount += chCount;
                                }
                            }
                        }
                    }
                }
            }

            if (SettingsKeyInfoProvider.GetBoolValue(node.NodeSiteName + ".CMSAllowAttachmentTranslation"))
            {
                foreach (BaseInfo ai in node.AllAttachments)
                {
                    ai.Generalized.EnsureBinaryData();

                    if (IsAllowedToWriteAttachment(settings, ai))
                    {
                        string attExtension = ai.GetStringValue("AttachmentExtension", "");
                        if (IsExtensionAllowed(attExtension, node.NodeSiteName))
                        {
                            string mimeType = ai.GetStringValue("AttachmentMimeType", "image/jpeg");
                            WriteTransUnit(xml, "cms.attachment;" + ai.GetGuidValue("AttachmentGUID", Guid.Empty), ai.GetValue("AttachmentBinary"), sourceLanguage, settings.GenerateTargetTag, mimeType, out wCount, out chCount);

                            wordCount += wCount;
                            charCount += chCount;
                        }
                    }
                }
            }

            WriteEnd(xml);
            xml.Flush();

            return sb.ToString();
        }


        private static void WriteCoupledData(XmlWriter xml, TreeNode node, string sourceLanguage, TranslationSettings settings, ref int wordCount, ref int charCount)
        {
            int wCount;
            int chCount;
            var fields = GetDocumentFields(node);
            WriteFields(xml, node, fields, sourceLanguage, settings.TranslateAttachments, settings.GenerateTargetTag, null, out wCount, out chCount);

            wordCount += wCount;
            charCount += chCount;
        }


        private static void WriteEditableItems(XmlWriter xml, MultiKeyDictionary<string> items, string sourceLanguage, TranslationSettings settings, string prefix, ref int wordCount, ref int charCount)
        {
            bool useAbsoluteLinks = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSTranslationsUseAbsoluteLinks"], false);

            foreach (string key in items.TypedKeys)
            {
                string content = items[key];

                if (useAbsoluteLinks && !String.IsNullOrEmpty(content))
                {
                    // Make EditableImages with absolute URLs
                    content = content.Replace("<property name=\"imagepath\">~", "<property name=\"imagepath\">" + URLHelper.GetFullApplicationUrl());
                    content = URLHelper.MakeLinksAbsolute(content);
                }

                int wCount;
                int chCount;

                WriteTransUnit(xml, prefix + key, content, sourceLanguage, settings.GenerateTargetTag, null, out wCount, out chCount);

                wordCount += wCount;
                charCount += chCount;
            }
        }


        private static bool IsFieldForTranslation(WebPartInstance instance, FormFieldInfo field)
        {
            if (!field.TranslateField || !IsFieldDataTypeAllowedForTranslation(field.DataType))
            {
                return false;
            }

            if (instance.MacroTable[field.Name] != null)
            {
                return false;
            }
            return true;
        }


        private static bool IsAllowedToWriteAttachment(TranslationSettings settings, BaseInfo info)
        {
            var isUnsorted = info.GetBooleanValue("AttachmentIsUnsorted", false);
            var isGrouped = info.GetGuidValue("AttachmentGroupGUID", Guid.Empty) != Guid.Empty;

            return settings.TranslateAttachments && (isUnsorted || isGrouped);
        }


        /// <summary>
        /// Returns true for Text, LongText, and Binary field types.
        /// </summary>
        /// <param name="dataType">DataType of the field</param>
        public static bool IsFieldDataTypeAllowedForTranslation(string dataType)
        {
            return DataTypeManager.SupportsTranslation(TypeEnum.Field, dataType);
        }


        /// <summary>
        /// Writes XLIFF trans-units generated from data from given object according to given field info.
        /// </summary>
        /// <param name="xml">XmlWriter object</param>
        /// <param name="obj">Object with the data</param>
        /// <param name="fields">List of fields</param>
        /// <param name="sourceLang">Source language of the data</param>
        /// <param name="processBinary">If true, binary files are included into translation</param>
        /// <param name="writeTarget">If true, target node is generated as well with the same content as source tag (needed for translations.com)</param>
        /// <param name="mimeType">Mime type of the binary data</param>
        /// <param name="charCount">Number of characters to translate within exported XLIFF</param>
        /// <param name="wordCount">Number of words to translate within exported XLIFF</param>
        private static void WriteFields(XmlWriter xml, ICMSObject obj, IEnumerable<FormFieldInfo> fields, string sourceLang, bool processBinary, bool writeTarget, string mimeType, out int wordCount, out int charCount)
        {
            wordCount = 0;
            charCount = 0;

            foreach (FormFieldInfo field in fields)
            {
                if (FieldIsSkipped(field, processBinary, obj))
                {
                    continue;
                }

                var fieldName = field.Name;
                object value = null;

                // Do not translate code name column of Info objects
                bool notCodeName = true;
                bool isExtensionAllowed = true;
                if (obj is BaseInfo)
                {
                    BaseInfo info = (BaseInfo)obj;
                    var codeNameColumn = info.Generalized.CodeNameColumn;
                    if (string.Equals(codeNameColumn, fieldName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        notCodeName = false;
                    }
                    else
                    {
                        value = info.Generalized.GetValueForTranslation(field.Name);
                    }
                }
                else
                {
                    value = obj.GetValue(field.Name);
                }

                if (field.DataType == FieldDataType.File)
                {
                    Guid attGuid = ValidationHelper.GetGuid(value, Guid.Empty);
                    if (attGuid != Guid.Empty)
                    {
                        var node = (TreeNode)obj;

                        // Translate GUID to attachment object
                        var attachment = DocumentHelper.GetAttachment(node, attGuid);
                        if (attachment != null)
                        {
                            if (IsExtensionAllowed(attachment.AttachmentExtension, node.NodeSiteName))
                            {
                                attachment.Generalized.EnsureBinaryData();

                                value = attachment.AttachmentBinary;
                                mimeType = attachment.AttachmentMimeType;
                                fieldName = fieldName + ";" + attachment.AttachmentGUID;
                            }
                            else
                            {
                                isExtensionAllowed = false;
                            }
                        }
                    }
                }

                // Include item only if not codename and has allowed extension if file
                if (notCodeName && isExtensionAllowed)
                {
                    int wCount, chCount;
                    WriteTransUnit(xml, fieldName, value, sourceLang, writeTarget, mimeType, out wCount, out chCount);

                    wordCount += wCount;
                    charCount += chCount;
                }
            }
        }


        private static bool FieldIsSkipped(FormFieldInfo field, bool processBinary, ICMSObject obj)
        {
            // Field is not configured for translation process
            if (!field.TranslateField || !IsFieldDataTypeAllowedForTranslation(field.DataType))
            {
                return true;
            }

            bool fieldIsBinary = (field.DataType == FieldDataType.Binary) || (field.DataType == FieldDataType.File);
            // Skip binary fields if configured
            if (!processBinary && fieldIsBinary)
            {
                return true;
            }

            if (fieldIsBinary)
            {
                var node = (TreeNode)obj;
                // Skip binary fields if attachments are not allowed
                if (!SettingsKeyInfoProvider.GetBoolValue(node.NodeSiteName + ".CMSAllowAttachmentTranslation"))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Writes [file][body] tags of XLIFF file with specified attributes.
        /// </summary>
        /// <param name="xml">XmlWriter object</param>
        /// <param name="original">Original attribute field</param>
        /// <param name="source">Source language attribute field</param>
        /// <param name="target">Target language attribute field</param>
        /// <param name="datatype">Datatype attribute field</param>
        private static void WriteHeader(XmlWriter xml, string original, string source, string target, string datatype)
        {
            xml.WriteStartElement("xliff");
            xml.WriteAttributeString("version", "1.2");
            xml.WriteStartElement("file");
            xml.WriteAttributeString("original", original);

            // Get source and target culture code
            source = GetCultureCode(source, TranslationCultureMappingDirectionEnum.SystemToService);
            target = GetCultureCode(target, TranslationCultureMappingDirectionEnum.SystemToService);

            xml.WriteAttributeString("source-language", source);
            xml.WriteAttributeString("target-language", target);
            xml.WriteAttributeString("datatype", datatype);
            xml.WriteStartElement("body");
        }


        /// <summary>
        /// Ends [file][body] tags of XLIFF file.
        /// </summary>
        /// <param name="xml">XmlWriter object</param>
        private static void WriteEnd(XmlWriter xml)
        {
            xml.WriteEndElement(); // End of body tag
            xml.WriteEndElement(); // End of file tag
            xml.WriteEndElement(); // End of xliff tag
        }


        /// <summary>
        /// Writes a trans-unit XLIFF element.
        /// </summary>
        /// <param name="xml">XmlWriter object</param>
        /// <param name="id">ID of the unit</param>
        /// <param name="source">Data to translate</param>
        /// <param name="sourceLang">Source language</param>
        /// <param name="writeTarget">If true, target node is generated as well with the same content as source tag (needed for translations.com)</param>
        /// <param name="mimeType">MIME type of the binary if the source is binary. Ignored parameter for non-binary data.</param>
        /// <param name="charCount">Number of characters to translate within exported XLIFF</param>
        /// <param name="wordCount">Number of words to translate within exported XLIFF</param>
        private static void WriteTransUnit(XmlWriter xml, string id, object source, string sourceLang, bool writeTarget, string mimeType, out int wordCount, out int charCount)
        {
            wordCount = 0;
            charCount = 0;

            // Do not write empty items
            if (source == null)
            {
                return;
            }

            bool isBinary = source is byte[];
            if (isBinary)
            {
                xml.WriteStartElement("bin-unit");
                xml.WriteAttributeString("id", id.ToLowerInvariant());
                xml.WriteAttributeString("mime-type", mimeType);
                xml.WriteStartElement("bin-source");
                xml.WriteStartElement("internal-file");
            }
            else
            {
                // Do not write empty items
                if (string.IsNullOrEmpty(ValidationHelper.GetString(source, "")))
                {
                    return;
                }

                xml.WriteStartElement("trans-unit");
                xml.WriteAttributeString("id", id.ToLowerInvariant());
                xml.WriteStartElement("source");
            }

            WriteTransUnitContent(xml, source, sourceLang, out wordCount, out charCount);

            if (isBinary)
            {
                // End extra internal-file or external-file element
                xml.WriteEndElement();
            }

            xml.WriteEndElement();

            // Write target tag if needed
            if (writeTarget)
            {
                if (isBinary)
                {
                    xml.WriteStartElement("bin-target");
                    xml.WriteStartElement("internal-file");
                }
                else
                {
                    xml.WriteStartElement("target");
                }

                WriteTransUnitContent(xml, source, sourceLang, out wordCount, out charCount);

                if (isBinary)
                {
                    // End extra internal-file or external-file element
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
            }

            xml.WriteEndElement();
        }


        /// <summary>
        /// Writes a content of trans-unit XLIFF element.
        /// </summary>
        /// <param name="xml">XmlWriter object</param>
        /// <param name="source">Data to translate</param>
        /// <param name="sourceLang">Source language</param>
        /// <param name="charCount">Number of characters to translate within exported XLIFF</param>
        /// <param name="wordCount">Number of words to translate within exported XLIFF</param>
        private static void WriteTransUnitContent(XmlWriter xml, object source, string sourceLang, out int wordCount, out int charCount)
        {
            if (source is byte[])
            {
                byte[] bin = (byte[])source;
                xml.WriteBase64(bin, 0, bin.Length);

                wordCount = 0;
                charCount = 0;
            }
            else
            {
                string src = GetTranslationValue(ValidationHelper.GetString(source, ""), sourceLang);
                WriteFormattedTransUnitText(xml, src);
                SetCounts(src, out wordCount, out charCount);
            }
        }


        /// <summary>
        /// Gets formatted text based on settings
        /// </summary>
        /// <param name="xml">XmlWriter object</param>
        /// <param name="text">Text to format</param>
        private static void WriteFormattedTransUnitText(XmlWriter xml, string text)
        {
            if (UseCDATAForTranslationUnit)
            {
                xml.WriteCData(text);
            }
            else
            {
                xml.WriteString(text);
            }
        }


        /// <summary>
        /// Returns new instance of XmlWriter for XLIFF export.
        /// </summary>
        /// <param name="sb">StringBuilder object to build the writer on</param>
        private static XmlWriter GetXmlWriter(StringBuilder sb)
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.OmitXmlDeclaration = true;
            xmlSettings.Indent = true;
            xmlSettings.CheckCharacters = false;

            return XmlWriter.Create(sb, xmlSettings);
        }


        /// <summary>
        /// Returns number of words and characters within the given text.
        /// </summary>
        /// <param name="text">Text to analyze</param>
        /// <param name="wordCount">Word count</param>
        /// <param name="charCount">Character count</param>
        private static void SetCounts(string text, out int wordCount, out int charCount)
        {
            wordCount = Regex.Matches(text, @"[A-Za-z0-9]+").Count;
            charCount = text.Length;
        }


        private static FormInfo GetComponentFormInfo(WebPartInstance instance)
        {
            return instance.IsWidget ? GetWidgetFormInfo(instance) : GetWebPartFormInfo(instance);
        }


        private static FormInfo GetWebPartFormInfo(WebPartInstance instance)
        {
            var webPart = WebPartInfoProvider.GetWebPartInfo(instance.WebPartType);
            if (webPart == null)
            {
                return null;
            }

            return webPart.GetWebPartFormInfo();
        }


        private static FormInfo GetWidgetFormInfo(WebPartInstance instance)
        {
            var widget = WidgetInfoProvider.GetWidgetInfo(instance.WebPartType);
            if (widget == null)
            {
                return null;
            }

            var webPart = WebPartInfoProvider.GetWebPartInfo(widget.WidgetWebPartID);
            if (webPart == null)
            {
                return null;
            }

            var properties = FormHelper.MergeFormDefinitions(webPart.WebPartProperties, widget.WidgetProperties);
            return PortalFormHelper.GetWidgetFormInfo(widget.WidgetName, instance.ParentZone.WidgetZoneType, properties, true, widget.WidgetDefaultValues);
        }


        /// <summary>
        /// Sets the translation string into property of given page component.
        /// </summary>
        /// <param name="componentsCache">Page components cache</param>
        /// <param name="formDefinitionsCache">Form definitions cache</param>
        /// <param name="parts">Identifier of the component and its property from XLIFF</param>
        /// <param name="templateInstance">Page template instance</param>
        /// <param name="translation">Translation of the property</param>
        /// <param name="sourceLang">Source language</param>
        /// <param name="targetLang">Target language</param>
        /// <param name="prefix">Prefix defines type of component</param>
        /// <param name="useResourceString">Indicates if resource string should be used</param>
        private static bool SetComponentProperty(Dictionary<string, WebPartInstance> componentsCache, Dictionary<string, FormInfo> formDefinitionsCache, string[] parts, PageTemplateInstance templateInstance, string translation, string sourceLang, string targetLang, string prefix = null, bool useResourceString = true)
        {
            var cacheKey = prefix + parts[0];

            // Get web part instance
            var instance = GetCachedWebPartInstance(componentsCache, templateInstance, cacheKey, parts[0]);
            if (instance == null)
            {
                return false;
            }

            // Get form definition for the web part instance
            var formDefinition = GetCachedFormInfo(formDefinitionsCache, instance, cacheKey);
            var fieldName = parts[1];

            // Get maximal allowed field length
            var fieldLength = GetFieldLength(formDefinition.GetFormField(fieldName), useResourceString);

            // Check validity when not using resource string (there are cut to maximal length automatically)
            if (!useResourceString && (translation.Length > fieldLength))
            {
                CheckValidLength(translation.Length, fieldLength, fieldName, parts[0]);
            }

            var translationText = useResourceString ? TranslateString(ValidationHelper.GetString(instance.GetValue(fieldName), ""), translation, sourceLang, targetLang, fieldLength) : translation;
            instance.SetValue(fieldName, translationText);

            return true;
        }


        /// <summary>
        /// Gets the component form definition from web part instance object or cache
        /// </summary>
        /// <param name="formDefinitionsCache">Form definitions cache</param>
        /// <param name="instance">Web part instance</param>
        /// <param name="cacheKey">Key to check cache if form definition is already retrieved from the database</param>
        private static FormInfo GetCachedFormInfo(Dictionary<string, FormInfo> formDefinitionsCache, WebPartInstance instance, string cacheKey)
        {
            if (formDefinitionsCache.ContainsKey(cacheKey))
            {
                return formDefinitionsCache[cacheKey];
            }

            var formDefinition = GetComponentFormInfo(instance);
            formDefinitionsCache[cacheKey] = formDefinition;

            return formDefinition;
        }


        /// <summary>
        /// Gets web part instance object from page template instance by given control identifier or cache key
        /// </summary>
        /// <param name="componentsCache">Page components cache</param>
        /// <param name="templateInstance">Page template instance</param>
        /// <param name="cacheKey">Cache key</param>
        /// <param name="controlId">Control identifier</param>
        private static WebPartInstance GetCachedWebPartInstance(Dictionary<string, WebPartInstance> componentsCache, PageTemplateInstance templateInstance, string cacheKey, string controlId)
        {
            if (componentsCache.ContainsKey(cacheKey))
            {
                return componentsCache[cacheKey];
            }

            var instance = templateInstance.GetWebPart(controlId);
            componentsCache[cacheKey] = instance;

            return instance;
        }


        /// <summary>
        /// Get maximal allowed length of the given field. If using resource string the result is substract by length of the "{$ $}" macro signatures
        /// </summary>
        /// <param name="field">Field info</param>
        /// <param name="useResourceString">Indicates if length is used for resource string or not</param>
        private static int GetFieldLength(FormFieldInfo field, bool useResourceString)
        {
            if (field == null)
            {
                return 0;
            }

            if (useResourceString)
            {
                // Localization macro {$ $}
                return field.Size - 4;
            }

            return field.Size;
        }

        #endregion


        #region "XLIFF Import methods"

        /// <summary>
        /// Translates object specified by XLIFF document. Returns translated object if translation went ok, null otherwise.
        /// </summary>
        /// <param name="xliff">XLIFF document</param>
        /// <param name="submission">Translation submission</param>
        public static ICMSObject ProcessTranslation(string xliff, TranslationSubmissionInfo submission = null)
        {
            return ProcessTranslation(new SystemIO.MemoryStream(GetTranslationsEncoding(SiteContext.CurrentSiteName).GetBytes(xliff)), submission);
        }


        /// <summary>
        /// Translates object specified by XLIFF document. Returns translated object if translation went ok, null otherwise.
        /// </summary>
        /// <param name="xliff">Stream with XLIFF document</param>
        /// <param name="submission">Translation submission</param>
        public static ICMSObject ProcessTranslation(SystemIO.Stream xliff, TranslationSubmissionInfo submission = null)
        {
            CheckLicense();

            if (xliff == null)
            {
                return null;
            }

            TreeProvider tree = new TreeProvider();

            XmlReaderSettings xmlSettings = new XmlReaderSettings();
            xmlSettings.ConformanceLevel = ConformanceLevel.Auto;
            xmlSettings.CheckCharacters = false;

            XmlReader xml = XmlReader.Create(xliff, xmlSettings);

            string sourceLang = null;
            string targetLang = null;
            string lastColumn = null;
            string lastNode = null;

            string mimeType = null;

            TreeNode targetNode = null;
            TreeNode node = null;
            BaseInfo info = null;
            ICMSObject obj = null;
            PageInfo targetPageInfo = null;
            PageTemplateInstance templateInstance = null;

            ProcessTranslationHandler handler = null;

            bool updateObject = false;
            bool updatePageInfo = false;

            Dictionary<string, WebPartInstance> webPartsTable = new Dictionary<string, WebPartInstance>(StringComparer.InvariantCultureIgnoreCase);
            Dictionary<string, FormInfo> webPartFormDefinitionsTable = new Dictionary<string, FormInfo>(StringComparer.InvariantCultureIgnoreCase);
            HashSet<FormFieldInfo> formFields = null;

            try
            {
                while (xml.Read())
                {
                    // Start of the table
                    if (xml.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }

                    string xmlNameLower = xml.Name.ToLowerInvariant();
                    switch (xmlNameLower)
                    {
                        // Get root settings
                        case "file":
                            {
                                // Get source and target culture
                                sourceLang = GetCultureCode(xml.GetAttribute("source-language"), TranslationCultureMappingDirectionEnum.ServiceToSystem);
                                targetLang = GetCultureCode(xml.GetAttribute("target-language"), TranslationCultureMappingDirectionEnum.ServiceToSystem);

                                if (string.IsNullOrEmpty(sourceLang) || string.IsNullOrEmpty(targetLang))
                                {
                                    return null;
                                }

                                // Get edited object
                                string original = xml.GetAttribute("original");
                                string[] id = original.Split(';');
                                if (id.Length == 2)
                                {
                                    if (id[0].Equals("cms.document", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        node = DocumentHelper.GetDocument(ValidationHelper.GetInteger(id[1], 0), tree);
                                        if (node == null)
                                        {
                                            throw new TargetDocumentNotExistsException(ResHelper.GetString("translationservice.documentdoesnotexist"));
                                        }

                                        targetNode = CreateTargetCultureNode(node, targetLang, false, false, false);

                                        if (targetNode == null)
                                        {
                                            return null;
                                        }

                                        // Call the handler
                                        handler = TranslationEvents.ProcessTranslation.StartEvent(node, targetNode, submission);
                                        handler.DontSupportCancel();

                                        // Load page info from node
                                        targetPageInfo = new PageInfo();
                                        targetPageInfo.LoadVersion(targetNode);

                                        var pti = targetPageInfo.UsedPageTemplateInfo;
                                        if (pti != null)
                                        {
                                            templateInstance = pti.TemplateInstance;
                                        }

                                        obj = targetNode;
                                        if (node == null)
                                        {
                                            return null;
                                        }

                                        // Get all document fields
                                        formFields = GetDocumentFields(targetNode);

                                        // Change the flag and force updating the node
                                        if (targetNode.DocumentIsWaitingForTranslation)
                                        {
                                            targetNode.ResetTranslationFlag();
                                            updateObject = true;
                                        }

                                        // Delete current attachments
                                        if ((submission != null) && submission.SubmissionTranslateAttachments)
                                        {
                                            AttachmentInfoProvider.DeleteAttachments(targetNode.DocumentID);
                                        }
                                    }
                                    else
                                    {
                                        // Object
                                        string objectType = id[0];
                                        int objectId = ValidationHelper.GetInteger(id[1], 0);

                                        info = ProviderHelper.GetInfoById(objectType, objectId);
                                        obj = info;
                                        if (info == null)
                                        {
                                            return null;
                                        }

                                        // Get all object fields
                                        var formInfo = FormHelper.GetFormInfo(info.TypeInfo.ObjectClassName, false);
                                        formFields = formInfo.GetFields(true, true).ToHashSetCollection();

                                        // Call the handler
                                        handler = TranslationEvents.ProcessTranslation.StartEvent(obj, obj, submission);
                                        handler.DontSupportCancel();
                                    }
                                }
                            }
                            break;

                        case "trans-unit":
                            lastColumn = xml.GetAttribute("id");
                            break;

                        case "bin-unit":
                            lastColumn = xml.GetAttribute("id");
                            mimeType = xml.GetAttribute("mime-type");
                            break;

                        case "target":
                            if (lastColumn != null)
                            {
                                string translation = xml.ReadInnerXml();

                                // Check for CDATA section
                                string trimmedTranslation = translation.Trim();
                                if (trimmedTranslation.StartsWith("<![CDATA[", StringComparison.InvariantCultureIgnoreCase) && trimmedTranslation.EndsWith("]]>", StringComparison.Ordinal))
                                {
                                    // Remove CDATA
                                    translation = trimmedTranslation.Substring(9, trimmedTranslation.Length - 12);
                                }
                                else
                                {
                                    // Make sure the HTML is decoded
                                    translation = HTMLHelper.HTMLDecode(translation);
                                }

                                if (lastColumn.StartsWith(EDITABLEPREFIX, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Backward compatibility for old XLIFF format
                                    string id = lastColumn.Substring(EDITABLEPREFIX.Length);

                                    if (targetPageInfo.EditableWebParts.ContainsKey(id))
                                    {
                                        targetPageInfo.EditableWebParts[id] = translation;
                                    }
                                    else
                                    {
                                        targetPageInfo.EditableRegions[id] = translation;
                                    }

                                    updatePageInfo = true;
                                }
                                else if (lastColumn.StartsWith(EDITABLEWEBPARTPREFIX, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var id = lastColumn.Substring(EDITABLEWEBPARTPREFIX.Length);
                                    targetPageInfo.EditableWebParts[id] = translation;
                                    updatePageInfo = true;
                                }
                                else if (lastColumn.StartsWith(EDITABLEREGIONPREFIX, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var id = lastColumn.Substring(EDITABLEREGIONPREFIX.Length);
                                    targetPageInfo.EditableRegions[id] = translation;
                                    updatePageInfo = true;
                                }
                                else if (lastColumn.StartsWith(WEBPARTIDPREFIX, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Web part property
                                    string id = lastColumn.Substring(WEBPARTIDPREFIX.Length);
                                    string[] parts = id.Split('/');
                                    if (parts.Length == 2)
                                    {
                                        if (templateInstance != null)
                                        {
                                            if (SetComponentProperty(webPartsTable, webPartFormDefinitionsTable, parts, templateInstance, translation, sourceLang, targetLang, lastColumn))
                                            {
                                                updatePageInfo = true;
                                            }
                                        }
                                    }
                                }
                                else if (lastColumn.StartsWith(WIDGETIDPREFIX, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Widget property
                                    string id = lastColumn.Substring(WIDGETIDPREFIX.Length);
                                    string[] parts = id.Split('/');
                                    if (parts.Length == 2)
                                    {
                                        ViewModeEnum viewMode = PortalContext.ViewMode;
                                        PortalContext.ViewMode = ViewModeEnum.Edit;
                                        var combinedTemplateInstance = targetPageInfo.TemplateInstance;
                                        PortalContext.ViewMode = viewMode;

                                        if (combinedTemplateInstance != null)
                                        {
                                            if (SetComponentProperty(webPartsTable, webPartFormDefinitionsTable, parts, combinedTemplateInstance, translation, sourceLang, targetLang, WIDGETIDPREFIX, false))
                                            {
                                                updatePageInfo = true;
                                            }
                                        }
                                    }
                                }
                                else if (lastColumn.StartsWith(DEFAULTWIDGETIDPREFIX, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    // Default widget property
                                    string id = lastColumn.Substring(DEFAULTWIDGETIDPREFIX.Length);
                                    string[] parts = id.Split('/');
                                    if (parts.Length == 2)
                                    {
                                        if (templateInstance != null)
                                        {
                                            if (SetComponentProperty(webPartsTable, webPartFormDefinitionsTable, parts, templateInstance, translation, sourceLang, targetLang, DEFAULTWIDGETIDPREFIX))
                                            {
                                                updatePageInfo = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    var useResourceString = (targetNode == null);

                                    // Get maximal allowed length of the field
                                    int fieldLength = GetFieldLength(formFields.Single(f => string.Equals(f.Name, lastColumn, StringComparison.InvariantCultureIgnoreCase)), useResourceString);

                                    // Skip checking length of the DocumentName because length of this column is handled automatically within saving the document
                                    if (!useResourceString && !string.Equals(lastColumn, "DocumentName", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        CheckValidLength(translation.Length, fieldLength, lastColumn);
                                    }

                                    if (TranslateField(obj, lastColumn, translation, sourceLang, targetLang, fieldLength, useResourceString))
                                    {
                                        updateObject = true;
                                    }
                                }
                            }
                            break;

                        case "internal-file":
                            if ((lastNode != "bin-target") || !SettingsKeyInfoProvider.GetBoolValue(targetNode.NodeSiteName + ".CMSAllowAttachmentTranslation"))
                            {
                                break;
                            }

                            string translationXml = xml.ReadInnerXml();
                            byte[] data = Convert.FromBase64String(translationXml);

                            DocumentAttachment newAtt = null;

                            // Binary data
                            string[] param = lastColumn.Split(';');
                            if (param.Length == 2)
                            {
                                Guid guid = ValidationHelper.GetGuid(param[1], Guid.Empty);

                                // Get the original attachment and create the copy with new data attached to the new target document
                                var origAtt = DocumentHelper.GetAttachment(node, guid);
                                newAtt = (origAtt != null) ? origAtt.Clone(true) : new DocumentAttachment();

                                newAtt.AttachmentGUID = Guid.Empty;
                                newAtt.AttachmentSiteID = targetNode.NodeSiteID;
                                newAtt.AttachmentBinary = data;
                                newAtt.AttachmentDocumentID = targetNode.DocumentID;
                                newAtt.AttachmentSize = data.Length;

                                if (!String.IsNullOrEmpty(mimeType) && ImageHelper.IsMimeImage(mimeType))
                                {
                                    // Update image dimensions
                                    var imageData = new ImageHelper(data);
                                    if (imageData != null)
                                    {
                                        newAtt.AttachmentImageHeight = imageData.ImageHeight;
                                        newAtt.AttachmentImageWidth = imageData.ImageWidth;
                                    }
                                }
                            }

                            if (lastColumn.StartsWith("cms.attachment;", StringComparison.InvariantCultureIgnoreCase))
                            {
                                DocumentHelper.AddAttachment(targetNode, null, Guid.Empty, newAtt.AttachmentGroupGUID, newAtt);
                            }
                            else
                            {
                                DocumentHelper.AddAttachment(targetNode, param[0], Guid.Empty, newAtt.AttachmentGroupGUID, newAtt);
                                updateObject = true;
                            }
                            break;
                    }

                    lastNode = xmlNameLower;
                }

                // Update info about editable regions & web parts
                if (updatePageInfo && (targetPageInfo != null))
                {
                    SaveTranslatedTemplate(targetNode, targetPageInfo, webPartsTable);
                }

                // Save changes of fields to resource strings
                var forceUpdate = (handler != null) && handler.EventArguments.ForceTargetObjectUpdate;
                if (forceUpdate || updateObject || updatePageInfo)
                {
                    if (info != null)
                    {
                        info.Generalized.SetObject();
                    }
                    else
                    {
                        SaveDocument(targetNode, tree);
                    }
                }
            }
            finally
            {
                // Dispose the handler
                if (handler != null)
                {
                    handler.Dispose();
                }
            }

            if (info != null)
            {
                return info;
            }

            return targetNode;
        }


        /// <summary>
        /// Checks license limitation.
        /// </summary>
        public static void CheckLicense()
        {
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain) && !LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.TranslationServices))
            {
                throw new LicenseException("You don't have valid license for Translation Services.");
            }
        }

        #endregion


        #region "Machine translation methods"

        /// <summary>
        /// Translates given object using machine translation service. Returns true if translation went ok, false if there was any error.
        /// </summary>
        /// <param name="service">Machine translation service to use to translate the object</param>
        /// <param name="obj">Object to translate</param>
        /// <param name="settings">Translation settings</param>
        public static bool Translate(AbstractMachineTranslationService service, TranslationSettings settings, ICMSObject obj)
        {
            if ((service == null) || (obj == null) || (settings == null))
            {
                return false;
            }

            // Check if service is available (returns false if service is not responding due to invalid credentials for example)
            if (!service.IsAvailable())
            {
                return false;
            }

            var node = obj as TreeNode;
            return node != null ? TranslateDocument(service, settings, node) : TranslateInfo(service, settings, (BaseInfo)obj);
        }


        /// <summary>
        /// Translates given object using machine translation service. Returns true if translation went ok, false if there was any error.
        /// </summary>
        /// <param name="service">Machine translation service to use to translate the object</param>
        /// <param name="info">Info object to translate</param>
        /// <param name="settings">Translation settings</param>
        private static bool TranslateInfo(AbstractMachineTranslationService service, TranslationSettings settings, BaseInfo info)
        {
            CheckLicense();

            if ((info == null) || (service == null) || (settings == null))
            {
                return false;
            }

            string sourceLang = settings.SourceLanguage;
            if (string.IsNullOrEmpty(sourceLang))
            {
                sourceLang = CultureHelper.DefaultUICultureCode;
            }

            // Translate the fields
            var fi = FormHelper.GetFormInfo(info.TypeInfo.ObjectClassName, false);
            if (fi == null)
            {
                return true;
            }

            var fields = fi.GetFields(true, true);
            if (TranslateFields(info, fields, service, sourceLang, settings.TargetLanguages))
            {
                info.Generalized.SetObject();
            }
            return true;
        }


        /// <summary>
        /// Translates given object using machine translation service. Returns true if translation went ok, false if there was any error.
        /// </summary>
        /// <param name="service">Machine translation service to use to translate the object</param>
        /// <param name="settings">Translation settings</param>
        /// <param name="node">Document to translate</param>
        private static bool TranslateDocument(AbstractMachineTranslationService service, TranslationSettings settings, TreeNode node)
        {
            CheckLicense();

            if ((node == null) || (service == null) || (settings == null))
            {
                return false;
            }

            TreeProvider tree = node.TreeProvider;

            string sourceLang = settings.SourceLanguage;
            if (string.IsNullOrEmpty(sourceLang))
            {
                sourceLang = CultureHelper.DefaultUICultureCode;
            }

            foreach (var targetLang in settings.TargetLanguages)
            {
                TreeNode target = CreateTargetCultureNode(node, targetLang, false, true, !settings.TranslateAttachments);

                bool saveNode = false;
                // Translate the fields
                if (settings.TranslateDocCoupledData)
                {
                    var fields = GetDocumentFields(node);
                    if (TranslateFields(target, fields, service, sourceLang, new HashSet<string> { targetLang }, false))
                    {
                        saveNode = true;
                    }
                }

                bool updatePageInfo = false;

                // Load page info from node
                PageInfo pageInfo = new PageInfo();
                pageInfo.LoadVersion(target);

                // Export editable regions
                if (settings.TranslateEditableItems)
                {
                    EditableItems items = target.DocumentContent;
                    foreach (string item in items.ColumnNames)
                    {
                        // Save the value
                        if (!pageInfo.UsedPageTemplateInfo.IsAspx)
                        {
                            // Save into the editable web parts in the Portal engine mode
                            pageInfo.EditableWebParts[item] = service.Translate(items[item], sourceLang, targetLang);
                        }
                        else
                        {
                            if (pageInfo.EditableWebParts.ContainsKey(item))
                            {
                                // Save into the editable web parts in the Portal engine mode for mixed templates
                                pageInfo.EditableWebParts[item] = service.Translate(items[item], sourceLang, targetLang);
                            }
                            else
                            {
                                // Save into the editable regions in the ASPX mode
                                pageInfo.EditableRegions[item] = service.Translate(items[item], sourceLang, targetLang);
                            }
                        }

                        updatePageInfo = true;
                    }
                }

                // Export web part properties
                if (settings.TranslateWebpartProperties)
                {
                    ViewModeEnum viewMode = PortalContext.ViewMode;
                    PortalContext.ViewMode = ViewModeEnum.Edit;
                    PageTemplateInstance template = pageInfo.TemplateInstance;
                    PortalContext.ViewMode = viewMode;

                    if (template != null)
                    {
                        List<WebPartZoneInstance> zones = template.WebPartZones;
                        foreach (WebPartZoneInstance zone in zones)
                        {
                            // Process only editor widgets
                            if (zone.WidgetZoneType != WidgetZoneTypeEnum.Editor)
                            {
                                continue;
                            }

                            foreach (WebPartInstance webPart in zone.WebParts)
                            {
                                var formInfo = GetComponentFormInfo(webPart);
                                if (formInfo == null)
                                {
                                    continue;
                                }

                                var fields = formInfo.GetFields(true, true);
                                foreach (FormFieldInfo field in fields)
                                {
                                    if (!IsFieldForTranslation(webPart, field))
                                    {
                                        continue;
                                    }

                                    string tempVal = ValidationHelper.GetString(webPart.GetValue(field.Name), "");
                                    if (string.IsNullOrEmpty(tempVal))
                                    {
                                        continue;
                                    }

                                    string origValue = GetTranslationValue(tempVal, sourceLang);
                                    var translation = service.Translate(origValue, sourceLang, targetLang);
                                    var maxFieldLength = GetFieldLength(field, false);

                                    // Check if translation length is not exceeded field length
                                    CheckValidLength(translation.Length, maxFieldLength, field.Name, webPart.ControlID);

                                    webPart.SetValue(field.Name, translation);
                                    updatePageInfo = true;
                                }
                            }
                        }
                    }

                    // Process web parts and default editor widgets
                    if (pageInfo.UsedPageTemplateInfo != null)
                    {
                        template = pageInfo.UsedPageTemplateInfo.TemplateInstance;
                        if (template != null)
                        {
                            List<WebPartZoneInstance> zones = template.WebPartZones;
                            foreach (WebPartZoneInstance zone in zones)
                            {
                                // Process web parts and default editor widgets
                                if ((zone.WidgetZoneType != WidgetZoneTypeEnum.Editor) && (zone.WidgetZoneType != WidgetZoneTypeEnum.None))
                                {
                                    continue;
                                }

                                foreach (WebPartInstance webPart in zone.WebParts)
                                {
                                    var formInfo = GetComponentFormInfo(webPart);
                                    if (formInfo == null)
                                    {
                                        continue;
                                    }

                                    var fields = formInfo.GetFields(true, true);
                                    foreach (FormFieldInfo field in fields)
                                    {
                                        if (!IsFieldForTranslation(webPart, field))
                                        {
                                            continue;
                                        }

                                        string tempVal = ValidationHelper.GetString(webPart.GetValue(field.Name), "");
                                        if (string.IsNullOrEmpty(tempVal))
                                        {
                                            continue;
                                        }

                                        string origValue = GetTranslationValue(tempVal, sourceLang);
                                        var translation = service.Translate(origValue, sourceLang, targetLang);
                                        var maxFieldLength = GetFieldLength(field, true);

                                        webPart.SetValue(field.Name, TranslateString(origValue, translation, sourceLang, targetLang, maxFieldLength));
                                        updatePageInfo = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (updatePageInfo)
                {
                    SaveTranslatedTemplate(target, pageInfo);
                    saveNode = true;
                }

                if (saveNode)
                {
                    SaveDocument(target, tree);
                }
            }

            return true;
        }


        /// <summary>
        /// Translates given object using specified machine translation service. Returns true if any field was updated.
        /// </summary>
        /// <param name="obj">Object to translate</param>
        /// <param name="fields">List of fields</param>
        /// <param name="service">Machine translation service to use</param>
        /// <param name="sourceLanguage">Source language of the data</param>
        /// <param name="targetLanguages">Target language(s) of the data</param>
        /// <param name="useResourceString">If true, field is translated using {$ $} macro, if false, translated value is saved directly in the field</param>
        private static bool TranslateFields(ICMSObject obj, IEnumerable<FormFieldInfo> fields, AbstractMachineTranslationService service, string sourceLanguage, HashSet<string> targetLanguages, bool useResourceString = true)
        {
            bool result = false;

            foreach (FormFieldInfo field in fields)
            {
                if (!field.TranslateField || !IsFieldDataTypeAllowedForTranslation(field.DataType))
                {
                    continue;
                }

                // Do not process binary files
                if ((field.DataType == FieldDataType.Binary) || (field.DataType == FieldDataType.File))
                {
                    continue;
                }

                foreach (var targetLanguage in targetLanguages)
                {
                    string name = field.Name;
                    string value = null;
                    bool isCodeNameColumn = false;

                    var info = obj as BaseInfo;
                    if (info != null)
                    {
                        var codeNameColumn = info.Generalized.CodeNameColumn;
                        if (string.Equals(codeNameColumn, name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            isCodeNameColumn = true;
                        }
                        else
                        {
                            value = ValidationHelper.GetString(info.Generalized.GetValueForTranslation(name), "");
                        }
                    }
                    else
                    {
                        value = ValidationHelper.GetString(obj.GetValue(name), "");
                    }

                    // Do not translate code name column of Info objects
                    if (isCodeNameColumn)
                    {
                        continue;
                    }

                    var fieldLength = GetFieldLength(field, useResourceString);

                    value = GetTranslationValue(value, sourceLanguage);
                    var translation = service.Translate(value, sourceLanguage, targetLanguage);

                    var skipLengthCheck = false;
                    var node = obj as TreeNode;
                    if (node != null)
                    {
                        // Special handling of the DocumentName which is cut to allowed length automatically within updating document
                        skipLengthCheck = string.Equals(name, "DocumentName", StringComparison.InvariantCultureIgnoreCase);
                    }

                    if (!skipLengthCheck && !useResourceString)
                    {
                        CheckValidLength(translation.Length, fieldLength, name);
                    }

                    if (TranslateField(obj, name, translation, sourceLanguage, targetLanguage, fieldLength, useResourceString))
                    {
                        result = true;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Checks if translation length is not longer than allowed field length else throws an exception
        /// </summary>
        /// <param name="translationLength">Length of the translation</param>
        /// <param name="columnLength">Maximal length of the column </param>
        /// <param name="columnName">Column name</param>
        /// <param name="componentName">Name of the component which field is checked</param>
        private static void CheckValidLength(int translationLength, int columnLength, string columnName, string componentName = null)
        {
            if ((columnLength <= 0) || (translationLength <= columnLength))
            {
                return;
            }

            // Throw an error when resource string are not used because inserting longer value than allowed into field may lead to SQL exception (database column size)
            var useGeneralMessage = String.IsNullOrEmpty(componentName);
            var resourceString = useGeneralMessage ? "translations.translationistoolong" : "translations.componenttranslationistoolong";
            var localizedString = CoreServices.Localization.GetString(resourceString);

            string error = useGeneralMessage
                ? String.Format(localizedString, columnName, columnLength, translationLength)
                : String.Format(localizedString, columnName, componentName, columnLength, translationLength);

            throw new ArgumentException(error);
        }

        #endregion


        #region "Submission operation methods"

        /// <summary>
        /// Cancels given submission
        /// </summary>
        /// <param name="submissionInfo">Submission info</param>
        public static string CancelSubmission(TranslationSubmissionInfo submissionInfo)
        {
            if (submissionInfo == null)
            {
                return null;
            }

            var service = TranslationServiceInfoProvider.GetTranslationServiceInfo(submissionInfo.SubmissionServiceID);
            if (service == null)
            {
                return null;
            }

            if (!service.TranslationServiceSupportsCancel)
            {
                return String.Format(ResHelper.GetString("translationservice.cancelnotsupported"), service.TranslationServiceDisplayName);
            }

            string error = String.Empty;
            try
            {
                if (!service.TranslationServiceIsMachine)
                {
                    var humanService = AbstractHumanTranslationService.GetTranslationService(service, SiteInfoProvider.GetSiteName(submissionInfo.SubmissionSiteID));
                    if (humanService != null)
                    {
                        error = humanService.CancelSubmission(submissionInfo);
                        if (!String.IsNullOrEmpty(error))
                        {
                            error = ResHelper.GetString("ContentRequest.TranslationFailed") + " \"" + error + "\"";
                        }
                    }
                }

                // Update status
                submissionInfo.SubmissionStatus = TranslationStatusEnum.TranslationCanceled;
                TranslationSubmissionInfoProvider.SetTranslationSubmissionInfo(submissionInfo);

                if (DocumentActionContext.CurrentResetIsWaitingForTranslationFlag)
                {
                    // Remove "Waiting for translation" flag from documents involved in translation submission
                    var where = new WhereCondition()
                        .WhereEquals("DocumentIsWaitingForTranslation", true)
                        .WhereIn("DocumentID", TranslationSubmissionItemInfoProvider.GetTranslationSubmissionItems()
                                                                     .Column("SubmissionItemTargetObjectID")
                                                                     .WhereEquals("SubmissionItemSubmissionID", submissionInfo.SubmissionID)
                                                                     .WhereEquals("SubmissionItemObjectType", "cms.document"));

                    DocumentHelper.ChangeDocumentCultureDataField("DocumentIsWaitingForTranslation", false, where);
                }
            }
            catch (Exception ex)
            {
                LogEvent(ex);
                return ex.Message;
            }

            return error;
        }


        /// <summary>
        /// Generates XLIFF documents again and resubmits given submission to the service provider. Returns null if everything went ok, otherwise return error message.
        /// </summary>
        /// <param name="submissionInfo">Submission info</param>
        public static string ResubmitSubmission(TranslationSubmissionInfo submissionInfo)
        {
            if (submissionInfo == null)
            {
                return null;
            }

            var info = TranslationServiceInfoProvider.GetTranslationServiceInfo(submissionInfo.SubmissionServiceID);
            if ((info == null) || (info.TranslationServiceIsMachine))
            {
                return null;
            }

            var service = AbstractHumanTranslationService.GetTranslationService(info, SiteInfoProvider.GetSiteName(submissionInfo.SubmissionSiteID));
            if (service == null)
            {
                return null;
            }

            int totalCharCount = 0;
            int totalWordCount = 0;
            string error = null;
            bool success = false;

            try
            {
                // Set status of the translation if not already set
                if (submissionInfo.SubmissionStatus != TranslationStatusEnum.ResubmittingSubmission)
                {
                    SaveSubmissionInfo(submissionInfo, TranslationStatusEnum.ResubmittingSubmission);
                }

                // Prepare settings
                var settings = PrepareTranslationSettings(submissionInfo);

                var condition = new WhereCondition()
                    .WhereEquals("SubmissionItemSubmissionID", submissionInfo.SubmissionID)
                    .WhereEquals("SubmissionItemObjectType", "cms.document");

                var columns = ObjectTypeManager.GetColumnNames(TranslationSubmissionItemInfo.OBJECT_TYPE);

                // We don't need these columns from database
                columns.Remove("SubmissionItemSourceXLIFF");
                columns.Remove("SubmissionItemTargetXLIFF");

                // Update all translation items with new XLIFF files
                var data = TranslationSubmissionItemInfoProvider.GetTranslationSubmissionItems(condition.ToString(true), null, -1, string.Join(",", columns));
                if (DataHelper.DataSourceIsEmpty(data))
                {
                    return null;
                }

                var tree = new TreeProvider();
                var grpSourceLanguage = data.ToList().GroupBy(i => i.SubmissionItemObjectID);
                foreach (var language in grpSourceLanguage)
                {
                    // Get document once for all target languages
                    int documentId = language.Key;
                    var node = DocumentHelper.GetDocument(documentId, tree);
                    if (node == null)
                    {
                        // Get information of the non-existing document from first target language item (data are same for all targets, so take the first)
                        var item = language.First();

                        // Get number of target cultures
                        int cnt = language.Count();

                        // Ensure characters and words count correctly
                        totalCharCount += item.SubmissionItemCharCount * cnt;
                        totalWordCount += item.SubmissionItemWordCount * cnt;

                        LogPageNotExists("RESUBMITSUBMISSION", item.SubmissionItemName);

                        // Skip processing submission item which doesn't exist anymore.
                        continue;
                    }

                    foreach (var submission in language)
                    {
                        int charCount, wordCount;
                        submission.SubmissionItemName = node.GetDocumentName();
                        submission.SubmissionItemTargetXLIFF = null; // Clear target XLIFF explicitly
                        submission.SubmissionItemSourceXLIFF = GetXLIFF(node, settings, submission.SubmissionItemTargetCulture, out wordCount, out charCount);
                        submission.SubmissionItemCharCount = charCount;
                        submission.SubmissionItemWordCount = wordCount;

                        totalCharCount += charCount;
                        totalWordCount += wordCount;

                        // Do not touch parent, submission is updated at the end of process
                        using (new CMSActionContext { TouchParent = false })
                        {
                            submission.Update();
                        }
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                // Set status of the translation
                SaveSubmissionInfo(submissionInfo, TranslationStatusEnum.SubmissionError);

                LogEvent(ex);
                return ex.Message;
            }
            finally
            {
                // Update counts
                submissionInfo.SubmissionWordCount = totalWordCount;
                submissionInfo.SubmissionCharCount = totalCharCount;

                // Submit updated XLIFF files to a translation service provider
                if (success)
                {
                    error = service.CreateSubmission(submissionInfo);
                }

                // Set status of the translation
                SaveSubmissionInfo(submissionInfo, String.IsNullOrEmpty(error) ? TranslationStatusEnum.WaitingForTranslation : TranslationStatusEnum.SubmissionError, error);

                if (!String.IsNullOrEmpty(error))
                {
                    error = ResHelper.GetString("ContentRequest.TranslationFailed") + " \"" + error + "\"";
                }
            }

            return error;
        }


        /// <summary>
        /// Sets submission status and updates info in database.
        /// </summary>
        /// <param name="submissionInfo">Submission info</param>
        /// <param name="status">Translation status</param>
        /// <param name="message">Translation status message</param>
        private static void SaveSubmissionInfo(TranslationSubmissionInfo submissionInfo, TranslationStatusEnum status, string message = null)
        {
            submissionInfo.SubmissionStatus = status;
            submissionInfo.SubmissionStatusMessage = message;

            TranslationSubmissionInfoProvider.SetTranslationSubmissionInfo(submissionInfo);
        }


        /// <summary>
        /// Prepares translations settings
        /// </summary>
        /// <param name="submissionInfo">Submission info object</param>
        private static TranslationSettings PrepareTranslationSettings(TranslationSubmissionInfo submissionInfo)
        {
            var settings = new TranslationSettings
            {
                TranslateAttachments = submissionInfo.SubmissionTranslateAttachments,
                TranslationDeadline = submissionInfo.SubmissionDeadline,
                SourceLanguage = submissionInfo.SubmissionSourceCulture,
                Instructions = submissionInfo.SubmissionInstructions,
                Priority = submissionInfo.SubmissionPriority
            };

            settings.TargetLanguages.AddRangeToSet(submissionInfo.SubmissionTargetCultures);

            // Propagate service name
            var service = TranslationServiceInfoProvider.GetTranslationServiceInfo(submissionInfo.SubmissionServiceID);
            if (service != null)
            {
                settings.TranslationServiceName = service.TranslationServiceName;
            }
            return settings;
        }


        /// <summary>
        /// Processes all the translations within the submission. Returns null if everything went ok, otherwise return error message.
        /// </summary>
        /// <param name="submissionInfo">Submission to process</param>
        public static string ProcessSubmission(TranslationSubmissionInfo submissionInfo)
        {
            if (submissionInfo == null)
            {
                return null;
            }

            string processedItem = String.Empty;
            try
            {
                // Set status of the translation if not already set
                if (submissionInfo.SubmissionStatus != TranslationStatusEnum.ProcessingSubmission)
                {
                    SaveSubmissionInfo(submissionInfo, TranslationStatusEnum.ProcessingSubmission);
                }

                // Get the submission items
                var data = TranslationSubmissionItemInfoProvider.GetTranslationSubmissionItems("SubmissionItemSubmissionID = " + submissionInfo.SubmissionID, null, -1, "SubmissionItemTargetXLIFF, SubmissionItemTargetCulture, SubmissionItemName");
                if (!DataHelper.DataSourceIsEmpty(data))
                {
                    foreach (DataRow dr in data.Tables[0].Rows)
                    {
                        string xliff = ValidationHelper.GetString(dr["SubmissionItemTargetXLIFF"], String.Empty);
                        processedItem = String.Format("{0} ({1})", ValidationHelper.GetString(dr["SubmissionItemName"], String.Empty), ValidationHelper.GetString(dr["SubmissionItemTargetCulture"], String.Empty));
                        try
                        {
                            if (ProcessTranslation(new SystemIO.MemoryStream(GetTranslationsEncoding(submissionInfo.SubmissionSiteID).GetBytes(xliff)), submissionInfo) == null)
                            {
                                SaveSubmissionInfo(submissionInfo, TranslationStatusEnum.ProcessingError);
                                return String.Format(ResHelper.GetString("translationservice.translationsimportfail"), HTMLHelper.HTMLEncode(processedItem));
                            }
                        }
                        catch (TargetDocumentNotExistsException)
                        {
                            LogPageNotExists("PROCESSSUBMISSION", processedItem);
                        }
                    }
                }

                SaveSubmissionInfo(submissionInfo, TranslationStatusEnum.TranslationCompleted);
            }
            catch (Exception ex)
            {
                LogEvent(ex);

                SaveSubmissionInfo(submissionInfo, TranslationStatusEnum.ProcessingError);
                return String.Format(ResHelper.GetString("translationservice.errorwhileprocessingdocument"), HTMLHelper.HTMLEncode(processedItem)) + " \"" + ex.Message + "\"";
            }

            return null;
        }


        /// <summary>
        /// Submits the node for translation. If the service is human translation service, creates the whole submission (submission will contain only specified document).
        /// </summary>
        /// <param name="settings">Translations settings</param>
        /// <param name="sourceNode">Node to submit (source language node with all necessary data)</param>
        /// <param name="submissionInfo">Submission info</param>
        /// <returns>Null if operation succeeded, otherwise returns error message.</returns>
        public static string SubmitToTranslation(TranslationSettings settings, TreeNode sourceNode, out TranslationSubmissionInfo submissionInfo)
        {
            CheckLicense();

            submissionInfo = null;

            if (settings == null)
            {
                return null;
            }

            var serviceInfo = TranslationServiceInfoProvider.GetTranslationServiceInfo(settings.TranslationServiceName);
            if (serviceInfo == null)
            {
                return null;
            }

            if (serviceInfo.TranslationServiceIsMachine)
            {
                try
                {
                    var service = AbstractMachineTranslationService.GetTranslationService(serviceInfo, sourceNode.NodeSiteName);
                    if (service != null)
                    {
                        if (Translate(service, settings, sourceNode))
                        {
                            return null;
                        }

                        return ResHelper.GetString("translationservice.translationfailed");
                    }
                }
                catch (Exception ex)
                {
                    LogEvent(ex);
                    return ex.Message;
                }
            }
            else
            {
                string err = null;
                try
                {
                    var service = AbstractHumanTranslationService.GetTranslationService(serviceInfo, sourceNode.NodeSiteName);
                    if (service != null)
                    {
                        var sourceLanguage = settings.SourceLanguage;
                        if (!service.CheckSourceLanguageAvailability(sourceLanguage))
                        {
                            return String.Format(ResHelper.GetString("translationservice.sourcelanguagenotsupported"), sourceLanguage);
                        }

                        // Check available languages
                        var unavailableLanguages = service.CheckTargetLanguagesAvailability(settings.TargetLanguages);
                        if (unavailableLanguages.Count > 0)
                        {
                            return String.Format(ResHelper.GetString("translationservice.targetlanguagenotsupported"), String.Join(", ", unavailableLanguages));
                        }

                        // Create submission in database
                        submissionInfo = CreateSubmissionInfo(settings, serviceInfo, sourceNode.TreeProvider.UserInfo.UserID, sourceNode.NodeSiteID, sourceNode.GetDocumentName());
                        submissionInfo.SubmissionParameter = sourceNode.NodeAlias;

                        // Create submission item per target culture
                        foreach (string targetLanguage in settings.TargetLanguages)
                        {
                            // Create a copy of the document in target language version and set WaitingForTranslation status
                            var targetNode = CreateTargetCultureNode(sourceNode, targetLanguage, true, false, !settings.TranslateAttachments);

                            // Create submission item
                            var submissionItem = CreateSubmissionItemInfo(settings, submissionInfo, sourceNode, targetNode.DocumentID, targetLanguage);

                            // Update the counts
                            submissionInfo.SubmissionItemCount++;
                            submissionInfo.SubmissionCharCount += submissionItem.SubmissionItemCharCount;
                            submissionInfo.SubmissionWordCount += submissionItem.SubmissionItemWordCount;
                        }

                        // Call service
                        err = service.CreateSubmission(submissionInfo);

                        // Return error message
                        if (!String.IsNullOrEmpty(err))
                        {
                            return ResHelper.GetString("ContentRequest.TranslationFailed") + " \"" + err + "\"";
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogEvent(ex);
                    return ex.Message;
                }
                finally
                {
                    // Update submission state if already created
                    if (submissionInfo != null)
                    {
                        if (string.IsNullOrEmpty(err))
                        {
                            // Save submission with ticket and the counts
                            TranslationSubmissionInfoProvider.SetTranslationSubmissionInfo(submissionInfo);
                        }
                        else
                        {
                            SaveSubmissionInfo(submissionInfo, TranslationStatusEnum.SubmissionError, err);
                        }
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Creates new submission item within given submission (saves it to DB also) and returns the object.
        /// </summary>
        /// <param name="settings">Translation settings</param>
        /// <param name="submission">Submission to which the item belongs</param>
        /// <param name="sourceNode">Node tu submit</param>
        /// <param name="targetDocId">Document ID of the target culture node (0 if does not exist)</param>
        public static TranslationSubmissionItemInfo CreateSubmissionItemInfo(TranslationSettings settings, TranslationSubmissionInfo submission, TreeNode sourceNode, int targetDocId, string targetLanguage)
        {
            return CreateSubmissionItemInfo(settings, submission, sourceNode, 0, Guid.NewGuid(), targetDocId, targetLanguage, String.Empty);
        }


        /// <summary>
        /// Creates new submission item within given submission (saves it to DB also) and returns the object.
        /// </summary>
        /// <param name="settings">Translation settings</param>
        /// <param name="submission">Submission to which the item belongs</param>
        /// <param name="sourceNode">Node to submit</param>
        /// <param name="originalGuid">GUID of the submission item</param>
        /// <param name="originalId">ID of the submission item</param>
        /// <param name="targetDocId">Document ID of the target culture node (0 if does not exist)</param>
        /// <param name="originalDocumentTicket">Original document ticket of re-submitted document</param>
        /// <param name="targetLanguage">Target language</param>
        private static TranslationSubmissionItemInfo CreateSubmissionItemInfo(TranslationSettings settings, TranslationSubmissionInfo submission, TreeNode sourceNode, int originalId, Guid originalGuid, int targetDocId, string targetLanguage, string originalDocumentTicket)
        {
            TranslationSubmissionItemInfo submissionItem;

            // Handle the event
            using (var h = TranslationEvents.CreateSubmissionItem.StartEvent(settings, submission, sourceNode, targetDocId, targetLanguage))
            {
                h.DontSupportCancel();

                int charCount, wordCount;
                submissionItem = new TranslationSubmissionItemInfo
                {
                    SubmissionItemID = originalId,
                    SubmissionItemGUID = originalGuid,
                    SubmissionItemObjectType = "cms.document",
                    SubmissionItemObjectID = sourceNode.DocumentID,
                    SubmissionItemSubmissionID = submission.SubmissionID,
                    SubmissionItemName = sourceNode.GetDocumentName(),
                    SubmissionItemTargetObjectID = targetDocId,
                    SubmissionItemType = XLIFFEXTENSION,
                    SubmissionItemTargetCulture = targetLanguage,
                    SubmissionItemSourceXLIFF = GetXLIFF(sourceNode, settings, targetLanguage, out wordCount, out charCount),
                    SubmissionItemCharCount = charCount,
                    SubmissionItemWordCount = wordCount,
                    SubmissionItemCustomData = originalDocumentTicket
                };

                submissionItem.Insert();

                h.EventArguments.SubmissionItem = submissionItem;
                h.FinishEvent();
            }

            return submissionItem;
        }


        /// <summary>
        /// Creates new submission to queue (saves it to DB also) and returns the object.
        /// </summary>
        /// <param name="settings">Translation settings</param>
        /// <param name="info">Translation service info</param>
        /// <param name="userId">ID of the user who created submission</param>
        /// <param name="siteId">ID of the site the submission belongs to</param>
        /// <param name="submissionName">Name of the submission</param>
        public static TranslationSubmissionInfo CreateSubmissionInfo(TranslationSettings settings, TranslationServiceInfo info, int userId, int siteId, string submissionName)
        {
            var submission = new TranslationSubmissionInfo
            {
                SubmissionName = submissionName,
                SubmissionDate = DateTime.Now,
                SubmissionItemCount = 0,
                SubmissionPriority = settings.Priority,
                SubmissionInstructions = settings.Instructions,
                SubmissionDeadline = settings.TranslationDeadline,
                SubmissionTranslateAttachments = settings.TranslateAttachments,
                SubmissionServiceID = info.TranslationServiceID,
                SubmissionSourceCulture = settings.SourceLanguage,
                SubmissionStatus = TranslationStatusEnum.WaitingForTranslation,
                SubmissionTargetCulture = settings.TargetLanguages.Join(";"),
                SubmissionSiteID = (siteId > 0) ? siteId : SiteContext.CurrentSiteID,
                SubmissionSubmittedByUserID = (userId > 0) ? userId : MembershipContext.AuthenticatedUser.UserID
            };

            submission.Insert();

            return submission;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Gets culture code for the translation process
        /// </summary>
        /// <param name="cultureCode">Culture code to map</param>
        /// <param name="direction">Direction of the mapping</param>
        public static string GetCultureCode(string cultureCode, TranslationCultureMappingDirectionEnum direction)
        {
            var arguments = TranslationEvents.MapCultureCode.StartEvent(cultureCode, cultureCode, direction);
            return direction == TranslationCultureMappingDirectionEnum.ServiceToSystem ? arguments.SystemCultureCode : arguments.ServiceCultureCode;
        }


        /// <summary>
        /// Gets list of fields to be translated
        /// </summary>
        /// <param name="node">Document</param>
        private static HashSet<FormFieldInfo> GetDocumentFields(TreeNode node)
        {
            var fields = new HashSet<FormFieldInfo>();

            // Add coupled data
            var formInfo = FormHelper.GetFormInfo(node.NodeClassName, false);
            if (formInfo != null)
            {
                fields.AddRangeToSet(formInfo.GetFields(true, true));
            }

            // Add general document fields, definitions from coupled class have higher priority
            formInfo = FormHelper.GetFormInfo("cms.document", false);
            if (formInfo != null)
            {
                fields = fields.Concat(formInfo.GetFields(true, true))
                               .GroupBy(item => item.Name)
                               .Select(group => group.First()).ToHashSetCollection();
            }

            return fields;
        }


        /// <summary>
        /// Gets allowed extensions for attachments that should be included in XLIFF
        /// </summary>
        /// <param name="siteName">Node site name</param>
        private static bool IsExtensionAllowed(string extension, string siteName)
        {
            if (allowedAttachmentExtensions == null)
            {
                allowedAttachmentExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var allowedFileTypes = ValidationHelper.GetString(SettingsKeyInfoProvider.GetValue(siteName + ".CMSTranslateFileTypes"), "")
                                                       .Split(';')
                                                       .Select(ex => ex.Trim().Replace(".", ""))
                                                       .Where(ex => !string.IsNullOrWhiteSpace(ex));
                allowedAttachmentExtensions.AddRangeToSet(allowedFileTypes);
            }

            return allowedAttachmentExtensions.Count == 0 || allowedAttachmentExtensions.Contains(extension.Replace(".", ""), StringComparer.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Gets submission file name with source and target cultures specified.
        /// </summary>
        /// <param name="submission">Translations submission</param>
        public static string GetSubmissionFileName(TranslationSubmissionInfo submission)
        {
            var name = new StringBuilder();
            if (String.IsNullOrEmpty(submission.SubmissionTicket))
            {
                // Target cultures
                var cultures = submission.SubmissionTargetCultures;
                var culturesCount = cultures.Count;
                var target = culturesCount == 1 ? cultures.First() : String.Format("{0} cultures", culturesCount);

                name.AppendFormat("{0} from {1} to {2}", DataHelper.GetNotEmpty(submission.SubmissionParameter, submission.SubmissionName), submission.SubmissionSourceCulture, target);
            }
            else
            {
                name.Append(submission.SubmissionTicket);
            }

            var fileName = name.ToString();
            var extension = fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? String.Empty : ".zip";

            return ValidationHelper.GetSafeFileName(fileName) + extension;
        }


        /// <summary>
        /// Processes the submission automatically if enabled. If auto import is disabled, does nothing.
        /// </summary>
        /// <param name="submission">Submission to process</param>
        public static string AutoImportSubmission(TranslationSubmissionInfo submission)
        {
            if (!AutoImportEnabled)
            {
                return null;
            }

            return ProcessSubmission(submission);
        }


        /// <summary>
        /// Returns encoding which should be used for translations (export/import of XLIFF) for given site.
        /// </summary>
        /// <param name="siteId">ID of the site</param>
        public static Encoding GetTranslationsEncoding(int siteId)
        {
            return GetTranslationsEncoding(SiteInfoProvider.GetSiteName(siteId));
        }


        /// <summary>
        /// Returns encoding which should be used for translations (export/import of XLIFF) for given site.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        public static Encoding GetTranslationsEncoding(string siteName)
        {
            Encoding encoding = null;
            string name = SettingsKeyInfoProvider.GetValue(siteName + ".CMSTranslationsEncoding");
            try
            {
                encoding = Encoding.GetEncoding(name);
            }
            catch
            {
                // Wrong encoding in settings
                LogError("Unable to get encoding from the settings.");
            }

            return encoding ?? Encoding.UTF8;
        }


        /// <summary>
        /// Returns absolute URL of the submission page where a translator can submit translated files.
        /// </summary>
        /// <param name="submission">Submission to create link for</param>
        public static string GetSubmissionLinkURL(TranslationSubmissionInfo submission)
        {
            if (submission == null)
            {
                return null;
            }

            string url = URLHelper.GetAbsoluteUrl(SUBMISSION_PAGE_URL) + "?submissionid=" + submission.SubmissionID;
            return URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(url, false));
        }


        /// <summary>
        /// Returns true if there is at least one translation submission item with target XLIFF for given page and submission is marked as ready.
        /// </summary>
        /// <param name="doc">Document to check</param>
        public static bool IsTranslationReady(TreeNode doc)
        {
            if (doc == null)
            {
                return false;
            }

            var query = GetLatestTranslatedSubmissionItemQuery(doc)
                .Columns("SubmissionItemSubmissionID");

            if (DataHelper.DataSourceIsEmpty(query))
            {
                return false;
            }

            // Get submission
            int submissionId = query.GetScalarResult<int>();
            var submission = TranslationSubmissionInfoProvider.GetTranslationSubmissionInfo(submissionId);

            return IsSubmissionReady(submission);
        }


        /// <summary>
        /// Checks if submission is ready to import the translation based on current submission status
        /// </summary>
        /// <param name="submission">Translation submission to check</param>
        internal static bool IsSubmissionReady(TranslationSubmissionInfo submission)
        {
            return (submission != null) && ((submission.SubmissionStatus == TranslationStatusEnum.TranslationReady));
        }


        /// <summary>
        /// Imports XLIFF documents from ZIP file. Returns list of unrecognized filenames, null if everything went ok.
        /// </summary>
        /// <param name="submissionInfo">Submission to export</param>
        /// <param name="inputStream">Input stream of a ZIP file</param>
        public static string ImportXLIFFfromZIP(TranslationSubmissionInfo submissionInfo, SystemIO.Stream inputStream)
        {
            string err = null;

            using (var zipArchive = new ZipArchive(inputStream, ZipArchiveMode.Read, true))
            {
                foreach(var zipEntry in zipArchive.Entries)
                {
                    string entryFileName = zipEntry.FullName;
                    if (entryFileName.EndsWith("." + XLIFFEXTENSION, StringComparison.OrdinalIgnoreCase))
                    {
                        // Process only .xliff files (ignore instructions and others)
                        entryFileName = Path.GetFileNameWithoutExtension(entryFileName);

                        string xliff;
                        using (var targetStream = new SystemIO.MemoryStream())
                        {
                            using (var zipEntryStream = zipEntry.Open())
                            {
                                zipEntryStream.CopyTo(targetStream);
                            }

                            xliff = GetTranslationsEncoding(submissionInfo.SubmissionSiteID).GetString(targetStream.ToArray());
                        }

                        bool imported = false;

                        if (!string.IsNullOrEmpty(xliff))
                        {
                            int index = entryFileName.LastIndexOf("_", StringComparison.Ordinal);
                            if ((index > 0) && (index < entryFileName.Length - 1))
                            {
                                int itemId = ValidationHelper.GetInteger(entryFileName.Substring(index + 1), 0);
                                TranslationSubmissionItemInfo item = TranslationSubmissionItemInfoProvider.GetTranslationSubmissionItemInfo(itemId);

                                if ((item != null) && (item.SubmissionItemSubmissionID == submissionInfo.SubmissionID))
                                {
                                    item.SubmissionItemTargetXLIFF = xliff;

                                    using (new CMSActionContext { TouchParent = false })
                                    {
                                        TranslationSubmissionItemInfoProvider.SetTranslationSubmissionItemInfo(item);
                                    }
                                    imported = true;
                                }
                            }
                        }

                        if (!imported)
                        {
                            err += ", " + entryFileName;
                        }
                    }
                }
            }

            if (String.IsNullOrEmpty(err))
            {
                return null;
            }

            // Update last modified date
            submissionInfo.SubmissionLastModified = DateTime.Now;
            submissionInfo.Update();

            return String.Format(ResHelper.GetString("translations.invalidfiles"), err.Substring(2));
        }


        /// <summary>
        /// Writes the XLIFF documents to translate into a zip file to a given stream.
        /// </summary>
        /// <param name="submissionInfo">Submission to export</param>
        /// <param name="outputStream">Stream to write the data to</param>
        public static void WriteSubmissionInZIP(TranslationSubmissionInfo submissionInfo, SystemIO.Stream outputStream)
        {
            if (submissionInfo == null)
            {
                return;
            }

            var data = TranslationSubmissionItemInfoProvider.GetTranslationSubmissionItems("SubmissionItemSubmissionID = " + submissionInfo.SubmissionID, null, -1, "SubmissionItemID, SubmissionItemTargetCulture, SubmissionItemName, SubmissionItemSourceXLIFF, SubmissionItemType");
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            var encoding = GetTranslationsEncoding(submissionInfo.SubmissionSiteID);

            // create the ZipEntry archive from the xml doc store in memory stream ms
            using (var zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, true))
            {
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    string extension = ValidationHelper.GetString(dr["SubmissionItemType"], XLIFFEXTENSION).Trim('.');
                    string xliff = ValidationHelper.GetString(dr["SubmissionItemSourceXLIFF"], "");
                    string targetLanguage = ValidationHelper.GetString(dr["SubmissionItemTargetCulture"], "");
                    string fileName = String.Format("{0}_{1}_{2}_{3}", ValidationHelper.GetString(dr["SubmissionItemName"], Guid.NewGuid().ToString()), submissionInfo.SubmissionSourceCulture, targetLanguage, ValidationHelper.GetInteger(dr["SubmissionItemID"], 0));

                    // Remove diacritics - some translation services cannot handle files with special characters
                    fileName = TextHelper.RemoveDiacritics(ValidationHelper.GetSafeFileName(fileName)) + "." + extension;

                    var zipEntry = zipArchive.CreateEntry(fileName);
                    using (var ms = new SystemIO.MemoryStream(encoding.GetBytes(xliff)))
                    {
                        using (var zipEntryStream = zipEntry.Open())
                        {
                            zipEntryStream.Write(ms.ToArray(), 0, Convert.ToInt32(ms.Length));
                        }
                    }
                }

                string templateDirectory = URLHelper.GetPhysicalPath("~/App_Data/CMSModules/Translations/Template/");
                if (Directory.Exists(templateDirectory))
                {
                    var resolver = MacroResolver.GetInstance();
                    resolver.SetNamedSourceData("Submission", submissionInfo);
                    resolver.SetNamedSourceData("SubmissionLink", GetSubmissionLinkURL(submissionInfo));

                    // Include template files to ZIP file
                    string[] templateFiles = Directory.GetFiles(templateDirectory);
                    foreach (string file in templateFiles)
                    {
                        FileInfo f = FileInfo.New(file);

                        byte[] fileBytes;
                        if (IsTextExtension(f.Extension))
                        {
                            string fileText = resolver.ResolveMacros(File.ReadAllText(file));
                            fileBytes = encoding.GetBytes(fileText);
                        }
                        else
                        {
                            fileBytes = File.ReadAllBytes(file);
                        }

                        var zipEntry = zipArchive.CreateEntry(f.Name);
                        using (var zipEntryStream = zipEntry.Open())
                        {
                            zipEntryStream.Write(fileBytes, 0, fileBytes.Length);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Returns true if given extension is considered as a text file.
        /// </summary>
        /// <param name="extension">Extension to check</param>
        private static bool IsTextExtension(string extension)
        {
            string ext = extension.TrimStart('.');
            string[] allowed = { "txt", "xml", "htm", "html", "xml" };
            return allowed.Any(e => ext.Equals(e, StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// Starts download of XLIFF document from given submission item.
        /// </summary>
        /// <param name="submissionInfo">Submission to export</param>
        /// <param name="response">Response object</param>
        public static void DownloadXLIFFinZIP(TranslationSubmissionInfo submissionInfo, HttpResponse response)
        {
            using (var outputMS = new SystemIO.MemoryStream())
            {
                WriteSubmissionInZIP(submissionInfo, outputMS);
                SendDataToDowload(response, outputMS.ToArray(), GetSubmissionFileName(submissionInfo), "application/zip");
            }
        }


        /// <summary>
        /// Starts download of XLIFF document from given submission item.
        /// </summary>
        /// <param name="item">Submission item</param>
        /// <param name="response">Response object</param>
        public static void DownloadXLIFF(TranslationSubmissionItemInfo item, HttpResponse response)
        {
            if (item == null)
            {
                return;
            }

            var submissionInfo = TranslationSubmissionInfoProvider.GetTranslationSubmissionInfo(item.SubmissionItemSubmissionID);
            if (submissionInfo == null)
            {
                return;
            }

            var encoding = GetTranslationsEncoding(submissionInfo.SubmissionSiteID);

            // Get target XLIFF, if empty get source XLIFF
            byte[] data = encoding.GetBytes(string.IsNullOrEmpty(item.SubmissionItemTargetXLIFF) ? item.SubmissionItemSourceXLIFF : item.SubmissionItemTargetXLIFF);

            string fileName = item.SubmissionItemName + "." + XLIFFEXTENSION;

            SendDataToDowload(response, data, fileName, XLIFFMIME);
        }


        /// <summary>
        /// Sends specified data to download.
        /// </summary>
        /// <param name="response">Response to use</param>
        /// <param name="data">Data to download</param>
        /// <param name="fileName">File name of the data</param>
        private static void SendDataToDowload(HttpResponse response, byte[] data, string fileName, string contentType)
        {
            // Send the data
            SystemIO.Stream stream = null;
            bool errorOccurred = false;
            try
            {
                // Clear response
                response.Clear();
                response.ClearHeaders();

                stream = response.OutputStream;

                // Write data to stream
                stream.Write(data, 0, data.Length);
                stream.Flush();

                // Set content type and headers
                response.ContentType = contentType;
                response.AddHeader("Content-Disposition", string.Format("attachment;filename=\"{0}\"", HTTPHelper.GetDispositionFilename(fileName)));
            }
            catch
            {
                errorOccurred = true;
                response.Clear();
            }
            finally
            {
                // Close stream
                if (stream != null)
                {
                    stream.Flush();
                    stream.Close();
                }
                if (!errorOccurred)
                {
                    // End response
                    if (response != null)
                    {
                        // Send all currently buffered output to the client, stops execution of the page
                        response.End();
                    }
                }
            }
        }


        /// <summary>
        /// Returns formatted (colored) status string.
        /// </summary>
        /// <param name="status">Status to format</param>
        public static string GetFormattedStatusString(TranslationStatusEnum status)
        {
            string statusClass;

            switch (status)
            {
                case TranslationStatusEnum.ProcessingError:
                case TranslationStatusEnum.SubmissionError:
                case TranslationStatusEnum.TranslationCanceled:
                    statusClass = "TranslationStatusError";
                    break;

                case TranslationStatusEnum.TranslationCompleted:
                case TranslationStatusEnum.TranslationReady:
                    statusClass = "TranslationStatusSuccess";
                    break;

                default:
                    statusClass = "TranslationStatusNeutral";
                    break;
            }

            return String.Format(@"<span class=""{0}"">{1}</span>", statusClass, status.ToLocalizedString(null));
        }


        /// <summary>
        /// Returns text representation of integer priority.
        /// </summary>
        /// <param name="priority">Submission priority</param>
        public static string GetPriorityText(int priority)
        {
            return ((TranslationPriorityEnum)priority).ToLocalizedString("TranslationPriorityEnum");
        }


        /// <summary>
        /// Checks whether the user is authorized to translate document.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="user">User to check</param>
        public static bool IsAuthorizedToTranslateDocument(TreeNode node, CurrentUserInfo user, HashSet<string> targetLanguages = null)
        {
            // Check module permission
            if (!user.IsAuthorizedPerResource("CMS.Content", "SubmitForTranslation"))
            {
                return false;
            }

            // Check create permission for document
            if (targetLanguages == null)
            {
                if (user.IsAuthorizedPerDocument(node, NodePermissionsEnum.Create) == AuthorizationResultEnum.Denied)
                {
                    return false;
                }
            }
            else
            {
                foreach (var targetCulture in targetLanguages)
                {
                    if (user.IsAuthorizedPerDocument(node, new[] { NodePermissionsEnum.Create, NodePermissionsEnum.Read }, true, targetCulture) == AuthorizationResultEnum.Denied)
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Returns true if at least one service is available (= is enabled and its IsAvailable() method returns true).
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        public static bool AnyServiceAvailable(string siteName)
        {
            var data = TranslationServiceInfoProvider.GetTranslationServices("TranslationServiceEnabled = 1", null, 0, "TranslationServiceName");
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return false;
            }

            foreach (DataRow dr in data.Tables[0].Rows)
            {
                string codeName = ValidationHelper.GetString(dr["TranslationServiceName"], "");

                // Check availability
                if (IsServiceAvailable(codeName, siteName))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true if specified service is available (= is enabled and its IsAvailable() method returns true).
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <param name="siteName">Name of the site</param>
        public static bool IsServiceAvailable(string serviceName, string siteName)
        {
            var info = TranslationServiceInfoProvider.GetTranslationServiceInfo(serviceName);
            if (info == null)
            {
                return false;
            }

            if (!info.TranslationServiceEnabled)
            {
                return false;
            }

            if (info.TranslationServiceIsMachine)
            {
                AbstractMachineTranslationService service = AbstractMachineTranslationService.GetTranslationService(info, siteName);
                if (service != null)
                {
                    return service.IsAvailable();
                }
            }
            else
            {
                AbstractHumanTranslationService service = AbstractHumanTranslationService.GetTranslationService(info, siteName);
                if (service != null)
                {
                    return service.IsAvailable();
                }
            }

            return false;
        }


        /// <summary>
        /// Reflects CMSEnableTranslations setting for specified site.
        /// </summary>
        /// <param name="siteName">Name of the site</param>
        public static bool IsTranslationAllowed(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSEnableTranslations");
        }


        internal static void LogInformation(string code, string message)
        {
            LogEvent(EventType.INFORMATION, code, message);
        }


        internal static void LogWarning(string code, string message)
        {
            LogEvent(EventType.WARNING, code, message);
        }


        /// <summary>
        /// Logs an error message to event log.
        /// </summary>
        /// <param name="message">Message to log</param>
        public static void LogError(string message)
        {
            LogEvent(EventType.ERROR, "EXCEPTION", message);
        }


        /// <summary>
        /// Logs an event if error occurred during action.
        /// </summary>
        /// <param name="ex">Exception occurring</param>
        public static void LogEvent(Exception ex)
        {
            LogEvent(EventType.ERROR, "EXCEPTION",
                "The translation service returned the following error:" + Environment.NewLine + EventLogProvider.GetExceptionLogMessage(ex));
        }


        /// <summary>
        /// Logs an event if error occurred during action.
        /// </summary>
        /// <param name="message">Error message</param>
        private static void LogEvent(string type, string code, string message)
        {
            try
            {
                // Log the event
                EventLogProvider.LogEvent(type, TRANSLATION_SOURCENAME, code, message);
            }
            catch
            {
                // Unable to log the event
            }
        }


        private static void LogPageNotExists(string code, string itemName)
        {
            EventLogProvider.LogEvent(EventType.WARNING, TRANSLATION_SOURCENAME, code,
                String.Format("The processing of page '{0}' was skipped. The page was already deleted from the system.", itemName));
        }


        /// <summary>
        /// Saves translated template.
        /// </summary>
        /// <param name="node">Document to update</param>
        /// <param name="pageInfo">PageInfo object</param>
        /// <param name="webpartsTable">Dict of web parts to clear the cache</param>
        private static void SaveTranslatedTemplate(TreeNode node, PageInfo pageInfo, Dictionary<string, WebPartInstance> webpartsTable = null)
        {
            node.SetValue("DocumentContent", pageInfo.GetContentXml());

            // Store customized editor widgets if at least one editor zone is customized
            var editorWidgets = node.GetStringValue("DocumentWebParts", string.Empty);
            if (!string.IsNullOrEmpty(editorWidgets.Trim()))
            {
                node.SetValue("DocumentWebParts", pageInfo.TemplateInstance.GetZonesXML(WidgetZoneTypeEnum.Editor));
            }

            PageTemplateInfoProvider.SetPageTemplateInfo(pageInfo.UsedPageTemplateInfo);

            if (webpartsTable == null)
            {
                return;
            }

            // Clear the cached web parts
            foreach (WebPartInstance webpartInstance in webpartsTable.Values)
            {
                if (webpartInstance != null)
                {
                    CacheHelper.TouchKey("webpartinstance|" + webpartInstance.InstanceGUID.ToString().ToLowerInvariant());
                }
            }
        }


        /// <summary>
        /// Saves document
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="tree">Tree provider</param>
        private static void SaveDocument(TreeNode node, TreeProvider tree)
        {
            // Get document workflow
            var workflow = node.GetWorkflow();
            if (workflow != null)
            {
                var step = node.WorkflowStep;
                if (step.StepIsPublished || step.StepIsArchived)
                {
                    // Create new version if document in published or archived step
                    node.CreateNewVersion();
                }
            }

            // Reset translation flag
            node.ResetTranslationFlag();
            DocumentHelper.UpdateDocument(node, tree);
        }


        /// <summary>
        /// Returns document of target culture from specified node or creates new culture version as a copy of the original.
        /// </summary>
        /// <param name="originalNode">Source culture document</param>
        /// <param name="targetLang">Target language</param>
        /// <param name="setWaitingForTranslation">If true, WaitingForTranslation flag of the target node is set to true</param>
        /// <param name="copyOriginalNodeData">If true, data from en existing originalNode (source culture) is copied to the target (this flag is taken into account only for existing target documents)</param>
        /// <param name="copyAttachments">Indicates if attachments from source node should be copied to target node</param>
        public static TreeNode CreateTargetCultureNode(TreeNode originalNode, string targetLang, bool setWaitingForTranslation, bool copyOriginalNodeData, bool copyAttachments = true)
        {
            if (originalNode == null)
            {
                return null;
            }

            var tree = originalNode.TreeProvider;

            // Document is not already translated (effective check)
            if (!originalNode.IsTranslated(targetLang))
            {
                // Create new node if the culture version does not exist, clone the default culture
                TreeNode newCulture = TreeNode.New(originalNode.ClassName);

                // The 'DocumentABTestConfiguration' is excluded from copying node to another culture as A/B test is linked to the culture specific node
                string[] excludeColumns = new[] { "DocumentABTestConfiguration" };

                DocumentHelper.CopyNodeData(originalNode, newCulture, new CopyNodeDataSettings(true, excludeColumns) { ResetChanges = true });

                if (setWaitingForTranslation)
                {
                    newCulture.DocumentIsWaitingForTranslation = true;
                }

                var settings = new NewCultureDocumentSettings(newCulture, targetLang, tree)
                {
                    Node = newCulture,
                    CultureCode = targetLang,
                    CopyAttachments = copyAttachments,
                    CopyCategories = true,
                    ClearAttachmentFields = !copyAttachments
                };

                // Don't overwrite waiting for translation flag
                using (new DocumentActionContext { ResetIsWaitingForTranslationFlag = false })
                {
                    DocumentHelper.InsertNewCultureVersion(settings);
                }

                return newCulture;
            }

            // Get translated document
            var targetNode = DocumentHelper.GetDocument(originalNode.NodeID, targetLang, tree);

            // Update flag
            if (setWaitingForTranslation && !targetNode.DocumentIsWaitingForTranslation)
            {
                targetNode.DocumentIsWaitingForTranslation = true;
                targetNode.Generalized.SetObject();
            }

            if (!copyOriginalNodeData)
            {
                return targetNode;
            }

            var excludedColumns = new[]
            {
                // Exclude coupled class ID column to avoid overwrite for existing culture version
                originalNode.CoupledClassIDColumn
            };

            // Copy the data of the source language version so the source data is translated (not the translated data)
            var copySettings = new CopyNodeDataSettings(true, excludedColumns)
            {
                CopyCoupledData = true,
                CopyVersionedData = true,
                CopyNonVersionedData = true,
                CopyDocumentData = false,
                CopySKUData = false,
                CopyTreeData = false,
                CopySystemDocumentData = false
            };

            DocumentHelper.CopyNodeData(originalNode, targetNode, copySettings);
            targetNode.DocumentCulture = targetLang;
            targetNode.DocumentName = originalNode.DocumentName;
            targetNode.SetValue("DocumentContent", originalNode.GetValue("DocumentContent"));

            return targetNode;
        }


        /// <summary>
        /// Translates a specified field of given CMS object. Object can be modified (normal string might be changed to resource strings) - if the object was modified, returns true, otherwise false.
        /// </summary>
        /// <param name="obj">Object the field of which to translate</param>
        /// <param name="columnName">Name of the column to translate</param>
        /// <param name="translation">Translation</param>
        /// <param name="sourceCulture">Source culture code</param>
        /// <param name="targetCulture">Target culture code</param>
        /// <param name="maxKeyLength">Maximal length of the resource string key</param>
        /// <param name="useResourceString">If true, field is translated using {$ $} macro, if false, translated value is saved directly in the field</param>
        private static bool TranslateField(ICMSObject obj, string columnName, string translation, string sourceCulture, string targetCulture, int maxKeyLength, bool useResourceString)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is BaseInfo)
            {
                // Do not translate code name column of Info objects
                var codeNameColumn = ((BaseInfo)obj).Generalized.CodeNameColumn;
                if (string.Equals(codeNameColumn, columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            object originalVal = obj.GetValue(columnName);

            string newValue;
            if (useResourceString)
            {
                string val = ValidationHelper.GetString(originalVal, "");
                newValue = TranslateString(val, translation, sourceCulture, targetCulture, maxKeyLength);
            }
            else
            {
                newValue = translation;
            }

            if (!string.IsNullOrEmpty(newValue))
            {
                // If the returned value is not null, than it means the new res. string was created -> need to update the field
                obj.SetValue(columnName, newValue);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Translates given string. Checks if original value is resource string. If not creates new and creates translations for original culture and translated culture.
        /// </summary>
        /// <param name="originalValue">Original value (possibly resource string macro)</param>
        /// <param name="translation">Translation in target culture</param>
        /// <param name="sourceCulture">Source culture</param>
        /// <param name="targetCulture">Target culture</param>
        /// <param name="maxKeyLength">Maximal resource string key length</param>
        internal static string TranslateString(string originalValue, string translation, string sourceCulture, string targetCulture, int maxKeyLength)
        {
            if (string.IsNullOrEmpty(originalValue))
            {
                return null;
            }

            string keyTrimmed = originalValue.Trim();
            string resKey;

            if (MacroProcessor.IsLocalizationMacro(keyTrimmed))
            {
                // Condition (keyTrimmed.Length >= 4) is to get rid of this case: {$}
                resKey = keyTrimmed.Substring(2, keyTrimmed.Length - 4);
            }
            else
            {
                // Field is not translated, create new resource string
                resKey = LocalizationHelper.GetUniqueResStringKey(originalValue, "custom.", maxKeyLength);

                // Create new resource string
                ResourceStringInfo ri = new ResourceStringInfo();
                ri.StringKey = resKey;
                ri.CultureCode = (string.IsNullOrEmpty(sourceCulture) ? CultureHelper.DefaultUICultureCode : sourceCulture);
                ri.TranslationText = originalValue;
                ri.StringIsCustom = true;
                ResourceStringInfoProvider.SetResourceStringInfo(ri);
            }

            string result = "{$" + resKey + "$}";

            // Create translation of the resource string
            var trans = ResourceStringInfoProvider.GetResourceStringInfo(resKey, targetCulture);
            if (trans.StringID == 0)
            {
                // If resource string is new then mark as 'Is custom'
                trans.StringIsCustom = true;
            }

            trans.TranslationText = translation;
            ResourceStringInfoProvider.SetResourceStringInfo(trans);
            return result;
        }


        /// <summary>
        /// Returns value for translation - if the value is localization macro, than it returns it value in source language, otherwise return the input value without change.
        /// </summary>
        /// <param name="src">Text to process</param>
        /// <param name="sourceLang">Source language</param>
        private static string GetTranslationValue(string src, string sourceLang)
        {
            // Check if it's not resource string macro
            if (!MacroProcessor.IsLocalizationMacro(src))
            {
                return src;
            }

            string key = src.Trim();
            key = key.Substring(2, src.Length - 4);

            return ResHelper.GetString(key, sourceLang);
        }


        /// <summary>
        /// Returns query for latest translated submission item of the given document.
        /// </summary>
        /// <param name="doc">Document waiting for translation</param>
        internal static ObjectQuery<TranslationSubmissionItemInfo> GetLatestTranslatedSubmissionItemQuery(TreeNode doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }

            return TranslationSubmissionItemInfoProvider.GetTranslationSubmissionItems()
                .TopN(1)
                .WhereEquals("SubmissionItemObjectType", "cms.document")
                .WhereNotNull("SubmissionItemTargetXLIFF")
                .WhereNotEmpty("SubmissionItemTargetXLIFF")
                .WhereEquals("SubmissionItemTargetObjectID", doc.DocumentID)
                .OrderByDescending("SubmissionItemLastModified");
        }

        #endregion


        #region "Scheduled task methods"

        /// <summary>
        /// Checks all the available translation services for given site and downloads the translations if they are available, stores them in the submission.
        /// </summary>
        /// <param name="siteName">Site where to check</param>
        public static string CheckAndDownloadTranslations(string siteName)
        {
            var condition = new WhereCondition()
                .WhereEquals("TranslationServiceEnabled", 1)
                .WhereIn("TranslationServiceID", new IDQuery(TranslationSubmissionInfo.OBJECT_TYPE, "SubmissionServiceID")
                                                        .WhereEquals("SubmissionSiteID", SiteInfoProvider.GetSiteID(siteName)));

            // Get only those service for which there is at least one submission
            var data = TranslationServiceInfoProvider.GetTranslationServices().Where(condition);
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return null;
            }

            foreach (DataRow dr in data.Tables[0].Rows)
            {
                // Check & process completed translations
                var service = AbstractHumanTranslationService.GetTranslationService(new TranslationServiceInfo(dr), siteName);
                if ((service == null) || !service.IsAvailable())
                {
                    continue;
                }

                string err = service.DownloadCompletedTranslations(siteName);
                if (!String.IsNullOrEmpty(err))
                {
                    return ResHelper.GetString("ContentRequest.TranslationFailed") + " \"" + err + "\"";
                }
            }

            // Save the last check time
            SettingsKeyInfoProvider.SetValue("CMSTranslationsLastStatusCheck", siteName, DateTime.Now.ToString("g", new System.Globalization.CultureInfo(CultureHelper.DefaultUICultureCode)));

            return null;
        }

        #endregion
    }
}
