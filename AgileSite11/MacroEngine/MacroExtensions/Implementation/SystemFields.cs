using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Wrapper class to provide system fields in the MacroEngine.
    /// </summary>
    internal class SystemFields : MacroFieldContainer
    {
        /// <summary>
        /// Registers the system fields.
        /// </summary>
        protected override void RegisterFields()
        {
            base.RegisterFields();

            // Register special values
            RegisterField(new MacroField("Domain", x => RequestContext.CurrentDomain));
            RegisterField(new MacroField("ApplicationPath", x => SystemContext.ApplicationPath.TrimEnd('/')));
            RegisterField(new MacroField("AppPath", x => SystemContext.ApplicationPath.TrimEnd('/')));
            RegisterField(new MacroField("RelativePath", x => RequestContext.CurrentRelativePath));
            RegisterField(new MacroField("Protocol", x => (RequestContext.URL != null ? RequestContext.CurrentScheme : "")));
            RegisterField(new MacroField("IP", x => RequestContext.UserHostAddress));
            RegisterField(new MacroField("CurrentURL", x => RequestContext.CurrentURL));
            RegisterField(new MacroField("CurrentCulture", x => CultureHelper.GetPreferredCulture()));
            RegisterField(new MacroField("ChildResolver", x => x.Resolver.CreateChild()));
            RegisterField(new MacroField("DatabaseSchema", () => SqlHelper.GetSafeOwner(SqlHelper.GetDBSchemaOrDefault())));

            // Register other macro types
            RegisterField(new MacroField("QueryString", () => QueryHelper.Instance));
            RegisterField(new MacroField("Cookies", () => CookieHelper.Instance));
            RegisterField(new MacroField("Custom", x => new CustomMacroContainer(x.Resolver)));
            RegisterField(new MacroField("Path", x => new PathMacroContainer(x.Resolver)));
        }
    }
}