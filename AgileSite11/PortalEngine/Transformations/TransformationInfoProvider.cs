using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.IO;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Enables access to Transformations.
    /// </summary>
    public class TransformationInfoProvider : AbstractInfoProvider<TransformationInfo, TransformationInfoProvider>, IFullNameInfoProvider
    {
        #region "Constructors"

        /// <summary>
        /// Initializes a TransformationInfoProvider.
        /// </summary>
        public TransformationInfoProvider()
            : base(TransformationInfo.TYPEINFO, new HashtableSettings
            {
                FullName = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Variables"

        /// <summary>
        /// List of default transformation namespaces
        /// </summary>
        private static readonly List<string> mDefaultNamespaces = new List<string> { "CMS.DocumentEngine.Web.UI" };

        // CMS.DocumentEngine.Web.UI directive with cc1 tag prefix 
        private static readonly string CC1_CONTROLS_DIRECTIVE = GetNamespaceRegistration("cc1", "CMS.DocumentEngine.Web.UI", null);

        /// <summary>
        /// Public transformation directives.
        /// </summary>
        private static string mTransformationDirectives;

        // Array of default directives.
        private static string[] mDefaultDirectives;

        // Transformation directives for internal purposes.
        private static string mInternalTransformationDirectives;

        // Virtual directory where the Transformations are located.
        private const string mTransformationsDirectory = "~/CMSVirtualFiles/Transformations";

        // Transformation base class used in transformation directives
        private static string mTransformationBaseClass = "CMS.DocumentEngine.Web.UI.CMSAbstractTransformation";

        private static Regex mEditButtonsRegex;

        private static Regex mEditButtonsDirectiveRegex;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the transformation base class
        /// </summary>
        public static string TransformationBaseClass
        {
            get
            {
                return mTransformationBaseClass;
            }
            set
            {
                mTransformationBaseClass = value;
            }

        }


        /// <summary>
        /// Gets or sets the value that indicates whether transformations should be stored externally
        /// </summary>
        public static bool StoreTransformationsInExternalStorage
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSStoreTransformationsInFS");
            }
            set
            {
                SettingsKeyInfoProvider.SetGlobalValue("CMSStoreTransformationsInFS", value);
            }
        }


        /// <summary>
        /// Gets the regular expression for edit buttons directive
        /// </summary>
        private static Regex EditButtonsDirectiveRegex
        {
            get
            {
                if (mEditButtonsDirectiveRegex == null)
                {
                    mEditButtonsDirectiveRegex = RegexHelper.GetRegex("<cc1:CMSEditModeButtonEditDelete.*?[/][>]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                }
                return mEditButtonsDirectiveRegex;
            }
        }


        /// <summary>
        /// Gets the Regex object for edit/delete buttons macro
        /// </summary>
        public static Regex EditButtonsRegex
        {
            get
            {
                if (mEditButtonsRegex == null)
                {
                    mEditButtonsRegex = RegexHelper.GetRegex("##editbuttons##", true);
                }
                return mEditButtonsRegex;
            }
        }


        /// <summary>
        /// Default transformation directives made into an array.
        /// </summary>
        public static string[] DefaultDirectives
        {
            get
            {
                if (mDefaultDirectives == null)
                {
                    mDefaultDirectives = InternalTransformationDirectives.Replace("%><%", "%>\n<%").ToLowerCSafe().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                }
                return mDefaultDirectives;
            }
        }


        /// <summary>
        /// Transformation directives.
        /// </summary>
        public static string TransformationDirectives
        {
            get
            {
                if (mTransformationDirectives == null)
                {
                    StringBuilder sb = new StringBuilder(GetControlDirective());

                    // Add namespaces
                    foreach (string ns in mDefaultNamespaces)
                    {
                        string reg = GetNamespaceRegistration(null, ns, null);
                        sb.AppendLine(reg);
                    }

                    mTransformationDirectives = sb.ToString();
                }

                return mTransformationDirectives;
            }
            set
            {
                mTransformationDirectives = value;
                mInternalTransformationDirectives = null;
            }
        }


        /// <summary>
        /// Internal transformation directives.
        /// </summary>
        public static string InternalTransformationDirectives
        {
            get
            {
                if (mInternalTransformationDirectives == null)
                {
                    StringBuilder directives = new StringBuilder(TransformationDirectives);

                    // NOTE: do not split into the new lines, it causes unwanted white spaces between template items
                    directives.Append(CC1_CONTROLS_DIRECTIVE);

                    mInternalTransformationDirectives = directives.ToString();
                }

                return mInternalTransformationDirectives;
            }
            set
            {
                mInternalTransformationDirectives = value;
            }
        }


        /// <summary>
        /// TransformationsDirectory - Read only
        /// </summary>
        public static string TransformationsDirectory
        {
            get
            {
                return mTransformationsDirectory;
            }
        }


        /// <summary>
        /// List of default transformation namespaces
        /// </summary>
        public static List<string> DefaultNamespaces
        {
            get
            {
                return mDefaultNamespaces;
            }
        }

        #endregion // Public properties


        #region "Public static methods"

        /// <summary>
        /// Returns web part layout info for specified path
        /// </summary>
        /// <param name="path">Path</param>
        public static TransformationInfo GetVirtualObject(string path)
        {
            List<string> prefixes = new List<string>();
            // Get layout code name and web part code name
            string transformationName = VirtualPathHelper.GetVirtualObjectName(path, TransformationsDirectory, ref prefixes);
            // Remove edit delete directive
            int separatorIndex = transformationName.IndexOf(VirtualPathHelper.URLParametersSeparator, StringComparison.Ordinal);
            if (separatorIndex > 0)
            {
                transformationName = transformationName.Substring(0, separatorIndex);
            }
            // Get class name
            string className = prefixes[0];
            // Return info
            return GetTransformation(className.ToLowerCSafe() + "." + transformationName.ToLowerCSafe());
        }


        /// <summary>
        /// Removes the transformation directives.
        /// </summary>
        /// <param name="code">Input code</param>
        public static string RemoveDirectives(string code)
        {
            if (!string.IsNullOrEmpty(code))
            {
                code = code.Replace(GetControlDirective(), string.Empty);

                // Check edit buttons
                Match m = EditButtonsDirectiveRegex.Match(code);

                if (m.Success)
                {
                    string value = m.Value.ToLowerCSafe();
                    // Check whether added automatically
                    if (value.Contains("addedautomatically=\"true\""))
                    {
                        string replacement = string.Empty;
                        if (value.Contains("addedbymacro=\"true\""))
                        {
                            replacement = "##editbuttons##";
                        }
                        code = EditButtonsDirectiveRegex.Replace(code, replacement);
                    }
                }

                // Add namespaces
                foreach (string ns in mDefaultNamespaces)
                {
                    string reg = GetNamespaceRegistration(null, ns, null);
                    code = code.Replace(reg, string.Empty);
                }

                code = code.Replace(CC1_CONTROLS_DIRECTIVE, string.Empty);
                code = code.TrimStart(' ', '\r', '\n');
            }

            return code;
        }


        /// <summary>
        /// Gets the transformation by ID.
        /// </summary>
        /// <param name="transformationId">Transformation ID</param>
        public static TransformationInfo GetTransformation(int transformationId)
        {
            return ProviderObject.GetInfoById(transformationId);
        }


        /// <summary>
        /// Gets the transformation by full name.
        /// </summary>
        /// <param name="transformationFullName">Transformation full name in format application.class.transformation</param>
        public static TransformationInfo GetTransformation(string transformationFullName)
        {
            return ProviderObject.GetTransformationInfoInternal(transformationFullName);
        }


        /// <summary>
        /// Gets the localized transformation by full name. Returns the default transformation if the localized version is not found.
        /// </summary>
        /// <param name="transformationFullName">Transformation full name in format application.class.transformation</param>
        /// <param name="cultureCode">Culture code</param>
        public static TransformationInfo GetLocalizedTransformation(string transformationFullName, string cultureCode)
        {
            string localizedFullName = transformationFullName + "_" + cultureCode;
            return GetTransformation(localizedFullName) ?? GetTransformation(transformationFullName);
        }


        /// <summary>
        /// Returns all transformations.
        /// </summary>
        public static ObjectQuery<TransformationInfo> GetTransformations()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Gets the transformations of the specified class.
        /// </summary>
        /// <param name="classId">Class ID</param>
        public static DataSet GetTransformations(int classId)
        {
            return GetTransformations()
                .WhereEquals("TransformationClassID", classId)
                .OrderBy("TransformationName");
        }


        /// <summary>
        /// Returns full transformation name by provided transformation ID.
        /// </summary>
        /// <param name="transformationId">Transformation ID</param>
        /// <returns>Transformation full name in format application.class.transformation or null.</returns>
        public static string GetTransformationFullName(int transformationId)
        {
            TransformationInfo transformationObj = GetTransformation(transformationId);

            if (transformationObj == null)
            {
                return null;
            }

            return transformationObj.TransformationFullName;
        }


        /// <summary>
        /// Saves the specified transformation to the database.
        /// </summary>
        /// <param name="transformation">Transformation</param>
        /// <remarks>If the transformation of the specified name already exists, it's updated, otherwise a new one is created in the database.</remarks>
        public static void SetTransformation(TransformationInfo transformation)
        {
            ProviderObject.SetInfo(transformation);
        }


        /// <summary>
        /// Deletes the transformation.
        /// </summary>
        /// <param name="transformationId">Transformation ID</param>
        public static void DeleteTransformation(int transformationId)
        {
            TransformationInfo transformation = GetTransformation(transformationId);
            DeleteTransformation(transformation);
        }


        /// <summary>
        /// Deletes the transformation.
        /// </summary>
        /// <param name="transformation">Transformation</param>
        public static void DeleteTransformation(TransformationInfo transformation)
        {
            ProviderObject.DeleteInfo(transformation);
        }


        /// <summary>
        /// Deletes all the transformations of the specified class.
        /// </summary>
        /// <param name="classId">Class ID</param>
        public static void DeleteTransformations(int classId)
        {
            // Get transformations
            DataSet ds = GetTransformations(classId);

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                // Delete the transformation
                int transformationId = ValidationHelper.GetInteger(dr["TransformationID"], 0);
                DeleteTransformation(transformationId);
            }
        }


        /// <summary>
        /// Generates the default transformation code for given class.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="transformationType">Type of the transformation to be generated.</param>
        /// <param name="defaultTransformationType">Default transformation type for the ASCX transformation.</param>
        public static string GenerateTransformationCode(string className, TransformationTypeEnum transformationType, DefaultTransformationTypeEnum defaultTransformationType = DefaultTransformationTypeEnum.Default)
        {
            if (string.IsNullOrEmpty(className))
            {
                return string.Empty;
            }

            var classInfo = DataClassInfoProvider.GetDataClassInfo(className);
            if (classInfo == null)
            {
                return string.Empty;
            }

            var tableSchema = classInfo.ClassFormDefinition;
            switch (transformationType)
            {
                case TransformationTypeEnum.Xslt:
                    if (!string.IsNullOrEmpty(className))
                    {
                        return GetXSLTransformation(tableSchema);
                    }
                    break;

                case TransformationTypeEnum.Ascx:
                    // ASCX transformation
                    switch (defaultTransformationType)
                    {
                        // Default transformation
                        case DefaultTransformationTypeEnum.Default:
                            return GetASCXTransformation(tableSchema);

                        // Atom transformation
                        case DefaultTransformationTypeEnum.Atom:
                            return GetAtomTransformation(classInfo);

                        // RSS transformation
                        case DefaultTransformationTypeEnum.RSS:
                            return GetRSSTransformation(classInfo);


                        // XML transformation
                        case DefaultTransformationTypeEnum.XML:
                            return GetXMLTransformation(tableSchema, className);
                    }
                    break;

                case TransformationTypeEnum.jQuery:
                    // jQuery transformation
                    return GetJQueryTransformation(tableSchema);

                default:
                    // Text transformation
                    return GetTextTransformation(tableSchema);
            }

            return string.Empty;
        }


        /// <summary>
        /// Gets the aspx code for edit delete buttons
        /// </summary>
        /// <param name="addedAutomatically">Indicates whether directive was added automatically</param>
        /// <param name="addedByMacro">Indicates whether directive was added by macro</param>
        /// <param name="mode">Edit buttons mode</param>
        public static string GetEditDeleteCode(bool addedAutomatically, bool addedByMacro, EditModeButtonEnum mode = EditModeButtonEnum.Both)
        {
            string edcode = "<cc1:CMSEditModeButtonEditDelete runat=\"server\" id=\"btnEditDeleteAutoInsert\" Path='<%# Eval(\"NodeAliasPath\") %>' #aa# #abm# #mode# />";
            string addedAutomaticallyCode = "AddedAutomatically=\"True\" EnableByParent=\"True\"";
            string addedByMacroCode = "AddedByMacro=\"True\"";

            edcode = edcode.Replace("#aa#", addedAutomatically ? addedAutomaticallyCode : string.Empty);
            edcode = edcode.Replace("#abm#", addedByMacro ? addedByMacroCode : string.Empty);

            // Set which button should be visible
            switch (mode)
            {
                case EditModeButtonEnum.Edit:
                    edcode = edcode.Replace("#mode#", "EditMode=\"Edit\"");
                    break;

                case EditModeButtonEnum.Delete:
                    edcode = edcode.Replace("#mode#", "EditMode=\"Delete\"");
                    break;

                default:
                    edcode = edcode.Replace("#mode#", string.Empty);
                    break;
            }

            return edcode;
        }


        /// <summary>
        /// Adds the layout directives to the transformation.
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="useInternal">If true, internal directives are used</param>
        /// <param name="mode">Indicates which mode for 'edit/delete buttons' should be used</param>
        /// <param name="type">Type of the transformation</param>
        public static string AddTransformationDirectives(string code, bool useInternal, EditModeButtonEnum mode, TransformationTypeEnum type)
        {
            if (type == TransformationTypeEnum.Ascx)
            {
                return AddTransformationDirectives(code, useInternal, mode);
            }

            // Do not add directives to non-ascx code.
            return code;
        }


        /// <summary>
        /// Adds the transformation directives to the transformation.
        /// </summary>
        /// <param name="code">Code</param>
        /// <param name="useInternal">If true, internal directives are used</param>
        /// <param name="mode">Indicates which mode for 'edit/delete buttons' should be used</param>
        public static string AddTransformationDirectives(string code, bool useInternal, EditModeButtonEnum mode)
        {
            if (!code.StartsWith("<%@ Control ", StringComparison.InvariantCultureIgnoreCase))
            {
                if (mode != EditModeButtonEnum.None)
                {
                    if (code.IndexOf("##editbuttons##", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        code = EditButtonsRegex.Replace(code, GetEditDeleteCode(true, true, mode));
                    }
                    else
                    {
                        code += GetEditDeleteCode(true, false, mode);
                    }
                }
                else
                {
                    code = EditButtonsRegex.Replace(code, string.Empty);
                }

                // Add the directives
                return (useInternal ? InternalTransformationDirectives : TransformationDirectives) + code;
            }

            // Already contains directives
            return code;
        }


        /// <summary>
        /// Returns the string with first selector removed.
        /// </summary>
        /// <param name="expression">Expression</param>
        /// <param name="delimiter">Selector delimiter</param>
        public static string RemoveFirstPart(string expression, string delimiter)
        {
            if (expression == null)
            {
                return null;
            }

            // Get the index of delimiter
            int delimIndex = expression.IndexOfCSafe(delimiter);
            if (delimIndex < 0)
            {
                // No delimiter, delete all
                return "";
            }
            else
            {
                // Remove the part before the delimiter and the delimiter itself
                return expression.Substring(delimIndex + delimiter.Length);
            }
        }

        #endregion // Public methods


        #region "Internal methods"

        /// <summary>
        /// Gets the transformation by full name.
        /// </summary>
        /// <param name="transformationFullName">Transformation full name</param>
        protected virtual TransformationInfo GetTransformationInfoInternal(string transformationFullName)
        {
            return GetInfoByFullName(transformationFullName, true);
        }

        
        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(TransformationInfo info)
        {
            if (info == null)
            {
                return;
            }

            if (String.IsNullOrEmpty(info.TransformationName))
            {
                throw new InvalidOperationException("Transformation name cannot be null or empty.");
            }

            if (info.TransformationIsHierarchical)
            {
                if (!ValidationHelper.IsIdentifier(info.TransformationName))
                {
                    throw new InvalidOperationException(String.Format("Transformation name '{0}' does not meet restrictions for identifier format.", info.TransformationName));
                }
            }
            else
            {
                if (!ValidationHelper.IsCodeName(info.TransformationName))
                {
                    throw new InvalidOperationException(String.Format("Transformation name '{0}' does not meet restrictions for code name format.", info.TransformationName));
                }
            }

            bool codeChanged = info.ItemChanged("TransformationCode") || info.ItemChanged("TransformationCSS");

            // Create new version GUID for VPP if the code changed
            if (codeChanged || string.IsNullOrEmpty(info.TransformationVersionGUID))
            {
                info.TransformationVersionGUID = Guid.NewGuid().ToString();
            }

            // Set transformation
            base.SetInfo(info);
        }

        #endregion


        #region "Overriden methods"

        /// <summary>
        /// Creates a new dictionary for caching the transformations by the full name.
        /// </summary>
        public ProviderInfoDictionary<string> GetFullNameDictionary()
        {
            return new ProviderInfoDictionary<string>(TransformationInfo.OBJECT_TYPE, "TransformationClassID;TransformationName");
        }


        /// <summary>
        /// Gets the where condition that searches the transformation based on the given full name.
        /// </summary>
        /// <param name="fullName">Transformation full name</param>
        public string GetFullNameWhereCondition(string fullName)
        {
            return new TransformationFullNameWhereConditionBuilder(fullName)
                .Build()
                .ToString(true);
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets the control directive for the transformation.
        /// </summary>
        private static string GetControlDirective()
        {
            // Get language code from web.config.
            string lang = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSProgrammingLanguage"], "");
            if (lang == "")
            {
                lang = "C#";
            }

            StringBuilder sb = new StringBuilder();

            // NOTE: do not split into the new lines, it causes unwanted white spaces between template items
            sb.Append(@"<%@ Control Language=""");
            sb.Append(lang);
            sb.Append(@""" AutoEventWireup=""true"" Inherits=""");
            sb.Append(TransformationBaseClass);
            sb.Append(@""" %>");

            return sb.ToString();
        }


        /// <summary>
        /// Gets the registration tag for the given namespace and assembly.
        /// </summary>
        /// <param name="tagPrefix">Tag prefix</param>
        /// <param name="ns">Namespace name</param>
        /// <param name="assembly">Assembly name</param>
        public static string GetNamespaceRegistration(string tagPrefix, string ns, string assembly)
        {
            if (tagPrefix == null)
            {
                tagPrefix = "cms";
            }
            if (assembly == null)
            {
                assembly = ns;
            }

            return string.Format("<%@ Register TagPrefix=\"{0}\" Namespace=\"{1}\" Assembly=\"{2}\" %>", tagPrefix, ns, assembly);
        }

        #endregion // Private methods


        #region "Generating methods"

        /// <summary>
        /// Returns default XSL transformation based on the given form.
        /// </summary>
        /// <param name="formSchema">Form schema</param>
        /// <returns>XSL transformation</returns>
        private static string GetXSLTransformation(string formSchema)
        {
            // Parse the form schema
            FormInfo fi = new FormInfo(formSchema);

            var text = new StringBuilder();
            text.AppendLine("<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">");
            text.AppendLine("  <xsl:output method=\"html\" />");
            text.AppendFormat("  <xsl:template match=\"Table\">{0}", Environment.NewLine);

            foreach (var field in fi.GetFields(true, true))
            {
                var caption = field.GetPropertyValue(FormFieldPropertyEnum.FieldCaption);
                if (!string.IsNullOrEmpty(caption))
                {
                    text.AppendFormat("    {0}: ", caption);
                }

                if (FormHelper.IsFieldOfType(field, FormFieldControlTypeEnum.CheckBoxControl))
                {
                    text.AppendLine("<xsl:choose>");
                    text.AppendFormat("<xsl:when test=\"{0}='true'\">Yes</xsl:when>{1}<xsl:otherwise>No</xsl:otherwise>{1}</xsl:choose>", field.Name, Environment.NewLine);
                }
                else
                {
                    text.Append(String.Format("<xsl:value-of disable-output-escaping=\"yes\" select=\"{0}\"/>", field.Name));
                }

                text.AppendLine("<br />");
            }
            text.AppendLine("</xsl:template>");
            text.Append("</xsl:stylesheet>");

            return text.ToString();
        }


        /// <summary>
        /// Returns default text transformation based on the given form.
        /// </summary>
        /// <param name="formSchema">Form schema</param>
        /// <returns>Text transformation</returns>
        private static string GetTextTransformation(string formSchema)
        {
            var form = new FormInfo(formSchema);
            return form.GetFields(true, true)
                .Where(f => !f.PrimaryKey)
                .Select(f => string.Format("{{%{0}%}}", f.Name))
                .Join(Environment.NewLine);
        }


        /// <summary>
        /// Returns default ASCX transformation based on the given form.
        /// </summary>
        /// <param name="formSchema">Form schema</param>
        /// <returns>ASCX transformation</returns>
        private static string GetASCXTransformation(string formSchema)
        {
            var form = new FormInfo(formSchema);
            return form.GetFields(true, true)
                .Where(f => !f.PrimaryKey)
                .Select(f => string.Format("<%# Eval(\"{0}\") %>", f.Name))
                .Join(Environment.NewLine);
        }


        /// <summary>
        /// Returns default jQuery transformation based on the given form.
        /// </summary>
        /// <param name="formSchema">Form schema</param>
        /// <returns>jQuery transformation</returns>
        private static string GetJQueryTransformation(string formSchema)
        {
            var form = new FormInfo(formSchema);
            return form.GetFields(true, true)
                .Where(f => !f.PrimaryKey)
                .Select(f => string.Format("${{{0}}}", f.Name))
                .Join(Environment.NewLine);
        }


        /// <summary>
        /// Returns default RSS transformation.
        /// </summary>
        /// <param name="classInfo">Class</param>
        /// <returns>RSS transformation</returns>
        private static string GetRSSTransformation(DataClassInfo classInfo)
        {
            if ((!classInfo.ClassIsDocumentType || !classInfo.ClassIsCoupledClass) && !classInfo.ClassIsCustomTable)
            {
                return String.Empty;
            }

            // RSS transformation pattern
            const string transformationPattern =
@"<item>
    <guid isPermaLink=""false"">{0}</guid>
    <title>{1}</title>
    <description>{2}</description>
    <pubDate>{3}</pubDate>
    <link><![CDATA[{4}]]></link>
</item>";

            string id;
            string title;
            string description;
            string published;
            string link;

            if (classInfo.ClassIsDocumentType && classInfo.ClassIsCoupledClass)
            {
                id = "<%# Eval(\"NodeGUID\") %>";
                title = "<%# EvalCDATA(\"DocumentName\") %>";
                description = "<%# EvalCDATA(\"NodeAliasPath\") %>";
                published = "<%# GetRSSDateTime(Eval(\"DocumentCreatedWhen\")) %>";
                link = "<%# GetAbsoluteUrl(GetDocumentUrlForFeed(), Eval<int>(\"NodeSiteID\")) %>";
            }
            else
            {
                var structureInfo = ClassStructureInfo.GetClassInfo(classInfo.ClassName);

                id = structureInfo.ContainsColumn("ItemGUID") ? "<%# Eval(\"ItemGUID\") %>" : "<![CDATA[<Please insert item identifier here>]]>";
                title = "<![CDATA[<Please insert title here>]]>";
                description = "<![CDATA[<Please insert description here>]]>";
                published = structureInfo.ContainsColumn("ItemCreatedWhen") ? "<%# GetRSSDateTime(Eval(\"ItemCreatedWhen\")) %>" : "<![CDATA[<Please insert item modified date here>]]>";
                link = "<Please insert URL here>";
            }

            return String.Format(transformationPattern, id, title, description, published, link);
        }


        /// <summary>
        /// Returns default Atom transformation.
        /// </summary>
        /// <param name="classInfo">Class info</param>
        /// <returns>Atom transformation</returns>
        private static string GetAtomTransformation(DataClassInfo classInfo)
        {
            if ((!classInfo.ClassIsDocumentType || !classInfo.ClassIsCoupledClass) && !classInfo.ClassIsCustomTable)
            {
                return String.Empty;
            }

            // Atom transformation pattern
            const string transformationPattern =
@"<entry>
    <title>{0}</title>
    <link href=""{1}""/>
    <id>urn:uuid:{2}</id>
    <published>{3}</published>
    <updated>{4}</updated>
    <author>
        <name>{5}</name>
    </author>
    <summary>{6}</summary>
</entry>";

            string title;
            string link;
            string id;
            string published;
            string updated;
            string authorName;
            string summary;

            if (classInfo.ClassIsDocumentType && classInfo.ClassIsCoupledClass)
            {
                // Document type parameters
                title = "<%# EvalCDATA(\"DocumentName\") %>";
                link = "<%# GetAbsoluteUrl(GetDocumentUrlForFeed(), Eval<int>(\"NodeSiteID\")) %>";
                id = "<%# Eval(\"NodeGUID\") %>";
                published = "<%# GetAtomDateTime(Eval(\"DocumentCreatedWhen\")) %>";
                updated = "<%# GetAtomDateTime(Eval(\"DocumentModifiedWhen\")) %>";
                authorName = "<cms:ObjectTransformation runat=\"server\" ObjectType=\"<%# CMS.DataEngine.PredefinedObjectType.USER %>\" ObjectID='<%# Eval<int>(\"NodeOwner\") %>' Transformation=\"{% FullName %}\" NoDataTransformation=\"N/A\" />";
                summary = "<%# EvalCDATA(\"NodeAliasPath\") %>";
            }
            else
            {
                var structureInfo = ClassStructureInfo.GetClassInfo(classInfo.ClassName);

                // Custom table parameters
                title = "<![CDATA[<Please insert title here>]]>";
                link = "<Please insert URL here>";
                id = structureInfo.ContainsColumn("ItemGUID") ? "<%# Eval(\"ItemGUID\") %>" : "<![CDATA[<Please insert item identifier here>]]>";
                published = structureInfo.ContainsColumn("ItemCreatedWhen") ? "<%# GetAtomDateTime(Eval(\"ItemCreatedWhen\")) %>" : "<![CDATA[<Please insert item created date here>]]>";
                updated = structureInfo.ContainsColumn("ItemModifiedWhen") ? "<%# GetAtomDateTime(Eval(\"ItemModifiedWhen\")) %>" : "<![CDATA[<Please insert item modified date here>]]>";
                authorName = "<![CDATA[<Please insert author name here>]]>";
                summary = "<![CDATA[<Please insert description here>]]>";
            }

            return String.Format(transformationPattern, title, link, id, published, updated, authorName, summary);
        }


        /// <summary>
        /// Returns default XML transformation based on the given form.
        /// </summary>
        /// <param name="formSchema">Form schema</param>
        /// <param name="className">Class name</param>
        /// <returns>XML transformation</returns>
        private static string GetXMLTransformation(string formSchema, string className)
        {
            var text = new StringBuilder();
            var safeClass = TranslationHelper.GetSafeClassName(className);
            text.AppendFormat("<{0}>{1}", safeClass, Environment.NewLine);

            var form = new FormInfo(formSchema);
            var elements = form.GetFields(true, true)
                                .Where(f => !f.PrimaryKey)
                                .Select(f => string.Format("  <{0}><%# EvalCDATA(\"{0}\") %></{0}>", f.Name))
                                .Join(Environment.NewLine);
            text.AppendLine(elements);

            text.AppendFormat("</{0}>", safeClass);

            return text.ToString();
        }

        #endregion
    }
}
