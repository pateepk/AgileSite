using System.ComponentModel;

using Newtonsoft.Json;

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// See <see cref="ContactImportResults"/> for detailed description.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class ResultModel
    {
        /// <summary>
        /// Number of contacts from input batch which already existed in DB and were updated or were in the input batch more than once.
        /// </summary>
        [JsonProperty("duplicities")]
        public int Duplicities
        {
            get;
            set;
        }


        /// <summary>
        /// Number of contacts from input batch which were not inserted or were not updated if they already existed in DB.
        /// </summary>
        [JsonProperty("failures")]
        public int Failures
        {
            get;
            set;
        }


        /// <summary>
        /// Number of contacts which were created.
        /// </summary>
        [JsonProperty("imported")]
        public int Imported
        {
            get;
            set;
        }


        /// <summary>
        /// Information about contact which were not imported
        /// </summary>
        [JsonProperty("notImportedContacts")]
        public NotImportedContactsResultModel NotImportedContacts
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public ResultModel()
        {
            NotImportedContacts = new NotImportedContactsResultModel();
        }


        /// <summary>
        /// Indicates whether the ResultModel equals to some other ResultModel
        /// </summary>
        /// <param name="obj">The ResultModel that will be used in the comparison</param>
        public override bool Equals(object obj)
        {
            var that = obj as ResultModel;

            if (that == null)
            {
                return false;
            }

            return (Duplicities == that.Duplicities) && (Failures == that.Failures) && (Imported == that.Imported);
        }


        /// <summary>
        /// Empty implementation override of the default hash function.
        /// It is here to avoid compile time warning. Equals() is used in tests only.
        /// </summary>
        public override int GetHashCode()
        {
            return 0;
        }
    }
}