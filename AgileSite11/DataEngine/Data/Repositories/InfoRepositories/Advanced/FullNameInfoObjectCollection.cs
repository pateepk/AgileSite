using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Collection of GeneralizedInfos indexed by full name of the object
    /// </summary>
    public class FullNameInfoObjectCollection<TInfo> : InfoObjectCollection<TInfo>
        where TInfo : BaseInfo
    {
        private Func<TInfo, string> NameTransformation
        {
            get;
        }


        /// <summary>
        /// Creates new instance of <see cref="FullNameInfoObjectCollection{TInfo}"/>.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="nameTransformation">Transformation function for object to name translation</param>
        /// <exception cref="ArgumentNullException"><paramref name="nameTransformation"/></exception>
        public FullNameInfoObjectCollection(string objectType, Func<TInfo, string> nameTransformation)
            : base(objectType)
        {
            if (nameTransformation == null)
            {
                throw new ArgumentNullException(nameof(nameTransformation));
            }

            NameTransformation = nameTransformation;
            AutomaticNameColumn = false;
        }


        /// <summary>
        /// Gets the unique object name from the given object.
        /// </summary>
        /// <param name="infoObj">Object</param>
        public override string GetObjectName(TInfo infoObj)
        {
            return NameTransformation(infoObj);
        }


        /// <summary>
        /// Gets the where condition for the given object name.
        /// </summary>
        /// <param name="name">Object name</param>
        public override IWhereCondition GetNameWhereCondition(string name)
        {
            var provider = TypeInfo?.ProviderObject as IFullNameInfoProvider;
            var whereCondition = provider?.GetFullNameWhereCondition(name);

            return !string.IsNullOrEmpty(whereCondition) 
                        ? new WhereCondition(whereCondition) 
                        : new WhereCondition(SqlHelper.NO_DATA_WHERE);
        }


        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        public override IInfoObjectCollection<TInfo> Clone()
        {
            // Create new instance and copy over the properties
            var result = new FullNameInfoObjectCollection<TInfo>(ObjectType, NameTransformation);
            CopyPropertiesTo(result);

            return result;
        }
    }
}