using System;
using System.Web;

using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.SiteProvider;

using IOExceptions = System.IO;

namespace CMS.SharePoint.Web.UI
{
    /// <summary>
    /// Multi file upload handler for SharePointFiles in SharePointLibraries
    /// </summary>
    public class SharePointMultiFileUploader : IHttpHandler
    {
        /// <summary>
        /// Name of the SharePointLibrary ID NameValuePair in the AdditionalParameters collection
        /// </summary>
        public const string SHAREPOINT_LIBRARY_ID_PARAMETER_NAME = "SharePointLibaryID";


        /// <summary>
        /// Name of the SharePointFile ID NameValuePair in the AdditionalParameters collection
        /// </summary>
        public const string SHAREPOINT_FILE_ID_PARAMETER_NAME = "SharePointFileID";


        /// <summary>
        /// Name of the default JS module that handles the returned JS commands
        /// </summary>
        public const string DEFAULT_JS_MODULE_NAME = "CMSSharePoint/SharePointFileUpload";


        /// <summary>
        /// Holds the instance of current UploadHelper. Must be initialized at the beginning of request processing.
        /// </summary>
        private UploaderHelper mHelper;


        /// <summary>
        /// String containing the response
        /// </summary>
        private string mResponse;


        // Cache fields
        private int mSharePointLibraryID = -1;
        private int mSharePointFileID = -1;
        private SharePointLibraryInfo mSharePointLibrary;


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


        /// <summary>
        /// Gets the SharePointLibraryID from additional parameters
        /// </summary>
        private int SharePointLibraryID
        {
            get
            {
                if (mSharePointLibraryID < 0)
                {
                    mSharePointLibraryID = ValidationHelper.GetInteger(mHelper.AdditionalArgsCollection.Get(SHAREPOINT_LIBRARY_ID_PARAMETER_NAME), 0);
                }
                return mSharePointLibraryID;
            }
        }


        /// <summary>
        /// Gets the SharePointLibraryID from additional parameters
        /// </summary>
        private int SharePointFileID
        {
            get
            {
                if (mSharePointFileID < 0)
                {
                    mSharePointFileID = ValidationHelper.GetInteger(mHelper.AdditionalArgsCollection.Get(SHAREPOINT_FILE_ID_PARAMETER_NAME), 0);
                }
                return mSharePointFileID;
            }
        }


        /// <summary>
        /// Gets the SharePointLibrary based on <see cref="SharePointLibraryID"/>
        /// </summary>
        private SharePointLibraryInfo SharePointLibrary
        {
            get
            {
                if (mSharePointLibrary == null && SharePointLibraryID > 0)
                {
                    mSharePointLibrary = SharePointLibraryInfoProvider.GetSharePointLibraryInfo(SharePointLibraryID);
                }

                return mSharePointLibrary;
            }
        }


        /// <summary>
        /// Processes the upload request.
        /// </summary>
        /// <param name="context">HttpContext of the current request</param>
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                // Get arguments passed via query string
                mHelper = new UploaderHelper(context);
                String appPath = context.Server.MapPath("~/");
                DirectoryHelper.EnsureDiskPath(mHelper.FilePath, appPath);

                if (mHelper.Canceled)
                {
                    // Remove file from server if canceled
                    mHelper.CleanTempFile();

                    return;
                }

                string error = ValidateRequest();
                if (!String.IsNullOrEmpty(error))
                {
                    SetErrorResponse(error);

                    return;
                }

                ProcessFile();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("SharePoint", "UPLOADFILEGENERAL", ex, SiteContext.CurrentSiteID);
                SetErrorResponse(ResHelper.GetString("sharepoint.unknownerror"));
            }
            finally
            {
                SendResponse(context.Response);
                RequestHelper.CompleteRequest();
            }
        }


        /// <summary>
        /// Validates request.
        /// </summary>
        /// <returns>Null if validation was successful, an error message otherwise.</returns>
        private string ValidateRequest()
        {
            if (!IsRequestValid())
            {
                if (SharePointLibrary == null)
                {
                    return ResHelper.GetString("SharePoint.Library.DoesntExist");
                }
                else
                {
                    return "Request parameters are invalid.";
                }
            }

            if (!VerifyPermissions())
            {
                 return ResHelper.GetString("SharePoint.Library.CannotModify");
            }

            return null;
        }


        /// <summary>
        /// Processes the file and completes the request
        /// </summary>
        private void ProcessFile()
        {
            bool fileSuccessfullyProcessed = mHelper.ProcessFile();

            if (mHelper.Complete && fileSuccessfullyProcessed)
            {
                try
                {
                    SetSuccessResponse();
                    if (SharePointFileID <= 0)
                    {
                        AddUploadedFileToLibrary();
                    }
                    else
                    {
                        SharePointFileInfo sharePointFileInfo = SharePointFileInfoProvider.GetSharePointFileInfo(SharePointFileID);
                        if (sharePointFileInfo == null)
                        {
                            SetErrorResponse(ResHelper.GetString("editedobject.notexists"));

                            return;
                        }
                        ReplaceUploadedFileInLibrary(sharePointFileInfo);
                    }
                }
                catch (IOExceptions.FileNotFoundException)
                {
                    SetErrorResponse(ResHelper.GetString("SharePoint.File.UploadError.FileNotFound"));
                }
                catch (SharePointConnectionNotFoundException)
                {
                    SetErrorResponse(ResHelper.GetString("SharePoint.Library.ReadOnlyWarning"));
                }
                catch (SharePointFileAlreadyExistsException)
                {
                    SetErrorResponse(String.Format(ResHelper.GetString("SharePoint.File.UploadError.AlreadyExists"), mHelper.FileName));
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("SharePoint", "UPLOADFILE", ex, SiteContext.CurrentSiteID);
                    SetErrorResponse(String.Format(ResHelper.GetString("SharePoint.File.UploadError.Unknown"), mHelper.FileName));
                }
                finally
                {
                    mHelper.CleanTempFile();
                }
            }
        }


        /// <summary>
        /// Takes a completely uploaded file from the context and adds it to the SharePoint library determined by <see cref="SharePointLibrary"/>.
        /// </summary>
        private void AddUploadedFileToLibrary()
        {
            FileInfo fileInfo = FileInfo.New(mHelper.FilePath);

            using (FileStream file = fileInfo.OpenRead())
            {
                SharePointFileInfoProvider.UploadFile(SharePointLibrary, file, mHelper.FileName);
            }
        }


        /// <summary>
        /// Takes a completely uploaded file from the context and replaces the file given by <paramref name="sharePointFileInfo"/> by the uploaded one.
        /// </summary>
        /// <param name="sharePointFileInfo">Info object of the file to be replaced</param>
        private void ReplaceUploadedFileInLibrary(SharePointFileInfo sharePointFileInfo)
        {
            FileInfo fileInfo = FileInfo.New(mHelper.FilePath);

            using (FileStream file = fileInfo.OpenRead())
            {
                SharePointFileInfoProvider.UpdateFile(sharePointFileInfo, file, mHelper.FileName);
            }
        }


        /// <summary>
        /// Sets response content to success, meaning the file has been successfully processed by the server.
        /// </summary>
        private void SetSuccessResponse()
        {
            mResponse = "0";
        }


        /// <summary>
        /// Makes sure the error <paramref name="message"/> is set as a response for the client.
        /// </summary>
        /// <param name="message">Error message to be set</param>
        private void SetErrorResponse(string message)
        {
            mResponse = String.Format("0|{0}", message);
        }


        /// <summary>
        /// Verifies that current user has the permissions to upload to library specified by the context.
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        private bool VerifyPermissions()
        {
            if (SharePointLibrary != null)
            {
                return SharePointLibrary.CheckPermissions(PermissionsEnum.Modify, SiteContext.CurrentSiteName, MembershipContext.AuthenticatedUser);
            }

            return false;
        }


        /// <summary>
        /// Verifies that the request is valid.
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool IsRequestValid()
        {
            return (SharePointLibraryID > 0) && (SharePointLibrary != null);
        }


        /// <summary>
        /// Sends the response (if any) stored in <see cref="mResponse" />.
        /// </summary>
        /// <param name="httpResponse">Instance of HttpResponse to send the response to.</param>
        private void SendResponse(HttpResponse httpResponse)
        {
            if (!String.IsNullOrEmpty(mResponse))
            {
                httpResponse.Write(mResponse);
                httpResponse.Flush();
            }
        }
    }
}
