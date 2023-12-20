<%@ WebHandler Language="C#" Class="CMSApp.KofcNC.UploadToParish" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace CMSApp.KofcNC
{

    /// <summary>
    /// Summary description for UploadToParish
    /// </summary>
    public class UploadToParish : Telerik.Web.UI.CloudUploadHandler
    {
        public override void SetKeyName(object sender, Telerik.Web.UI.CloudUpload.SetKeyNameEventArgs e)
        {
            var customFolder = "parishes/";
            // TODO: Clean the file name here
            e.KeyName = string.Format("{0}{1}_{2}", customFolder, Guid.NewGuid(), e.OriginalFileName);
        }
    }
}



