using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.FormEngine
{
    /// <summary>
    /// Contains utility methods related to <see cref="FormInfo"/> class.
    /// </summary>
    public class FormInfoHelper : AbstractHelper<FormInfoHelper>
    {
        /// <summary>
        /// Gets unique name for <paramref name="name"/> within the specified <paramref name="formInfo"/> by appending
        /// a suffix containing ordinal number.
        /// </summary>
        /// <param name="formInfo">Form info for which the name is to be unique.</param>
        /// <param name="name">Name from which to create a unique name.</param>
        /// <returns>Returns name derived from <paramref name="name"/> which is unique within the form.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formInfo"/> or <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is an empty string.</exception>
        public static string GetUniqueFieldName(FormInfo formInfo, string name)
        {
            return HelperObject.GetUniqueFieldNameInternal(formInfo, name);
        }


        /// <summary>
        /// Gets unique name for <paramref name="name"/> within the specified <paramref name="existingFieldNames"/> by appending
        /// a suffix containing ordinal number.
        /// </summary>
        /// <param name="existingFieldNames">Set of existing field names in for which the name is to be unique.</param>
        /// <param name="name">Name from which to create a unique name.</param>
        /// <returns>Returns name derived from <paramref name="name"/> which is unique within the specified names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="existingFieldNames"/> or <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is an empty string.</exception>
        public static string GetUniqueFieldName(ISet<string> existingFieldNames, string name)
        {
            return HelperObject.GetUniqueFieldNameInternal(existingFieldNames, name);
        }


        /// <summary>
        /// Gets unique name for <paramref name="name"/> within the specified <paramref name="formInfo"/> by appending
        /// a suffix containing ordinal number.
        /// </summary>
        /// <param name="formInfo">Form info for which the name is to be unique.</param>
        /// <param name="name">Name from which to create a unique name.</param>
        /// <returns>Returns name derived from <paramref name="name"/> which is unique within the form.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="formInfo"/> or <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is an empty string.</exception>
        protected virtual string GetUniqueFieldNameInternal(FormInfo formInfo, string name)
        {
            if (formInfo == null)
            {
                throw new ArgumentNullException(nameof(formInfo));
            }

            HashSet<string> existingFieldNames = new HashSet<string>(formInfo.GetFields<FormFieldInfo>().Select(ffi => ffi.Name), StringComparer.InvariantCultureIgnoreCase);

            return GetUniqueFieldNameInternal(existingFieldNames, name);
        }


        /// <summary>
        /// Gets unique name for <paramref name="name"/> within the specified <paramref name="existingFieldNames"/> by appending
        /// a suffix containing ordinal number.
        /// </summary>
        /// <param name="existingFieldNames">Set of existing field names in for which the name is to be unique.</param>
        /// <param name="name">Name from which to create a unique name.</param>
        /// <returns>Returns name derived from <paramref name="name"/> which is unique within the specified names.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="existingFieldNames"/> or <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is an empty string.</exception>
        protected virtual string GetUniqueFieldNameInternal(ISet<string> existingFieldNames, string name)
        {
            if (existingFieldNames == null)
            {
                throw new ArgumentNullException(nameof(existingFieldNames));
            }
            if (String.IsNullOrEmpty(name))
            {
                throw name == null ? new ArgumentNullException(nameof(name)) : new ArgumentException("Name must be specified.", nameof(name));
            }

            int uniqueIndex = 1;
            bool unique = false;
            string uniqueName = name;

            while (!unique)
            {
                if (!existingFieldNames.Contains(uniqueName))
                {
                    unique = true;
                }
                else
                {
                    uniqueName = name + "_" + uniqueIndex;
                    uniqueIndex++;
                }
            }

            return uniqueName;
        }
    }
}
