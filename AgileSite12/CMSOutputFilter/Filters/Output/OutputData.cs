using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml.Serialization;

using CMS.Base;
using CMS.Helpers;
using CMS.PortalEngine;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Output data container.
    /// </summary>
    [Serializable]
    public class OutputData
    {
        #region "Variables"

        /// <summary>
        /// Output HTML with debug information trimmed.
        /// </summary>
        protected string mTrimmedHtml;

        /// <summary>
        /// Output data for the stream.
        /// </summary>
        protected byte[] mDataPlain;

        /// <summary>
        /// Output data for the stream with debug information trimmed.
        /// </summary>
        protected byte[] mTrimmedDataPlain;

        /// <summary>
        /// Output data in gzip format for the stream.
        /// </summary>
        protected byte[] mDataCompressed;

        /// <summary>
        /// Output data in gzip format for the stream with debug information trimmed.
        /// </summary>
        protected byte[] mTrimmedDataCompressed;

        /// <summary>
        /// True if the output contains some substitutions.
        /// </summary>
        protected bool? mHasSubstitutions;

        #endregion


        #region "Serializable properties"

        /// <summary>
        /// If true, the output has the debug information
        /// </summary>
        public bool HasDebugInformation
        {
            get;
            set;
        }


        /// <summary>
        /// Output HTML.
        /// </summary>
        public string Html
        {
            get;
            set;
        }


        /// <summary>
        /// GZip the data.
        /// </summary>
        public bool GZip
        {
            get;
            set;
        }

        #endregion


        #region "Automatically evaluated properties"

        /// <summary>
        /// Returns the encoding used by this object
        /// </summary>
        [XmlIgnore]
        public Encoding Encoding
        {
            get;
        }


        /// <summary>
        /// True if the output contains some substitutions.
        /// </summary>
        [XmlIgnore]
        public bool HasSubstitutions
        {
            get
            {
                if (mHasSubstitutions == null)
                {
                    mHasSubstitutions = Html.Contains("{~");
                }

                return mHasSubstitutions.Value;
            }
            set
            {
                mHasSubstitutions = value;
            }
        }


        /// <summary>
        /// HTML trimmed with unnecessary information such as debug.
        /// </summary>
        [XmlIgnore]
        public string TrimmedHtml
        {
            get
            {
                if (!HasDebugInformation)
                {
                    // No debug information, return standard HTML
                    return Html;
                }

                if (mTrimmedHtml == null)
                {
                    // Remove the debug information
                    int debugEnd = Html.LastIndexOf("<!--LOGEND-->", StringComparison.OrdinalIgnoreCase);
                    if (debugEnd >= 0)
                    {
                        int debugStart = Html.LastIndexOf("<!--LOG-->", debugEnd, StringComparison.OrdinalIgnoreCase);
                        if (debugStart >= 0)
                        {
                            string trimmedHtml = Html.Remove(debugStart, debugEnd - debugStart + 13);

                            // Inject the output cache information
                            string info = "<div style=\"padding: 2px; font-weight: bold; background-color: #eecccc; border: solid 1px #ff0000;\">" + ResHelper.GetString("Debug.OutputCacheInfo") + "</div>";
                            trimmedHtml = trimmedHtml.Insert(debugStart, info);

                            // Assign the trimmed HTML
                            mTrimmedHtml = trimmedHtml;
                        }
                    }

                    // Assign standard HTML
                    if (mTrimmedHtml == null)
                    {
                        mTrimmedHtml = Html;
                    }
                }

                return mTrimmedHtml;
            }
            set
            {
                mTrimmedHtml = value;
            }
        }


        /// <summary>
        /// Output data.
        /// </summary>
        [XmlIgnore]
        public byte[] Data
        {
            get
            {
                byte[] data = ClientGZip ? mDataCompressed : mDataPlain;

                if (data == null)
                {
                    data = GetData(Html);

                    if (ClientGZip)
                    {
                        mDataCompressed = data;
                    }
                    else
                    {
                        mDataPlain = data;
                    }
                }

                return data;
            }
        }


        /// <summary>
        /// Output data trimmed with unnecessary information such as debug.
        /// </summary>
        [XmlIgnore]
        public byte[] TrimmedData
        {
            get
            {
                if (!HasDebugInformation)
                {
                    // No debug information, return standard data
                    return Data;
                }
                else
                {
                    byte[] trimmedData = ClientGZip ? mTrimmedDataCompressed : mTrimmedDataPlain;

                    // Some debug information, return trimmed data
                    if (trimmedData == null)
                    {
                        trimmedData = GetData(TrimmedHtml);

                        if (ClientGZip)
                        {
                            mTrimmedDataCompressed = trimmedData;
                        }
                        else
                        {
                            mTrimmedDataPlain = trimmedData;
                        }
                    }

                    return trimmedData;
                }
            }
        }


        /// <summary>
        /// Returns whether current user supports GZip and it is allowed on server.
        /// </summary>
        [XmlIgnore]
        private bool ClientGZip
        {
            get
            {
                // If GZip is not allowed, return false
                if (!GZip || !RequestHelper.AllowGZip)
                {
                    return false;

                }

                return OutputFilterContext.GZipSupported;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public OutputData()
        {
            Encoding = Encoding.UTF8;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="outputHtml">Output HTML</param>
        /// <param name="gZip">GZip the data</param>
        /// <param name="outputEncoding">Output encoding</param>
        public OutputData(string outputHtml, bool gZip, Encoding outputEncoding)
        {
            HasDebugInformation = DebugContext.DebugPresentInResponse;

            Html = outputHtml;
            GZip = gZip;
            Encoding = outputEncoding;
        }


        /// <summary>
        /// Writes the data to the stream and closes the stream.
        /// </summary>
        /// <param name="s">Stream to write to</param>
        /// <param name="trimmed">If true, the trimmed version of the output is written to the output</param>
        /// <param name="allowSubstitutions">If true, the substitutions are allowed in the code</param>
        /// <returns>Returns the size of data written to the output</returns>
        public long WriteOutputToStream(Stream s, bool trimmed, bool allowSubstitutions)
        {
            // Add encoding if gzipped
            if (ClientGZip)
            {
                CMSHttpContext.Current.Response.AppendHeader("Content-Encoding", "deflate");
                CMSHttpContext.Current.Response.AppendHeader("Vary", "Accept-Encoding");
                RequestContext.ResponseIsCompressed = true;
            }

            byte[] data = null;

            if (allowSubstitutions && HasSubstitutions)
            {
                // Resolve only in preview or live site mode
                ViewModeEnum viewMode = PortalContext.ViewMode;
                switch (viewMode)
                {
                    case ViewModeEnum.LiveSite:
                    case ViewModeEnum.Preview:
                        {
                            // Resolve the substitutions
                            string html = trimmed ? TrimmedHtml : Html;

                            // Resolve the substitutions
                            HasSubstitutions = ResponseOutputFilter.ResolveSubstitutions(ref html);

                            // Get the data
                            data = GetData(html);
                        }
                        break;
                }
            }

            if (data == null)
            {
                // Get the data standard way
                data = trimmed ? TrimmedData : Data;
            }

            // Write only if data exists
            if (data.Length > 0)
            {
                // Write data
                s.Write(data, 0, data.Length);
            }

            // Close the stream
            s.Close();

            return data.Length;
        }


        /// <summary>
        /// Gets the data for given HTML.
        /// </summary>
        /// <param name="html">Output HTML</param>
        protected byte[] GetData(string html)
        {
            // Get the bytes from string
            byte[] data = Encoding.GetBytes(html);

            if (ClientGZip)
            {
                // GZip the html
                using (MemoryStream ms = new MemoryStream())
                {
                    using (DeflateStream gs = new DeflateStream(ms, CompressionMode.Compress))
                    {
                        gs.Write(data, 0, data.Length);
                    }

                    // Get the data
                    data = ms.ToArray();
                }
            }

            return data;
        }

        #endregion
    }
}