using System;
using System.Linq;
using System.Text;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Properties which are needed to perform mass action on the DocumentList. This mainly includes filter values (so correct where condition can be produced), 
    /// URLs of the opened dialogs, etc.
    /// </summary>
    public class DocumentListMassActionsParameters
    {
        /// <summary>
        /// True if all levels of documents are displayed. False if only one nested level is displayed.
        /// </summary>
        public bool ShowAllLevels
        {
            get;
            set;
        }


        /// <summary>
        /// ID of the document type which child nodes are to be displayed. 0 if all document types are displayed.
        /// </summary>
        public int ClassID
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition of the filter.
        /// </summary>
        public string CurrentWhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the page is displayed as dialog. 
        /// </summary>
        public bool RequiresDialog
        {
            get;
            set;
        }


        /// <summary>
        /// WOpenerNodeID from the url query.
        /// </summary>
        public int WOpenerNodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Node whose children are displayed.
        /// </summary>
        public TreeNode Node
        {
            get;
            set;
        }


        /// <summary>
        /// Random identifier which can be used as a key when storing dialog parameters to the WindowHelper.
        /// </summary>
        public string Identifier
        {
            get;
            set;
        }


        #region "Paths"

        /// <summary>
        /// Return URL for delete action.
        /// </summary>
        public string DeleteReturnUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Return URL for translate action.
        /// </summary>
        public string TranslateReturnUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Return URL for archive action.
        /// </summary>
        public string ArchiveReturnUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Return URL for publish action.
        /// </summary>
        public string PublishReturnUrl
        {
            get;
            set;
        }


        /// <summary>
        /// Function which creates path where content tree in copy/move/link dialogs will start.
        /// The only parameter is name of the action ("Move", "Copy" or "Link").
        /// </summary>
        public Func<string, string> GetCopyMoveLinkBaseActionUrl
        {
            get;
            set;
        }

        #endregion
    }
}