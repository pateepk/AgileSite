using System;

using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Multi document query interface for a specific query
    /// </summary>
    public interface IMultiDocumentQuery<TQuery, TInnerQuery, TObject> : 
        IMultiObjectQuery<TQuery, TInnerQuery, TObject>, 
        IDocumentQuery<TQuery, TObject>, 
        IMultiDocumentQuery
        where TObject : TreeNode, new()
    {
    }


    /// <summary>
    /// Multi document query interface
    /// </summary>
    public interface IMultiDocumentQuery : 
        IMultiObjectQuery,
        IDocumentQuery
    {
    }
}
