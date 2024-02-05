using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object event arguments
    /// </summary>
    public class ObjectEventArgs : ObjectEventArgs<BaseInfo>
    {
    }


    /// <summary>
    /// Object event arguments
    /// </summary>
    public class ObjectEventArgs<TObject> : CMSEventArgs     
    {
        /// <summary>
        /// Processed object
        /// </summary>
        public TObject Object
        {
            get;
            set;
        }


        /// <summary>
        /// Object content. Used when it makes sense: Search indexing
        /// </summary>
        public string Content
        {
            get;
            set;
        }
    }
}