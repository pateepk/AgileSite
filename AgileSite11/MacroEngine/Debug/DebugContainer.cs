using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Container for debugging object within macro engine. Reports all variables and properties
    /// </summary>
    public class DebugContainer : AbstractHierarchicalObject<DebugContainer>, IMacroObject, IEnumerable
    {
        #region "Properties"

        /// <summary>
        /// Debugged object
        /// </summary>
        public object Object
        {
            get;
            protected set;
        }


        /// <summary>
        /// If true, only public members are exposed
        /// </summary>
        public bool OnlyPublicMembers
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">Object to debug</param>
        public DebugContainer(object obj)
        {
            Object = obj;
            UseLocalProperties = true;
            UseLocalColumns = true;
        }


        /// <summary>
        /// Register the object columns
        /// </summary>
        protected override void RegisterColumns()
        {
            base.RegisterColumns();

            var type = Object.GetType();

            var flags = GetBindingFlags();
            var fields = type.GetFields(flags);

            // Register all fields
            foreach (var fieldInfo in fields)
            {
                var info = fieldInfo;
                if (fieldInfo.Name.IndexOfCSafe("k__BackingField") < 0)
                {
                    RegisterColumn(fieldInfo.Name, container =>
                        {
                            try
                            {
                                return Wrap(info.GetValue(container.Object), OnlyPublicMembers);
                            }
                            catch (Exception ex)
                            {
                                return ex.Message;
                            }
                        });
                }
            }
        }


        /// <summary>
        /// Gets the binding flags for getting the members
        /// </summary>
        private BindingFlags GetBindingFlags()
        {
            var flags = BindingFlags.Instance | BindingFlags.Public;

            if (!OnlyPublicMembers)
            {
                flags |= BindingFlags.NonPublic;
            }
            return flags;
        }


        /// <summary>
        /// Register the object columns
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            var type = Object.GetType();

            var flags = GetBindingFlags();
            var properties = type.GetProperties(flags);

            foreach (var propertyInfo in properties)
            {
                var info = propertyInfo;

                // Only non-indexers
                var parameters = info.GetIndexParameters();
                if (parameters.Length == 0)
                {
                    RegisterProperty(propertyInfo.Name, container =>
                    {
                        try
                        {
                            return Wrap(info.GetValue(container.Object, null), OnlyPublicMembers);
                        }
                        catch (Exception ex)
                        {
                            return ex.Message;
                        }

                    });
                }
            }
        }


        /// <summary>
        /// Returns the string representation of the given object
        /// </summary>
        public override string ToString()
        {
            return Object.ToString();
        }


        /// <summary>
        /// Gets the object enumerator
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            var enumObj = Object as IEnumerable;
            if (enumObj != null)
            {
                foreach (var item in enumObj)
                {
                    yield return Wrap(item, OnlyPublicMembers);
                }
            }
        }


        /// <summary>
        /// Wraps the object with debug container. Returns null if the object is null
        /// </summary>
        /// <param name="value">Value to wrap</param>
        /// <param name="onlyPublic">If true, only public members are exposed</param>
        public static object Wrap(object value, bool onlyPublic = false)
        {
            // No value
            if (value == null)
            {
                return null;
            }

            // Keep primitive types and string as their value
            if (value.GetType().IsPrimitive || (value is string))
            {
                return value;
            }

            // Wrap class into a container
            return new DebugContainer(value)
            {
                OnlyPublicMembers = onlyPublic
            };
        }


        /// <summary>
        /// Returns the default text representation in the macros (this is called when the expression is resolved to its final value and should be converted to string).
        /// </summary>
        public string ToMacroString()
        {
            return ToString();
        }


        /// <summary>
        /// Returns the object which represents current object in the macro engine. 
        /// Whenever the object implementing IMacroObject interface is used within macro engine this method is called its result is used instead.
        /// </summary>
        public object MacroRepresentation()
        {
            return this;
        }

        #endregion
    }
}
