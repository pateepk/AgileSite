namespace CMS.DataEngine
{
    /// <summary>
    /// Object event arguments
    /// </summary>
    public class ObjectChangeOrderEventArgs : ObjectChangeOrderEventArgs<BaseInfo>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectChangeOrderEventArgs()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="infoObject">Object instance</param>
        /// <param name="newOrder">Specific new document order index to be set</param>
        /// <param name="relativeOrder">Indicates if the NewOrder index is relative to current document position</param>
        /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
        public ObjectChangeOrderEventArgs(BaseInfo infoObject, int newOrder, bool relativeOrder, string orderColumn)
            : base(infoObject, newOrder, relativeOrder, orderColumn)
        {
        }
    }


    /// <summary>
    /// Object event arguments
    /// </summary>
    public class ObjectChangeOrderEventArgs<TObject> : ObjectEventArgs<TObject>
    {
        /// <summary>
        /// Specific new document order index to be set.
        /// </summary>
        public int NewOrder
        {
            get;
            protected set;
        }


        /// <summary>
        /// Indicates if the NewOrder index is relative to current document position.
        /// </summary>
        public bool RelativeOrder
        {
            get;
            protected set;
        }


        /// <summary>
        /// Name of the order column. If null, OrderColumn from TypeInfo is taken.
        /// </summary>
        public string OrderColumn
        {
            get;
            protected set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectChangeOrderEventArgs()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="infoObject">Object instance</param>
        /// <param name="newOrder">Specific new document order index to be set</param>
        /// <param name="relativeOrder">Indicates if the NewOrder index is relative to current document position</param>
        /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
        public ObjectChangeOrderEventArgs(TObject infoObject, int newOrder, bool relativeOrder, string orderColumn)
        {
            Object = infoObject;
            NewOrder = newOrder;
            RelativeOrder = relativeOrder;
            OrderColumn = orderColumn;
        }
    }
}