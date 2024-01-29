using System;
using System.Web.UI.WebControls;
using System.Collections.Generic;

using CMS.DocumentEngine;
using CMS.WorkflowEngine;

using TreeNode = CMS.DocumentEngine.TreeNode;
using CMS.DataEngine;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Document manager interface.
    /// </summary>
    public interface ICMSDocumentManager
    {
        #region "Public properties"

        /// <summary>
        /// Indicates if default values should be loaded when new document or language version is being created.
        /// </summary>
        bool LoadDefaultValues
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if manager should register for common events (save etc.).
        /// </summary>
        bool RegisterEvents
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the UI should be redirected if document not found.
        /// </summary>
        bool RedirectForNonExistingDocument
        {
            get;
            set;
        }


        /// <summary>
        /// Component name
        /// </summary>
        string ComponentName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether reload page property should be automatically added to the page for reload page functionality.
        /// </summary>
        bool SetRefreshFlag
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the page should be redirected to the current page which is being edited.
        /// </summary>
        bool SetRedirectPageFlag
        {
            get;
            set;
        }


        /// <summary>
        /// Site name.
        /// </summary>
        string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if control is used on live site.
        /// </summary>
        bool IsLiveSite
        {
            get;
            set;
        }


        /// <summary>
        /// Tree provider instance used to access data. If no TreeProvider is assigned, a new TreeProvider instance is created.
        /// </summary>
        TreeProvider Tree
        {
            get;
            set;
        }


        /// <summary>
        /// Gets Workflow manager instance.
        /// </summary>
        WorkflowManager WorkflowManager
        {
            get;
        }


        /// <summary>
        /// Version manager instance.
        /// </summary>
        VersionManager VersionManager
        {
            get;
        }


        /// <summary>
        /// Indicates if check-in/check-out functionality is automatic.
        /// </summary>
        bool AutoCheck
        {
            get;
        }


        /// <summary>
        /// Document workflow.
        /// </summary>
        WorkflowInfo Workflow
        {
            get;
        }


        /// <summary>
        /// Document workflow step.
        /// </summary>
        WorkflowStepInfo Step
        {
            get;
        }


        /// <summary>
        /// Next steps
        /// </summary>
        List<WorkflowStepInfo> NextSteps
        {
            get;
        }


        /// <summary>
        /// Indicates if content should be refreshed (action is being processed)
        /// </summary>
        bool RefreshActionContent
        {
            get;
        }


        /// <summary>
        /// Document ID of document language version, which should be used as a source data for new culture version
        /// </summary>
        int SourceDocumentID
        {
            get;
            set;
        }


        /// <summary>
        /// Returns currently edited document if it is available in the given context.
        /// </summary>
        TreeNode Node
        {
            get;
        }


        /// <summary>
        /// Returns current document in any culture.
        /// </summary>
        TreeNode InvariantNode
        {
            get;
        }


        /// <summary>
        /// Source document for new language version
        /// </summary>
        TreeNode SourceNode
        {
            get;
        }


        /// <summary>
        /// Node ID. Together with DocumentCulture property identifies edited document.
        /// </summary>
        int NodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Document ID. Identifies edited document. Alternative to NodeID and CultureCode with higher priority.
        /// </summary>
        int DocumentID
        {
            get;
            set;
        }


        /// <summary>
        /// Community group ID
        /// </summary>
        int GroupID
        {
            get;
            set;
        }


        /// <summary>
        /// Parent node ID. Indicates parent node for document insertion.
        /// </summary>
        int ParentNodeID
        {
            get;
            set;
        }


        /// <summary>
        /// Returns parent node for new document insertion.
        /// </summary>
        TreeNode ParentNode
        {
            get;
            set;
        }


        /// <summary>
        /// Class ID of the document type of new document.
        /// </summary>
        int NewNodeClassID
        {
            get;
            set;
        }


        /// <summary>
        /// Class name of the document type of new document.
        /// </summary>
        string NewNodeClassName
        {
            get;
            set;
        }


        /// <summary>
        /// Node class of the document type of new document.
        /// </summary>
        DataClassInfo NewNodeClass
        {
            get;
        }


        /// <summary>
        /// Node culture of new document.
        /// </summary>
        string NewNodeCultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if java script functions for document management should be rendered.
        /// </summary>
        bool RenderScript
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if document permissions should be checked.
        /// </summary>
        bool CheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Culture code of document
        /// </summary>
        string CultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if another document should be created after newly created.
        /// </summary>
        bool CreateAnother
        {
            get;
        }


        /// <summary>
        /// Indicates whether dialog should be closed after action
        /// </summary>
        bool CloseDialog
        {
            get;
        }


        /// <summary>
        /// Returns true if the changes should be saved.
        /// </summary>
        bool SaveChanges
        {
            get;
        }


        /// <summary>
        /// Indicates if confirm dialog should be displayed if any change to the document was made
        /// </summary>
        bool ConfirmChanges
        {
            get;
        }


        /// <summary>
        /// Indicates if the support script for save changes should be registered.
        /// </summary>
        bool RegisterSaveChangesScript
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether changes should be tracked for whole form or just for parts defined by data-tracksavechanges attribute.
        /// </summary>
        bool UseFullFormSaveChanges
        {
            get;
            set;
        }


        /// <summary>
        /// Manager mode (Update, Insert, New culture version)
        /// </summary>
        FormModeEnum Mode
        {
            get;
            set;
        }


        /// <summary>
        /// Messages placeholder
        /// </summary>
        MessagesPlaceHolder MessagesPlaceHolder
        {
            get;
        }


        /// <summary>
        /// Local messages placeholder to override 
        /// </summary>
        MessagesPlaceHolder LocalMessagesPlaceHolder
        {
            get;
            set;
        }


        /// <summary>
        /// Document info panel
        /// </summary>
        CMSDocumentPanel DocumentPanel
        {
            get;
        }


        /// <summary>
        /// Local document info panel to override 
        /// </summary>
        CMSDocumentPanel LocalDocumentPanel
        {
            get;
            set;
        }


        /// <summary>
        /// Document information text
        /// </summary>
        string DocumentInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Document information label.
        /// </summary>
        Label DocumentInfoLabel
        {
            get;
        }


        /// <summary>
        /// Indicates if the control should stop processing
        /// </summary>
        bool StopProcessing
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if workflow actions should be displayed and handled
        /// </summary>
        bool HandleWorkflow
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if document can be saved within current context
        /// </summary>
        bool AllowSave
        {
            get;
        }


        /// <summary>
        /// Indicates if workflow action is processed
        /// </summary>
        bool ProcessingAction
        {
            get;
        }


        /// <summary>
        /// Indicates if DocumentHelper should be used to manage the document (Use FALSE if modifying non-versioned properties.)
        /// </summary>
        bool UseDocumentHelper
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the data is consistent.
        /// </summary>
        bool DataConsistent
        {
            get;
        }

        #endregion


        #region "Events"

        /// <summary>
        /// On data validation event handler.
        /// </summary>
        event EventHandler<DocumentManagerEventArgs> OnValidateData;


        /// <summary>
        /// On check permissions event handler.
        /// </summary>
        event EventHandler<SimpleDocumentManagerEventArgs> OnCheckPermissions;


        /// <summary>
        /// On check consistency event handler.
        /// </summary>
        event EventHandler<SimpleDocumentManagerEventArgs> OnCheckConsistency;


        /// <summary>
        /// On save data event handler.
        /// </summary>
        event EventHandler<DocumentManagerEventArgs> OnSaveData;


        /// <summary>
        /// Occurs when saving data fails.
        /// </summary>
        event EventHandler<DocumentManagerEventArgs> OnSaveFailed;


        /// <summary>
        /// On load data event handler.
        /// </summary>
        event EventHandler<DocumentManagerEventArgs> OnLoadData;


        /// <summary>
        /// On before action event handler.
        /// </summary>
        event EventHandler<DocumentManagerEventArgs> OnBeforeAction;


        /// <summary>
        /// On after action event handler.
        /// </summary>
        event EventHandler<DocumentManagerEventArgs> OnAfterAction;

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures documents consistency (Automatic blog post hierarchy etc.)
        /// </summary>
        void EnsureDocumentsConsistency();


        /// <summary>
        /// Checks if given action is allowed
        /// </summary>
        bool IsActionAllowed(string actionName);


        /// <summary>
        /// Clears properties
        /// </summary>
        void ClearProperties();


        /// <summary>
        /// Saves document.
        /// </summary>
        bool SaveDocument();


        /// <summary>
        /// Approves document.
        /// </summary>
        /// <param name="stepId">Workflow step ID (optional)</param>
        /// <param name="comment">Comment</param>
        void ApproveDocument(int stepId, string comment);


        /// <summary>
        /// Publishes document
        /// </summary>
        /// <param name="comment">Comment</param>
        void PublishDocument(string comment);


        /// <summary>
        /// Rejects document
        /// </summary>
        /// <param name="historyId">Workflow history ID (optional)</param>
        /// <param name="comment">Comment</param>
        void RejectDocument(int historyId, string comment);


        /// <summary>
        /// Checks-in document
        /// </summary>
        /// <param name="comment">Comment</param>
        void CheckInDocument(string comment);


        /// <summary>
        /// Check-outs document
        /// </summary>
        void CheckOutDocument();


        /// <summary>
        /// Undoes check-out document
        /// </summary>
        void UndoCheckOutDocument();


        /// <summary>
        /// Archives document
        /// </summary>
        /// <param name="stepId">Workflow step ID (optional)</param>
        /// <param name="comment">Comment</param>
        void ArchiveDocument(int stepId, string comment);


        /// <summary>
        /// Clears current node.
        /// </summary>
        void ClearNode();


        /// <summary>
        /// Gets script for save another action
        /// </summary>
        string GetSaveAnotherScript();

        /// <summary>
        /// Gets script for close dialog action
        /// </summary>
        string GetSaveAndCloseScript();

        /// <summary>
        /// Gets allow submit script for save changes support
        /// </summary>
        /// <returns>Allow submit script for save changes support</returns>
        string GetAllowSubmitScript();


        /// <summary>
        /// Gets submit script for save changes support
        /// </summary>
        /// <returns>Submit script for save changes support</returns>
        string GetSubmitScript();


        /// <summary>
        /// Gets default document information text
        /// </summary>
        /// <param name="includeWorkflowInfo">Indicates if workflow information should be included</param>
        string GetDocumentInfo(bool includeWorkflowInfo);


        /// <summary>
        /// Gets document information text
        /// </summary>
        /// <param name="includeWorkflowInfo">Indicates if workflow information should be included</param>
        void ShowDocumentInfo(bool includeWorkflowInfo);


        /// <summary>
        /// Shows default document information text
        /// </summary>
        /// <param name="includeWorkflowInfo">Indicates if workflow information should be included</param>
        /// <param name="message">Additional message</param>
        void ShowDocumentInfo(bool includeWorkflowInfo, string message);


        /// <summary>
        /// Indicates if local messages placeholder is used
        /// </summary>
        bool HasLocalMessagesPlaceHolder();


        /// <summary>
        /// Gets java-script function for save action
        /// </summary>
        /// <param name="createAnother">Create another flag</param>
        string GetJSSaveFunction(string createAnother);


        /// <summary>
        /// Gets java-script function
        /// </summary>
        /// <param name="action">Action name</param>
        /// <param name="argument">Action argument</param>
        /// <param name="comment">Action comment</param>
        string GetJSFunction(string action, string argument, string comment);


        /// <summary>
        /// Renders scripts
        /// </summary>
        void RenderScripts();


        /// <summary>
        /// Updates the current document
        /// </summary>
        /// <param name="useDocumentHelper">If true, the document helper is used to update the document</param>
        void UpdateDocument(bool useDocumentHelper);


        /// <summary>
        /// Clears content changed flag
        /// </summary>
        void ClearContentChanged();

        #endregion
    }
}