using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provider returns query based on object class name
    /// </summary>
    internal sealed class ClassNameExecutingQueryProvider : IExecutingQueryProvider
    {
        private string mObjectClassName;


        /// <summary>
        /// Creates a new instance of <see cref="ClassNameExecutingQueryProvider"/>
        /// </summary>
        internal ClassNameExecutingQueryProvider(ObjectTypeInfo typeInfo)
        {
            Refresh(typeInfo);
        }


        /// <summary>
        /// Returns executing query as <see cref="IDataQuery"/>
        /// </summary>
        public IDataQuery GetExecutionQuery()
        {
            return new DataQuery(mObjectClassName, null);
        }


        /// <summary>
        /// Refreshes the underlying data affecting the query retrieval
        /// </summary>
        /// <param name="typeInfo">Type info holding all necessary data</param>
        public void Refresh(ObjectTypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException("typeInfo");
            }

            mObjectClassName = typeInfo.ObjectClassName;
        }
    }
}