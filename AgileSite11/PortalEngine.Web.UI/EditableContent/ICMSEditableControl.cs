using CMS.DocumentEngine;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Interface class for the editable controls (controls that needs to store the content within the document content).
    /// </summary>
    public interface ICMSEditableControl : ISpellCheckableControl
    {
        /// <summary>
        /// Page manager control.
        /// </summary>
        IPageManager PageManager
        {
            get;
            set;
        }


        /// <summary>
        /// Control view mode.
        /// </summary>
        ViewModeEnum ViewMode
        {
            get;
            set;
        }


        /// <summary>
        /// Enabled.
        /// </summary>
        bool Enabled
        {
            get;
            set;
        }


        /// <summary>
        /// Control ID.
        /// </summary>
        string ID
        {
            get;
            set;
        }


        /// <summary>
        /// Loads the control content.
        /// </summary>
        /// <param name="content">Dynamic content to load to the web part</param>
        /// <param name="forceReload">If true, the content is forced to be reloaded</param>
        void LoadContent(string content, bool forceReload = false);


        /// <summary>
        /// Loads the control content.
        /// </summary>
        /// <param name="pageInfo">Page info with the web part content</param>
        /// <param name="forceReload">If true, the content is forced to be reloaded</param>
        void LoadContent(PageInfo pageInfo, bool forceReload = false);


        /// <summary>
        /// Returns the current web part content.
        /// </summary>
        string GetContent();


        /// <summary>
        /// Saves the control content.
        /// </summary>
        /// <param name="pageInfo">Page info where to store the content</param>
        void SaveContent(PageInfo pageInfo);


        /// <summary>
        /// Returns true if entered data is valid. If data is invalid, it returns false and displays an error message.
        /// </summary>
        bool IsValid();
    }
}