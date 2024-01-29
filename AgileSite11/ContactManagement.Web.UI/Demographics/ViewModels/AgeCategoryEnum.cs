using CMS.Helpers;

namespace CMS.ContactManagement.Web.UI.Internal
{
    /// <summary>
    /// Represents age category used in <see cref="ContactsGroupedByAgeViewModel"/>.
    /// </summary>
    public enum AgeCategoryEnum
    {
        /// <summary>
        /// Represents contact category with unknown age.
        /// </summary>
        [EnumStringRepresentation("om.contact.demographics.graphicalrepresentation.age.unknown")]
        Unknown,

        /// <summary>
        /// Represents contact category with age between 18 and 24.
        /// </summary>
        [EnumStringRepresentation("om.contact.demographics.graphicalrepresentation.age.18to24")]
        _18_24,


        /// <summary>
        /// Represents contact category with age between 25 and 34.
        /// </summary>
        [EnumStringRepresentation("om.contact.demographics.graphicalrepresentation.age.25to34")]
        _25_34,


        /// <summary>
        /// Represents contact category with age between 35 and 44.
        /// </summary>
        [EnumStringRepresentation("om.contact.demographics.graphicalrepresentation.age.35to44")]
        _35_44,


        /// <summary>
        /// Represents contact category with age between 45 and 54.
        /// </summary>
        [EnumStringRepresentation("om.contact.demographics.graphicalrepresentation.age.45to54")]
        _45_54,


        /// <summary>
        /// Represents contact category with age between 55 and 64.
        /// </summary>
        [EnumStringRepresentation("om.contact.demographics.graphicalrepresentation.age.55to64")]
        _55_64,


        /// <summary>
        /// Represents contact category with age greater or equal to 65.
        /// </summary>
        [EnumStringRepresentation("om.contact.demographics.graphicalrepresentation.age.65plus")]
        _65plus
    }
}