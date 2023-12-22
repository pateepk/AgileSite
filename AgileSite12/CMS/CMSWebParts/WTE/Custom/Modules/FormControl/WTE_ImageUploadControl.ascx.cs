using System;
using System.IO;
using System.IO.Packaging;
using System.Web;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.FormEngine;
using CMS.FormEngine.Web.UI;
using CMS.Helpers;
using CMS.IO;
using CMS.OnlineForms.Web.UI;
using CMS.SiteProvider;
using Telerik.Web.UI;
using Telerik.Web.UI.Skins;

namespace CMSApp.CMSWebParts.WTE.Custom.Modules.FormControl
{
    public partial class WTE_ImageUploadControl : FormEngineUserControl
    {
        #region "Variables"

        private string mValue;

        #endregion "Variables"

        #region "Properties"

        protected string SessionKeyGUID
        {
            get
            {
                string ret = InputClientID + "_WTEUploadFileGUID";
                return ret;
            }
        }

        protected string SessionKeyFileName
        {
            get
            {
                string ret = InputClientID + "_WTEUploadFileName";
                return ret;
            }
        }

        protected string fileGuid
        {
            get
            {
                return GetSession(SessionKeyGUID);
            }
            set
            {
                SetSession(SessionKeyGUID, value);
            }
        }

        private string fileGuidwithExt
        {
            get
            {
                return GetSession(SessionKeyFileName);
            }
            set
            {
                SetSession(SessionKeyFileName, value);
            }
        }

        /// <summary>
        /// Gets or sets the enabled state of the control.
        /// </summary>
        public override bool Enabled
        {
            get
            {
                return uploader.Enabled;
            }
            set
            {
                uploader.Enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets form control value.
        /// </summary>
        public override object Value
        {
            get
            {
                // make sure the value is updated
                SaveUploadedFile(false);
                if (!String.IsNullOrWhiteSpace(fileGuidwithExt))
                {
                    return fileGuidwithExt;
                }
                if (String.IsNullOrEmpty(mValue) || (mValue == Guid.Empty.ToString()))
                {
                    return null;
                }
                return mValue;
            }
            set
            {
                mValue = ValidationHelper.GetString(value, String.Empty);
            }
        }

        public string ValueString
        {
            get
            {
                string ret = null;
                if (Value != null)
                {
                    ret = Value.ToString();
                }
                return ret;
            }
            set
            {
                mValue = ValidationHelper.GetString(value, String.Empty);
            }
        }

        /// <summary>
        /// Returns true if the form control provides some value to the field
        /// </summary>
        public override bool HasValue
        {
            get
            {
                //return !(Form is CMSForm);
                return !String.IsNullOrWhiteSpace(ValueString);
            }
        }

        /// <summary>
        /// Returns client ID of the inner upload control.
        /// </summary>
        public override string InputClientID
        {
            get
            {
                return uploader.UploadControl.ClientID;
            }
        }

        #endregion "Properties"

        #region "Methods"

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (FieldInfo != null)
            {
                uploader.ID = FieldInfo.Name;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var test = ruMain.UploadedFiles;
            if (Form != null)
            {
                uploader.OnUploadFile += UploadControl_FileUploaded;
                uploader.OnDeleteFile += UploadControl_OnFileDeleted;
            }

            // Apply styles
            if (!String.IsNullOrEmpty(ControlStyle))
            {
                uploader.Attributes.Add("style", ControlStyle);
                ControlStyle = null;
            }
            if (!String.IsNullOrEmpty(CssClass))
            {
                uploader.CssClass = CssClass;
                CssClass = null;
            }

            // Set image auto resize configuration
            if (FieldInfo != null)
            {
                int uploaderWidth;
                int uploaderHeight;
                int uploaderMaxSideSize;

                ImageHelper.GetAutoResizeDimensions(FieldInfo.Settings, SiteContext.CurrentSiteName, out uploaderWidth, out uploaderHeight, out uploaderMaxSideSize);

                uploader.ResizeToWidth = uploaderWidth;
                uploader.ResizeToHeight = uploaderHeight;
                uploader.ResizeToMaxSideSize = uploaderMaxSideSize;
            }

            CheckFieldEmptiness = false;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Hide hidden button
            hdnPostback.Style.Add("display", "none");

            // Do not process special actions if there is no form
            if (Form == null)
            {
                return;
            }

            // Uploader is not supported in CMSForm anymore
            if (Form is CMSForm)
            {
                lblError.Text = ResHelper.GetString("uploader.pagesnotsupported");
                lblError.Visible = true;
                plcUpload.Visible = false;
                return;
            }

            //bool isbizform = (Form is BizForm);
            //string filename1 = String.Empty;
            //string filename2 = String.Empty;
            //try
            //{
            //    //filename1 = ((BizForm)Form).GetFileNameForUploader(mValue);
            //    filename2 = FormHelper.GetOriginalFileName(mValue);
            //}
            //catch (Exception ex)
            //{
            //    ///
            //}

            SaveUploadedFile(true);

            //if (uploader.PostedFile != null)
            //{
            //    string vpath = "~/sites";
            //    string physicalpath = Server.MapPath(vpath);
            //    byte[] previewFileBinary = new byte[uploader.PostedFile.ContentLength];
            //    uploader.PostedFile.InputStream.Read(previewFileBinary, 0, uploader.PostedFile.ContentLength);
            //    //string guidname = Guid.NewGuid().ToString();
            //    fileGuid = Guid.NewGuid().ToString();
            //    System.IO.FileInfo fi = new System.IO.FileInfo(uploader.PostedFile.FileName);
            //    fileGuidwithExt = fileGuid + fi.Extension;
            //    string guidfilename = fileGuidwithExt;
            //    string fname = guidfilename + "|" + uploader.PostedFile.FileName;
            //    string physicalfname = physicalpath + "/" + guidfilename;

            //    CMS.IO.File.WriteAllBytes(physicalfname, previewFileBinary);

            //    //http://slipcash.agilesite.com/sites/b7eddbfc-da9d-477f-9e54-ae1992a2472c.JPG

            //    uploader.CurrentFileName = fname;
            //    Value = fname;

            //    uploader.CurrentFileName = fileGuidwithExt;
            //    Value = fileGuidwithExt;

            //    uploader.CurrentFileUrl = "~/sites/" + fileGuidwithExt;
            //    imgPreview.ImageUrl = "~/sites/" + fileGuidwithExt;
            //    Form.Data.SetValue(FieldInfo.Name, uploader.CurrentFileName);
            //}
            //else
            //{
            //    uploader.CurrentFileName = (Form is BizForm) ? ((BizForm)Form).GetFileNameForUploader(mValue) : FormHelper.GetOriginalFileName(mValue);
            //    uploader.CurrentFileUrl = "~/CMSPages/GetBizFormFile.aspx?filename=" + FormHelper.GetGuidFileName(mValue) + "&sitename=" + Form.SiteName;
            //}

            // Register post back button for update panel
            if (Form.ShowImageButton && Form.SubmitImageButton.Visible)
            {
                ControlsHelper.RegisterPostbackControl(Form.SubmitImageButton);
            }
            else if (Form.SubmitButton.Visible)
            {
                ControlsHelper.RegisterPostbackControl(Form.SubmitButton);
            }
        }

        /// <summary>
        /// Returns true if user control is valid.
        /// </summary>
        public override bool IsValid()
        {
            var postedFile = uploader.PostedFile;

            // Check allow empty
            if ((FieldInfo != null) && !FieldInfo.AllowEmpty && ((Form == null) || Form.CheckFieldEmptiness))
            {
                if (String.IsNullOrEmpty(uploader.CurrentFileName) && (postedFile == null))
                {
                    // Empty error
                    if ((ErrorMessage != null) && !ErrorMessage.EqualsCSafe(ResHelper.GetString("BasicForm.InvalidInput"), true))
                    {
                        ValidationError = ErrorMessage;
                    }
                    else
                    {
                        ValidationError += ResHelper.GetString("BasicForm.ErrorEmptyValue");
                    }
                    return false;
                }
            }

            if ((postedFile != null) && (!String.IsNullOrEmpty(postedFile.FileName.Trim())))
            {
                // Test if file has allowed file-type
                string customExtension = ValidationHelper.GetString(GetValue("extensions"), String.Empty);
                string extensions = null;

                if (CMSString.Compare(customExtension, "custom", true) == 0)
                {
                    extensions = ValidationHelper.GetString(GetValue("allowed_extensions"), String.Empty);
                }

                // Only extensions that are also allowed in settings can be uploaded
                extensions = UploadHelper.RestrictExtensions(extensions, SiteContext.CurrentSiteName);

                string ext = CMS.IO.Path.GetExtension(postedFile.FileName);
                string validationError = String.Empty;

                if (extensions.EqualsCSafe(UploadHelper.NO_ALLOWED_EXTENSION))
                {
                    validationError = ResHelper.GetString("uploader.noextensionallowed");
                }
                else if (!UploadHelper.IsExtensionAllowed(ext, extensions))
                {
                    validationError = String.Format(ResHelper.GetString("BasicForm.ErrorWrongFileType"), HTMLHelper.HTMLEncode(ext.TrimStart('.')), extensions.Replace(";", ", "));
                }

                if (!String.IsNullOrEmpty(validationError))
                {
                    ValidationError += validationError;
                    return false;
                }
            }

            return true;
        }

        #endregion "Methods"

        #region "page events"

        /// <summary>
        /// a file is uploaded (note this event trigger for each file, but only when a postback occurs)
        /// Post back is trigger through the client handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void OnFileUploaded(object sender, FileUploadedEventArgs e)
        {
            //Clear changes and remove uploaded image from Cache
            Context.Cache.Remove(Session.SessionID + "UploadedFile");
            using (Stream stream = e.File.InputStream)
            {
                byte[] imgData = new byte[stream.Length];
                stream.Read(imgData, 0, imgData.Length);
                MemoryStream ms = new MemoryStream();
                ms.Write(imgData, 0, imgData.Length);
                Context.Cache.Insert(Session.SessionID + "UploadedFile", ms, null, DateTime.Now.AddMinutes(20), TimeSpan.Zero);
            }
        }

        protected void UploadControl_FileUploaded(object sender, EventArgs e)
        {
            Form.RaiseOnUploadFile(sender, e);
        }

        protected void UploadControl_OnFileDeleted(object sender, EventArgs e)
        {
            Form.RaiseOnDeleteFile(sender, e);
        }

        #endregion "page events"

        #region "Methods"

        protected string GetUploadedFileName()
        {
            string ret = String.Empty;
            if (!String.IsNullOrWhiteSpace(fileGuidwithExt))
            {
                ret = fileGuidwithExt;
            }
            return ret;
        }

        /// <summary>
        /// Save the uploaded file
        /// </summary>
        protected void SaveUploadedFile(bool p_writefile = false)
        {
            if (uploader.PostedFile != null)
            {
                //string guidname = Guid.NewGuid().ToString();

                if (String.IsNullOrWhiteSpace(fileGuid))
                {
                    // we do not have this, so set a new one.
                    fileGuid = Guid.NewGuid().ToString();
                }
                string vpath = "~/sites";
                string physicalpath = Server.MapPath(vpath);
                System.IO.FileInfo fi = new System.IO.FileInfo(uploader.PostedFile.FileName);
                fileGuidwithExt = fileGuid + fi.Extension;
                string guidfilename = fileGuidwithExt;
                string fname = guidfilename + "|" + uploader.PostedFile.FileName;
                string physicalfname = physicalpath + "/" + guidfilename;

                if (p_writefile)
                {
                    byte[] previewFileBinary = new byte[uploader.PostedFile.ContentLength];
                    uploader.PostedFile.InputStream.Read(previewFileBinary, 0, uploader.PostedFile.ContentLength);
                    CMS.IO.File.WriteAllBytes(physicalfname, previewFileBinary);
                    //http://slipcash.agilesite.com/sites/b7eddbfc-da9d-477f-9e54-ae1992a2472c.JPG

                    uploader.CurrentFileName = fname;
                    mValue = fname;

                    uploader.CurrentFileName = fileGuidwithExt;
                    mValue = fileGuidwithExt;

                    imgPreview.ImageUrl = "http://slipcash.agilesite.com/sites/" + fileGuidwithExt;
                    //uploader.CurrentFileUrl = "~/sites/" + fileGuidwithExt;
                    //imgPreview.ImageUrl = "~/sites/" + fileGuidwithExt;
                    //Form.Data.SetValue(FieldInfo.Name, uploader.CurrentFileName);
                    uploader.CurrentFileName = String.Empty; // does this blank it out?
                    uploader.CurrentFileUrl = String.Empty;
                }
            }
            else
            {
                // reset the guid here.
                fileGuid = String.Empty;
                imgPreview.ImageUrl = "http://slipcash.agilesite.com/sites/" + mValue;
                // there is no posted file, but there is a value
                uploader.CurrentFileName = String.Empty; // does this blank it out?
                uploader.CurrentFileUrl = String.Empty;
            }
        }

        #endregion "Methods"

        #region "session helper"

        /// <summary>
        /// Get Session data
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected string GetSession(string p_key)
        {
            string ret = String.Empty;
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        ret = (string)HttpContext.Current.Session[p_key];
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected void SetSession(string p_key, object p_value)
        {
            try
            {
                if (p_value == null)
                {
                    ClearSession(p_key);
                }
                else
                {
                    if (HttpContext.Current != null)
                    {
                        if (HttpContext.Current.Session != null)
                        {
                            HttpContext.Current.Session[p_key] = p_value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Get request parameter
        /// </summary>
        /// <param name="p_key"></param>
        /// <returns></returns>
        protected void ClearSession(string p_key)
        {
            try
            {
                if (HttpContext.Current != null)
                {
                    if (HttpContext.Current.Session != null)
                    {
                        HttpContext.Current.Session.Remove(p_key);
                    }
                }
            }
            catch (Exception ex)
            {
                // for now.
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        #endregion "session helper"
    }
}