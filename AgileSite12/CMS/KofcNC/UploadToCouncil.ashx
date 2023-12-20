<%@ WebHandler Language="C#"  Class="CMSApp.KofcNC.UploadToCouncil" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMSApp.KofcNC
{
    /// <summary>
    /// Summary description for UploadToCouncil
    /// </summary>
    public class UploadToCouncil : Telerik.Web.UI.CloudUploadHandler
    {
        public override void SetKeyName(object sender, Telerik.Web.UI.CloudUpload.SetKeyNameEventArgs e)
        {
            var customFolder = "councils/";
            // TODO: Clean the file name here
            e.KeyName = string.Format("{0}{1}_{2}", customFolder, Guid.NewGuid(), e.OriginalFileName);
        }
    }
}