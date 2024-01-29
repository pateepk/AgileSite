using System;
using System.Web;

using CMS.Base.Web.UI;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.UIControls;

namespace CMS.Forums.Web.UI
{
    /// <summary>
    /// Multifile forums uploader class for Http handler.
    /// </summary>
    public class ForumsUploader : IHttpHandler
    {
        #region "Public Methods"

        /// <summary>
        /// Processes request
        /// </summary>
        /// <param name="context">HTTP context</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                // Get arguments passed via query string
                UploaderHelper args = new UploaderHelper(context);
                String appPath = context.Server.MapPath("~/");
                DirectoryHelper.EnsureDiskPath(args.FilePath, appPath);

                if (args.Canceled)
                {
                    // Remove file from server if canceled
                    args.CleanTempFile();
                }
                else
                {
                    if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.forums", CMSAdminControl.PERMISSION_MODIFY))
                    {
                        throw new Exception("Current user is not granted with modify permission per 'cms.forums' resource.");
                    }

                    bool fileSuccessfullyProcessed = args.ProcessFile();
                    if (args.Complete && fileSuccessfullyProcessed)
                    {
                        if (args.IsForumAttachmentUpload)
                        {
                            HandleForumUpload(args, context);
                        }
                        args.CleanTempFile();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                EventLogProvider.LogException("MultiFileUploader", "UPLOADFORUM", ex);

                // Send error message
                if (context.Response.IsClientConnected)
                {
                    context.Response.Write(String.Format(@"0|{0}", TextHelper.EnsureLineEndings(ex.Message, " ")));
                    context.Response.ContentType = "text/plain";
                    context.Response.Flush();
                }
            }
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Provides operations necessary to create and store new cms file.
        /// </summary>
        /// <param name="args">Upload arguments.</param>
        /// <param name="context">HttpContext instance.</param>
        private void HandleForumUpload(UploaderHelper args, HttpContext context)
        {
            try
            {
                ForumInfo fi = ForumInfoProvider.GetForumInfo(args.ForumArgs.PostForumID);
                if (fi != null)
                {
                    ForumGroupInfo fgi = ForumGroupInfoProvider.GetForumGroupInfo(fi.ForumGroupID);
                    if (fgi != null)
                    {
                        ForumAttachmentInfo fai = new ForumAttachmentInfo(args.FilePath, 0, 0, 0)
                        {
                            AttachmentPostID = args.ForumArgs.PostID,
                            AttachmentSiteID = fgi.GroupSiteID
                        };
                        ForumAttachmentInfoProvider.SetForumAttachmentInfo(fai);
                    }
                }
            }
            catch (Exception ex)
            {
                args.Message = ex.Message;

                // Log the error
                EventLogProvider.LogException("MultiFileUploader", "UPLOADFORUM", ex);
            }
            finally
            {
                if (!string.IsNullOrEmpty(args.AfterSaveJavascript))
                {
                    args.AfterScript = String.Format(
@"
if (window.{0} != null) {{
    window.{0}(files)
}} else if ((window.parent != null) && (window.parent.{0} != null)) {{
    window.parent.{0}(files) 
}}
", 
                        args.AfterSaveJavascript
                    );
                }
                else
                {
                    args.AfterScript = String.Format(
@"
if (window.InitRefresh_{0})
{{
    window.InitRefresh_{0}('{1}', false, false);
}}
else {{ 
    if ('{1}' != '') {{
        alert('{1}');
    }}
}}
", 
                        args.ParentElementID, 
                        ScriptHelper.GetString(args.Message.Trim(), false)
                    );
                }

                args.AddEventTargetPostbackReference();

                if (context.Response.IsClientConnected)
                {
                    context.Response.Write(args.AfterScript);
                    context.Response.ContentType = "application/javascript";
                    context.Response.Flush();
                }
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets a value indicating whether another request can use the System.Web.IHttpHandler instance.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion
    }
}