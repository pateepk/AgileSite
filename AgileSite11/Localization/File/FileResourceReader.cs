using System;
using System.Xml;

using CMS.IO;

namespace CMS.Localization
{
    /// <summary>
    /// Reader for the resource files.
    /// </summary>
    public class FileResourceReader : IDisposable
    {
        #region "Variables"

        private StreamReader mStreamReader;
        private XmlReader mReader;

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true if the file is completed.
        /// </summary>
        public bool IsCompleted
        {
            get;
            private set;
        }


        /// <summary>
        /// File path.
        /// </summary>
        public string FilePath
        {
            get;
            private set;
        }


        /// <summary>
        /// Reader for the resource file.
        /// </summary>
        protected XmlReader Reader
        {
            get
            {
                if (mReader == null)
                {
                    IsCompleted = true;

                    // Check the file existence
                    if (File.Exists(FilePath))
                    {
                        lock (this)
                        {
                            StreamReader reader = null;
                            XmlReader xml = null;

                            try
                            {
                                // Reader setting
                                XmlReaderSettings rs = new XmlReaderSettings();
                                rs.CloseInput = true;

                                // Open reader
                                reader = StreamReader.New(FilePath);
#pragma warning disable BH1014 // Do not use System.IO
                                xml = XmlReader.Create(System.IO.TextReader.Synchronized(reader), rs);
#pragma warning restore BH1014 // Do not use System.IO

                                // Read the root node
                                if (xml.ReadToFollowing("root"))
                                {
                                    if (xml.ReadToDescendant("data"))
                                    {
                                        IsCompleted = false;
                                    }
                                }

                                mReader = xml;
                                mStreamReader = reader;
                            }
                            catch
                            {
                                // Safely close readers
                                if (xml != null)
                                {
                                    xml.Close();
                                    mReader = null;
                                }

                                if (reader != null)
                                {
                                    reader.Close();
                                    mStreamReader = null;
                                }

                                IsCompleted = true;
                            }
                        }
                    }
                }

                return mReader;
            }
        }

        #endregion


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath">File path to read</param>
        public FileResourceReader(string filePath)
        {
            FilePath = filePath;
        }


        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            // Safely close readers
            if (!IsCompleted)
            {
                CloseReaders();
            }
        }


        /// <summary>
        /// Closes the readers.
        /// </summary>
        protected void CloseReaders()
        {
            IsCompleted = true;

            lock (this)
            {
                if (mReader != null)
                {
                    mReader.Close();
                }

                if (mStreamReader != null)
                {
                    mStreamReader.Close();
                    mStreamReader.Dispose();
                }
            }
        }


        /// <summary>
        /// Gets the next string from the resource file.
        /// </summary>
        /// <param name="name">Returns string name</param>
        /// <param name="value">Returns string value</param>
        public bool GetNextString(ref string name, ref string value)
        {
            if (IsCompleted)
            {
                return false;
            }

            lock (this)
            {
                // Get the reader
                var xml = Reader;
                if (xml == null)
                {
                    return false;
                }

                bool result = false;

                // Read name
                xml.MoveToAttribute("name");

                if ((xml.NodeType == XmlNodeType.Attribute) && (xml.Name == "name"))
                {
                    name = xml.Value;

                    // Go to value
                    if (xml.ReadToFollowing("value"))
                    {
                        value = xml.ReadElementContentAsString();

                        result = true;
                    }
                }

                // If no other data found, close the readers
                if (!xml.ReadToFollowing("data"))
                {
                    CloseReaders();
                }

                return result;
            }
        }
    }
}