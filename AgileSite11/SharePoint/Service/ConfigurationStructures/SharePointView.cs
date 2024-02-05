using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.SharePoint
{
    /// <summary>
    /// Represents the &lt;View> element used for SharePoint data retrieval configuration.
    /// Only certain configuration options are made available.
    /// The class is version independent and each SharePoint service implementation may process it differently.
    /// </summary>
    /// <remarks>
    /// The class will likely extend in the future.
    /// </remarks>
    public class SharePointView
    {
        #region "Fields"

        private List<string> mViewFields = new List<string>();
        private readonly Dictionary<string, string> mAttributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); 

        #endregion


        #region "Properties"

        /// <summary>
        /// Provides access to the collection of &lt;View> element attributes.
        /// Attributes which are not contained in the collection have their default meaning.
        /// Access to some attributes is provided via short hand properties.
        /// </summary>
        /// <seealso cref="Scope"/>
        /// <remarks>
        /// For informational list of possible attributes see http://msdn.microsoft.com/en-us/library/office/ms438338(v=office.14).aspx
        /// Please note that the actual recognized attributes are service type and version dependent and each implementing <see cref="ISharePointService"/> may handle them differently.
        /// </remarks>
        public Dictionary<string, string> Attributes
        {
            get
            {
                return mAttributes;
            }
        }


        /// <summary>
        /// Gets or sets the Scope attribute value.
        /// Null value removes the attribute (keeping it default).
        /// Typical values are <strong>FilesOnly</strong> (show only the files of a specific folder), <strong>Recursive</strong> (show all files of all folders)
        /// or <strong>RecursiveAll</strong> (show all files and all subfolders of all folders). The default lists only files and subfolders of a specific folder.
        /// </summary>
        public string Scope
        {
            get
            {
                if (mAttributes.ContainsKey("Scope"))
                {
                    return mAttributes["Scope"];
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    mAttributes.Remove("Scope");
                }
                else
                {
                    mAttributes["Scope"] = value;
                }
            }
        }


        /// <summary>
        /// The inner XML of &lt;Query> element.
        /// </summary>
        public string QueryInnerXml
        {
            get;
            set;
        }


        /// <summary>
        /// List of view fields (fields that will be retrieved from the server; some system fields are always retrieved).
        /// </summary>
        public List<string> ViewFields
        {
            get
            {
                return mViewFields;
            }
            set
            {
                mViewFields = value;
            }
        }


        /// <summary>
        /// Specifies the limit of rows retrieved from SharePoint server.
        /// Non-positive values mean no limit.
        /// </summary>
        public int RowLimit
        {
            get;
            set;
        }

        #endregion
    }
}
