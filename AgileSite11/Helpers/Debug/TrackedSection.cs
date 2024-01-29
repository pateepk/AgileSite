using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using CMS.Base;
using CMS.IO;

namespace CMS.Helpers
{
    using TrackDictionary = StringSafeDictionary<List<TrackedSection>>;

    /// <summary>
    /// Defines a concurrent section within the code
    /// </summary>
    public class TrackedSection : IDisposable
    {
        #region "Variables"

        /// <summary>
        /// List of currently opened concurrent sections organized by track name
        /// </summary>
        private static readonly TrackDictionary mOpenSections = new TrackDictionary();

        /// <summary>
        /// Object for locking the context
        /// </summary>
        private static readonly object lockObject = new object();


        /// <summary>
        /// List of guids identifying the tracks on which this section belongs to
        /// </summary>
        protected List<Guid> mTracks = new List<Guid>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Section name
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }


        /// <summary>
        /// Time when the section has started
        /// </summary>
        public DateTime Started
        {
            get;
            protected set;
        }

        
        /// <summary>
        /// Section thread ID
        /// </summary>
        public int ThreadID
        {
            get;
            protected set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Tracks the current list of running concurrent sections
        /// </summary>
        /// <param name="name">Track name</param>
        /// <param name="reason">Track reason</param>
        public static void TrackOpenSections(string name, string reason)
        {
            Guid trackGuid = Guid.NewGuid();

            lock (lockObject)
            {
                var threads = new SafeDictionary<int, TrackedSection>();
                int currentThreadId = CMSThread.GetCurrentThreadId();

                // Add the track GUIDs to all currently opened sections
                var sections = mOpenSections[name];
                if (sections != null)
                {
                    foreach (var section in sections)
                    {
                        var sectionThreadId = section.ThreadID;
                        if (currentThreadId != sectionThreadId)
                        {
                            // Remove the higher track if found (will be covered by this)
                            var existing = threads[sectionThreadId];
                            if (existing != null)
                            {
                                existing.RemoveTrack(trackGuid);
                            }

                            section.AddTrack(trackGuid);

                            threads[sectionThreadId] = section;
                        }
                    }
                }

                // Write track intro
                string text = String.Format(
                    @"
----------------------------------------
Track origin
----------------------------------------
Thread ID: {0}
Track started: {1}
Reason: {2}
Stack trace: 

{3}
",
                    currentThreadId,
                    DateTime.Now,
                    reason,
                    DebugHelper.GetStack()
                );

                WriteTrack(name, trackGuid, text);
            }
        }


        /// <summary>
        /// Adds the given track to the section
        /// </summary>
        /// <param name="trackGuid">Track GUID</param>
        protected void AddTrack(Guid trackGuid)
        {
            if (mTracks == null)
            {
                mTracks = new List<Guid>();
            }

            mTracks.Add(trackGuid);
        }


        /// <summary>
        /// Removes the given track from the section
        /// </summary>
        /// <param name="trackGuid">Track GUID</param>
        protected void RemoveTrack(Guid trackGuid)
        {
            if (mTracks != null)
            {
                mTracks.Remove(trackGuid);
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the track</param>
        public TrackedSection(string name)
        {
            ThreadID = CMSThread.GetCurrentThreadId();
            Started = DateTime.Now;
            Name = name;

            lock (lockObject)
            {
                // Ensure the list of sections
                var sections = mOpenSections[name];
                if (sections == null)
                {
                    sections = new List<TrackedSection>();

                    mOpenSections[name] = sections;
                }

                sections.Add(this);
            }
        }


        /// <summary>
        /// Disposes the object
        /// </summary>
        [HideFromDebugContext]
        public void Dispose()
        {
            lock (lockObject)
            {
                // Remove from list of sections
                var sections = mOpenSections[Name];
                if (sections != null)
                {
                    sections.Remove(this);
                }

                // Write the tracks
                var tracks = mTracks;
                if (tracks != null)
                {
                    foreach (var track in tracks)
                    {
                        string text = String.Format(
                            @"
----------------------------------------
Thread ID {0}
----------------------------------------
Track started: {1}
Track ended: {2}
Stack trace: 

{3}
",
                            CMSThread.GetCurrentThreadId(),
                            Started,
                            DateTime.Now,
                            DebugHelper.GetStack()
                            );

                        WriteTrack(Name, track, text);
                    }
                }
            }
        }


        /// <summary>
        /// Writes the given track to the track file
        /// </summary>
        /// <param name="name">Track name</param>
        /// <param name="track">Track guid</param>
        /// <param name="text">Track text</param>
        private static void WriteTrack(string name, Guid track, string text)
        {
            try
            {
                string trackFile = String.Format("~/App_Data/track_{0}_{1}.txt", name, track);
                trackFile = StorageHelper.GetFullFilePhysicalPath(trackFile);

                File.AppendAllText(trackFile, text);
            }
            catch
            {
            }
        }

        #endregion
    }
}
