using CMS;
using CMS.DataEngine;
using CMS.Relationships;

[assembly: RegisterModule(typeof(RelationshipsModule))]

namespace CMS.Relationships
{
    /// <summary>
    /// Represents the Relationships module.
    /// </summary>
    public class RelationshipsModule : Module
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public RelationshipsModule()
            : base(new RelationshipsModuleMetadata())
        {
        }
    }
}