using CMS.DataEngine;
using CMS.OnlineForms;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents properties of a <see cref="FileUploaderComponent"/>.
    /// </summary>
    public class FileUploaderProperties : FormComponentProperties<BizFormUploadFile>
    {
        /// <summary>
        /// Gets or sets the default value of the form component and underlying field.
        /// </summary>
        public override BizFormUploadFile DefaultValue
        {
            get;
            set;
        } = new BizFormUploadFile();


        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploaderProperties"/> class.
        /// </summary>
        /// /// <remarks>
        /// The constructor initializes the base class to data type <see cref="FieldDataType.Text"/> and size 500.
        /// </remarks>
        public FileUploaderProperties()
            : base(BizFormUploadFile.DATATYPE_FORMFILE)
        {
        }
    }
}
