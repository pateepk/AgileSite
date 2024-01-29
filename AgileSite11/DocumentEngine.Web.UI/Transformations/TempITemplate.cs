using System;
using System.Linq;
using System.Text;
using System.Web.UI;

using CMS.EventLog;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// ITemplate based class used for lazy loading of templates due to performance options
    /// </summary>
    internal class TempITemplate : ITemplate
    {
        #region "Variables"
        
        private ITemplate mTemplate = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the current Page instance
        /// </summary>
        public Page Page
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the template path
        /// </summary>
        public string Path
        {
            get;
            private set;
        }

        
        /// <summary>
        /// Current ITemplate object
        /// </summary>
        public ITemplate Template
        {
            get
            {
                if (mTemplate == null)
                {
                    try
                    {
                        mTemplate = Page.LoadTemplate(Path);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        Exception newex = new Exception("[TempITemplate.Template]: " + ex.Message, ex);

                        EventLogProvider.LogException("Controls", "LoadTransformation", newex);

                        return new CMSErrorTransformationTemplate(newex);
                    }
                }
                return mTemplate;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Instantiate in control container
        /// </summary>
        /// <param name="container">Container control</param>
        public void InstantiateIn(Control container)
        {
            Template.InstantiateIn(container);
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="page">Current page</param>
        /// <param name="path">Current path</param>
        public TempITemplate(Page page, string path)
        {
            Path = path;
            Page = page;
        }

        #endregion
    }
}
