namespace CMS.DataEngine
{
    /// <summary>
    /// Search field constants
    /// </summary>
    public class SearchFieldsConstants
    {
        /// <summary>
        /// Name of content field in iDocument.
        /// </summary>
        public const string CONTENT = "_content";


        /// <summary>
        /// Name of id field in iDocument.
        /// </summary>
        public const string ID = "_id";


        /// <summary>
        /// Name of ID (DataClass) column in iDocument
        /// </summary>
        public const string IDCOLUMNNAME = "_idcolumnname";


        /// <summary>
        /// Name of type field in iDocument.
        /// </summary>
        public const string TYPE = "_type";


        /// <summary>
        /// Name of index field in iDocument.
        /// </summary>
        public const string INDEX = "_index";


        /// <summary>
        /// Name of the site field in iDocument.
        /// </summary>
        public const string SITE = "_site";


        /// <summary>
        /// Name of the created field in iDocument.
        /// </summary>
        public const string CREATED = "_created";


        /// <summary>
        /// Name of the culture field in iDocument.
        /// </summary>
        public const string CULTURE = "_culture";


        /// <summary>
        /// Special field for partial index rebuild. This field contains site name and node alias path of a document separated by semicolon (used for document move for example).
        /// </summary>
        public const string PARTIAL_REBUILD = "_movedfield";


        /// <summary>
        /// Custom title field.
        /// </summary>
        public const string CUSTOM_TITLE = "_customtitle";


        /// <summary>
        /// Custom content field.
        /// </summary>
        public const string CUSTOM_CONTENT = "_customcontent";


        /// <summary>
        /// Custom date field.
        /// </summary>
        public const string CUSTOM_DATE = "_customdate";


        /// <summary>
        /// Custom URL field.
        /// </summary>
        public const string CUSTOM_URL = "_customurl";


        /// <summary>
        /// Custom image URL.
        /// </summary>
        public const string CUSTOM_IMAGEURL = "_customimage";


        /// <summary>
        /// Score field
        /// </summary>
        public const string SCORE = "_score";


        /// <summary>
        /// Position field
        /// </summary>
        public const string POSITION = "_position";
    }
}
