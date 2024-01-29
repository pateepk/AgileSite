using System.Collections.Generic;

using CMS.Base;


namespace CMS.PortalEngine
{
    /// <summary>
    /// Argument class for events raised for load variants
    /// </summary>
    public class WebPartLoadVariantsArgs : CMSEventArgs
    {
        /// <summary>
        /// Indicates whether load should be raised at all cases
        /// </summary>
        public bool ForceLoad
        {
            get;
            internal set;
        }


        /// <summary>
        /// Variant mode
        /// </summary>
        public VariantModeEnum VariantMode
        {
            get;
            internal set;
        }


        /// <summary>
        /// Instance for variants' web part
        /// </summary>
        public WebPartInstance WebPartInstance
        {
            get;
            internal set;
        }


        /// <summary>
        /// Instance for variants' zone
        /// </summary>
        public WebPartZoneInstance WebPartZoneInstance
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the document ID if the instance holds a widget
        /// </summary>
        public int DocumentID
        {
            get;
            internal set;
        }
    }


    /// <summary>
    /// Argument class for events raised after web part movement
    /// </summary>
    public class MoveWebPartsArgs : CMSEventArgs
    {
        /// <summary>
        /// Source zone instance
        /// </summary>
        public WebPartZoneInstance Zone
        {
            get;
            set;
        }


        /// <summary>
        /// Target zone instance
        /// </summary>
        public WebPartZoneInstance TargetZone
        {
            get;
            set;
        }


        /// <summary>
        /// Template info object
        /// </summary>
        public PageTemplateInfo TemplateInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Instance of copied web part
        /// </summary>
        public WebPartInstance WebPartInstance
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Argument class for events raised after web part is removed
    /// </summary>
    public class RemoveWebPartsArgs : CMSEventArgs
    {
        /// <summary>
        /// Removed web part instance
        /// </summary>
        public WebPartInstance WebPartInstance
        {
            get;
            set;
        }


        /// <summary>
        /// Source zone instance
        /// </summary>
        public WebPartZoneInstance Zone
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Argument class for events raised for layout zone id change.
    /// </summary>
    public class ChangeLayoutZoneIdArgs : CMSEventArgs
    {
        /// <summary>
        /// Gets or sets the old layout zone id.
        /// </summary>
        public string OldZoneId
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the new layout zone id.
        /// </summary>
        public string NewZoneId
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the page template id where the layout zone is located.
        /// </summary>
        public int PageTemplateId
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the layout zone web parts.
        /// </summary>
        public List<WebPartInstance> ZoneWebParts
        {
            get;
            set;
        }
    }
}