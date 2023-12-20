namespace CMS.DataEngine
{
    /// <summary>
    /// Object event arguments
    /// </summary>
    public class ObjectSortEventArgs : ObjectSortEventArgs<BaseInfo>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectSortEventArgs()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="infoObject">Object instance</param>
        /// <param name="ascending">Indicates if the sort should be ascending</param>
        /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
        /// <param name="nameColumn">Column by the content of which the alphabetical order will be set</param>
        public ObjectSortEventArgs(BaseInfo infoObject, bool ascending, string orderColumn, string nameColumn)
            : base(infoObject, ascending, orderColumn, nameColumn)
        {
        }
    }


    /// <summary>
    /// Object event arguments
    /// </summary>
    public class ObjectSortEventArgs<TObject> : ObjectEventArgs<TObject>
    {
        /// <summary>
        /// Name of the order column. If null, OrderColumn from TypeInfo is taken.
        /// </summary>
        public string OrderColumn
        {
            get;
            protected set;
        }


        /// <summary>
        /// Column by the content of which the alphabetical order will be set.
        /// </summary>
        public string NameColumn
        {
            get;
            protected set;
        }
        
        
        /// <summary>
        /// Indicates if the sort should be ascending.
        /// </summary>
        public bool Ascending
        {
            get;
            protected set;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectSortEventArgs()
        {
        }
        

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="infoObject">Object instance</param>
        /// <param name="ascending">Indicates if the sort should be ascending</param>
        /// <param name="orderColumn">Name of the order column. If null, OrderColumn from TypeInfo is taken</param>
        /// <param name="nameColumn">Column by the content of which the alphabetical order will be set</param>
        public ObjectSortEventArgs(TObject infoObject, bool ascending, string orderColumn, string nameColumn)
        {
            Object = infoObject;
            Ascending = ascending;
            OrderColumn = orderColumn;
            NameColumn = nameColumn;
        }
    }
}