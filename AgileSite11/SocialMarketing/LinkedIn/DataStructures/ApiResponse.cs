using System;
using System.Linq;
using System.Net;
using System.Text;


namespace CMS.SocialMarketing.LinkedInInternal
{
    /// <summary>
    /// Represents a response to LinkedIn API request.
    /// </summary>
    internal class ApiResponse
    {
        /// <summary>
        /// Contains the HTTP status code of the response.
        /// </summary>
        public HttpStatusCode HttpStatusCode
        {
            get;
            set;
        }


        /// <summary>
        /// Contains the reponse body.
        /// </summary>
        public string ResponseBody
        {
            get;
            set;
        }
    }
}
