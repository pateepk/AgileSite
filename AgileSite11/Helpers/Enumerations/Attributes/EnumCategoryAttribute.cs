using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.Helpers
{
    /// <summary>
    /// Allows to differentiate enum's members to categories.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class EnumCategoryAttribute : Attribute
    {
        private readonly string mCategory;


        /// <summary>
        /// Gets the category.
        /// </summary>
        public string Category
        {
            get
            {
                return mCategory;
            }
        }


        /// <summary>
        /// Specifies the category for an enum field.
        /// </summary>
        /// <param name="category">Category of the attributed enum field</param>
        public EnumCategoryAttribute(string category)
        {
            mCategory = category;
        }
    }
}
