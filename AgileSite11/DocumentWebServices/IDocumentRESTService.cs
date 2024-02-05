using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

using CMS.WebServices;

namespace CMS.DocumentWebServices
{
    /// <summary>
    /// REST service interface for documents
    /// </summary>
    [ServiceContract]
    public interface IDocumentRESTService
    {
        #region "Document methods"

        /// <summary>
        /// Selects tree node(s) according to provided parameters and returns them as dataset. 
        /// Three oprations are supported: document (= select single document), all (= select documents), childrenof (= all children of given node)
        /// If classNames not specified, the result does not contain coupled data.
        /// </summary>
        /// <param name="operation">Operation to perform with document</param>
        [OperationContract]
        [WebGet(UriTemplate = "/content/{operation}")]
        [RESTSecurityInvoker]
        Stream GetDocument(string operation);


        /// <summary>
        /// Processes given document.
        /// </summary>
        /// <param name="operation">Operation to perform with document</param>
        /// <param name="stream">Stream with document data</param>
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/content/{operation}")]
        [RESTSecurityInvoker]
        Stream UpdateDocument(string operation, Stream stream);


        /// <summary>
        /// Creates new document.
        /// </summary>
        /// <param name="operation">Operation to perform with document</param>
        /// <param name="stream">Stream with document data</param>
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/content/{operation}")]
        [RESTSecurityInvoker]
        Stream CreateDocument(string operation, Stream stream);


        /// <summary>
        /// Deletes specified document.
        /// </summary>
        /// <param name="operation">Operation to perform with document</param>
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/content/{operation}")]
        [RESTSecurityInvoker]
        Stream DeleteDocument(string operation);

        #endregion


        #region "Translation methods"

        /// <summary>
        /// Gateway for submiting an XLIFF document.
        /// </summary>
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "")]
        [RESTSecurityInvoker]
        Stream Translate(Stream stream);

        #endregion
    }
}