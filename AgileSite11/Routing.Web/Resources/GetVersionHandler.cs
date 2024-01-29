using System;
using System.Web;

using CMS.Base;
using CMS.Helpers;
using CMS.Routing.Web;

[assembly: RegisterHttpHandler("CMSPages/GetCMSVersion.aspx", typeof(GetVersionHandler), Order = 1)]

namespace CMS.Routing.Web
{
    /// <summary>
    /// Handler to get CMS version
    /// </summary>
    internal class GetVersionHandler : IHttpHandler
    {
        /// <summary>
        /// Gets a value indicating whether another request can use the IHttpHandler instance.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Processes the incoming HTTP request and returns current system version if version key is valid.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        public void ProcessRequest(HttpContext context)
        {
            var versionKey = QueryHelper.GetString("versionkey", string.Empty);
            if (EncryptionHelper.VerifyVersionRSA(versionKey))
            {
                Version v = CMSVersion.Version;
                if (v != null)
                {
                    var response = context.Response;

                    response.Clear();
                    response.Write(v.ToString(3));

                    RequestHelper.EndResponse();
                }
            }

            RequestHelper.Respond404();
        }
    }
}
