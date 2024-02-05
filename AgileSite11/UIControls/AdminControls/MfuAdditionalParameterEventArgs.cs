using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.UIControls
{
    /// <summary>
    /// Special type of event arguments for when a set of parameters is being constructed.
    /// </summary>
    public class MfuAdditionalParameterEventArgs : EventArgs
    {

        private StringSafeDictionary<string> mParameters = new StringSafeDictionary<string>();

        /// <summary>
        /// Gets the additional parameters to be added to the collection.
        /// </summary>
        /// <returns>Array of parameterNames and values. Pair items are parameterNames, odd-ones are values</returns>
        public string[] GetParameters()
        {
            List<string> list = new List<string>();
            foreach (string key in mParameters.Keys)
            {
                list.Add(key);
                list.Add(mParameters[key]);
            }
            return list.ToArray();
        }


        /// <summary>
        /// Adds a parameter to the collection
        /// Fails if parameter is already in the collection
        /// </summary>
        /// <param name="parameterName">Key identifying the parameter</param>
        /// <param name="value">Vylue of teh parameter</param>
        /// <returns>True if adding was successful, false otherwise</returns>
        public bool AddParameter(string parameterName, string value)
        {
            if (!mParameters.ContainsKey(parameterName))
            {
                mParameters.Add(parameterName, value);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Adds multiple parameters to the collection.
        /// Ignores whole pair if one of the values is null.
        /// Ignores parameters already contained in the collection.
        /// </summary>
        /// <param name="parameters">Array of parameterNames and values. Pair items are parameterNames, odd-ones are values</param>
        public void AddParameters(string[] parameters)
        {
            int length = parameters.Length;
            for (int i = 1; i < length; i += 2)
            {
                string key = parameters[i - 1];
                string value = parameters[i];
                if ((key != null) && (value != null))
                {
                    AddParameter(key, value);
                }
            }
        }


        /// <summary>
        /// Indicates whether given parameter is already specified.
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <returns>True if already specified, false otherwise</returns>
        public bool HasParam(string parameterName)
        {
            return mParameters.ContainsKey(parameterName);
        }
    }
}
