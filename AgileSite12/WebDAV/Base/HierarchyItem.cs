using System;
using System.IO;

using CMS.EventLog;
using CMS.DocumentEngine;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

using File = CMS.IO.File;
using FileAccess = CMS.IO.FileAccess;
using FileMode = CMS.IO.FileMode;
using FileShare = CMS.IO.FileShare;
using FileStream = CMS.IO.FileStream;


namespace CMS.WebDAV
{
    /// <summary>
    /// Base class for resource or folder.
    /// </summary>
    internal abstract class HierarchyItem : IHierarchyItem, ILock
    {
        #region "Constants"

        /// <summary>
        /// Buffer size.
        /// </summary>
        public const int BUFFER_SIZE = 65535;

        #endregion


        #region "Variables"

        /// <summary>
        /// Url parser.
        /// </summary>
        private UrlParser mUrlParser = null;
        private WorkflowManager mWorkflowManager = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets engine.
        /// </summary>
        public WebDAVEngine Engine
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets workflow manager.
        /// </summary>
        public virtual WorkflowManager WorkflowManager
        {
            get
            {
                return mWorkflowManager ?? (mWorkflowManager = WorkflowManager.GetInstance(UrlParser.TreeProvider));
            }
        }


        /// <summary>
        /// Gets URL parser.
        /// </summary>
        public UrlParser UrlParser
        {
            get
            {
                if (mUrlParser == null)
                {
                    return Engine.UrlParser;
                }

                return mUrlParser;
            }
            protected set
            {
                mUrlParser = value;
            }
        }

        #endregion


        #region "Contructors"

        /// <summary>
        /// Initializes hierarchy item.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="name">Name</param>
        /// <param name="created">Creation date</param>
        /// <param name="modified">Modification date</param>
        /// <param name="urlParser">Url parser</param>
        /// <param name="engine">WebDAV engine</param>
        /// <param name="parent">Parent item</param>
        public HierarchyItem(string path, string name, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
        {
            Path = path;
            Name = name;
            Created = created.ToUniversalTime();
            Modified = modified.ToUniversalTime();
            UrlParser = urlParser;
            Engine = engine;
            Parent = parent;
        }

        #endregion


        #region "IHierarchyItem Members"

        /// <summary>
        /// Gets the name of the item in repository.
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the creation date of the item in repository.
        /// </summary>
        public DateTime Created
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the last modification date of the item in repository.
        /// </summary>
        public DateTime Modified
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the parent folder of the item in repository (null for the root of the repository).
        /// </summary>
        public IFolder Parent
        {
            get;
            protected set;
        }


        /// <summary>
        /// Unique item path in the repository relative to storage root.
        /// </summary>
        public string Path
        {
            get;
            protected set;
        }


        /// <summary>
        /// Creates a copy of this item with a new name in the destination folder.
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <param name="deep">Indicates whether to copy entire subtree</param>
        public abstract WebDAVResponse CopyTo(IFolder folder, string destName, bool deep);


        /// <summary>
        /// Moves this item to the destination folder under a new name.
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        public abstract WebDAVResponse MoveTo(IFolder folder, string destName);


        /// <summary>
        /// Deletes this item.
        /// </summary>
        public abstract WebDAVResponse Delete();


        /// <summary>
        /// Gets values of all properties or selected properties for this item.
        /// </summary>
        /// <param name="props">Array of properties</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse GetProperties(ref Property[] props)
        {
            props = new Property[] { };
            return new OkResponse();
        }


        /// <summary>
        /// Modifies and removes properties for this item.
        /// </summary>
        /// <param name="setProps">Array of properties to be set</param>
        /// <param name="delProps">Array of properties to be removed. Value field is ignored.
        /// Specifying the removal of a property that does not exist is not an error. </param>
        public virtual WebDAVResponse UpdateProperties(Property[] setProps, Property[] delProps)
        {
            MultipropResponse resp = new MultipropResponse();
            return resp;
        }


        /// <summary>
        /// Gets names of all properties for this item.
        /// </summary>
        /// <param name="props">New array of properties must be created
        /// and returned through this parameter. </param>
        public WebDAVResponse GetPropertyNames(ref Property[] props)
        {
            props = new Property[] { };
            return new NotAllowedResponse();
        }

        #endregion


        #region "ILock Members"

        /// <summary>
        /// Locks this item.
        /// </summary>
        /// <param name="lockInfo">Describes a lock on this item</param>
        public abstract WebDAVResponse Lock(ref LockInfo lockInfo);


        /// <summary>
        /// Updates lock timeout information on this item.
        /// </summary>
        /// <param name="lockInfo">Describes a lock on this item</param>
        public WebDAVResponse RefreshLock(ref LockInfo lockInfo)
        {
            return new OkResponse();
        }


        /// <summary>
        /// Removes lock with the specified token from this item or deletes lock-null item.
        /// </summary>
        /// <param name="lockToken">Lock with this token should be removed from the item</param>
        public virtual WebDAVResponse Unlock(string lockToken)
        {
            return new NoContentResponse();
        }


        /// <summary>
        /// Gets the array of all locks for this item.
        /// </summary>
        public LockInfo[] ActiveLocks
        {
            get
            {
                return new LockInfo[] { };
            }
        }

        #endregion


        #region "Protected Methods"

        /// <summary>
        /// Streams the data file to the response.
        /// </summary>
        /// <param name="output">Output stream</param>
        /// <param name="byteStart">The zero-based byte offset in resource content at which to begin
        /// copying bytes to the output stream.</param>
        /// <param name="count">The number of bytes to be written to the output stream</param>
        /// <param name="filePath">File path</param>
        protected virtual void WriteFile(Stream output, long byteStart, long count, string filePath)
        {
            // Check if file exists
            if ((filePath == null) || !File.Exists(filePath))
            {
                throw new Exception(string.Format("[HierarchyItem_WriteFile]: The file '{0}' doesn't exist.", filePath));
            }

            Stream str = null;

            try
            {
                // Open the file
                str = FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE);
                long dataLength = str.Length;

                if ((dataLength > 0) && (count > 0) && (byteStart < dataLength))
                {
                    str.Position = byteStart;
                    // Buffer used for streaming
                    byte[] buffer = null;
                    int bytesToRead = 0;
                    // Set bytes to read
                    long readLength = dataLength - byteStart;
                    if (readLength >= count)
                    {
                        bytesToRead = (int)count;
                    }
                    else
                    {
                        bytesToRead = (int)readLength;
                    }

                    // Stream to the client
                    while (bytesToRead > 0)
                    {
                        // Prepare the buffer
                        int bufferSize = Math.Min(BUFFER_SIZE, bytesToRead);

                        buffer = new byte[bufferSize];

                        // Read a chunk of bytes from the stream
                        int readChunkLength = str.Read(buffer, 0, bufferSize);

                        // Write the data to the current output stream
                        output.Write(buffer, 0, readChunkLength);

                        // Flush the data to the output
                        output.Flush();

                        // Reduce bytesToRead
                        bytesToRead -= readChunkLength;
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("HierarchyItem_WriteFile", "WebDAV", ex);
            }
            finally
            {
                if (str != null)
                {
                    str.Close();
                }
            }
        }


        /// <summary>
        /// Streams the byte array to the response.
        /// </summary>
        /// <param name="output">Output stream</param>
        /// <param name="byteStart">The zero-based byte offset in resource content at which to begin
        /// copying bytes to the output stream.</param>
        /// <param name="count">The number of bytes to be written to the output stream</param>
        /// <param name="data">Data to write</param>
        protected virtual void WriteBytes(Stream output, long byteStart, long count, byte[] data)
        {
            // Check data binary
            if (data == null)
            {
                throw new Exception("[HierarchyItem_WriteBytes]: No binary data.");
            }

            long dataLength = data.LongLength;

            if ((dataLength > 0) && (count > 0) && (byteStart < dataLength))
            {
                try
                {
                    // Set bytes to read
                    int bytesToRead = 0;
                    long readLength = dataLength - byteStart;

                    if (readLength >= count)
                    {
                        bytesToRead = (int)count;
                    }
                    else
                    {
                        bytesToRead = (int)readLength;
                    }

                    int offset = (int)byteStart;

                    // Stream to the client
                    while (bytesToRead > 0)
                    {
                        // Get a chunk size to be read
                        int readChunkLength = Math.Min(BUFFER_SIZE, bytesToRead);

                        // Write the data to the current output stream
                        output.Write(data, offset, readChunkLength);

                        // Flush the data to the output
                        output.Flush();

                        // Reduce bytesToRead
                        bytesToRead -= readChunkLength;

                        // Set new offset
                        offset += readChunkLength;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception
                    EventLogProvider.LogException("HierarchyItem_WriteBytes", "WebDAV", ex);
                }
            }
        }

        #endregion
    }
}