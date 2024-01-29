using System.Collections.Generic;
using System.Linq;

namespace CMS.SalesForce
{

    /// <summary>
    /// Represents a model of the Sales force entity attribute.
    /// </summary>
    public abstract class EntityAttributeModel
    {

        #region "Protected members"

        /// <summary>
        /// The entity model that contains this attribute model.
        /// </summary>
        protected EntityModel mEntityModel;

        /// <summary>
        /// The SalesForce representation of this attribute model.
        /// </summary>
        protected WebServiceClient.Field mSource;

        /// <summary>
        /// The type of entity attribute values.
        /// </summary>
        protected EntityAttributeValueType mType;

        /// <summary>
        /// A list of picklist entries.
        /// </summary>
        protected PicklistEntry[] mPicklistEntries;

        #endregion

        #region "Public properties"

        /// <summary>
        /// Gets the entity model that contains this attribute model.
        /// </summary>
        public EntityModel EntityModel
        {
            get
            {
                return mEntityModel;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the attributes with this model contain auto-numbers.
        /// </summary>
        public bool IsAutoNumber
        {
            get
            {
                return mSource.autoNumber;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the attributes with this model contain external identifiers.
        /// </summary>
        public bool IsExternalId
        {
            get
            {
                return mSource.externalIdSpecified ? mSource.externalId : false;
            }
        }

        /// <summary>
        /// Gets the maximum length of the binary content of the attributes with this model.
        /// </summary>
        public int ByteLength
        {
            get
            {
                return mSource.byteLength;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the attributes with this model contain calculated values.
        /// </summary>
        public bool IsCalculated
        {
            get
            {
                return mSource.calculated;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the attributes with this model contain case-sensitive values.
        /// </summary>
        public bool IsCaseSensitive
        {
            get
            {
                return mSource.caseSensitive;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the value of attributes with this model can be set when the entity is being created.
        /// </summary>
        public bool IsCreatable
        {
            get
            {
                return mSource.createable;
            }
        }

        /// <summary>
        /// Gets the value indicating whether this attribute model is custom.
        /// </summary>
        public bool IsCustom
        {
            get
            {
                return mSource.custom;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the attributes with this model have default value.
        /// </summary>
        public bool HasDefaultValue
        {
            get
            {
                return mSource.defaultedOnCreate;
            }
        }

        /// <summary>
        /// Gets the maximum number of digits for the values of attributes with this model.
        /// </summary>
        public int IntegerPrecision
        {
            get
            {
                return mSource.digits;
            }
        }

        /// <summary>
        /// Gets the help text associated with this model.
        /// </summary>
        public string HelpText
        {
            get
            {
                return mSource.inlineHelpText;
            }
        }

        /// <summary>
        /// Gets the label associated with this model.
        /// </summary>
        public string Label
        {
            get
            {
                return mSource.label;
            }
        }

        /// <summary>
        /// Gets the maximum number of characters for the values of attributes with this model.
        /// </summary>
        public int Length
        {
            get
            {
                return mSource.length;
            }
        }

        /// <summary>
        /// Gets the name of this model.
        /// </summary>
        public string Name
        {
            get
            {
                return mSource.name;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the attributes with this model contain names.
        /// </summary>
        public bool IsNameField
        {
            get
            {
                return mSource.nameField;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the attributes with this model contain entity names.
        /// </summary>
        public bool IsParentName
        {
            get
            {
                return mSource.namePointingSpecified ? mSource.namePointing : false;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the attributes with this model can have null values.
        /// </summary>
        public bool IsNullable
        {
            get
            {
                return mSource.nillable;
            }
        }

        /// <summary>
        /// Gets the collection of picklist entries.
        /// </summary>
        public IEnumerable<PicklistEntry> PicklistEntries
        {
            get
            {
                if (mPicklistEntries == null)
                {
                    if (mSource.picklistValues != null)
                    {

                        mPicklistEntries = mSource.picklistValues.Select(x => new PicklistEntry(x)).ToArray();
                    }
                    else
                    {
                        mPicklistEntries = new PicklistEntry[0];
                    }
                }

                return mPicklistEntries.AsEnumerable();
            }
        }

        /// <summary>
        /// Gets the maximum number of digits for decimal values of attributes with this model.
        /// </summary>
        public int Precision
        {
            get
            {
                return mSource.precision;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the values of attributes with this model are restricted to the picklist entries.
        /// </summary>
        public bool IsRestrictedPicklist
        {
            get
            {
                return mSource.restrictedPicklist;
            }
        }

        /// <summary>
        /// Gets the maximum number of decimal places for decimal values of attributes with this model.
        /// </summary>
        public int Scale
        {
            get
            {
                return mSource.scale;
            }
        }

        /// <summary>
        /// Gets the value type of the attributes with this model.
        /// </summary>
        public EntityAttributeValueType Type
        {
            get
            {
                return mType;
            }
        }

        /// <summary>
        /// Gets the value indicating whether the value of attributes with this model can be set when the entity is being updated.
        /// </summary>
        public bool IsUpdateable
        {
            get
            {
                return mSource.updateable;
            }
        }

        #endregion

        #region "Constructors"

        internal EntityAttributeModel(EntityModel entityModel, WebServiceClient.Field source, EntityAttributeValueType type)
        {
            mEntityModel = entityModel;
            mSource = source;
            mType = type;
        }

        #endregion

        #region "Methods"

        internal abstract object ConvertValue(object value);
        internal abstract string SerializeValue(object value);
        internal abstract object DeserializeValue(string value);

        #endregion

    }

}