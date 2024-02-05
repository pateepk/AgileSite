using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.IO;

using SystemIO = System.IO;

namespace CMS.Helpers
{
    /// <summary>
    /// Class that writes data to the given context's response and handles multipart and range requests. 
    /// </summary>
    public class ResponseDataSender
    {
        #region "Constants"

        // Buffer size 64 kB.
        private const int BUFFER_SIZE = 65535;

        // Connection timeout (seconds).
        private const int CONNECTION_TIMEOUT = 1;

        // Check connection every X buffering iterations.
        private const int CONNECTION_CHECK = 10;

        #endregion


        #region "HTTP constants"

        // Range start constant
        private const int RANGE_START = 0;

        // Range end constant
        private const int RANGE_END = 1;

        private const string MULTIPART_BOUNDARY = "<q1w2e3r4t5y6u7i8o9p0>";
        private const string MULTIPART_CONTENTTYPE = "multipart/byteranges; boundary=" + MULTIPART_BOUNDARY;
        private const string MULTIPART_BOUNDARY_DELIMITER = "--";
        private const string HTTP_HEADER_ACCEPT_RANGES = "Accept-Ranges";
        private const string HTTP_HEADER_ACCEPT_RANGES_BYTES = "bytes";
        private const string HTTP_HEADER_CONTENT_TYPE = "Content-Type";
        private const string HTTP_HEADER_CONTENT_RANGE = "Content-Range";
        private const string HTTP_HEADER_CONTENT_LENGTH = "Content-Length";
        private const string HTTP_HEADER_RANGE = "Range";

        private const string HTTP_METHOD_HEAD = "HEAD";

        #endregion


        #region "Variables"

        private bool mAcceptRange = true;
        private static bool? mAcceptRanges;

        #endregion


        #region "Properties"

        /// <summary>
        /// Logs the exceptions caused by the process.
        /// </summary>
        public virtual bool LogExceptions
        {
            get;
            set;
        }


        /// <summary>
        /// Whether to log exception caused by communication problems (e.g. when remote host closes the connection).
        /// Log exceptions has to be set to TRUE.
        /// </summary>
        public virtual bool LogCommunicationExceptions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether range requests are enabled (ex. for resumable downloads). If false, the HTTP Handler
        /// ignores the Range HTTP Header and returns the entire contents.
        /// </summary>
        public static bool AcceptRanges
        {
            get
            {
                if (mAcceptRanges == null)
                {
                    mAcceptRanges = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSGetFileAcceptRanges"], true);
                }

                return mAcceptRanges.Value;
            }
        }

        #endregion


        #region "Current request properties"

        /// <summary>
        ///  Current HTTP context.
        /// </summary>
        protected HttpContextBase Context
        {
            get;
            private set;
        }


        /// <summary>
        /// Current HTTP response.
        /// </summary>
        protected HttpResponseBase Response
        {
            get
            {
                return Context.Response;
            }
        }


        /// <summary>
        /// Indicates whether it is range request.
        /// </summary>
        public bool IsRangeRequest
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether it is multipart range request.
        /// </summary>
        public bool IsMultipart
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates if resumable downloads should be supported for current file.
        /// </summary>
        public bool AcceptRange
        {
            get
            {
                return mAcceptRange;
            }
            set
            {
                mAcceptRange = value;
            }
        }


        /// <summary>
        /// 2D Array in format {{START_RANGE,END_RANGE},{START_RANGE, END_RANGE}}.
        /// </summary>
        public long[,] Ranges
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether ranges are valid.
        /// TRUE: ranges are valid or request is not range request
        /// FALSE: all other cases
        /// </summary>
        public bool AreRangesValid
        {
            get;
            private set;
        }


        /// <summary>
        /// Size of data.
        /// </summary>
        public long DataLength
        {
            get;
            private set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new ResponseDataSender instance.
        /// </summary>
        /// <param name="context">HTTP context that will be used for sending a data.</param>
        public ResponseDataSender(HttpContextBase context)
        {
            Context = context;
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Streams the data file to the response.
        /// </summary>
        /// <param name="filepath">File path</param>
        /// <param name="returnOutputData">If true, output data is returned</param>
        /// <returns>Returns streamed binary data if requested by <paramref name="returnOutputData"/>. Binary data are never returned for range requests.</returns>
        public virtual byte[] WriteFile(string filepath, bool returnOutputData = false)
        {
            if ((filepath == null) || !File.Exists(filepath))
            {
                return null;
            }

            SystemIO.Stream str = null;

            try
            {
                // Open the file
                str = FileStream.New(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE);

                // Get the file size
                DataLength = str.Length;
                int dataToRead = (int)DataLength;

                // Parse ranges and set range-related properties
                Ranges = GetRange(DataLength, Context);

                bool downloadBroken = false;
                string fileContentType = Response.ContentType;

                // Get Content-Length response header value and write it to the response
                int responseContentLength = GetResponseContentLength(fileContentType);
                Response.AppendHeader(HTTP_HEADER_CONTENT_LENGTH, responseContentLength.ToString());

                if (AcceptRanges)
                {
                    // Range requests are being accepted
                    Response.AppendHeader(HTTP_HEADER_ACCEPT_RANGES, HTTP_HEADER_ACCEPT_RANGES_BYTES);
                }

                if (IsMultipart)
                {
                    // Set content type of multipart response (file's actual mime type is written into the response later)
                    Response.ContentType = MULTIPART_CONTENTTYPE;
                }

                if (IsRangeRequest)
                {
                    // Do not return binary data for range requests as these requests ask only for incomplete binary data
                    returnOutputData = false;
                }

                // Buffer used for streaming
                byte[] buffer = null;

                if (DataLength > 0)
                {
                    // Log to the request
                    RequestDebug.LogRequestOperation("WriteFile", filepath + " (" + DataHelper.GetSizeString(dataToRead) + ")", 1);

                    try
                    {
                        if (!RequestHelper.HttpMethod.EqualsCSafe(HTTP_METHOD_HEAD, true))
                        {
                            // Flush the HEAD information to the client
                            Response.Flush();

                            // Stream the chunks to the client for each requested range
                            for (int i = Ranges.GetLowerBound(0); i <= Ranges.GetUpperBound(0); i++)
                            {
                                // Resume downloading checking
                                str.Position = Ranges[i, RANGE_START];

                                // Calculate the total amount of bytes for current range
                                int bytesToRead = Convert.ToInt32(Ranges[i, RANGE_END] - Ranges[i, RANGE_START]) + 1;

                                // Append headers needed for multipart response
                                if (IsMultipart)
                                {
                                    // The multipart boundary, actual mime type of this part and content range
                                    Response.Output.WriteLine(MULTIPART_BOUNDARY_DELIMITER + MULTIPART_BOUNDARY);
                                    Response.Output.WriteLine(HTTP_HEADER_CONTENT_TYPE + ": " + fileContentType);
                                    Response.Output.WriteLine(HTTP_HEADER_CONTENT_RANGE + ": bytes " + Ranges[i, RANGE_START] + "-" + Ranges[i, RANGE_END] + "/" + DataLength);

                                    // Intermediate header end indication
                                    Response.Output.WriteLine();
                                }

                                // Set request timeout
                                Context.Server.ScriptTimeout = AttachmentHelper.ScriptTimeout;

                                // Set starting position
                                int offset = (int)Ranges[i, RANGE_START];
                                int beginOffset = 0;
                                int iterationIndex = 0;
                                DateTime nextConnectionCheck = DateTime.Now.AddSeconds(CONNECTION_TIMEOUT);

                                beginOffset = offset;

                                // Stream the range to the client
                                while (bytesToRead > 0)
                                {
                                    // Prepare the buffer
                                    int bufferSize = BUFFER_SIZE;
                                    if (returnOutputData || (bufferSize > dataToRead))
                                    {
                                        bufferSize = dataToRead;
                                    }

                                    buffer = new byte[bufferSize];

                                    // Read whole file or a chunk of bytes from the stream
                                    int readChunkLength = str.Read(buffer, 0, Math.Min(bufferSize, bytesToRead));


                                    // Write the data to the current output stream
                                    Response.OutputStream.Write(buffer, 0, readChunkLength);

                                    // Flush the data to the output
                                    Response.Flush();

                                    // Reduce bytesToRead
                                    bytesToRead -= readChunkLength;

                                    // Set new offset
                                    offset += readChunkLength;

                                    // Verify that the client is connected (not on the first iteration)
                                    if ((iterationIndex++ > CONNECTION_CHECK) && (nextConnectionCheck < DateTime.Now))
                                    {
                                        iterationIndex = 0;

                                        if (Response.IsClientConnected)
                                        {
                                            nextConnectionCheck = DateTime.Now.AddSeconds(CONNECTION_TIMEOUT);
                                        }
                                        else
                                        {
                                            // Stop downstreaming
                                            bytesToRead = -1;
                                            downloadBroken = true;
                                        }
                                    }
                                }

                                if (IsMultipart)
                                {
                                    // Mark the end of the part
                                    Response.Output.WriteLine();
                                }

                                if (downloadBroken)
                                {
                                    break;
                                }
                            }

                            if (!downloadBroken)
                            {
                                if (IsMultipart)
                                {
                                    // Close the multipart response
                                    Response.Output.WriteLine(MULTIPART_BOUNDARY_DELIMITER + MULTIPART_BOUNDARY + MULTIPART_BOUNDARY_DELIMITER);
                                }
                            }
                        }

                        // End safely the current request and finish the current call stack.
                        // CompleteRequest() does not fire ThreadAbortException.
                        RequestHelper.CompleteRequest();
                    }
                    catch (Exception ex)
                    {
                        // Log the exception
                        if (LogExceptions)
                        {
                            if (LogCommunicationExceptions || !IsCommunicationError(ex))
                            {
                                CoreServices.EventLog.LogException("ResponseDataSender", "SendData", ex);
                            }
                            else
                            {
                                // Do not log that remote host has closed the connection
                            }
                        }
                    }

                    // Return the output data
                    if (returnOutputData)
                    {
                        return buffer;
                    }
                }
            }
            finally
            {
                if (str != null)
                {
                    str.Close();
                }
            }

            return null;
        }


        /// <summary>
        /// Streams the byte array to the response.
        /// </summary>
        /// <param name="data">Data to write</param>
        public void WriteBytes(byte[] data)
        {
            if (data == null)
            {
                return;
            }

            // Get size of data
            DataLength = data.Length;

            if (DataLength <= 0)
            {
                return;
            }

            // Log to the request
            RequestDebug.LogRequestOperation("WriteBytes", DataHelper.GetSizeString(DataLength), 1);

            try
            {
                // Parse ranges and set range-related properties
                Ranges = GetRange(DataLength, Context);

                bool downloadBroken = false;
                string fileContentType = Response.ContentType;

                // Get Content-Length response header value and write it to the response
                int responseContentLength = GetResponseContentLength(fileContentType);
                Response.AppendHeader(HTTP_HEADER_CONTENT_LENGTH, responseContentLength.ToString());

                if (AcceptRanges)
                {
                    // Range requests are being accepted
                    Response.AppendHeader(HTTP_HEADER_ACCEPT_RANGES, HTTP_HEADER_ACCEPT_RANGES_BYTES);
                }

                if (IsMultipart)
                {
                    // Set content type of multipart response (file's actual mime type is written into the response later)
                    Response.ContentType = MULTIPART_CONTENTTYPE;
                }

                if (!RequestHelper.HttpMethod.EqualsCSafe(HTTP_METHOD_HEAD, true))
                {
                    // Flush the HEAD information to the client
                    Response.Flush();

                    // Stream the chunks to the client for each requested range
                    for (int i = Ranges.GetLowerBound(0); i <= Ranges.GetUpperBound(0); i++)
                    {
                        // Calculate the total amount of bytes for current range
                        int bytesToRead = Convert.ToInt32(Ranges[i, RANGE_END] - Ranges[i, RANGE_START]) + 1;

                        // Append headers needed for multipart response
                        if (IsMultipart)
                        {
                            // The multipart boundary, actual mime type of this part and content range
                            Response.Output.WriteLine(MULTIPART_BOUNDARY_DELIMITER + MULTIPART_BOUNDARY);
                            Response.Output.WriteLine(HTTP_HEADER_CONTENT_TYPE + ": " + fileContentType);
                            Response.Output.WriteLine(HTTP_HEADER_CONTENT_RANGE + ": bytes " + Ranges[i, RANGE_START] + "-" + Ranges[i, RANGE_END] + "/" + DataLength);

                            // Intermediate header end indication
                            Response.Output.WriteLine();
                        }

                        // Set request timeout
                        Context.Server.ScriptTimeout = AttachmentHelper.ScriptTimeout;

                        // Set starting position
                        int offset = (int)Ranges[i, RANGE_START];
                        int beginOffset = 0;
                        int iterationIndex = 0;
                        DateTime nextConnectionCheck = DateTime.Now.AddSeconds(CONNECTION_TIMEOUT);

                        beginOffset = offset;

                        // Stream the range to the client
                        while (bytesToRead > 0)
                        {
                            // Get a chunk size to be read
                            int readChunkLength = Math.Min(BUFFER_SIZE, bytesToRead);

                            // Write the data to the current output stream
                            Response.OutputStream.Write(data, offset, readChunkLength);

                            // Flush the data to the output
                            Response.Flush();

                            // Reduce bytesToRead
                            bytesToRead -= readChunkLength;
                            // Set new offset
                            offset += readChunkLength;


                            // Verify that the client is connected (not on the first iteration)
                            if ((iterationIndex++ > CONNECTION_CHECK) && (nextConnectionCheck < DateTime.Now))
                            {
                                iterationIndex = 0;

                                if (Response.IsClientConnected)
                                {
                                    nextConnectionCheck = DateTime.Now.AddSeconds(CONNECTION_TIMEOUT);
                                }
                                else
                                {
                                    // Stop downstreaming
                                    bytesToRead = -1;
                                    downloadBroken = true;
                                }
                            }
                        }

                        if (IsMultipart)
                        {
                            // Mark the end of the part
                            Response.Output.WriteLine();
                        }

                        if (downloadBroken)
                        {
                            break;
                        }
                    }

                    if (!downloadBroken)
                    {
                        if (IsMultipart)
                        {
                            // Close the multipart response
                            Response.Output.WriteLine(MULTIPART_BOUNDARY_DELIMITER + MULTIPART_BOUNDARY + MULTIPART_BOUNDARY_DELIMITER);
                        }
                    }
                }

                Response.End();
            }
            catch (ThreadAbortException)
            {
                // Do not log Reponse.End()'s exception
                Thread.ResetAbort();

            }
            catch (Exception ex)
            {
                // Log the exception
                if (LogExceptions)
                {
                    if (LogCommunicationExceptions || !IsCommunicationError(ex))
                    {
                        CoreServices.EventLog.LogException("ResponseDataSender", "SendData", ex);
                    }
                    else
                    {
                        // Do not log that remote host has closed the connection
                    }
                }
            }
        }


        /// <summary>
        /// Parses the range header from the request.
        /// </summary>
        /// <param name="size">Size of data</param>
        /// <param name="currentContext">Current HTTP context</param>
        /// <returns>2D Array in format {{START_RANGE,END_RANGE},{START_RANGE, END_RANGE}}</returns>
        public virtual long[,] GetRange(long size, HttpContextBase currentContext)
        {
            long[,] rangesArray = null;

            long endIndex = size - 1;

            // Try to retrieve range header value from request
            string sourceRange = RequestHelper.GetHeader(HTTP_HEADER_RANGE, string.Empty).Replace("\"", string.Empty);

            IsRangeRequest = !string.IsNullOrEmpty(sourceRange);

            AreRangesValid = true;
            if (!IsRangeRequest)
            {
                // Return the entire file range when range was not requested
                rangesArray = new long[1, 2] { { 0, endIndex } };
            }
            else
            {
                // Split ranges to array
                string[] ranges = sourceRange.Replace("bytes=", string.Empty).Split(',');

                rangesArray = new long[ranges.GetUpperBound(0) + 1, 2];

                // Check consistency of each range
                for (int i = ranges.GetLowerBound(0); i <= ranges.GetUpperBound(0); i++)
                {
                    // Get start and end of range
                    string[] singleRange = ranges[i].Split('-');

                    // Determine the end of the requested range
                    if (string.IsNullOrEmpty(singleRange[RANGE_END]))
                    {
                        // Take the entire range when end wasn't specified
                        rangesArray[i, RANGE_END] = endIndex;
                    }
                    else
                    {
                        rangesArray[i, RANGE_END] = long.Parse(singleRange[RANGE_END]);
                    }

                    // Determine the beginning of the requested range
                    if (string.IsNullOrEmpty(singleRange[RANGE_START]))
                    {
                        // Return the last n bytes when no beginning was specified
                        rangesArray[i, RANGE_START] = endIndex - rangesArray[i, RANGE_END];
                        rangesArray[i, RANGE_END] = endIndex;
                    }
                    else
                    {
                        rangesArray[i, RANGE_START] = long.Parse(singleRange[RANGE_START]);
                    }

                    // Ensure that the range end does not exceed the size of the data
                    rangesArray[i, RANGE_END] = Math.Min(endIndex, rangesArray[i, RANGE_END]);

                    // Check whether the requested range values are valid
                    if (((rangesArray[i, RANGE_START] > endIndex) | (rangesArray[i, RANGE_END] > endIndex)) || ((rangesArray[i, RANGE_START] < 0) | (rangesArray[i, RANGE_END] < 0)) || (rangesArray[i, RANGE_END] < rangesArray[i, RANGE_START]))
                    {
                        // When end value exceeds the file size or is lower than 0 or is lower than start value
                        AreRangesValid = false;
                    }
                }
            }

            // Set property indicating whether request is multipart request
            IsMultipart = Convert.ToBoolean(rangesArray.GetUpperBound(0) > 0);
            return rangesArray;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets response content length and sets up response.
        /// </summary>
        /// <param name="fileContentType">Content type</param>
        /// <returns>Response content length</returns>
        private int GetResponseContentLength(string fileContentType)
        {
            int responseContentLength = default(int);

            if (!AcceptRanges || !IsRangeRequest || !AcceptRange || !AreRangesValid)
            {
                // Not a range request or range requests are not accepted

                // Set content length (complete size)
                responseContentLength = Convert.ToInt32(DataLength);
                Response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                // Range request (if the range arrays contain more than one entry, it is a multipart range request)

                // Calculate the entire response length
                for (int i = Ranges.GetLowerBound(0); i <= Ranges.GetUpperBound(0); i++)
                {
                    // The length of the content (for current range)
                    responseContentLength += Convert.ToInt32(Ranges[i, RANGE_END] - Ranges[i, RANGE_START]) + 1;

                    if (IsMultipart)
                    {
                        // Calculate the length of the intermediate headers
                        responseContentLength += MULTIPART_BOUNDARY.Length;
                        responseContentLength += fileContentType.Length;
                        responseContentLength += Ranges[i, RANGE_START].ToString().Length;
                        responseContentLength += Ranges[i, RANGE_END].ToString().Length;
                        responseContentLength += DataLength.ToString().Length;
                        // Length of line break and other needed characters in one multipart header
                        responseContentLength += 49;
                    }
                }

                if (IsMultipart)
                {
                    // Calculate the length of the last intermediate header
                    responseContentLength += MULTIPART_BOUNDARY.Length;
                    // Length of dash and line break characters
                    responseContentLength += 6;
                }
                else
                {
                    // Indicate range in the initial HTTP Header
                    Response.AppendHeader(HTTP_HEADER_CONTENT_RANGE, "bytes " + Ranges[0, RANGE_START] + "-" + Ranges[0, RANGE_END] + "/" + DataLength);
                }

                // Set partial response status code
                Response.StatusCode = (int)HttpStatusCode.PartialContent;
            }
            return responseContentLength;
        }


        /// <summary>
        /// Determines whether given exception is caused by communication problem (e.g. when remote host closes the connection).
        /// </summary>
        /// <param name="ex">Exception to examine</param>
        /// <returns>TRUE if exception is thrown due to communication problems</returns>
        private bool IsCommunicationError(Exception ex)
        {
            List<long> communicationErrorCodes = new List<long>
            {
                0x800704CD,
                0x800703E3,
                0x80070016
            };
            return (ex is HttpException) && communicationErrorCodes.Contains(ValidationHelper.GetHResult(((HttpException)ex).ErrorCode));
        }

        #endregion
    }
}
