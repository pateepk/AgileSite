<%@ WebHandler Language="C#" Class="CMSApp.KofcNC.UploadToAssembly" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CMSApp.KofcNC
{
    /// <summary>
    /// Summary description for UploadToAssembly
    /// </summary>
    public class UploadToAssembly : Telerik.Web.UI.CloudUploadHandler
    {

        public override void SetKeyName(object sender, Telerik.Web.UI.CloudUpload.SetKeyNameEventArgs e)
        {
            var customFolder = "assemblies/";
            // TODO: Clean the file name here
            e.KeyName = string.Format("{0}{1}_{2}", customFolder, Guid.NewGuid(), e.OriginalFileName);
        }
    }
}



