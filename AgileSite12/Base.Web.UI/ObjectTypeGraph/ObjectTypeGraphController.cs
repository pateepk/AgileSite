using System;
using System.Net;
using System.Net.Http;

using CMS.Base.Web.UI;
using CMS.Core;
using CMS.EventLog;
using CMS.WebApi;

[assembly: RegisterCMSApiController(typeof(ObjectTypeGraphController))]

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Handles retrieval of data for vis.js object type graphs.
    /// </summary>
    /// <exclude />
    [AllowOnlyEditor]
    [HandleExceptions]
    public sealed class ObjectTypeGraphController : CMSApiController
    {
        private readonly IObjectTypeGraphService mObjectTypeGraphService;

        /// <summary>
        /// Default constructor that initializes the <see cref="IObjectTypeGraphService"/> implementation.
        /// </summary>
        public ObjectTypeGraphController()
                : this(Service.Resolve<IObjectTypeGraphService>())
        {
        }


        internal ObjectTypeGraphController(IObjectTypeGraphService service)
        {
            mObjectTypeGraphService = service;
        }


        /// <summary>
        /// Returns graph data for the specified object type in JSON format.
        /// </summary>
        /// <param name="objectType">The object type for which the graph data is loaded.</param>
        /// <param name="scope">Indicates which types of related object types are loaded for standard objects. Integer representing bits.</param>
        public HttpResponseMessage GetGraphData(string objectType, int scope)
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, mObjectTypeGraphService.LoadGraphData(objectType, (ObjectTypeGraphScopeEnum)scope), "application/json");
            }
            catch (ArgumentException e)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { error = e.Message }, "application/json");
            }
            catch (Exception e)
            {
                EventLogProvider.LogException("ObjectTypeGraphController", "GetGraphData", e);

                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { error = "Unexpected server error." }, "application/json");
            }
        }
    }
}