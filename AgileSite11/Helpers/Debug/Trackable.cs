using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Defines a base class for the classes that can be tracked
    /// </summary>
    public class Trackable<T> : DisposableObject
        where T : Trackable<T>
    {
        #region "Variables"

        /// <summary>
        /// Type name
        /// </summary>
        protected static string TypeName = typeof (T).Name;


        /// <summary>
        /// If true, the tracking of this type is enabled. Default false, configurable with web.config key CMSTrack[TypeName].
        /// </summary>
        public static BoolAppSetting Track = new BoolAppSetting("CMSTrack" + TypeName) { MasterKeyName = "CMSTrackAll" };

        #endregion


        #region "Methods"

        /// <summary>
        /// Static constructor
        /// </summary>
        static Trackable()
        {
            TypeManager.RegisterGenericType(typeof(Trackable<T>));
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public Trackable()
        {
            if (Track)
            {
                Using(new TrackedSection(TypeName));
            }
        }


        /// <summary>
        /// Tracks all open trackable objects of this type. Only tracks the section if the tracking of current object is enabled. 
        /// </summary>
        /// <param name="reason">Track reason</param>
        public static void TrackAllOpen(string reason)
        {
            if (Track)
            {
                TrackedSection.TrackOpenSections(TypeName, reason);
            }
        }

        #endregion
    }
}
