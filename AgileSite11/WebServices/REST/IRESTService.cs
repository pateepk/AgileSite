using System.IO;
using System.ServiceModel;
using System.ServiceModel.Syndication;
using System.ServiceModel.Web;

namespace CMS.WebServices
{
    /// <summary>
    /// REST service interface
    /// </summary>
    [ServiceContract]
    public interface IRESTService
    {
        #region "Object methods"


        #region "Single object retrieval"

        /// <summary>
        /// Returns object of given type with specified ID. If ID is not integer, than it's considered object name and 
        /// object from current site with given name is returned.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/{id}")]
        [RESTSecurityInvoker]
        Stream GetObject(string objectType, string id);


        /// <summary>
        /// Returns object of given type with given name from specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="objectName">Code name of the object</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/site/{siteName}/{objectName}")]
        [RESTSecurityInvoker]
        Stream GetSiteObject(string objectType, string siteName, string objectName);


        /// <summary>
        /// Returns global object of given type with given name.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/global/{objectName}")]
        [RESTSecurityInvoker]
        Stream GetGlobalObject(string objectType, string objectName);


        /// <summary>
        /// Returns object of given type with given name from current site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/currentsite/{objectName}")]
        [RESTSecurityInvoker]
        Stream GetCurrentSiteObject(string objectType, string objectName);

        #endregion


        #region "Multiple objects retrieval"

        /// <summary>
        /// Returns objects of given type from current site.
        /// </summary>
        /// <param name="objectType">Object type of the object(s)</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}")]
        [RESTSecurityInvoker]
        Stream GetObjects(string objectType);


        /// <summary>
        /// Returns objects of given type from specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/site/{siteName}")]
        [RESTSecurityInvoker]
        Stream GetSiteObjects(string objectType, string siteName);


        /// <summary>
        /// Returns global objects of given type.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/global")]
        [RESTSecurityInvoker]
        Stream GetGlobalObjects(string objectType);


        /// <summary>
        /// Returns objects of given type from current site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/currentsite")]
        [RESTSecurityInvoker]
        Stream GetCurrentSiteObjects(string objectType);


        /// <summary>
        /// Returns all objects of given object type.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/all")]
        [RESTSecurityInvoker]
        Stream GetAllObjects(string objectType);


        /// <summary>
        /// Returns specific child objects of an object of given type with specified ID.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        /// <param name="childObjectType">Object type of children</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/{id}/children/{childObjectType}")]
        [RESTSecurityInvoker]
        Stream GetChildren(string objectType, string id, string childObjectType);


        /// <summary>
        /// Returns specific binding objects of an object of given type with specified ID.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        /// <param name="bindingObjectType">Object type of bindings</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/{id}/bindings/{bindingObjectType}")]
        [RESTSecurityInvoker]
        Stream GetBindings(string objectType, string id, string bindingObjectType);

        #endregion


        #region "Modifying methods"

        /// <summary>
        /// Deletes object of given type satisfying given parameters (WHERE/TOPN).
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/{objectType}")]
        [RESTSecurityInvoker]
        Stream DeleteObjects(string objectType);


        /// <summary>
        /// Deletes object of given type with specified ID. If ID is not integer, than it's considered object name and 
        /// object from current site with given name is returned.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/{objectType}/{id}")]
        [RESTSecurityInvoker]
        Stream DeleteObject(string objectType, string id);


        /// <summary>
        /// Deletes object of given type with given name from specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="objectName">Code name of the object</param>
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/{objectType}/site/{siteName}/{objectName}")]
        [RESTSecurityInvoker]
        Stream DeleteSiteObject(string objectType, string siteName, string objectName);


        /// <summary>
        /// Deletes global object of given type with given name.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/{objectType}/global/{objectName}")]
        [RESTSecurityInvoker]
        Stream DeleteGlobalObject(string objectType, string objectName);


        /// <summary>
        /// Deletes object of given type with given name from current site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/{objectType}/currentsite/{objectName}")]
        [RESTSecurityInvoker]
        Stream DeleteCurrentSiteObject(string objectType, string objectName);


        /// <summary>
        /// Updates object of given type with specified ID. If ID is not integer, than it's considered object name and 
        /// object from current site with given name is returned.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        /// <param name="stream">Object data</param>
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/{objectType}/{id}")]
        [RESTSecurityInvoker]
        Stream UpdateObject(string objectType, string id, Stream stream);


        /// <summary>
        /// Updates object of given type with given name from specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="objectName">Code name of the object</param>
        /// <param name="stream">Object data</param>
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/{objectType}/site/{siteName}/{objectName}")]
        [RESTSecurityInvoker]
        Stream UpdateSiteObject(string objectType, string siteName, string objectName, Stream stream);


        /// <summary>
        /// Updates global object of given type with given name.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        /// <param name="stream">Object data</param>
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/{objectType}/global/{objectName}")]
        [RESTSecurityInvoker]
        Stream UpdateGlobalObject(string objectType, string objectName, Stream stream);


        /// <summary>
        /// Updates object of given type with given name from current site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="objectName">Code name of the object</param>
        /// <param name="stream">Object data</param>
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/{objectType}/currentsite/{objectName}")]
        [RESTSecurityInvoker]
        Stream UpdateCurrentSiteObject(string objectType, string objectName, Stream stream);


        /// <summary>
        /// Creates an object of given type specified by it's parameters.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="stream">Object data</param>
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{objectType}")]
        [RESTSecurityInvoker]
        Stream CreateObject(string objectType, Stream stream);


        /// <summary>
        /// Creates object of given type with given name assigns it to specified site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="stream">Object data</param>
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{objectType}/site/{siteName}")]
        [RESTSecurityInvoker]
        Stream CreateSiteObject(string objectType, string siteName, Stream stream);


        /// <summary>
        /// Creates object of given type with given name and assigns it to current site.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="stream">Object data</param>
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{objectType}/currentsite")]
        [RESTSecurityInvoker]
        Stream CreateCurrentSiteObject(string objectType, Stream stream);


        /// <summary>
        /// Creates global object of given type.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="stream">Object data</param>
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/{objectType}/global")]
        [RESTSecurityInvoker]
        Stream CreateGlobalObject(string objectType, Stream stream);

        #endregion


        #endregion


        #region "Service Methods"

        /// <summary>
        /// Exposes Service Document (available data).
        /// </summary>
        [OperationContract]
        [WebGet(UriTemplate = "")]
        [RESTSecurityInvoker]
        AtomPub10ServiceDocumentFormatter GetServiceDocument();


        /// <summary>
        /// Exposes the list of all sites.
        /// </summary>
        /// <param name="objectType">Object type</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/site")]
        [RESTSecurityInvoker]
        AtomPub10ServiceDocumentFormatter GetSiteList(string objectType);


        /// <summary>
        /// Returns list of child object types of given object.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/{id}/children")]
        [RESTSecurityInvoker]
        AtomPub10ServiceDocumentFormatter GetChildObjectTypes(string objectType, string id);


        /// <summary>
        /// Returns list of binding object types of given object.
        /// </summary>
        /// <param name="objectType">Object type of the object</param>
        /// <param name="id">ID of the object</param>
        [OperationContract]
        [WebGet(UriTemplate = "/{objectType}/{id}/bindings")]
        [RESTSecurityInvoker]
        AtomPub10ServiceDocumentFormatter GetBindingsObjectTypes(string objectType, string id);

        #endregion


        #region "Macro methods"

        /// <summary>
        /// Returns the result of the macro expression.
        /// </summary>
        /// <param name="expression">Data macro expression to evaluate</param>
        [WebGet(UriTemplate = "/macro/{expression}")]
        [OperationContract]
        [RESTSecurityInvoker]
        Stream GetMacroResult(string expression);

        #endregion


        #region "Security methods"

        /// <summary>
        /// Returns hash string which can be than used for given URL instead of authentication.
        /// </summary>
        /// <param name="url">URL of which to get the hash</param>
        [OperationContract]
        [WebGet(UriTemplate = "/getauthhash?url={url}")]
        [RESTSecurityInvoker]
        Stream GetHashForURL(string url);


        /// <summary>
        /// Validates user name and password and sets security token which will protect application against CSRF. 
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="password">Password.</param>
        /// <param name="token">Token.</param>
        [OperationContract]
        [WebInvoke(Method="POST", UriTemplate = "/settoken?username={userName}&password={password}&token={token}")]
        [RESTSecurityInvoker]
        Stream SetToken(string userName, string password, string token);

        #endregion
    }
}