namespace CMS.Base
{
    /// <summary>
    /// Container containing current request settings.
    /// </summary>
    public class RequestSettings
    {
        #region "Variables"

        /// <summary>
        /// Flags for enabled status of particular debugs
        /// </summary>
        private bool[] mFlags = new bool[DebugHelper.RegisteredDebugsCount];

        #endregion


        #region "Properties"

        /// <summary>
        /// Flags for enabled status of particular debugs
        /// </summary>
        private bool[] Flags
        {
            get
            {
                return mFlags;
            }
            set
            {
                mFlags = value;
            }
        }


        /// <summary>
        /// Gets or sets the enabled status for particular debug
        /// </summary>
        /// <param name="key">Debug key</param>
        public bool this[int key]
        {
            get
            {
                // Treat all non-registered debugs as disabled by default
                if (key < 0)
                {
                    return false;
                }

                return Flags[key];
            }
            set
            {
                // Treat all non-registered debugs as disabled by default
                if (key < 0)
                {
                    return;
                }

                Flags[key] = value;
            }
        }


        /// <summary>
        /// Gets or sets the enabled status for particular debug
        /// </summary>
        /// <param name="debug">Debug</param>
        public bool this[DebugSettings debug]
        {
            get
            {
                return this[debug.DebugKey];
            }
            set
            {
                this[debug.DebugKey] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates a new instance of <see cref="RequestSettings"/> 
        /// and initialize the <see cref="Flags"/> with shallow copy of current <see cref="Flags"/>.
        /// </summary>
        internal RequestSettings Clone()
        {
            var result = new RequestSettings
            {
                Flags = (bool[]) Flags.Clone()
            };

            return result;
        }

        #endregion
    }
}