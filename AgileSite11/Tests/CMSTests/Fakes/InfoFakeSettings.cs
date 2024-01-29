using System;

namespace CMS.Tests
{
    /// <summary>
    /// Info fake settings
    /// </summary>
    public class InfoFakeSettings
    {
        private bool mIncludeInheritedFields = true;


        /// <summary>
        /// Object type to fake. If not provided, all object types of the given instance type are faked.
        /// </summary>
        public string ObjectType
        {
            get;
            set;
        }


        /// <summary>
        /// Instance type to fake
        /// </summary>
        public Type Type
        {
            get;
            set;
        }


        /// <summary>
        /// If true, columns from parent type are also retrieved. Default true.
        /// </summary>
        public bool IncludeInheritedFields
        {
            get
            {
                return mIncludeInheritedFields;
            }
            set
            {
                mIncludeInheritedFields = value;
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">Type which should be faked</param>
        public InfoFakeSettings(Type type)
        {
            Type = type;
        }
    }
}