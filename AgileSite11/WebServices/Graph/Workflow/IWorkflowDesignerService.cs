using System;
using System.Runtime.Serialization;
using System.ServiceModel;

using CMS.WorkflowEngine;
using CMS.Helpers.UniGraphConfig;

namespace CMS.WebServices
{
    /// <summary>
    /// Workflow graph contract for purposes of JavaScript proxy object generation. Its name is same as the name of interface so if we used IGraphService only,
    /// we could access from JavaScript only one implementation. Eventually we want multiple graph implementations with different service implementations on a page.
    /// </summary>
    [ServiceContract(Namespace = "")]
    public interface IWorkflowDesignerService : IGraphService
    {
    }
}
