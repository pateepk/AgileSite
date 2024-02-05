using System;
using System.Collections.Generic;

using CMS.IO;

namespace CMS.DataEngine
{
    internal static class FileReader
    {
        /// <summary>
        /// Reads file and separates it into list.
        /// </summary>        
        /// <param name="scriptsFolder">Scripts folder</param>
        /// <param name="file">Name of the file with the script</param>
        /// <returns>Returns list of strings representing lines of the scripts file</returns>
        public static List<string> ReadAndSplit(string scriptsFolder, string file)
        {
            var lines = new List<string>();

            string fullPath = Path.Combine(scriptsFolder, file);
            using (StreamReader stream = File.OpenText(fullPath))
            {
                while (!stream.EndOfStream)
                {
                    lines.Add(stream.ReadLine());
                }
            }

            return lines;
        }
    }
}
