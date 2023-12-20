using System.Diagnostics;
namespace CMS.CMSImportExport
{
    /// <summary>
    /// Web template additional object
    /// </summary>
    [DebuggerDisplay("{GetType()}: {ObjectType}, {ObjectCodeName}, IsSite:{IsSiteObject}")]
    public class ExportWebTemplateAdditionalObject
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Object code name.
        /// </summary>
        public string ObjectCodeName
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates if object is site dependent.
        /// </summary>
        public bool IsSiteObject
        {
            get;
            set;
        }
    }
}