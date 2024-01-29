using System;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Class generating web part user control files.
    /// </summary>
    public sealed class WebPartCodeGenerator
    {
        private const string CODE_TEMPLATE_PREFIX = "CMS.PortalEngine.Web.UI.WebParts.CodeTemplates.";
        private readonly string applicationPath;
        private readonly bool isWebApplicationProject;


        /// <summary>
        /// Create new instance of <see cref="WebPartCodeGenerator"/>.
        /// </summary>
        public WebPartCodeGenerator()
        {
            applicationPath = SystemContext.ApplicationPath;
            isWebApplicationProject = SystemContext.IsWebApplicationProject;
        }


        /// <summary>
        /// Create new instance of <see cref="WebPartCodeGenerator"/> with context parameters for testing purposes.
        /// </summary>
        internal WebPartCodeGenerator(string applicationPath, bool isWebApplicationProject)
        {
            this.applicationPath = applicationPath;
            this.isWebApplicationProject = isWebApplicationProject;
        }


        /// <summary>
        /// Generates the web part code files content.
        /// </summary>
        /// <param name="wpi">Web part info</param>
        public WebPartCodeGeneratorResult GenerateWebPartCode(WebPartInfo wpi)
        {
            return GenerateWebPartCode(wpi, null);
        }


        /// <summary>
        /// Generates the web part code.
        /// </summary>
        /// <param name="wpi">Web part info</param>
        /// <param name="baseControl">Base control nested within the web part</param>
        internal WebPartCodeGeneratorResult GenerateWebPartCode(WebPartInfo wpi, string baseControl)
        {
            string code = String.Empty;
            string markup = String.Empty;
            string designer = String.Empty;

            if (wpi != null)
            {
                // Generate the ASCX
                markup = GetCodeTemplateContent("Webpart.ascx");

                // Prepare the path
                string path = URLHelper.UnResolveUrl(WebPartInfoProvider.GetWebPartUrl(wpi), applicationPath);

                // Prepare the class name
                string className = path.Trim('~', '/');
                if (className.EndsWith(".ascx", StringComparison.OrdinalIgnoreCase))
                {
                    className = className.Substring(0, className.Length - 5);
                }
                className = ValidationHelper.GetIdentifier(className, "_");

                markup = Regex.Replace(markup, "Inherits=\"[^\"]+\"", "Inherits=\"" + className + "\"", RegexOptions.IgnoreCase);

                markup = Regex.Replace(markup, "(CodeFile|CodeBehind)=\"[^\"]+\"", (isWebApplicationProject ? "CodeBehind=\"" : "CodeFile=\"") + path + ".cs\"", RegexOptions.IgnoreCase);

                // Generate the code
                code = GetCodeTemplateContent("Webpart.ascx.cs");

                const string CLASS_PATTERN = "( class\\s+)[^\\s]+";
                code = Regex.Replace(code, CLASS_PATTERN, "$1" + className);

                // Prepare the properties
                var fi = new FormInfo(wpi.WebPartProperties);

                var sbInit = new StringBuilder();

                string propertiesCode = CodeGenerator.GetPropertiesCode(fi, true, baseControl, sbInit, true);

                // Replace in code
                code = code.Replace("// ##PROPERTIES##", propertiesCode);
                code = code.Replace("// ##SETUP##", sbInit.ToString());

                // Generate the designer
                if (isWebApplicationProject)
                {
                    designer = GetCodeTemplateContent("Webpart.ascx.designer.cs");

                    designer = Regex.Replace(designer, CLASS_PATTERN, "$1" + className);
                }
            }

            return new WebPartCodeGeneratorResult(markup, code, designer);
        }


        /// <summary>
        /// Gets code template content from embedded files.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>Content of <paramref name="fileName"/> file.</returns>
        private static string GetCodeTemplateContent(string fileName)
        {
            var currentAssembly = Assembly.GetAssembly(typeof(CMSAbstractWebPart));

            using (var resourceStream = currentAssembly.GetManifestResourceStream(CODE_TEMPLATE_PREFIX + fileName))
            using (var reader = StreamReader.New(resourceStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
