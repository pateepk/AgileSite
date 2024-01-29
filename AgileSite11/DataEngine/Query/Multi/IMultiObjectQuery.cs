using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Multi object query interface for a specific query
    /// </summary>
    public interface IMultiObjectQuery<TQuery, TInnerQuery, TObject> : 
        IMultiQuery<TQuery, TInnerQuery>, 
        IObjectQuery<TQuery, TObject>,
        IMultiObjectQuery
        where TObject : BaseInfo
    {
    }


    /// <summary>
    /// Multi object query interface
    /// </summary>
    public interface IMultiObjectQuery : 
        IMultiQuery,
        IObjectQuery
    {
    }
}