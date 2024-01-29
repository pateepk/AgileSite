using System;

using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document query parameters interface for a specific query
    /// </summary>
    public interface IDocumentQuery<TQuery, TObject> : 
        IObjectQuery<TQuery, TObject>, 
        IDocumentQuery
        where TObject : TreeNode, new()
    {
        #region "General methods"

        /// <summary>
        /// Ensures that there is no restriction applied for the result.
        /// </summary>
        TQuery All();

        /// <summary>
        /// Ensures default restrictions for the result.
        /// </summary>
        TQuery Default();

        /// <summary>
        /// Ensures that latest version of the documents is retrieved.
        /// </summary>
        /// <param name="latest">If true, the latest (edited) version is retrieved, otherwise published version is retrieved</param>
        TQuery LatestVersion(bool latest = true);

        /// <summary>
        /// Ensures that published version of the documents is retrieved.
        /// </summary>
        /// <param name="published">If true, published version is retrieved, otherwise latest (edited) version is retrieved</param>
        TQuery PublishedVersion(bool published = true);

        /// <summary>
        /// Ensures that only documents published on a live site are retrieved.
        /// </summary>
        /// <param name="published">If true, only published documents are retrieved, otherwise all documents are retrieved</param>
        TQuery Published(bool published = true);

        /// <summary>
        /// Ensures that the result will be filtered based on user Read permission.
        /// </summary>
        /// <param name="check">If true, the permission check is enabled, otherwise disabled</param>
        TQuery CheckPermissions(bool check = true);

        /// <summary>
        /// Ensures that duplicate document are filtered from the result. This means that linked documents are not retrieved, if there is the original document already included in the results.
        /// </summary>
        /// <param name="filter">If true, the permission check is enabled, otherwise disabled</param>
        TQuery FilterDuplicates(bool filter = true);

        #endregion
      

        #region "Relationship methods"

        /// <summary>
        /// Ensures that only documents in relationship with a specified document are retrieved.
        /// </summary>
        /// <param name="nodeGuid">Node GUID of the related document</param>
        /// <param name="relationshipName">Name of the relationship. If not provided documents from all relationships will be retrieved.</param>
        /// <param name="side">Side of the related document within the relation</param>
        TQuery InRelationWith(Guid nodeGuid, string relationshipName = null, RelationshipSideEnum side = RelationshipSideEnum.Both);

        #endregion


        #region "Nesting level methods"

        /// <summary>
        /// Ensures that only documents within specified nesting level are retrieved.
        /// </summary>
        /// <param name="level">Nesting level</param>
        TQuery NestingLevel(int level);

        #endregion


        #region "Site methods"

        /// <summary>
        /// Filters the data to include only records on the current site.
        /// </summary>
        TQuery OnCurrentSite();

        #endregion


        #region "Path methods"

        /// <summary>
        /// Filters the data to include only documents on given path(s).
        /// </summary>
        /// <param name="paths">List of document paths</param>
        TQuery Path(params string[] paths);

        /// <summary>
        /// Filters the data to include only documents on given path.
        /// </summary>
        /// <param name="path">Document path</param>
        /// <param name="type">Path type to define selection scope</param>
        TQuery Path(string path, PathTypeEnum type = PathTypeEnum.Explicit);

        /// <summary>
        /// Filters the data to exclude documents on given path(s).
        /// </summary>
        /// <param name="paths">List of document paths</param>
        TQuery ExcludePath(params string[] paths);
        
        /// <summary>
        /// Filters the data to exclude documents on given path.
        /// </summary>
        /// <param name="path">Document path</param>
        /// <param name="type">Path type to define excluded scope</param>
        TQuery ExcludePath(string path, PathTypeEnum type = PathTypeEnum.Explicit);

        #endregion


        #region "Culture methods"

        /// <summary>
        /// Filters the data to include only documents translated to given culture(s).
        /// </summary>
        /// <param name="cultures">List of document cultures</param>
        TQuery Culture(params string[] cultures);
        
        /// <summary>
        /// The data will be combined with documents in site default culture if not translated to the requested one. This behavior can be used only when a single culture of documents is requested.
        /// </summary>
        /// <param name="combine">If true, documents will be combined with default culture, otherwise only documents translated to requested culture are retrieved</param>
        TQuery CombineWithDefaultCulture(bool combine = true);
        
        /// <summary>
        /// Ensures that all culture versions of the documents are retrieved.
        /// </summary>
        /// <param name="all">If true, all culture versions are retrieved, otherwise documents of specified culture(s) are retrieved</param>
        TQuery AllCultures(bool all = true);

        #endregion
    }


    /// <summary>
    /// Document query parameters interface
    /// </summary>
    public interface IDocumentQuery : IObjectQuery
    {
        /// <summary>
        /// Document query parameters.
        /// </summary>
        DocumentQueryProperties Properties
        {
            get;
            set;
        }
    }
}
