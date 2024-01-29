using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// This attribute is used to represent a string value of an enum elements.
    /// 
    /// There can be only one StringValue attribute on an enum value.
    /// 
    /// It supports automatic resource string translation (IsResourceString property).
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class StringValueAttribute : Attribute
    {
        #region "Private fields"

        private string stringValue;

        #endregion


        #region "Properties"

        /// <summary>
        /// Holds the stringvalue for a value in an enum.
        /// </summary>
        public string StringValue
        {
            get
            {
                if (IsResourceString)
                {
                    return ResHelper.GetString(stringValue);
                }
                return stringValue;
            }
            protected set
            {
                stringValue = value;
            }
        }


        /// <summary>
        /// If set to true, StringValue is resolved by ResHelper before returning.
        /// </summary>
        public bool IsResourceString { get; set; }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value">String value</param>
        public StringValueAttribute(string value)
        {
            StringValue = value;
        }


        /// <summary>
        /// Constructor used to init a StringValue Attribute
        /// </summary>
        /// <param name="value">String value</param>
        /// <param name="isResourceString">True if string is resource string and should be resolved before returning</param>
        public StringValueAttribute(string value, bool isResourceString)
        {
            StringValue = value;
            IsResourceString = isResourceString;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Gets string representation (the same as StringValue).
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return StringValue;
        }

        #endregion
    }


    /// <summary>
    /// This attribute is used to represent a string value of an enum elements.
    /// 
    /// Unlike StringValueAttribute, this attribute can be used multiple times. Which attribute should be used will be specified by key.
    /// 
    /// Key can't be generic (C# limitation) nor Enum (Attribute params can not be dynamic). Thus it is int.
    /// </summary>
    /// <example>
    /// enum XXX
    /// {
    ///     [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription, "chat.system.cmsdesk.leaveroom", IsResourceString = true)]
    ///     [KeyStringValue((int)ChatMessageTypeStringValueUsageEnum.LiveSiteMessage, "chat.system.userhasleftroom")]
    ///     LeaveRoom = 2,
    /// }
    /// 
    /// XXX.LeaveRoom.ToStringValue((int)ChatMessageTypeStringValueUsageEnum.CMSDeskDescription);
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class KeyStringValueAttribute : StringValueAttribute
    {
        #region "Public properties"

        /// <summary>
        /// Gets or sets Key of this attribute. This key is used later to lookup this attribute in method ToStringValue().
        /// </summary>
        public int Key { get; set; }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">String value</param>
        public KeyStringValueAttribute(int key, string value) : base(value)
        {
            Key = key;
        }

        #endregion
    }


    /// <summary>
    /// Enum extension methods which works with StringValueAttribute and KeyStringValueAttribute.
    /// </summary>
    public static class EnumExtensionMethods
    {
        /// <summary>
        /// Gets the string value for a given enum element from the StringValue attribute. If passed
        /// enum element doesn't contain StringValue attribute, name of the enum element is returned.
        /// </summary>
        /// <param name="en">Enum</param>
        /// <returns>String value of an enum element</returns>
        public static string ToStringValue(this Enum en)
        {
            // Get the type
            Type type = en.GetType();

            // Get fieldinfo for this type
            FieldInfo fieldInfo = type.GetField(en.ToString());

            // Get the StringValue attributes
            StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];

            // Return the first if there was a match.
            if (attribs != null && attribs.Length > 0)
            {
                return attribs[0].ToString();
            }

            return en.ToString();
        }


        /// <summary>
        /// Gets the string value of an enum element from the first KeyStringValue with Key equals to <paramref name="key"/>.
        /// </summary>
        /// <param name="en">Enum</param>
        /// <param name="key">Key of KeyStringValue</param>
        /// <returns>String value of an enum</returns>
        public static string ToStringValue(this Enum en, int key)
        {
            // Get the type
            Type type = en.GetType();

            // Get fieldinfo for this type
            FieldInfo fieldInfo = type.GetField(en.ToString());

            // Get the stringvalue attributes
            KeyStringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(typeof(KeyStringValueAttribute), false) as KeyStringValueAttribute[];

            KeyStringValueAttribute foundAttrib = attribs.FirstOrDefault(att => att.Key == key);
            // Return the first if there was a match.
            if (foundAttrib != null)
            {
                return foundAttrib.ToString();
            }

            return en.ToString();
        }
    }
}
