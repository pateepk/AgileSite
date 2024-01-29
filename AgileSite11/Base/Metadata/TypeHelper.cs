using System;
using System.Collections;
using System.Collections.Generic;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Type helper.
    /// </summary>
    public static class TypeHelper
    {
        #region "Variables"

        /// <summary>
        /// Maximal length of the node document name.
        /// </summary>
        public const int MAX_NAME_LENGTH = 100;


        private static int? mMaxNameLength = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Maximal length of the node document name.
        /// </summary>
        public static int MaxCodeNameLength
        {
            get
            {
                if (mMaxNameLength == null)
                {
                    mMaxNameLength = CoreServices.Conversion.GetInteger(CoreServices.AppSettings["CMSMaxCodeNameLength"], MAX_NAME_LENGTH);
                }

                return mMaxNameLength.Value;
            }
            set
            {
                mMaxNameLength = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the nice object type name for this type
        /// </summary>
        /// <param name="objectType">Type to localize</param>
        /// <param name="culture">Culture code</param>
        public static string GetNiceObjectTypeName(string objectType, string culture = null)
        {
            if (String.IsNullOrEmpty(objectType))
            {
                return null;
            }

            return CoreServices.Localization.GetString(GetObjectTypeResourceKey(objectType), culture);
        }


        /// <summary>
        /// Ensures maximal allowed code name length.
        /// </summary>
        /// <param name="codeName">Original name</param>
        /// <param name="maxLength">Maximum code name length, if not set (0), cuts the code name to the general MaxCodeNameLength</param>
        public static string EnsureMaxCodeNameLength(string codeName, int maxLength)
        {
            int max = GetMaxCodeNameLength(maxLength);

            // Cut the code name to the correct length
            if (codeName.Length > max)
            {
                return codeName.Substring(0, max);
            }

            return codeName;
        }


        /// <summary>
        /// Gets the maximum code name length
        /// </summary>
        /// <param name="maxLength">Maximum code name length, if not set (0), returns MaxCodeNameLength</param>
        public static int GetMaxCodeNameLength(int maxLength)
        {
            // Get the maximum length
            int max = MaxCodeNameLength;
            if ((maxLength > 0) && (maxLength < max))
            {
                max = maxLength;
            }

            return max;
        }


        /// <summary>
        /// Gets nice name from the given object type
        /// </summary>
        /// <param name="name">Name to convert to nice name</param>
        public static string GetNiceName(string name)
        {
            // Get the last part after dot
            int dotIndex = name.LastIndexOfCSafe('.');
            if (dotIndex >= 0)
            {
                name = name.Substring(dotIndex + 1);
            }
            return name;
        }


        /// <summary>
        /// Gets the resource key for the name of the object type. Name of type should be in singular form.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static string GetObjectTypeResourceKey(string objectType)
        {
            return "ObjectType." + CoreServices.Conversion.GetIdentifier(objectType);
        }


        /// <summary>
        /// Gets the plural for the given name
        /// </summary>
        /// <param name="name">Name to convert</param>
        public static string GetPlural(string name)
        {
            if (name.EndsWithAny(StringComparison.InvariantCultureIgnoreCase, "list", "history"))
            {
                // Skip - Special case - List
            }
            else if (name.EndsWithCSafe("ss", true))
            {
                // Class -> Classes
                name += "es";
            }
            else if (name.EndsWithCSafe("s", true) && !name.EndsWithAny(StringComparison.InvariantCultureIgnoreCase, "as", "is", "os", "us"))
            {
                // Skip, already plural
            }
            else if (name.EndsWithCSafe("y", true))
            {
                if (name.EndsWithAny(StringComparison.InvariantCultureIgnoreCase, "ay", "ey", "iy", "oy", "uy"))
                {
                    // Boy -> Boys
                    name += "s";
                }
                else
                {
                    // Category -> Categories
                    name = name.Substring(0, name.Length - 1) + "ies";
                }
            }
            else if (name.EndsWithCSafe("o", true))
            {
                if (name.EndsWithAny(StringComparison.InvariantCultureIgnoreCase, "ao", "eo", "io", "oo", "uo"))
                {
                    // Boo -> Boos
                    name += "s";
                }
                else
                {
                    // Hero -> Heroes
                    name += "es";
                }
            }
            else if (name.EndsWithAny(StringComparison.InvariantCultureIgnoreCase, "s", "z", "x", "sz", "ch"))
            {
                // Status -> Statuses
                name += "es";
            }
            else if (name.EndsWithCSafe("fe", true))
            {
                // Life -> Lives
                name = name.Substring(0, name.Length - 2) + "ves";
            }
            else
            {
                // User -> Users
                name += "s";
            }

            return name;
        }

        #endregion


        #region "Type list methods"

        /// <summary>
        /// Returns a new list created from the given values
        /// </summary>
        /// <param name="names">Values</param>
        public static List<string> NewList(params ICollection[] names)
        {
            var result = new List<string>();

            if (names != null)
            {
                // Process all lists
                foreach (ICollection list in names)
                {
                    // Process all items
                    foreach (string item in list)
                    {
                        result.Add(item);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Resolves the object types.
        /// </summary>
        /// <param name="objectTypes">Object types constants separated by semicolon</param>
        public static List<string> GetTypes(string objectTypes)
        {
            List<string> result = new List<string>();

            if (objectTypes != null)
            {
                string[] types = objectTypes.Split(';');
                foreach (string type in types)
                {
                    if (type != "")
                    {
                        AddType(result, type);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Adds the specified types to the resulting array.
        /// </summary>
        /// <param name="list">Current types list</param>
        /// <param name="addList">Object types to add</param>
        public static void AddTypes(List<string> list, List<string> addList)
        {
            if (addList != null)
            {
                foreach (string type in addList)
                {
                    AddType(list, type);
                }
            }
        }


        /// <summary>
        /// Adds the type to the type list.
        /// </summary>
        /// <param name="list">Type list</param>
        /// <param name="type">Type to add</param>
        public static void AddType(List<string> list, string type)
        {
            if (type == null)
            {
                return;
            }

            type = type.Trim();
            if ((type != "") && !list.Contains(type))
            {
                list.Add(type);
            }
        }

        #endregion


        /// <summary>
        /// Gets the resource key for groups or categories of objects (usually plural), typically used in object trees to label nodes with children. 
        /// </summary>
        /// <remarks>
        /// Not all object types are guaranteed to have a translation for the resource string produced by this function.
        /// </remarks>
        /// <param name="objectType">Object type</param>
        public static string GetTasksResourceKey(string objectType)
        {
            return "ObjectTasks." + objectType.Replace(".", "_").Replace("#", "_");
        }
    }
}